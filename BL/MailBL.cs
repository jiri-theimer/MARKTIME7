using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.Linq;
using BO;


namespace BL
{
    public interface IMailBL
    {
        public BO.Result SendMessage(int j40id, Rebex.Mail.MailMessage message); //v Result.pid vrací x40id
        public BO.Result SendMessage(int j40id, string toEmail, string toName, string subject, string body, bool ishtml, string record_entity, int record_pid);  //v BO.Result.pid vrací x40id
        public BO.Result SendMessage(BO.x40MailQueue rec, bool istest, BL.TheColumnsProvider cp);  //v Result.pid vrací x40id
        public BO.Result SendMessageWithoutFactory(string strHtmlBody, string strSubject, string strTo, string strBcc, string strFromAddress, string strFromName);    //odešle zprávu bez vazby na databázi a bez rebexu
        public void AddAttachment(string fullpath, string displayname, string contenttype = null);
        public void AddAttachment(Rebex.Mail.Attachment att);
        public void ClearAttachments();
        public BO.x40MailQueue LoadMessageByPid(int x40id);
        public BO.x40MailQueue LoadMessageByGuid(string guid);
        public bool SaveMailJob2Temp(string strJobGuid, BO.x40MailQueue recX40, string strUploadGuid, List<BO.x43MailQueue_Recipient> lisX43);
        public IEnumerable<BO.x40MailQueue> GetList(BO.myQueryX40 mq);
        public int SaveX40(Rebex.Mail.MailMessage m, BO.x40MailQueue rec);
        public void StopPendingMessagesInBatch(string batchguid);
        public void RestartMessagesInBatch(string batchguid);
        public List<string> GetAllx43Emails();
        public List<string> GetLast10Receivers();
        public Rebex.Mail.MailMessage LoadMailMessageByPid(int x40id);  //načte poštovní zprávu z eml souboru
        public string GetDefaultGridFields(string prefix);  //vrací výchozí nastavení polí pro mail zprávu
        public List<string> GetMailList(int j02id, int j11id, int x67id, int p28id, int p24id, int record_pid, string record_prefix, int j04id, bool b11IsRecordOwner = false, bool b11IsRecordCreator = false);   //vrací seznam e-mail adres uživatelů
        public List<string> GetGsmList(int j02id, int j11id, int x67id, int p28id, int p24id, int record_pid, string record_prefix, int j04id, bool b11IsRecordOwner = false, bool b11IsRecordCreator = false);
        public void Set_SubjectInBody(string s);

    }
    class MailBL : BaseBL, IMailBL
    {
        private BO.j40MailAccount _account;
        public MailBL(BL.Factory mother) : base(mother)
        {
            Rebex.Licensing.Key = "==FmUGVeH5TmvcatEzt0Z4rBvjoahsK0e0albLXKX2bgBB+JxAEBiGKcZlB73B+U5q3G5YP==";

        }
        private List<Rebex.Mail.Attachment> _attachments;
        private string _subjectinbody;
        public void Set_SubjectInBody(string s)
        {
            _subjectinbody = s;
        }
        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,case when a.x40DatetimeProcessed is not null then a.x40DatetimeProcessed else a.x40DateInsert end as MessageTime,");
            sb(_db.GetSQL1_Ocas("x40", false, false));
            sb(" FROM x40MailQueue a");
            sb(strAppend);
            return sbret();
        }
        public BO.x40MailQueue LoadMessageByPid(int x40id)
        {
            sb(GetSQL1(" WHERE a.x40ID=@pid"));
            return _db.Load<BO.x40MailQueue>(sbret(), new { pid = x40id });
        }
        public BO.x40MailQueue LoadMessageByGuid(string guid)
        {
            sb(GetSQL1(" WHERE a.x40MessageGuid=@guid"));
            return _db.Load<BO.x40MailQueue>(sbret(), new { guid = guid });
        }
        public Rebex.Mail.MailMessage LoadMailMessageByPid(int x40id)
        {
            var rec = LoadMessageByPid(x40id);
            if (rec == null || rec.x40MessageGuid == null) return null;

            string strEmlFullPath = _mother.UploadFolder + "\\" + rec.x40EmlFolder + "\\" + rec.x40MessageGuid + ".eml";
            if (!System.IO.File.Exists(strEmlFullPath)) return null;

            var m = new Rebex.Mail.MailMessage();
            m.Load(strEmlFullPath);

            return m;
        }

