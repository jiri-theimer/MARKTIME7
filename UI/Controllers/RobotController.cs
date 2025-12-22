
using BL;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using UI.Models;


namespace UI.Controllers
{
    public class RobotController : Controller
    {

        private readonly BL.Singleton.BackgroundWorkerQueue _backgroundWorkerQueue;
        private BL.Factory _f;

        private readonly IHttpClientFactory _httpclientfactory; //client pro PIPE api
        private readonly BL.TheColumnsProvider _colsProvider;
        private readonly BL.Singleton.ThePeriodProvider _pp;
        private string _guid { get; set; }
        private string _mode { get; set; }

        private string _logdir { get; set; }

        private IEnumerable<BO.x01License> _lisX01 { get; set; }


        public RobotController(BL.Factory f, BL.Singleton.BackgroundWorkerQueue backgroundWorkerQueue, IHttpClientFactory hcf, BL.TheColumnsProvider cp, BL.Singleton.ThePeriodProvider pp)
        {
            _backgroundWorkerQueue = backgroundWorkerQueue;
            _httpclientfactory = hcf;
            _f = f;
            _colsProvider = cp;
            _pp = pp;

        }
        public IActionResult Index(string mode, string today, string oper, string logindomain)     //mode: debug
        {
            var v = new RobotViewModel() { D0 = DateTime.Now, Today = DateTime.Today, ManualOper = oper };

            if (!string.IsNullOrEmpty(today))
            {
                v.D0 = BO.Code.Bas.String2Date(today);
                v.Today = new DateTime(v.D0.Year, v.D0.Month, v.D0.Day);

            }

            _mode = mode;

            _lisX01 = _f.App.lisX01.Where(p => p.x01RobotLogin != null && !p.isclosed);
            if (!string.IsNullOrEmpty(logindomain))
            {
                _lisX01 = _lisX01.Where(p => p.x01LoginDomain == logindomain);  //omezit spuštění robota pouze na jeden záznam x01id
            }
            if (!ValidateOnStart(v))
            {
                return View(v);
            }

            LogInfoTextOnly(BO.j91RobotTaskFlag.Start, $"Robot Start, guid: {v.Guid}, _lisX01: {_lisX01.Count()}x");

            if (_f.App.IsRobot)
            {
                _backgroundWorkerQueue.QueueBackgroundWorkItem(async token =>
                {
                    await Task.Delay(1);

                    handle_start_robot(v);
                });
            }
            else
            {
                handle_start_robot(v);
            }



            return View(v);
        }

