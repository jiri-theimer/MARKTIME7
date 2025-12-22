
using BL;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using Rebex.Mime.Headers;


namespace UI.Controllers
{
    public class MailController : BaseController
    {
        private readonly BL.Singleton.ThePeriodProvider _pp;
        private readonly BL.TheColumnsProvider _colsProvider;
        public MailController(BL.Singleton.ThePeriodProvider pp, BL.TheColumnsProvider cp)
        {
            _pp = pp;
            _colsProvider = cp;
        }

        public IActionResult SendMail(int x40id, string uploadguid, int j02id, string record_entity, int record_pid, int x31id, string trdxfile, string mailsubject, int j61id, int b05id, int j72id, int senderror, string kostra)
        {
            
            if (record_pid == 0 || string.IsNullOrEmpty(record_entity))
            {
                record_entity = "j02";  //bezkontextové sestavy jsou automaticky převedeny na prefix=j02
                record_pid = Factory.CurrentUser.j02ID;
            }
            var receivers = new List<BO.x43MailQueue_Recipient>();

            if (string.IsNullOrEmpty(uploadguid))
            {
                uploadguid = BO.Code.Bas.GetGuid();
            }

            var v = new Models.Mail.SendMailViewModel() { UploadGuid = uploadguid, b05ID = b05id };
            v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "x40", SelectedX04ID = Factory.Lic.x04ID_Default };
            v.Rec = new BO.x40MailQueue();
            if (!string.IsNullOrEmpty(kostra))
            {
                v.Rec.x40SkeletonFile = kostra;
            }

            if (v.b05ID > 0)
            {
                
               Handle_Najit_Interni_Emaily(v.b05ID, record_pid, record_entity, receivers);
                    
                var recB05 = Factory.WorkflowBL.GetList_b05(null, 0, 0, v.b05ID, 0).First();
                v.Notepad.HtmlContent = recB05.b05Notepad; v.Notepad.SelectedX04ID = recB05.x04ID;
            }
            if (record_entity == "p85" && record_pid > 0)
            {
                //na vstupu je html výraz k odeslání
                var rec = Factory.p85TempboxBL.Load(record_pid);
                v.Notepad.HtmlContent = rec.p85Message;
                record_pid = Factory.CurrentUser.j02ID; ; record_entity = "j02";
            }

            v.Rec.x40RecordEntity = record_entity;
            v.Rec.x40RecordPid = record_pid;

            if (v.Rec.x40RecordEntity != "j02")
            {
                v.IsShowGridColumns = Factory.CBL.LoadUserParamBool($"sendmail-isgridcolumns-{v.Rec.x40RecordEntity}", true);
                v.j61GridColumns = Factory.CBL.LoadUserParam($"sendmail-gridcolumns-{v.Rec.x40RecordEntity}", Factory.MailBL.GetDefaultGridFields(record_entity));
                v.Rec.x40IsRecordLink = Factory.CBL.LoadUserParamBool("sendmail-isrecordlink", true);
            }


            v.Rec.x40IsUserSignature = Factory.CBL.LoadUserParamBool("sendmail-isusersignature", true);

            if (j02id > 0)
            {
                v.Recipient = Factory.j02UserBL.Load(j02id).j02Email;
            }
            if (!string.IsNullOrEmpty(mailsubject))
            {
                v.Rec.x40Subject = mailsubject;
            }

            v.Rec.j40ID = Factory.CBL.LoadUserParamInt("sendmail-j40id");
            if (v.Rec.j40ID == 0 && Factory.j40MailAccountBL.LoadDefaultSmtpAccount() != null)
            {
                v.Rec.j40ID = Factory.j40MailAccountBL.LoadDefaultSmtpAccount().pid;
            }



            if (v.Rec.j40ID > 0)
            {
                var recJ40 = Factory.j40MailAccountBL.Load(v.Rec.j40ID);
                if (recJ40 != null)
                {
                    v.SelectedJ40Name = recJ40.j40Name != null ? recJ40.j40Name : recJ40.j40SmtpEmail;
                }
            }
            v.Rec.x40MessageGuid = BO.Code.Bas.GetGuid();