        public void ClearAttachments()
        {
            _attachments = null;
        }
        public void AddAttachment(string fullpath, string displayname, string contenttype = null)
        {
            if (_attachments == null) _attachments = new List<Rebex.Mail.Attachment>();

            if (string.IsNullOrEmpty(displayname))
            {
                displayname = BO.Code.File.GetFileInfo(fullpath).Name;
            }

            if (string.IsNullOrEmpty(contenttype))
            {
                _attachments.Add(new Rebex.Mail.Attachment(fullpath, displayname));
            }
            else
            {
                _attachments.Add(new Rebex.Mail.Attachment(fullpath, displayname, contenttype));
            }

        }
        public void AddAttachment(Rebex.Mail.Attachment att)
        {
            if (_attachments == null) _attachments = new List<Rebex.Mail.Attachment>();
            _attachments.Add(att);
        }
        private BO.x40MailQueue InhaleMessageSender(int j40id, BO.x40MailQueue rec)
        {
            if (j40id > 0)
            {
                _account = _mother.j40MailAccountBL.Load(j40id);
            }
            else
            {
                _account = _mother.j40MailAccountBL.LoadDefaultSmtpAccount();        //výchozí (globální) SMTP účet         
            }
            if (_account == null)
            {
                return new BO.x40MailQueue() { j40ID = 0 };
            }
            rec.j40ID = _account.pid;
            rec.x40SenderAddress = _account.j40SmtpEmail;
            rec.x40SenderName = _account.j40Name;

            if (_account.j40SmtpUsePersonalReply)
            {
                rec.x40SenderName = _mother.CurrentUser.FullNameAsc;
            }
            else
            {
                rec.x40SenderName = _account.j40SmtpName;
            }

            return rec;
        }
        public BO.Result SendMessage(int j40id, string toEmail, string toName, string subject, string body, bool ishtml, string record_entity, int record_pid)  //v BO.Result.pid vrací x40id
        {

            BO.x40MailQueue rec = new BO.x40MailQueue() { x40Recipient = toEmail, x40Subject = subject, x40Body = body, x40IsHtmlBody = ishtml, x40MessageGuid = BO.Code.Bas.GetGuid(), x40RecordPid = record_pid, x40RecordEntity = record_entity };

            rec = InhaleMessageSender(j40id, rec);
            return SendMessage(rec, false, null);

        }
        public BO.Result SendMessage(BO.x40MailQueue rec, bool istest, BL.TheColumnsProvider cp)  //v BO.Result.pid vrací x40id
        {
            rec = InhaleMessageSender(rec.j40ID, rec);
            Rebex.Mail.MailMessage m = new Rebex.Mail.MailMessage();
            if (!string.IsNullOrEmpty(rec.x40Subject)) m.Subject = rec.x40Subject;
            if (!string.IsNullOrEmpty(rec.x40Body))
            {
                if (rec.x40IsHtmlBody)
                {
                    m.BodyHtml = rec.x40Body;
                }
                else
                {
                    m.BodyText = rec.x40Body;
                }
            }
            m.From = new Rebex.Mime.Headers.MailAddress(rec.x40SenderAddress, rec.x40SenderName);

            var lis = new List<string>();
            if (!String.IsNullOrEmpty(rec.x40Recipient))
            {
                lis = BO.Code.Bas.ConvertString2List(rec.x40Recipient.Replace(";", ","), ",");
                foreach (string s in lis.Where(p => string.IsNullOrEmpty(p.Trim()) == false))
                {
                    m.To.Add(new Rebex.Mime.Headers.MailAddress(s.Trim()));
                }
            }
            if (!String.IsNullOrEmpty(rec.x40CC))
            {
                lis = BO.Code.Bas.ConvertString2List(rec.x40CC.Replace(";", ","), ",");
                foreach (string s in lis)
                {
                    m.CC.Add(new Rebex.Mime.Headers.MailAddress(s));
                }
            }
            if (!String.IsNullOrEmpty(rec.x40BCC))
            {
                lis = BO.Code.Bas.ConvertString2List(rec.x40BCC.Replace(";", ","), ",");
                foreach (string s in lis)
                {
                    m.Bcc.Add(new Rebex.Mime.Headers.MailAddress(s));
                }
            }
            if (!String.IsNullOrEmpty(rec.x40TO_j61))
            {
                lis = BO.Code.Bas.ConvertString2List(rec.x40TO_j61.Replace(";", ","), ",");
                foreach (string s in lis.Where(p => !string.IsNullOrEmpty(p.Trim())))
                {
                    m.To.Add(new Rebex.Mime.Headers.MailAddress(s.Trim()));
                }
            }
            if (!String.IsNullOrEmpty(rec.x40CC_j61))
            {
                lis = BO.Code.Bas.ConvertString2List(rec.x40CC_j61.Replace(";", ","), ",");
                foreach (string s in lis)
                {
                    m.CC.Add(new Rebex.Mime.Headers.MailAddress(s));
                }
            }
            if (!String.IsNullOrEmpty(rec.x40BCC_j61))
            {
                lis = BO.Code.Bas.ConvertString2List(rec.x40BCC_j61.Replace(";", ","), ",");
                foreach (string s in lis)
                {
                    m.Bcc.Add(new Rebex.Mime.Headers.MailAddress(s));
                }
            }


            return handle_smtp_finish(m, rec, istest, cp);
        }
        public BO.Result SendMessage(int j40id, Rebex.Mail.MailMessage message)
        {
            var rec = new BO.x40MailQueue();
            if (_account == null)
            {
                rec = InhaleMessageSender(j40id, rec);
                message.From = new Rebex.Mime.Headers.MailAddress(rec.x40SenderAddress, rec.x40SenderName);

            }
            return handle_smtp_finish(message, rec, false, null);
        }
        public BO.Result SendMessageWithoutFactory(string strHtmlBody, string strSubject, string strTo, string strBcc, string strFromAddress, string strFromName)    //odešle zprávu bez vazby na databázi a bez rebexu
        {
            var m = new System.Net.Mail.MailMessage() { Body = strHtmlBody, Subject = strSubject, IsBodyHtml = true, BodyEncoding = Encoding.UTF8, SubjectEncoding = Encoding.UTF8 };

            m.From = new System.Net.Mail.MailAddress(strFromAddress, strFromName);

            m.To.Add(strTo);
            if (strBcc != null)
            {
                m.Bcc.Add(strBcc);
            }


            using (SmtpClient client = new SmtpClient("smtp.mycorecloud.net", 25))
            {
                client.Credentials = new System.Net.NetworkCredential("smtp-marktime", "Oomaidee3Ais");

                //m.Headers.Add("Message-ID", System.Guid.NewGuid().ToString("N"));                
                client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;    //odeslanou zprávu uložit na serveru jako EML soubor
                client.PickupDirectoryLocation = _mother.App.LogFolder;
                client.Send(m);//nejdříve uložit eml soubor do temp složky

                client.DeliveryMethod = SmtpDeliveryMethod.Network; //nyní opravdu odeslat
                client.EnableSsl = false;

                try
                {
                    client.Send(m);
                    return new BO.Result(false);

                }
                catch (Exception e)
                {
                    return new BO.Result(true, e.Message);
                }

            }
        }