        private void handle_start_robot(RobotViewModel v)
        {

            LogInfoTextOnly(BO.j91RobotTaskFlag.Start, "handle_start_robot: START");

            foreach (var recX01 in _lisX01)
            {
                _f.db = null;

                if (recX01.x01RobotLogin == null)
                {
                    LogInfoTextOnly(BO.j91RobotTaskFlag.Start, null, $"recX01.x01RobotLogin = null: chybí robot login,  skip {recX01.x01AppName}");
                    continue;
                }

                try
                {
                    _f.InhaleUserByLogin(recX01.x01RobotLogin);
                    _f.SetLic(recX01);
                }
                catch (Exception e)
                {
                    LogInfoTextOnly(BO.j91RobotTaskFlag.Start, null, $"InhaleUserByLogin, login: {recX01.x01RobotLogin}, error: {e.Message}, skip");
                    continue;
                }


                if (BO.Code.Bas.bit_compare_or(recX01.x01LockFlag, 4))
                {
                    LogInfoTextOnly(BO.j91RobotTaskFlag.Start, $"recX01.x01LockFlag=4: v databázi vypnutý robot, skip, {recX01.x01AppName}"); //uzamknutá databáze
                    continue;
                }


                if (_f.CurrentUser == null)
                {
                    LogInfoTextOnly(BO.j91RobotTaskFlag.Start, $"robot-login: {recX01.x01RobotLogin}", "_f.CurrentUser = null, skip");
                    continue;
                }
                else
                {
                    LogInfoTextOnly(BO.j91RobotTaskFlag.Start, $"Začíná běh robota spuštěný pod uživatelem: {_f.CurrentUser.j02Login}, db: {recX01.x01AppName}");
                }



                v.smtp_account = _f.j40MailAccountBL.LoadDefaultSmtpAccount();


                Handle_AutoWorkflow(); //robotické workflow
                Handle_o24Reminder(v);

                Handle_Hlidac(v);

                Handle_Task_Recurrence(v);  //opakované úkoly
                Handle_p40_Recurrence(v);   //opakované úkony
                Handle_Invoice_Recurrence(v);   //opakované faktury

                Handle_ImapRules(v);    //načítání došlé pošty podle imap pravidel

                Handle_Reports_Scheduled(v);    //naplánované tiskové sestavy

                //Handle_MailQueue();


                if (DateTime.Now.Hour >= 15 && DateTime.Now.Hour <= 17 && !UzBezelKrok(BO.j91RobotTaskFlag.CnbKurzy, DateTime.Today))
                {
                    Handle_Cnb(v);
                }

                if ((DateTime.Now.Hour >= 6 && DateTime.Now.Hour <= 24) || v.ManualOper == "bells")
                {
                    //mezi 6:00 - 24:00 ráno běží plnění bells logu
                    if (_f.FBL.RunSql("exec dbo.j97_recovery"))
                    {
                        LogInfo(BO.j91RobotTaskFlag.Bells, "ok");
                    }
                    else
                    {
                        LogInfo(BO.j91RobotTaskFlag.Bells, null, _f.CurrentUser.GetLastMessageNotify());
                    }
                }


                if ((DateTime.Now.Hour >= 3 && DateTime.Now.Hour <= 6 && !UzBezelKrok(BO.j91RobotTaskFlag.ClearTemp, DateTime.Today)) || v.ManualOper == "recovery")
                {
                    //mezi 3:00 - 06:00 ráno běží čištění temp tabulek a temp souborů

                    if (_f.FBL.RunSql("exec dbo.recovery_clear_temp"))
                    {
                        LogInfo(BO.j91RobotTaskFlag.ClearTemp, "ok");
                        _f.FBL.ClearSpaceUsed("temp", 0);
                    }
                    else
                    {
                        LogInfo(BO.j91RobotTaskFlag.ClearTemp, null, _f.CurrentUser.GetLastMessageNotify());
                    }

                }

                if (v.ManualOper == "backup" || (DateTime.Now.Hour >= 20 && !UzBezelKrok(BO.j91RobotTaskFlag.DbBackup, DateTime.Today)))
                {
                    //zálohování databáze
                    string strDbName = null;
                    if (_f.App.HostingMode == BL.Singleton.HostingModeEnum.SharedApp || _f.App.HostingMode == BL.Singleton.HostingModeEnum.TotalCloud)
                    {
                        strDbName = "a7" + recX01.x01LoginDomain.Split(".")[0];
                    }
                    else
                    {
                        strDbName = _f.App.ParseDbNameFromConnectString();
                    }
                    if (strDbName != null)
                    {
                        if (_f.FBL.BackupDatabase(strDbName) != null)
                        {
                            LogInfo(BO.j91RobotTaskFlag.DbBackup, "ok");
                        }
                    }

                }

                if ((DateTime.Now.Hour >= 3 && DateTime.Now.Hour <= 6 && !UzBezelKrok(BO.j91RobotTaskFlag.CentralPing, DateTime.Today)) || v.ManualOper == "ping")
                {
                    //mezi 3:00 - 06 ráno běží central ping vůči mas.marktime.net           

                    var cping = new BL.Code.PingSupport();
                    try
                    {
                        var ret = cping.SendPing(_f).Result;

                        LogInfo(BO.j91RobotTaskFlag.CentralPing, "ok");
                    }
                    catch (Exception e)
                    {
                        LogInfo(BO.j91RobotTaskFlag.CentralPing, null, e.Message);
                    }

                }

                LogInfo(BO.j91RobotTaskFlag.End, null);
            }



            ValidateOnEnd(v);


        }

        private bool UzBezelKrok(BO.j91RobotTaskFlag flag, DateTime d)
        {
            var laststep = _f.FBL.GetLastRobotRun(flag);
            if (laststep == null)
            {
                return false;
            }
            if (laststep.j91Date >= d)
            {
                return true;
            }
            return false;
        }