            if (x31id > 0)  //tisková sestava bez kontextu odeslana do pošty
            {
                var recX31 = Factory.x31ReportBL.Load(x31id);
                var cc = new TheReportSupport();
                string strPdfPath = cc.GeneratePdfReport(Factory, _pp, recX31, v.UploadGuid, 0, true, 0, null, null, false, j72id);

                if (string.IsNullOrEmpty(v.Rec.x40Subject))
                {
                    v.Rec.x40Subject = recX31.x31Name;
                }

            }
            if (!string.IsNullOrEmpty(trdxfile))
            {
                var cc = new TheReportSupport();
                cc.GeneratePdfGridReport(Factory, trdxfile, v.UploadGuid, false);
                if (string.IsNullOrEmpty(v.Rec.x40Subject))
                {
                    v.Rec.x40Subject = "MARKTIME REPORT";
                }


            }

            
            if (v.Rec.x40RecordEntity == "j02" && v.Rec.x40RecordPid > 0 && v.Rec.x40RecordPid != Factory.CurrentUser.pid)
            {
                var recJ02 = Factory.j02UserBL.Load(v.Rec.x40RecordPid);
                receivers.Add(new BO.x43MailQueue_Recipient() { x43Email = recJ02.j02Email, x43RecipientFlag = BO.x43RecipientIdEnum.recTO });
            }
            if (v.Rec.x40RecordEntity == "p31" && v.Rec.x40RecordPid > 0)
            {
                var recP31 = Factory.p31WorksheetBL.Load(v.Rec.x40RecordPid);
                var recJ02 = Factory.j02UserBL.Load(recP31.j02ID);
                receivers.Add(new BO.x43MailQueue_Recipient() { x43Email = recJ02.j02Email, x43RecipientFlag = BO.x43RecipientIdEnum.recTO });
            }
            