        private BO.Result handle_smtp_finish(Rebex.Mail.MailMessage m, BO.x40MailQueue rec, bool istest, BL.TheColumnsProvider cp)     //finální odeslání zprávy
        {
            if (_account == null)
            {
                return handle_result_error("Chybí poštovní účet odesílatele");
            }
            if (m.From == null)
            {
                return handle_result_error("Chybí odesílatel zprávy");
            }
            if (m.To.Count == 0)
            {
                return handle_result_error("Chybí příjemce zprávy");
            }
            if (string.IsNullOrEmpty(m.BodyHtml) && string.IsNullOrEmpty(m.BodyText) && string.IsNullOrEmpty(m.Subject))
            {
                return handle_result_error("Chybí předmět nebo text zprávy.");
            }

            if (rec.x40IsRecordLink && !string.IsNullOrEmpty(m.Subject) && !m.Subject.Contains("MARKTIME"))
            {
                m.Subject = "MARKTIME: " + m.Subject;
            }

            if (_account.j40SmtpUsePersonalReply)
            {
                var recJ02 = _mother.j02UserBL.Load(_mother.CurrentUser.j02ID);
                m.ReplyTo.Add(new Rebex.Mime.Headers.MailAddress(recJ02.j02Email, recJ02.FullNameAsc));
            }

            m.DefaultCharset = System.Text.Encoding.UTF8;
            if (rec.x40RecordEntity != null)
            {
                m.Headers.Add("marktime-prefix", rec.x40RecordEntity);
            }
            if (rec.x40RecordPid > 0)
            {
                m.Headers.Add("marktime-pid", rec.x40RecordPid.ToString());
            }


            BO.Result ret = new BO.Result(false);

            m.BodyHtml = GetFinalHtmlBody(rec, cp);     //zpráva bude v html
            m.BodyText = null;

            //Uložit zprávu jako EML soubor            
            if (string.IsNullOrEmpty(rec.x40EmlFolder))
            {
                rec.x40EmlFolder = $"eml\\{DateTime.Now.Year}\\{DateTime.Now.Month}";
            }
            string strEmlFullPath = $"{_mother.UploadFolder}\\{rec.x40EmlFolder}\\{m.MessageId.Id}.eml";
            if (!Directory.Exists($"{_mother.UploadFolder}\\{rec.x40EmlFolder}")) Directory.CreateDirectory(_mother.UploadFolder + "\\" + rec.x40EmlFolder);

            var client = new Rebex.Net.Smtp();

            //Logujeme odesílání pošty, do rec.x40EmlFolder ukládat i logwriter         
            string strLogFile = $"{_mother.UploadFolder}\\{rec.x40EmlFolder}\\{m.MessageId.Id}.log";
            client.LogWriter = new Rebex.FileLogWriter(strLogFile, Rebex.LogLevel.Debug);


            try
            {
                switch (_account.j40SslModeFlag)
                {
                    case SslModelFlagEnum.Implicit:
                        client.Connect(_account.j40SmtpHost, _account.j40SmtpPort, Rebex.Net.SslMode.Implicit);
                        break;
                    case SslModelFlagEnum.Explicit:
                        client.Connect(_account.j40SmtpHost, _account.j40SmtpPort, Rebex.Net.SslMode.Explicit);

                        break;
                    default:
                        client.Connect(_account.j40SmtpHost, _account.j40SmtpPort);


                        break;
                }
            }
            catch (Exception e)
            {
                return handle_result_error(e.Message);
            }


            if (_account.j40SmtpEnableSsl)
            {
                try
                {
                    client.Secure();
                }
                catch (Exception e)
                {
                    
                    return handle_result_error(e.Message);
                }

            }

            if (!client.IsAuthenticated && !_account.j40SmtpUseDefaultCredentials)
            {

                client.Login(_account.j40SmtpLogin, new BO.Code.Cls.Crypto().Decrypt(_account.j40SmtpPassword));

            }

            if (_attachments != null)
            {
                foreach (var att in _attachments)
                {
                    m.Attachments.Add(att);
                }
            }



            m.Save(strEmlFullPath, Rebex.Mail.MailFormat.Mime);
            rec.x40EmlFileSize = (int)(new System.IO.FileInfo(strEmlFullPath).Length);
            rec.x40MessageGuid = m.MessageId.Id;

            try
            {
                if (!istest)
                {
                    client.Send(m);
                }
                if (istest)
                {
                    rec.x40ErrorMessage = "Testovací režim";
                }
                else
                {
                    rec.x40ErrorMessage = "";
                }
                rec.x40Status = BO.x40StateENUM.IsProceeded;
                rec.x40DatetimeProcessed = DateTime.Now;
                ret.pid = SaveX40(m, rec);  //uložení zprávy do db
                ret.Flag = ResultEnum.Success;

            }
            catch (Exception ex)
            {

                this.AddMessageTranslated(ex.Message);
                rec.x40ErrorMessage = ex.Message;
                rec.x40Status = BO.x40StateENUM.IsError;
                ret.pid = SaveX40(m, rec);  //uložení zprávy do db
                ret.Flag = ResultEnum.Failed;
                ret.Message = rec.x40ErrorMessage;
            }

            this.ClearAttachments();    //pro jistotu vyčistit kolekci příloh zprávy

            if (ret.pid == 0)
            {
                //nedošlo k uložení záznamu zprávy
                BO.Code.File.LogError($"Nedošlo k uložení poštovní zprávy do tabulky [x40MailQueue]. x40MessageGuid: {rec.x40MessageGuid},x40RecordPid: {rec.x40RecordPid},x40RecordEntity: {rec.x40RecordEntity}, subject: {rec.x40Subject}");
            }

            return ret;

            //m.Save(_mother.TempFolder + "\\eml\\" + mail.MessageId.Id + ".msg", Rebex.Mail.MailFormat.OutlookMsg);



        }