        private void Handle_AutoWorkflow()
        {
            var lisB06 = _f.b06WorkflowStepBL.GetList(new BO.myQuery("b06")).Where(p => p.b06AutoRunFlag == BO.b06AutoRunFlagEnum.NeUzivatelskyKrok);
            foreach (var recB06 in lisB06.Where(p => p.b06FrameworkSql != null))    //b06FrameworkSql: sql pro hromadné robotické SQL
            {
                List<int> pids = _f.WorkflowBL.GetRecordPidsFromFrameworkSQL(recB06);   //vrací seznam vyhovujících záznamů

                LogInfo(BO.j91RobotTaskFlag.AutoWorkflowSteps, $"Krok {recB06.b06Name}, počet potenciálních záznamů: {pids.Count()}, framework-sql: {recB06.b06FrameworkSql}");

                foreach (int pid in pids)
                {
                    if (_f.WorkflowBL.ValidateRunWorkflowStep(recB06, pid))
                    {
                        int intB05ID = _f.WorkflowBL.RunWorkflowStep(false, pid, recB06.b01Entity, null, 0, recB06, null);  //spustit workflow robotický krok
                    }


                }
            }
        }


        private bool ValidateOnStart(RobotViewModel v)
        {
            _logdir = _f.App.LogFolder + "\\ROBOT\\" + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString();

            if (!System.IO.Directory.Exists(_logdir))
            {
                System.IO.Directory.CreateDirectory(_logdir);
            }



            if (_lisX01.Count() == 0)
            {
                v.ErrorMessage = "Tabulka [x01License] neobsahuje ani jeden záznam pro spouštění robota.";
                LogInfoTextOnly(BO.j91RobotTaskFlag.Start, null, v.ErrorMessage);
                return false;
            }
            _f.db = null;
            _f.InhaleUserByLogin(_lisX01.First().x01RobotLogin);    //je třeba načíst první záznam v x01License
            if (_f.CurrentUser == null || _f.CurrentUser.j02Login == null)
            {
                v.ErrorMessage = $"Nelze načíst uživatelský účet robota: [{_lisX01.First().x01RobotLogin}]!";
            }


            var recP85 = _f.p85TempboxBL.LoadByGuid("robot");
            if (recP85 == null)
            {
                recP85 = new BO.p85Tempbox() { p85GUID = "robot", p85Message = "Robot running", p85FreeDate01 = DateTime.Now, p85FreeText01 = _f.CurrentUser.j02Login };
            }
            else
            {
                if (Convert.ToDateTime(recP85.p85FreeDate01).AddSeconds(60) > DateTime.Now)
                {
                    v.InfoMessage = $"Robot může být spuštěn maximálně jednou za 60 sekund, naposledy spuštěno: {recP85.p85FreeDate01}";
                    return false;
                }

                if (recP85.p85FreeDate02 == null && Convert.ToDateTime(recP85.p85FreeDate01).AddSeconds(300) > DateTime.Now)
                {
                    v.InfoMessage = $"Pravděpdobně nyní běží nějaká úloha robota. Čekáme 300 sekund, naposledy spuštěno: {recP85.p85FreeDate01}";
                    return false;
                }

                recP85.p85FreeDate01 = DateTime.Now;
                recP85.p85FreeDate02 = null;


            }
            _f.p85TempboxBL.Save(recP85);

            return true;
        }

        private void ValidateOnEnd(RobotViewModel v)
        {



            LogInfoTextOnly(BO.j91RobotTaskFlag.End, "Konec běhu robota, metoda [ValidateOnEnd].");


            _f.db = null;
            _f.InhaleUserByLogin(_lisX01.First().x01RobotLogin);    //je třeba načíst první záznam v x01RobotLogin


            if (v.ManualOper == "backup" || (DateTime.Now.Hour >= 20 && !UzBezelKrok(BO.j91RobotTaskFlag.DbBackupVerze6Cloud, DateTime.Today)))
            {
                handle_backup_verze6_cloud();  //zálohování cloud databází, které běží ještě ve verzi 6
                LogInfo(BO.j91RobotTaskFlag.DbBackupVerze6Cloud, "ok");
            }

            var recP85 = _f.p85TempboxBL.LoadByGuid("robot");
            if (recP85 != null)
            {
                recP85.p85FreeDate02 = DateTime.Now;
                _f.p85TempboxBL.Save(recP85);
            }



        }