            if (v.Rec.x40RecordEntity == "p84" && v.Rec.x40RecordPid > 0)
            {
                var recP84 = Factory.p84UpominkaBL.Load(v.Rec.x40RecordPid);
                var recP83 = Factory.p83UpominkaTypeBL.Load(recP84.p83ID);
                
                if (v.b05ID == 0)
                {
                    if (j61id == 0)
                    {
                        j61id = Factory.p84UpominkaBL.NajdiVychoziJ61ID(recP83, recP84);
                    }
                    Handle_Najit_Emaily_Klienta(recP84.p28ID, v.Rec.x40RecordEntity, receivers);
                }
               

                    var recP92 = Factory.p92InvoiceTypeBL.Load(recP84.p92ID);
                if (recP92.x31ID_Invoice > 0)
                {
                    var recX31 = Factory.x31ReportBL.Load(recP92.x31ID_Invoice);
                    var cc = new TheReportSupport();
                    cc.GeneratePdfReport(Factory, null, recX31, v.UploadGuid, recP84.p91ID, true, 0, null, null, false);  //pdf vygenerováno do temp složky

                }
            }
            if (v.Rec.x40RecordEntity == "p90" && v.Rec.x40RecordPid > 0)
            {
                var recP90 = Factory.p90ProformaBL.Load(v.Rec.x40RecordPid);
                var recP89 = Factory.p89ProformaTypeBL.Load(recP90.p89ID);
               
                if (v.b05ID == 0)
                {
                    if (j61id == 0 && recP89.j61ID > 0)
                    {
                        j61id = recP89.j61ID;
                    }
                    Handle_Najit_Emaily_Klienta(recP90.p28ID, v.Rec.x40RecordEntity, receivers);
                }
                
            }
            if (v.Rec.x40RecordEntity == "p91" && v.Rec.x40RecordPid > 0)
            {
                //najít e-mail adresy klienta faktury
                var recP91 = Factory.p91InvoiceBL.Load(v.Rec.x40RecordPid);
                var recP92 = Factory.p92InvoiceTypeBL.Load(recP91.p92ID);
                var recP28 = Factory.p28ContactBL.Load(recP91.p28ID);
                
                if (v.b05ID == 0)
                {
                    if (j61id == 0)
                    {
                        j61id = Factory.p91InvoiceBL.NajdiVychoziJ61ID(recP92, recP91, recP28);
                    }
                    Handle_Najit_Emaily_Klienta(recP28.pid, v.Rec.x40RecordEntity, receivers);
                }
               

                if (Factory.o27AttachmentBL.GetTempFiles(v.UploadGuid).Count() == 0) //pokud na vstupu už není vygenerovaný nějaký report
                {
                    if (recP92.x31ID_Invoice > 0)
                    {
                        var recP93 = Factory.p93InvoiceHeaderBL.Load(recP92.p93ID);
                        var recX31 = Factory.x31ReportBL.Load(recP92.x31ID_Invoice);
                        var cc = new TheReportSupport();
                        if (recP93.p93PfxCertificate != null && Factory.CurrentUser.j04IsModule_p91)
                        {
                            cc.PfxPath = $"{Factory.WwwUsersFolder}\\PLUGINS\\{recP93.p93PfxCertificate}";  //kvalifikovaný certifikát
                            cc.PfxPassword = recP93.p93PfxPassword;
                        }
                        //bool bolIsDOC = Factory.CBL.LoadUserParamBool("ReportContext-IsIncludeISDOC", false);
                        cc.GeneratePdfReport(Factory, null, recX31, v.UploadGuid, recP91.pid, true, 0, null, null, false);  //pdf vygenerováno do temp složky

                    }
                    if (recP92.x31ID_Attachment > 0)
                    {
                        var lisP31 = Factory.p31WorksheetBL.GetList(new BO.myQueryP31() { p91id = recP91.pid }).Where(p => p.p70ID == BO.p70IdENUM.Vyfakturovano && p.p34IncomeStatementFlag == BO.p34IncomeStatementFlagENUM.Vydaj);
                        if (lisP31.Count() > 0) //příloha se generuje, pokud existují výdajové úkony se statusem [Fakturovat]
                        {
                            var recX31 = Factory.x31ReportBL.Load(recP92.x31ID_Attachment);
                            var cc = new TheReportSupport();
                            cc.GeneratePdfReport(Factory, null, recX31, v.UploadGuid, recP91.pid, true, 0);  //pdf přílohy vygenerováno do temp složky
                        }
                    }
                }


            }

            

            if (j61id > 0 && v.Rec.x40RecordPid > 0)
            {
                v.SelectedJ61ID = j61id;
                var recJ61 = MailMergeByTextTemplate(v.SelectedJ61ID, v.Rec.x40RecordEntity, v.Rec.x40RecordPid);
                if (recJ61 != null)
                {
                    v.SelectedJ61Name = recJ61.j61Name;
                    v.Notepad.HtmlContent = recJ61.j61MailBody;
                    v.Rec.x40Subject = recJ61.j61MailSubject;
                    v.Rec.x40TO_j61 = recJ61.j61MailTO;
                    v.Rec.x40CC_j61 = recJ61.j61MailCC;
                    v.Rec.x40BCC_j61 = recJ61.j61MailBCC;
                   
                    if (recJ61.j61GridColumnsFlag == BO.j61UserFlagEnum.Yes)
                    {
                        v.IsShowGridColumns = true;
                        v.j61GridColumns = recJ61.j61GridColumns;
                    }
                    if (recJ61.j61GridColumnsFlag == BO.j61UserFlagEnum.No) v.IsShowGridColumns = false;
                    if (recJ61.j61UserSignatureFlag == BO.j61UserFlagEnum.Yes) v.Rec.x40IsUserSignature = true;
                    if (recJ61.j61UserSignatureFlag == BO.j61UserFlagEnum.No) v.Rec.x40IsUserSignature = false;
                    if (recJ61.j61RecordLinkFlag == BO.j61UserFlagEnum.Yes) v.Rec.x40IsRecordLink = true;
                    if (recJ61.j61RecordLinkFlag == BO.j61UserFlagEnum.No) v.Rec.x40IsRecordLink = false;
                }


            }