        public void StopPendingMessagesInBatch(string batchguid)
        {
            _db.RunSql("UPDATE x40MailQueue set x40Status=4 WHERE x40BatchGuid=@guid AND x40Status=1", new { guid = batchguid });

        }
        public void RestartMessagesInBatch(string batchguid)
        {
            _db.RunSql("UPDATE x40MailQueue set x40Status=1 WHERE x40BatchGuid=@guid AND x40Status=4", new { guid = batchguid });

        }

        public int SaveX40(Rebex.Mail.MailMessage m, BO.x40MailQueue rec)
        {
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            if (string.IsNullOrEmpty(rec.x40MessageGuid) == true)
            {
                rec.x40MessageGuid = BO.Code.Bas.GetGuid();
            }
            p.AddString("x40MessageGuid", rec.x40MessageGuid);
            p.AddInt("x01ID", _mother.CurrentUser.x01ID, true);
            p.AddString("x40BatchGuid", rec.x40BatchGuid);
            p.AddInt("j40ID", rec.j40ID, true);
            p.AddString("x40RecordEntity", rec.x40RecordEntity);

            if (rec.pid == 0)
            {
                if (rec.j02ID_Creator == 0) rec.j02ID_Creator = _mother.CurrentUser.pid;
                p.AddInt("j02ID_Creator", rec.j02ID_Creator, true);
            }
            p.AddInt("x40RecordPid", rec.x40RecordPid, true);

            p.AddString("x40Subject", rec.x40Subject);
            p.AddString("x40Body", rec.x40Body);
            p.AddInt("x04ID", rec.x04ID, true);
            p.AddBool("x40IsRecordLink", rec.x40IsRecordLink);
            p.AddBool("x40IsUserSignature", rec.x40IsUserSignature);
            p.AddInt("o24ID", rec.o24ID, true);
            p.AddString("x40SkeletonFile", rec.x40SkeletonFile);

            p.AddInt("j61ID", rec.j61ID, true);
            p.AddString("x40CC_j61", rec.x40CC_j61);
            p.AddString("x40BCC_j61", rec.x40BCC_j61);
            p.AddString("x40TO_j61", rec.x40TO_j61);

            if (rec.x40RecordPid > 0 && rec.x40RecordEntity != null)
            {
                p.AddBool("x40IsLastInstance", true);   //poslední instance zprávy pro záznam entity
            }

            if (m != null)
            {
                p.AddString("x40SenderAddress", m.From.First().Address);
                p.AddString("x40SenderName", m.From.First().DisplayName);
                p.AddString("x40Recipient", String.Join(", ", m.To.Select(p => p.Address)));
                p.AddString("x40BCC", String.Join(",", m.Bcc.Select(p => p.Address)));
                p.AddString("x40CC", String.Join(",", m.CC.Select(p => p.Address)));
                //p.AddString("x40Subject", m.Subject);                
                if (m.HasBodyHtml)
                {
                    //p.AddString("x40Body", m.BodyHtml);
                    p.AddBool("x40IsHtmlBody", true);
                }
                else
                {
                    //p.AddString("x40Body", m.BodyText);
                    p.AddBool("x40IsHtmlBody", false);
                }


                p.AddString("x40Attachments", String.Join(",", m.Attachments.Select(p => p.DisplayName)));
            }
            else
            {
                p.AddString("x40SenderAddress", rec.x40SenderAddress);
                p.AddString("x40SenderName", rec.x40SenderName);
                p.AddString("x40Recipient", rec.x40Recipient);
                p.AddString("x40BCC", rec.x40BCC);
                p.AddString("x40CC", rec.x40CC);
                p.AddBool("x40IsHtmlBody", rec.x40IsHtmlBody);
                p.AddString("x40Attachments", rec.x40Attachments);
            }


            p.AddDateTime("x40DatetimeProcessed", rec.x40DatetimeProcessed);
            p.AddString("x40ErrorMessage", rec.x40ErrorMessage);
            p.AddEnumInt("x40Status", rec.x40Status);
            p.AddBool("x40IsAutoNotification", rec.x40IsAutoNotification);

            p.AddString("x40EmlFolder", rec.x40EmlFolder);
            p.AddInt("x40EmlFileSize", rec.x40EmlFileSize);
            p.AddString("x40GridColumns", rec.x40GridColumns);

            int intPID = _db.SaveRecord("x40MailQueue", p, rec, false, true);
            if (intPID > 0)
            {
                if (rec.x40RecordPid > 0 && rec.x40RecordEntity != null)
                {
                    _db.RunSql("if exists(select x40ID FROM x40MailQueue WHERE x40RecordPid=@recpid AND x40RecordEntity=@prefix AND x40ID<>@pid) update x40MailQueue SET x40IsLastInstance=0 WHERE x40RecordPid=@recpid AND x40RecordEntity=@prefix AND x40ID<>@pid", new { recpid = rec.x40RecordPid, prefix = rec.x40RecordEntity, pid = intPID });
                }
            }

                return intPID;
        }