        private void LogInfoTextOnly(BO.j91RobotTaskFlag flag, string strInfo, string strError = null)
        {
            var strPath = string.Format("{0}\\robot-{1}.log", _logdir, DateTime.Now.ToString("yyyy.MM.dd"));

            try
            {
                string strUserInfo = null;
                if (_f != null && _f.CurrentUser != null)
                {
                    strUserInfo = _f.CurrentUser.j02Login;
                }
                var lis = new List<string>();
                if (!string.IsNullOrEmpty(strInfo))
                {
                    lis.Add($"{DateTime.Now}, {flag}, {strUserInfo}, INFO: {strInfo}");
                }
                if (!string.IsNullOrEmpty(strError))
                {
                    lis.Add($"{DateTime.Now}, {flag}, {strUserInfo} ERROR: {strError}");
                }
                System.IO.File.AppendAllLines(strPath, lis);
            }
            catch
            {

            }
        }
        private void LogInfo(BO.j91RobotTaskFlag flag, string strInfo, string strError = null)
        {
            if (_f != null)
            {
                if (strInfo != null)
                {
                    strInfo = flag.ToString() + ": " + strInfo;
                }
                if (strError != null)
                {
                    strError = flag.ToString() + ": " + strError;
                }
                if (strInfo == null && strError == null)
                {
                    strInfo = flag.ToString();
                }
                var c = new BO.j91RobotLog() { j91Date = DateTime.Now, j91TaskFlag = flag, j91InfoMessage = strInfo, j91ErrorMessage = strError, j91Account = _f.CurrentUser.j02Login, x01ID = _f.CurrentUser.x01ID };
                _f.FBL.AppendRobotLog(c);

            }
            else
            {
                LogInfoTextOnly(flag, strInfo, strError);
            }



        }


        private void Handle_Cnb(RobotViewModel v)
        {

            var errs = new List<string>();
            var succs = new List<string>();


            var strJ27Codes = _f.Lic.x01ImportCnb_j27Codes;
            if (string.IsNullOrEmpty(strJ27Codes)) return;
            foreach (string strJ27Code in BO.Code.Bas.ConvertString2List(strJ27Codes))
            {
                var recJ27 = _f.FBL.LoadCurrencyByCode(strJ27Code);
                if (_f.m62ExchangeRateBL.LoadByQuery(BO.m62RateTypeENUM.InvoiceRate, recJ27.j27ID, 2, v.Today, 0) == null)
                {
                    if (_f.m62ExchangeRateBL.ImportOneRate(_httpclientfactory.CreateClient(), v.Today, recJ27.j27ID) == 0)
                    {
                        errs.Add("ERROR import: " + recJ27.j27Code);
                        LogInfo(BO.j91RobotTaskFlag.CnbKurzy, null, "ERROR CNB Import: " + recJ27.j27Code);
                    }
                    else
                    {
                        succs.Add(strJ27Code);
                    }
                }

            }

            if (succs.Count() > 0)
            {
                LogInfo(BO.j91RobotTaskFlag.CnbKurzy, "ok");
            }

        }