            v.Recipient = string.Join(";", receivers.Where(p => p.x43RecipientFlag == BO.x43RecipientIdEnum.recTO).Select(p => p.x43Email));
            v.Rec.x40CC = string.Join(";", receivers.Where(p => p.x43RecipientFlag == BO.x43RecipientIdEnum.recCC).Select(p => p.x43Email));
            v.Rec.x40BCC = string.Join(";", receivers.Where(p => p.x43RecipientFlag == BO.x43RecipientIdEnum.recBCC).Select(p => p.x43Email));



            if (x40id > 0)
            {   //kopírování zprávy do nové podle vzoru x40id
                v.Rec = Factory.MailBL.LoadMessageByPid(x40id);
                v.Recipient = v.Rec.x40Recipient;
                v.Rec.x40CC = v.Rec.x40CC;
                v.Rec.x40BCC = v.Rec.x40BCC;
                v.Rec.x40BCC_j61 = null;
                v.Rec.x40CC_j61 = null;
                v.Rec.x40TO_j61 = null;
                v.Rec.x40Subject = v.Rec.x40Subject;
                v.Rec.x40IsRecordLink = v.Rec.x40IsRecordLink;
                v.Rec.x40IsUserSignature = v.Rec.x40IsUserSignature;

                v.Notepad.HtmlContent = v.Rec.x40Body;
                v.Notepad.SelectedX04ID = v.Rec.x04ID;
                v.j61GridColumns = v.Rec.x40GridColumns;

                if (v.Rec.x40Attachments != null)
                {
                    var m = Factory.MailBL.LoadMailMessageByPid(v.Rec.pid);
                    if (m != null && m.Attachments.Count() > 0)
                    {
                        foreach (var att in m.Attachments)
                        {
                            string strTempFullPath = this.Factory.TempFolder + "\\" + v.UploadGuid + "_" + att.FileName;
                            if (!System.IO.File.Exists(strTempFullPath))
                            {
                                string strInfoxFullPath = Factory.TempFolder + "\\" + v.UploadGuid + "_" + att.FileName + ".infox";
                                System.IO.File.WriteAllText(strInfoxFullPath, att.ContentType.MediaType + "|0| " + att.FileName + "|" + v.UploadGuid + "_" + att.FileName + "|" + v.UploadGuid + "|0||");

                                att.Save(strTempFullPath);

                            }
                        }
                    }
                }

            }

            if (string.IsNullOrEmpty(v.Rec.x40Subject) && v.Rec.x40RecordEntity != "j02")
            {
                v.Rec.x40Subject = Factory.CBL.GetObjectAlias(v.Rec.x40RecordEntity, v.Rec.x40RecordPid);
            }

            if (senderror > 0)
            {
                //odeslat chybu
                var recP85 = Factory.p85TempboxBL.Load(senderror);
                v.Rec.x40IsHtmlBody = true;
                v.Rec.x40Subject = $"Nahlášení chyby v MARKTIME ({recP85.p85FreeText06})";
                v.Recipient = "info@marktime.cz";
                v.Notepad.HtmlContent = $"Lokalita: {recP85.p85FreeText05}<br><code>{recP85.p85FreeText01}</code><br>{DateTime.Now}<hr>Login: {recP85.p85FreeText02}<br>Jméno: {recP85.p85FreeText03}<br>E-mail: {recP85.p85FreeText04}<br>Databáze: {recP85.p85FreeText06}<hr>{recP85.p85Message}";
            }

            RefreshState_SendMail(v);

            return View(v);
        }