        private BO.Result handle_result_error(string strError)
        {
            this.AddMessageTranslated(strError);
            BO.Code.File.LogError($"SMTP message, strError: {strError}, user: {_mother.CurrentUser.j02Login}");
            return new BO.Result(true, strError);
        }

        private string FindEmlFileByGuid(string strGUID)
        {

            DirectoryInfo dir = new DirectoryInfo(_mother.TempFolder);

            foreach (FileInfo file in dir.GetFiles("*.eml").OrderByDescending(p => p.CreationTime))
            {
                StreamReader reader = file.OpenText();
                string s = "";
                while ((s = reader.ReadLine()) != null)
                {
                    if (s.Contains("Message-ID"))
                    {
                        if (s.Contains(strGUID))
                        {
                            reader.Close();
                            return file.FullName;
                        }
                        reader.Close();
                        break;
                    }
                }

            }

            return "";
        }

        public bool SaveMailJob2Temp(string strJobGuid, BO.x40MailQueue recX40, string strUploadGuid, List<BO.x43MailQueue_Recipient> lisX43)
        {
            var recTemp = new BO.p85Tempbox() { p85Prefix = "x40", p85GUID = strJobGuid, p85FreeText01 = recX40.x40Subject, p85Message = recX40.x40Body, p85FreeText04 = strUploadGuid };
            if (_mother.p85TempboxBL.Save(recTemp) == 0)
            {
                return false;
            }
            foreach (var c in lisX43)
            {
                recTemp = new BO.p85Tempbox() { p85Prefix = "x43", p85GUID = strJobGuid, p85FreeText01 = c.x43Email, p85FreeText02 = c.x43DisplayName, p85OtherKey3 = (int)c.x43RecipientFlag };
                _mother.p85TempboxBL.Save(recTemp);

            }

            return true;
        }