        private void Handle_Task_Recurrence(RobotViewModel v)
        {
            var lisP59 = _f.p58TaskRecurrenceBL.GetList_p59_waiting_on_generate(v.Today.AddDays(-10), v.D0);
            foreach (var c in lisP59)
            {
                _f.p58TaskRecurrenceBL.Generate_Recurrence_Instance(c.p58ID, c.p59ID);
            }

        }
        private void Handle_Invoice_Recurrence(RobotViewModel v)
        {
            var lisP76 = _f.p75InvoiceRecurrenceBL.GetList_p76_waiting_on_generate(v.Today.AddDays(-10), v.D0);
            foreach (var c in lisP76)
            {
                _f.p75InvoiceRecurrenceBL.Generate_Recurrence_Instance(c.p75ID, c.p76ID);
            }

        }
        private void Handle_Reports_Scheduled(RobotViewModel v)
        {
            var lisX31 = _f.x31ReportBL.GetList(new BO.myQueryX31() { IsRecordValid = true }).Where(p => p.x31IsScheduling && (p.x31Entity == null || p.x31Entity == "j02") && p.x31SchedulingReceivers != null);
            foreach (var c in lisX31)
            {
                if (_f.x31ReportBL.IsReportWaiting4Generate(v.D0, c))
                {
                    var cc = new TheReportSupport();
                    string strGUID = BO.Code.Bas.GetGuid();

                    if (c.x31Entity == "j02")
                    {
                        var emails = c.x31SchedulingReceivers.Split(",");   //hodnotu j02id načíst podle e-mail adresy
                        for (int i = 0; i < emails.Count(); i++)
                        {
                            if (!string.IsNullOrEmpty(emails[i]))
                            {
                                var recJ02 = _f.j02UserBL.LoadByEmail(emails[i], 0, _f.CurrentUser.IsHostingModeTotalCloud);
                                if (recJ02 != null)
                                {
                                    strGUID = BO.Code.Bas.GetGuid();
                                    string strTempPdfFileName = cc.GeneratePdfReport(_f, _pp, c, strGUID, recJ02.pid, true, c.x21ID_Scheduling);
                                    if (strTempPdfFileName != null)
                                    {
                                        _f.MailBL.ClearAttachments();   //v robotu je třeba vyčistit, aby tam nezůstali přílohy z předchozích zpráv!!
                                        _f.MailBL.AddAttachment(strTempPdfFileName, !string.IsNullOrEmpty(c.x31ExportFileNameMask) ? c.x31ExportFileNameMask : BO.Code.File.ConvertToSafeFileName(c.x31Name) + ".pdf");
                                        var recX40 = new BO.x40MailQueue() { x40Subject = c.x31Name, x40Recipient = emails[i], j40ID = v.smtp_account.pid };
                                        recX40.x40Body = "Toto je automaticky generovaná zpráva ze systému MARKTIME, neodpovídejte na ní.\r\n\r\nS pozdravem\r\n\r\nMARKTIME robot";
                                        var ret = _f.MailBL.SendMessage(recX40, false, _colsProvider);
                                        if (ret.issuccess)
                                        {
                                            c.x31LastScheduledRun = DateTime.Now;
                                            _f.x31ReportBL.Save(c, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        string strTempPdfFileName = cc.GeneratePdfReport(_f, _pp, c, strGUID, 0, true, c.x21ID_Scheduling);
                        if (strTempPdfFileName != null)
                        {
                            _f.MailBL.ClearAttachments();   //v robotu je třeba vyčistit, aby tam nezůstali přílohy z předchozích zpráv!!
                            _f.MailBL.AddAttachment(strTempPdfFileName, !string.IsNullOrEmpty(c.x31ExportFileNameMask) ? c.x31ExportFileNameMask : BO.Code.File.ConvertToSafeFileName(c.x31Name) + ".pdf");
                            var recX40 = new BO.x40MailQueue() { x40Subject = c.x31Name, x40Recipient = c.x31SchedulingReceivers, j40ID = v.smtp_account.pid };
                            recX40.x40Body = "Toto je automaticky generovaná zpráva ze systému MARKTIME, neodpovídejte na ní.\r\n\r\nS pozdravem\r\n\r\nMARKTIME robot";
                            var ret = _f.MailBL.SendMessage(recX40, false, _colsProvider);
                            if (ret.issuccess)
                            {
                                c.x31LastScheduledRun = DateTime.Now;
                                _f.x31ReportBL.Save(c, null);
                            }
                        }
                    }


                }
            }
        }
        private void Handle_p40_Recurrence(RobotViewModel v)
        {
            var lisP39 = _f.p40WorkSheet_RecurrenceBL.GetList_p39_waiting_on_generate(v.Today.AddDays(-31), v.D0);
            foreach (var c in lisP39)
            {
                _f.p40WorkSheet_RecurrenceBL.Generate_Recurrence_Instance(c);

            }

        }

        private void Handle_Hlidac(RobotViewModel v)
        {
            
            var lisB20 = _f.b20HlidacBL.GetList(new BO.myQuery("b20"));

            Hlidac_Otestuj_Otevrene_Zaznamy("p28", _f.b20HlidacBL.GetListB21_ValidNotClosedRecs("p28"), lisB20, v);
            Hlidac_Otestuj_Otevrene_Zaznamy("p41", _f.b20HlidacBL.GetListB21_ValidNotClosedRecs("p41"), lisB20, v);
            Hlidac_Otestuj_Otevrene_Zaznamy("p56", _f.b20HlidacBL.GetListB21_ValidNotClosedRecs("p56"), lisB20, v);
            Hlidac_Otestuj_Otevrene_Zaznamy("p91", _f.b20HlidacBL.GetListB21_ValidNotClosedRecs("p91"), lisB20, v);
            Hlidac_Otestuj_Otevrene_Zaznamy("x01", _f.b20HlidacBL.GetListB21_ValidNotClosedRecs("x01"), lisB20, v);



        }
        private void Hlidac_Otestuj_Otevrene_Zaznamy(string prefix, IEnumerable<BO.b21HlidacBinding> lisB21, IEnumerable<BO.b20Hlidac> lisB20, RobotViewModel v)
        {
            lisB21 = lisB21.Where(p => p.b20TypeFlag != BO.b20TypeFlagEnum.WipFaHodinyZakazPrekrocit && p.b20TypeFlag != BO.b20TypeFlagEnum.WipFaHonorarZakazPrekrocit && p.b20TypeFlag != BO.b20TypeFlagEnum.WipFaHodinyZdarma && p.b20TypeFlag != BO.b20TypeFlagEnum.WipFaHonorarZdarma);
            if (lisB21.Count() == 0) return;


            foreach (var recB20 in lisB20)
            {
                var sb = new System.Text.StringBuilder();
                var notify2emails = new List<string>();
                foreach (var c in lisB21.Where(p => p.b20ID == recB20.pid))
                {

                    HttpClient httpclient = (recB20.b20TypeFlag == BO.b20TypeFlagEnum.ZmenaAdresy || recB20.b20TypeFlag == BO.b20TypeFlagEnum.Insolvence ? _httpclientfactory.CreateClient() : null);


                    if (_f.b20HlidacBL.Handle_NastalaUdalostHlidace(recB20, c, ref sb, ref notify2emails, httpclient))
                    {
                        //nastala událost hlídače

                        LogInfo(BO.j91RobotTaskFlag.Hlidac, null, $"Nastala událost v hlídači {recB20.b20Name} u záznamu: {recB20.b20Entity}, pid: {c.b21RecordPid}");

                    }

                }
                if (sb.Length > 0)
                {
                    if (recB20.b20NotifyMessage != null)
                    {
                        sb.Insert(0, recB20.b20NotifyMessage);
                    }
                    else
                    {
                        sb.Insert(0, $"<h2>Hlídač zachytil událost [{recB20.b20Name}]!</h2>");

                    }
                    sb.AppendLine($"<small>Čas: {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}</small>");

                    if (v.smtp_account != null)
                    {
                        _f.b20HlidacBL.SendNotification(sb, notify2emails, recB20, v.smtp_account);
                    }
                    else
                    {
                        LogInfo(BO.j91RobotTaskFlag.Hlidac, null, "v.smtp_account is null!");
                    }


                    LogInfo(BO.j91RobotTaskFlag.Hlidac, sb.ToString());
                }
            }


        }

        private void Handle_ImapRules(RobotViewModel v)
        {
            _f.o42ImapRuleBL.Run_ImapRobot();
        }


        private void Handle_o24Reminder(RobotViewModel v)
        {
            if (v.smtp_account == null) return;
            var lisO24 = _f.o24ReminderBL.GetList_Wait_On_Process();

            foreach (var c in lisO24)
            {
                if (c.CalcReminderDate() != null)
                {
                    DateTime d = Convert.ToDateTime(c.CalcReminderDate());
                    if (DateTime.Now > d.AddMinutes(-8))
                    {

                        string strRecAlias = _f.CBL.GetObjectAlias(c.o24RecordEntity, c.o24RecordPid);
                        var recX40 = new BO.x40MailQueue()
                        {
                            x40RecordEntity = (c.o24RecordEntity.Substring(0, 2) == "dd" ? "o23" : c.o24RecordEntity),
                            x40RecordPid = c.o24RecordPid,
                            x40IsRecordLink = true,
                            x40IsHtmlBody = true,
                            x40GridColumns = _f.MailBL.GetDefaultGridFields(c.o24RecordEntity),
                            x40Subject = "Připomenutí - " + strRecAlias,
                            o24ID = c.pid,
                            j40ID = v.smtp_account.pid
                        };
                        var receivers = _f.MailBL.GetMailList(c.j02ID, c.j11ID, c.x67ID, c.p28ID, c.p24ID, c.o24RecordPid, c.o24RecordEntity, 0);
                        if (receivers == null || receivers.Count() == 0 || receivers.Count() > 50)
                        {
                            continue;   //nedělat nic, pokud nejsou příjemci zprávy nebo jich je víc než 50
                        }
                        recX40.x40Recipient = string.Join(";", receivers);
                        var sb = new System.Text.StringBuilder();
                        if (c.o24RecordDate != null)
                        {
                            sb.AppendLine($"Dobrý den,<p></p>Posílám naplánované upozornění k datu: <code>{Convert.ToDateTime(c.o24RecordDate).ToString("dd.MM.yyyy HH:mm")}</code>;");
                        }
                        else
                        {
                            sb.AppendLine($"Dobrý den,<p></p>Posílám naplánované upozornění k datu: <code>{Convert.ToDateTime(c.o24StaticDate).ToString("dd.MM.yyyy HH:mm")}</code>.");
                        }
                        sb.AppendLine($" pro záznam:<br> <strong>{strRecAlias}</strong>.");
                        if (c.o24Memo != null)
                        {
                            sb.AppendLine($"<p><mark>{c.o24Memo}</mark></p>");
                        }

                        sb.AppendLine("<p></p><p></p>");
                        sb.AppendLine("S pozdravem");
                        sb.AppendLine("<p></p>MARKTIME Robot<p></p><p></p>");

                        if ((DateTime.Now - d).TotalHours > 10)
                        {
                            sb.AppendLine("<p></p><p></p><i>PS: Omlouvám se za velmi opožděné zpracování tohoto upozornění.</i>");

                        }

                        recX40.x40Body = sb.ToString();

                        if (c.o24MediumFlag == BO.o24MediumFlagEnum.Email || c.o24MediumFlag == BO.o24MediumFlagEnum.EmailPlusSms)  //odeslat upozornění přes e-mail
                        {
                            var ret = _f.MailBL.SendMessage(recX40, false, _colsProvider);
                            if (ret.issuccess)
                            {
                                c.o24DatetimeProcessed = DateTime.Now;
                                _f.o24ReminderBL.Save(c);
                            }
                        }

                        if (c.o24MediumFlag == BO.o24MediumFlagEnum.Sms || c.o24MediumFlag == BO.o24MediumFlagEnum.EmailPlusSms)  //odeslat upozornění přes sms
                        {
                            var sms = new BL.Code.SmsManagerSupport(_f);
                            var numbers = _f.MailBL.GetGsmList(c.j02ID, c.j11ID, c.x67ID, c.p28ID, c.p24ID, c.o24RecordPid, c.o24RecordEntity, 0);
                            if (numbers != null && numbers.Count() > 0)
                            {
                                var ret = sms.SendMessage(string.Join(";", numbers), $"MARKTIME upozornění pro datum: {Convert.ToDateTime(c.o24RecordDate).ToString("dd.MM.yyyy HH:mm")}, záznam: {strRecAlias}", _httpclientfactory.CreateClient(), c.pid);    //najednou lze zprávu odeslat až na 20 čísel

                                if (c.o24MediumFlag == BO.o24MediumFlagEnum.Sms && ret.Result.issuccess)
                                {
                                    c.o24DatetimeProcessed = DateTime.Now;
                                    _f.o24ReminderBL.Save(c);
                                }
                            }




                        }



                    }
                }
            }
        }

        private bool IsTime4Run(BO.j91RobotTaskFlag flag, double hour_from, double hour_until, double dblPoKolikaMinutachPoustet)
        {
            if (!(DateTime.Today.AddHours(hour_from) <= DateTime.Now && DateTime.Today.AddHours(hour_until) >= DateTime.Now))
            {
                return false;
            }
            var c = _f.FBL.GetLastRobotRun(flag);
            if (c == null)
            {
                return true;
            }
            if (c.j91Date.AddMinutes(dblPoKolikaMinutachPoustet) > DateTime.Now)
            {
                return false;
            }
            return true;
        }


        private void handle_backup_verze6_cloud()
        {
            LogInfoTextOnly(BO.j91RobotTaskFlag.DbBackupVerze6Cloud, $"Start zálohování cloud databází ve staré verzi 6");
            //zálohování cloud databází ve verzi 6, které ještě jedou
            var dt = _f.FBL.GetDataTable("select a03Database_Application as dbname from mtrc.dbo.a03Customer where a03Database_Application is not null and a03Cloud is not null and a03PosledniZapisKdy>DATEADD(day,-10,GETDATE())");
            foreach (DataRow dbrow in dt.Rows)
            {
                _f.FBL.BackupDatabase(dbrow["dbname"].ToString());

            }

            LogInfoTextOnly(BO.j91RobotTaskFlag.DbBackupVerze6Cloud, $"Konec zálohování cloud databází ve staré verzi 6");


        }

    }
}