        private void RefreshState_SendMail(Models.Mail.SendMailViewModel v)
        {
            v.lisGridColumns = GetListGridColumns(v.j61GridColumns, v.Rec.x40RecordEntity, v.Rec.x40RecordPid);

            if (v.Last10Receivers == null)
            {
                v.Last10Receivers = Factory.MailBL.GetLast10Receivers().Distinct().Take(20).ToList<string>();
            }
        }

        private IEnumerable<BO.TheGridColumn> GetListGridColumns(string j61gridcolumns, string record_prefix, int record_pid)
        {
            if (string.IsNullOrEmpty(j61gridcolumns)) return null;

            var mq = new BO.InitMyQuery(Factory.CurrentUser).Load(record_prefix);
            mq.pids = new List<int>() { record_pid };
            mq.TopRecordsOnly = 1;

            mq.explicit_columns = _colsProvider.ParseTheGridColumns(record_prefix, j61gridcolumns, Factory);
            var dt = Factory.gridBL.GetGridTable(mq);
            foreach (var c in mq.explicit_columns)
            {
                c.Rezerva = BO.Code.Bas.ParseCellValueFromDb(dt.Rows[0], c);


            }

            return mq.explicit_columns;
        }
        [HttpPost]
        public IActionResult SendMail(Models.Mail.SendMailViewModel v)
        {
            RefreshState_SendMail(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "cols")
                {
                    //uložit naposledy definované sloupce
                    Factory.CBL.SetUserParam($"sendmail-gridcolumns-{v.Rec.x40RecordEntity}", v.j61GridColumns);

                }
                if (v.PostbackOper == "j61id")
                {
                    var recJ61 = MailMergeByTextTemplate(v.SelectedJ61ID, v.Rec.x40RecordEntity, v.Rec.x40RecordPid);
                    v.j61MailBody = recJ61.j61MailBody;
                    if (recJ61.j61GridColumnsFlag == BO.j61UserFlagEnum.Yes)
                    {
                        v.j61GridColumns = recJ61.j61GridColumns;
                    }


                    RefreshState_SendMail(v);
                }
                if (v.PostbackOper == "j11id" && v.SelectedJ11ID > 0)
                {
                    Handle_Receiver_From_List(v, Factory.j02UserBL.GetList(new BO.myQueryJ02() { j11id = v.SelectedJ11ID }));

                }
                if (v.PostbackOper == "j07id" && v.SelectedJ07ID > 0)
                {
                    Handle_Receiver_From_List(v, Factory.j02UserBL.GetList(new BO.myQueryJ02() { j07id = v.SelectedJ07ID }));

                }
                return View(v);
            }


            if (ModelState.IsValid)
            {
                foreach (BO.o27Attachment c in Factory.o27AttachmentBL.GetTempFiles(v.UploadGuid))
                {
                    Factory.MailBL.AddAttachment(c.FullPath, c.o27OriginalFileName, c.o27ContentType);
                }


                v.Rec.x40Recipient = v.Recipient;

                v.Rec.x40GridColumns = (v.IsShowGridColumns ? v.j61GridColumns : null);
                v.Rec.x40Body = v.Notepad.HtmlContent;
                v.Rec.x04ID = v.Notepad.SelectedX04ID;
                v.Rec.j61ID = v.SelectedJ61ID;
                var ret = Factory.MailBL.SendMessage(v.Rec, v.IsTest, _colsProvider);



                if (v.Rec.j40ID > 0)
                {
                    Factory.CBL.SetUserParam("sendmail-j40id", v.Rec.j40ID.ToString());
                    Factory.CBL.SetUserParam("sendmail-isusersignature", BO.Code.Bas.GB(v.Rec.x40IsUserSignature));
                    Factory.CBL.SetUserParam($"sendmail-isgridcolumns-{v.Rec.x40RecordEntity}", BO.Code.Bas.GB(v.IsShowGridColumns));

                    if (v.Rec.x40RecordEntity == "p94" && v.Rec.x40RecordPid > 0 && v.SelectedJ61ID > 0)
                    {
                        Factory.CBL.SetUserParam("mail-p94-j61id", v.SelectedJ61ID.ToString());
                    }
                    if (v.Rec.x40RecordEntity == "p91" && v.Rec.x40RecordPid > 0)
                    {
                        if (v.SelectedJ61ID > 0)
                        {
                            Factory.CBL.SetUserParam("mail-p91-j61id", v.SelectedJ61ID.ToString());
                        }
                        else
                        {
                            if (Factory.CBL.LoadUserParamInt("mail-p91-j61id") > 0)
                            {
                                Factory.CBL.SetUserParam("mail-p91-j61id", "0");
                            }
                        }

                    }

                }

                if (ret.Flag == BO.ResultEnum.Success)  //případná chybová hláška je již naplněná v BL vrstvě
                {
                    v.SetJavascript_CallOnLoad(v.Rec.pid);
                    return View(v);
                }


            }