        public IEnumerable<BO.x40MailQueue> GetList(BO.myQueryX40 mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql("SELECT a.*," + _db.GetSQL1_Ocas("x40", false, false, true) + " FROM x40MailQueue a", mq, _mother.CurrentUser);
            return _db.GetList<BO.x40MailQueue>(fq.FinalSql, fq.Parameters);
        }


        public List<string> GetAllx43Emails()
        {

            return _db.GetList<BO.GetString>("select distinct x43Email as Value FROM x43MailQueue_Recipient").Select(p => p.Value).ToList();


        }

        public List<string> GetLast10Receivers()
        {

            return _db.GetList<BO.GetString>("select TOP 50 x40Recipient as Value FROM x40MailQueue WHERE j02ID_Creator=@j02id ORDER BY x40ID DESC", new { j02id = _mother.CurrentUser.pid }).Select(p => p.Value).ToList();


        }

        private string GetFinalHtmlBody(BO.x40MailQueue rec, BL.TheColumnsProvider cp)
        {
            string strSkeleton = rec.x40SkeletonFile;
            if (string.IsNullOrEmpty(strSkeleton)) strSkeleton = "send_message_template.html";

            string strSkeletonPath = $"{_mother.UploadFolder}\\mail\\{strSkeleton}";    //nejdřív preference html kostry zprávy ze složky zákazníka
            if (!System.IO.File.Exists(strSkeletonPath))
            {
                strSkeletonPath = $"{_mother.App.RootUploadFolder}\\_distribution\\mail\\{strSkeleton}";    //jinak vzít kostru z distribution složku
            }
            string strHtml = BO.Code.File.GetFileContent(strSkeletonPath);

            var sb = new System.Text.StringBuilder();

            if (!string.IsNullOrEmpty(rec.x40Body))
            {
                if (!rec.x40IsHtmlBody)
                {
                    sb.AppendLine("<div>");
                    sb.Append(BO.Code.Bas.Text2Html(rec.x40Body));
                    sb.AppendLine("</div>");
                }
                else
                {
                    sb.Append(rec.x40Body);
                }

            }

            if (rec.x40IsUserSignature)
            {
                string strSignature = _mother.j02UserBL.Load(_mother.CurrentUser.pid).j02EmailSignature;
                strHtml = strHtml.Replace("#signature#", $"<p></p>{BO.Code.Bas.Text2Html(strSignature)}");
            }
            else
            {
                strHtml = strHtml.Replace("#signature#", "");
            }

            if (rec.x40IsRecordLink)
            {
                string s = $"{_mother.Lic.x01AppHost}/Record/RecPage?prefix={rec.x40RecordEntity}&pid={rec.x40RecordPid}";
                if (rec.x40RecordEntity == "p31")
                {
                    s = $"{_mother.Lic.x01AppHost}/TheGrid/FlatView?prefix={rec.x40RecordEntity}&myqueryinline=pids|list_int|{rec.x40RecordPid}";

                }
                if (rec.x40RecordEntity == "p94")
                {
                    var recP94 = _mother.p91InvoiceBL.GetList_p94(0, rec.x40RecordPid).First();
                    s = $"{_mother.Lic.x01AppHost}/Record/RecPage?prefix=p91&pid={recP94.p91ID}";

                }
                strHtml = strHtml.Replace("#link#", $"<a href='{s}' target='_blank'>Přejít do MARKTIME</a>");
            }
            else
            {
                strHtml = strHtml.Replace("#link#", "");
            }


            if (rec.x40GridColumns == null)
            {
                strHtml = strHtml.Replace("#body#", sb.ToString());
                strHtml = strHtml.Replace("#subject#", (_subjectinbody == null ? rec.x40Subject : _subjectinbody));
                return strHtml;
            }


            sb.AppendLine("<table class='mytab'>");

            var mq = new BO.InitMyQuery(_mother.CurrentUser).Load(rec.x40RecordEntity);
            mq.TopRecordsOnly = 1;
            mq.pids = new List<int>() { rec.x40RecordPid };

            mq.explicit_columns = cp.ParseTheGridColumns(rec.x40RecordEntity, rec.x40GridColumns, _mother);
            var dt = _mother.gridBL.GetGridTable(mq);
            foreach (var c in mq.explicit_columns)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td style='max-width:120px;'>{c.Header}:</td>");

                sb.AppendLine("<td>");
                if (dt.Rows[0][c.UniqueName] != System.DBNull.Value)
                {
                    sb.AppendLine(BO.Code.Bas.ParseCellValueFromDb(dt.Rows[0], c));

                }
                sb.AppendLine("</td>");

                sb.AppendLine("</tr>");
            }


            sb.AppendLine("</table>");

            strHtml = strHtml.Replace("#body#", sb.ToString());
            strHtml = strHtml.Replace("#subject#", (_subjectinbody == null ? rec.x40Subject : _subjectinbody));



            return strHtml;
        }


        public string GetDefaultGridFields(string prefix)
        {
            switch (prefix)
            {
                case "p56":
                    return "a__p56Task__p56Name,a__p56Task__AktualniStav,a__p56Task__p56PlanUntil,a__p56Task__p56PlanFrom,a__p56Task__p56Plan_Hours,p56_p41__p41Project__p41Name,p56_p41__p41Project__KlientProjektu";
                case "p91":
                    return "a__p91Invoice__p91Code,a__p91Invoice__p91Client,a__p91Invoice__p91Date,a__p91Invoice__p91DateSupply,a__p91Invoice__p91DateMaturity,a__p91Invoice__j27Code,a__p91Invoice__p91Amount_WithoutVat,a__p91Invoice__p91Amount_TotalDue";
                case "p28":
                    return "a__p28Contact__p28Name,p28_p29__p29ContactType__p29Name,a__p28Contact__p28RegID,a__p28Contact__p28VatID,a__p28Contact__Adresa1";
                case "o23":
                    return "a__o23Doc__o23Name,a__o23Doc__DocType,a__o23Doc__o23FreeDate01,a__o23Doc__WorkflowStav,a__o23Doc__DateInsert_o23Doc,a__o23Doc__UserInsert_o23Doc";
                case "p31":
                    return "a__p31Worksheet__p31Date,p31_p41__p41Project__NazevProjektu,p31_p41_client__p28Contact__p28Name,p31_p32__p32Activity__p32Name,p32_p34__p34ActivityGroup__p34Name,a__p31Worksheet__p31Text,a__p31Worksheet__p31Value_Orig,a__p31Worksheet__j27Code_Billing_Orig";
                case "p41":
                    return "a__p41Project__NazevProjektu,a__p41Project__TypProjektu,a__p41Project__p41Code,a__p41Project__KlientProjektu";
                case "o22":
                    return "a__o22Milestone__o22Name,a__o22Milestone__Typ,a__o22Milestone__o22PlanFrom,a__o22Milestone__o22PlanUntil,o22_p41__p41Project__KlientPlusProjekt";
                default:
                    return null;
            }
        }

        public List<string> GetGsmList(int j02id, int j11id, int x67id, int p28id, int p24id, int record_pid, string record_prefix, int j04id, bool b11IsRecordOwner = false, bool b11IsRecordCreator = false)
        {
            return GetReceivers(j02id, j11id, x67id, p28id, p24id, record_pid, record_prefix, true, j04id, b11IsRecordOwner, b11IsRecordCreator);
        }
        public List<string> GetMailList(int j02id, int j11id, int x67id, int p28id, int p24id, int record_pid, string record_prefix, int j04id, bool b11IsRecordOwner = false, bool b11IsRecordCreator = false)
        {
            return GetReceivers(j02id, j11id, x67id, p28id, p24id, record_pid, record_prefix, false, j04id, b11IsRecordOwner, b11IsRecordCreator);
        }
        private List<string> GetReceivers(int j02id, int j11id, int x67id, int p28id, int p24id, int record_pid, string record_prefix, bool bolRetGsmNumbers, int j04id, bool b11IsRecordOwner = false, bool b11IsRecordCreator = false)
        {
            var lis = new List<string>();

            if (j02id > 0)
            {
                if (bolRetGsmNumbers)
                {
                    lis.Add(_mother.j02UserBL.Load(j02id).j02Mobile);
                }
                else
                {
                    lis.Add(_mother.j02UserBL.Load(j02id).j02Email);
                }

            }
            if (j04id > 0)
            {
                //příjemci podle aplikační role
                var lisJ02 = _mother.j02UserBL.GetList(new BO.myQueryJ02() { j04id = j04id });
                foreach (var c in lisJ02)
                {
                    if (bolRetGsmNumbers)
                    {
                        lis.Add(_mother.j02UserBL.Load(c.pid).j02Mobile);
                    }
                    else
                    {
                        lis.Add(_mother.j02UserBL.Load(c.pid).j02Email);
                    }
                }
            }
            if (j11id > 0)  //příjemci z týmu
            {
                bool b = false;
                if (!bolRetGsmNumbers)
                {
                    var recJ11 = _mother.j11TeamBL.Load(j11id);
                    if (recJ11.j11Email != null)
                    {
                        lis.Add(recJ11.j11Email);   //skupinový e-mail
                        b = true;
                    }
                }
                if (!b)
                {
                    var lisJ02 = _mother.j02UserBL.GetList(new BO.myQueryJ02() { j11id = j11id });
                    foreach (var c in lisJ02)
                    {
                        if (bolRetGsmNumbers)
                        {
                            lis.Add(c.j02Mobile);
                        }
                        else
                        {
                            lis.Add(c.j02Email);
                        }

                    }
                }
            }
            if (p28id > 0)
            {
                var lisO32 = _mother.p28ContactBL.GetList_o32(p28id, 0, 0);
                if (bolRetGsmNumbers)
                {
                    lisO32 = lisO32.Where(p => p.o33ID == BO.o33FlagEnum.Tel);
                }
                else
                {
                    lisO32 = lisO32.Where(p => p.o33ID == BO.o33FlagEnum.Email);
                }
                foreach (var c in lisO32)
                {
                    lis.Add(c.o32Value);
                }
            }
            if (p24id > 0)
            {
                bool b = false;
                if (!bolRetGsmNumbers)
                {
                    var recP24 = _mother.p24ContactGroupBL.Load(p24id);
                    if (recP24.p24Email != null)
                    {
                        lis.Add(recP24.p24Email);   //skupinový e-mail
                        b = true;
                    }
                }
                if (!b)
                {
                    var lisO32 = _mother.p28ContactBL.GetList_o32(0, p24id, 0);
                    if (bolRetGsmNumbers)
                    {
                        lisO32 = lisO32.Where(p => p.o33ID == BO.o33FlagEnum.Tel);
                    }
                    else
                    {
                        lisO32 = lisO32.Where(p => p.o33ID == BO.o33FlagEnum.Email);
                    }
                    foreach (var c in lisO32)
                    {
                        lis.Add(c.o32Value);
                    }
                }

            }
            if (x67id > 0 && record_pid > 0)
            {
                var lisX69 = _mother.x67EntityRoleBL.GetList_X69(record_prefix, record_pid).Where(p => p.x67ID == x67id);

                var recX67 = _mother.x67EntityRoleBL.Load(x67id);
                if (recX67.x67Entity != record_prefix)
                {
                    if (record_prefix == "p91" && recX67.x67Entity == "p41")
                    {
                        var recP91 = _mother.p91InvoiceBL.Load(record_pid);
                        lisX69 = _mother.x67EntityRoleBL.GetList_X69("p41", recP91.p41ID_First).Where(p => p.x67ID == x67id);
                    }
                    if (record_prefix == "p56" && recX67.x67Entity == "p41")
                    {
                        var recP56 = _mother.p56TaskBL.Load(record_pid);
                        lisX69 = _mother.x67EntityRoleBL.GetList_X69("p41", recP56.p41ID).Where(p => p.x67ID == x67id);
                    }
                }


                foreach (var c in lisX69)
                {
                    if (c.j02ID > 0)
                    {
                        if (bolRetGsmNumbers)
                        {
                            lis.Add(_mother.j02UserBL.Load(c.j02ID).j02Mobile);
                        }
                        else
                        {
                            lis.Add(_mother.j02UserBL.Load(c.j02ID).j02Email);
                        }

                    }
                    if (c.j11ID > 0)
                    {
                        bool b = false;
                        if (!bolRetGsmNumbers)
                        {
                            var recJ11 = _mother.j11TeamBL.Load(c.j11ID);
                            if (recJ11.j11Email != null)
                            {
                                lis.Add(recJ11.j11Email);   //skupinový e-mail
                                b = true;
                            }
                        }
                        if (!b)
                        {
                            var lisJ02 = _mother.j02UserBL.GetList(new BO.myQueryJ02() { j11id = c.j11ID });
                            foreach (var recJ02 in lisJ02)
                            {
                                if (bolRetGsmNumbers)
                                {
                                    lis.Add(recJ02.j02Mobile);
                                }
                                else
                                {
                                    lis.Add(recJ02.j02Email);
                                }

                            }
                        }

                    }
                }
            }

            if (record_pid > 0 && record_prefix != null && b11IsRecordOwner)
            {
                var intJ02ID = _mother.CBL.GetRecord_j02ID_Owner(record_prefix, record_pid);
                var recJ02 = _mother.j02UserBL.Load(intJ02ID);
                if (recJ02 != null)
                {
                    if (bolRetGsmNumbers)
                    {
                        lis.Add(recJ02.j02Mobile);
                    }
                    else
                    {
                        lis.Add(recJ02.j02Email);
                    }
                }
            }
            if (record_pid > 0 && record_prefix != null && b11IsRecordCreator)
            {
                var intJ02ID = _mother.CBL.GetRecord_j02ID_Creator(record_prefix, record_pid);
                var recJ02 = _mother.j02UserBL.Load(intJ02ID);
                if (recJ02 != null)
                {
                    if (bolRetGsmNumbers)
                    {
                        lis.Add(recJ02.j02Mobile);
                    }
                    else
                    {
                        lis.Add(recJ02.j02Email);
                    }
                }
            }

            return lis;

        }


    }
}