            return View(v);
        }

        private void Handle_Receiver_From_List(UI.Models.Mail.SendMailViewModel v, IEnumerable<BO.j02User> lisJ02)
        {
            if (!string.IsNullOrEmpty(v.Recipient))
            {
                v.Recipient = v.Recipient.Replace(";", ",");
            }

            var lis = BO.Code.Bas.ConvertString2List(v.Recipient, ",");
            foreach (var c in lisJ02.Where(p => p.j02Email != null).OrderBy(p => p.FullnameDesc))
            {
                if (lis.Where(p => p.ToLower() == c.j02Email.ToLower()).Count() == 0)
                {
                    lis.Add(c.j02Email);
                }
            }
            v.Recipient = string.Join(", ", lis.Where(p => p.Trim().Length > 2));

        }

        public IEnumerable<UI.Models.Mail.MailItemAutoComplete> AutoCompleteSource()
        {
            var ret = new List<UI.Models.Mail.MailItemAutoComplete>();

            var lis0 = Factory.MailBL.GetAllx43Emails();
            foreach (string email in lis0.Where(p=>p !=null))
            {
                ret.Add(new UI.Models.Mail.MailItemAutoComplete() { address = email.ToLower() });
            }

            var mqJ02 = new BO.myQueryJ02();
            new BO.myQueryJ02().explicit_orderby = "a.j02LastName";
            var lis1 = Factory.j02UserBL.GetList(mqJ02).Where(p => p.j02Email != null);
            foreach (var c in lis1)
            {
                if (!ret.Any(p => p.address == c.j02Email.ToLower()))
                {
                    ret.Insert(0, new UI.Models.Mail.MailItemAutoComplete() { address = c.j02Email, text = c.FullnameDesc });
                }

            }


            return ret;
        }

        public BO.Result Delete(int x40id)
        {
            if (!Factory.CurrentUser.IsAdmin) return new BO.Result(true, "Nejsi admin");

            var rec = Factory.MailBL.LoadMessageByPid(x40id);

            string s = Factory.CBL.DeleteRecord("x40", rec.pid);
            if (s == "1")
            {
                string strEmlFullPath = Factory.UploadFolder + "\\" + rec.x40EmlFolder + "\\" + rec.x40MessageGuid + ".eml";
                if (System.IO.File.Exists(strEmlFullPath))
                {
                    System.IO.File.Move(strEmlFullPath, Factory.UploadFolder + "\\DELETED\\" + rec.x40MessageGuid + ".eml");
                }
                strEmlFullPath = Factory.UploadFolder + "\\" + rec.x40EmlFolder + "\\" + rec.x40MessageGuid + ".msg";
                if (System.IO.File.Exists(strEmlFullPath))
                {
                    System.IO.File.Move(strEmlFullPath, Factory.UploadFolder + "\\DELETED\\" + rec.x40MessageGuid + ".msg");
                }
                return new BO.Result(false);
            }
            else
            {
                return new BO.Result(true, s);
            }
        }
        public IActionResult Record(int pid)
        {
            var v = new UI.Models.Mail.x40RecMessage();
            v.Rec = Factory.MailBL.LoadMessageByPid(pid);

            if (v.Rec == null)
            {
                return RecNotFound(v);
            }
            var strEmlPath = $"{Factory.UploadFolder}\\{v.Rec.x40EmlFolder}\\{v.Rec.x40MessageGuid}.eml";
            v.IsEmlFileExists = System.IO.File.Exists(strEmlPath);
            if (v.IsEmlFileExists)
            {
                v.Mail = Factory.MailBL.LoadMailMessageByPid(v.Rec.pid);
            }
            try
            {
                v.lisGridColumns = GetListGridColumns(v.Rec.x40GridColumns, v.Rec.x40RecordEntity, v.Rec.x40RecordPid);
            }
            catch
            {
                //nic
            }


            return View(v);
        }
        private void Handle_Najit_Interni_Emaily(int b05id,int pid, string prefix, List<BO.x43MailQueue_Recipient> receivers)
        {
            if (pid == 0)
            {
                return;
            }
            if (b05id > 0)
            {
                var lisB05 = Factory.WorkflowBL.GetList_b05(prefix, pid, 0, 0, 0).Where(p => p.j02ID_Sys != Factory.CurrentUser.pid);
                foreach (var c in lisB05)
                {
                    var recJ02 = Factory.j02UserBL.Load(c.j02ID_Sys);
                    if (!receivers.Any(p=>p.x43Email == recJ02.j02Email))
                    {
                        receivers.Add(new BO.x43MailQueue_Recipient() { x43Email = recJ02.j02Email, x43RecipientFlag = BO.x43RecipientIdEnum.recTO });
                    }
                    
                }
            }            
            var lis = Factory.x67EntityRoleBL.GetList_X69(prefix, pid);
            foreach(var c in lis)
            {
                if (c.j02ID > 0)
                {
                    var recJ02 = Factory.j02UserBL.Load(c.j02ID);                    
                    if (!receivers.Any(p => p.x43Email == recJ02.j02Email))
                    {
                        receivers.Add(new BO.x43MailQueue_Recipient() { x43Email = recJ02.j02Email, x43RecipientFlag = BO.x43RecipientIdEnum.recTO });
                    }
                }
                if (c.j11ID > 0)
                {
                    var recJ11 = Factory.j11TeamBL.Load(c.j11ID);
                    if (recJ11.j11Email != null && !receivers.Any(p => p.x43Email == recJ11.j11Email))
                    {
                        receivers.Add(new BO.x43MailQueue_Recipient() { x43Email = recJ11.j11Email, x43RecipientFlag = BO.x43RecipientIdEnum.recTO });
                    }
                }
            }
        }
        private void Handle_Najit_Emaily_Klienta(int p28id, string prefix, List<BO.x43MailQueue_Recipient> receivers)
        {
            var lisO32 = Factory.p28ContactBL.GetList_o32(p28id, 0, 0);
            if (prefix=="p91" || prefix == "p90")
            {
                lisO32 = lisO32.Where(p => p.o32IsDefaultInInvoice);
            }
            foreach (var c in lisO32)
            {
                switch (c.o33ID)
                {
                    case BO.o33FlagEnum.Email:
                        receivers.Add(new BO.x43MailQueue_Recipient() { x43Email = c.o32Value, x43RecipientFlag = BO.x43RecipientIdEnum.recTO }); break;
                    case BO.o33FlagEnum.EmailCC:
                        receivers.Add(new BO.x43MailQueue_Recipient() { x43Email = c.o32Value, x43RecipientFlag = BO.x43RecipientIdEnum.recCC }); break;
                    case BO.o33FlagEnum.EmailBCC:
                        receivers.Add(new BO.x43MailQueue_Recipient() { x43Email = c.o32Value, x43RecipientFlag = BO.x43RecipientIdEnum.recBCC }); break;
                }
            }
            var lisP30 = Factory.p28ContactBL.GetList_p30(p28id).Where(p => p.PersonInvoiceEmail != null);
            foreach (var c in lisP30)
            {
                receivers.Add(new BO.x43MailQueue_Recipient() { x43Email = c.PersonInvoiceEmail, x43RecipientFlag = BO.x43RecipientIdEnum.recTO });
            }
        }

        public ActionResult DownloadEmlFile(string guid)
        {
            var rec = Factory.MailBL.LoadMessageByGuid(guid);
            if (rec == null)
            {
                return this.NotFound(new UI.Models.Mail.x40RecMessage());

            }
            string fullPath = Factory.UploadFolder + "\\" + rec.x40EmlFolder + "\\" + rec.x40MessageGuid + ".eml";


            if (System.IO.File.Exists(fullPath))
            {
                Response.Headers["Content-Length"] = rec.x40EmlFileSize.ToString();
                Response.Headers["Content-Disposition"] = "inline; filename=mail_message.eml";
                var fileContentResult = new FileContentResult(System.IO.File.ReadAllBytes(fullPath), "message/rfc822");

                return fileContentResult;
                //return File(System.IO.File.ReadAllBytes(fullPath), "message/rfc822", "poštovní_zpráva.eml");
            }
            else
            {
                return RedirectToAction("FileDownloadNotFound", "o23");
            }



        }

        public ActionResult DownloadSmtpLog(string guid)
        {
            var rec = Factory.MailBL.LoadMessageByGuid(guid);
            if (rec == null)
            {
                return this.NotFound(new UI.Models.Mail.x40RecMessage());

            }
            string fullPath = Factory.UploadFolder + "\\" + rec.x40EmlFolder + "\\" + rec.x40MessageGuid + ".log";


            if (System.IO.File.Exists(fullPath))
            {

                //zkopírovat do log soubor do TEMPu, protože logfile může držet ještě rebex mail knihovna
                System.IO.File.Copy(fullPath, $"{Factory.TempFolder}\\{rec.x40MessageGuid}.log",true);

                return new FileContentResult(System.IO.File.ReadAllBytes($"{Factory.TempFolder}\\{rec.x40MessageGuid}.log"),"text/plain; charset=utf-8");

            }
            else
            {
                return NotFound("Log soubor nebyl nalezen | Log file not found.");
            }



        }




        public BO.j61TextTemplate MailMergeByTextTemplate(int j61id, string prefix, int recpid)
        {
            if (j61id == 0) return null;
            var recJ61 = Factory.j61TextTemplateBL.Load(j61id);
            if (recJ61 != null && recpid > 0 && !string.IsNullOrEmpty(prefix))
            {
                System.Data.DataTable dt = null;
                if (recJ61.j61SqlSource == null)
                {
                    dt = Factory.gridBL.GetList4MailMerge(prefix, recpid);  //default merge sql dotaz
                }
                else
                {
                    dt = Factory.gridBL.GetList4MailMerge(recpid, recJ61.j61SqlSource);  //sql dotaz na míru
                }

                recJ61.j61MailBody = BO.Code.MergeContent.GetMergedContent(recJ61.j61MailBody, dt);
                if (recJ61.j61MailSubject != null && recJ61.j61MailSubject.Contains("["))
                {
                    recJ61.j61MailSubject = BO.Code.MergeContent.GetMergedContent(recJ61.j61MailSubject, dt);
                }
                if (recJ61.j61MailCC != null && recJ61.j61MailCC.Contains("["))
                {
                    recJ61.j61MailCC = BO.Code.MergeContent.GetMergedContent(recJ61.j61MailCC, dt);
                }
                if (recJ61.j61MailBCC != null && recJ61.j61MailBCC.Contains("["))
                {
                    recJ61.j61MailBCC = BO.Code.MergeContent.GetMergedContent(recJ61.j61MailBCC, dt);
                }




            }

            return recJ61;
        }




    }



}