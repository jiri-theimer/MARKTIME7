
using BO;
using Rebex.Mail;
using Rebex.Net;

namespace BL
{
    public interface Io43InboxBL
    {
        public BO.o43Inbox Load(int pid);
        public BO.o43Inbox LoadByGuid(string guid);
        public BO.o43Inbox LoadByMessageId(string messageid);
        public IEnumerable<BO.o43Inbox> GetList(BO.myQueryO43 mq);
        public int Save(BO.o43Inbox rec);
        public bool TryImportMessages(int j40id, int intTakelastTopMessages, string strImapFolder, string strSeenFlag, string strFlaggedFlag,string strNahazovatZpravamPriznak);
        public bool TryReloadOneMessage(int pid,string strNahazovatZpravamPriznak);
        public List<string> GetFolderList(int j40id);
        public BO.Result TryImportAsFile(int pid, string format);
        public IEnumerable<ImapMessageInfo> GetMessageList(Imap client, int intTakelastTopMessages, string strSeenFlag, string strFlaggedFlag);
        public Imap ConnectToImapServer(BO.j40MailAccount config, string strFolder);
        public BO.o43Inbox GetRecordFromMessage(Imap client, ImapMessageInfo c);
        public int TestIfExistLogRecord(string messageid);
        public List<BO.StringPair> GetInboxFiles(int pid, bool tryimport_ifempty);

    }
    class o43InboxBL : BaseBL, Io43InboxBL
    {

        public o43InboxBL(BL.Factory mother) : base(mother)
        {
            Rebex.Licensing.Key = "==FmUGVeH5TmvcatEzt0Z4rBvjoahsK0e0albLXKX2bgBB+JxAEBiGKcZlB73B+U5q3G5YP==";
        }


        public List<BO.StringPair> GetInboxFiles(int pid,bool tryimport_ifempty)
        {
            var ret = new List<BO.StringPair>();

            var rec = Load(pid);
            if (rec.o43ArchiveFolder == null) rec.o43ArchiveFolder = $"{rec.o43DateMessage.Value.Year}\\{rec.o43DateMessage.Value.Month}";
            string strFolder = $"{_mother.UploadFolder}\\{rec.o43ArchiveFolder}";

            if (System.IO.File.Exists($"{strFolder}\\{rec.o43MessageID}.msg"))
            {
                ret.Add(new BO.StringPair() { Value = $"{strFolder}\\{rec.o43MessageID}.msg", Key = "msg" });
            }
            if (System.IO.File.Exists($"{strFolder}\\{rec.o43MessageID}.eml"))
            {
                ret.Add(new BO.StringPair() { Value = $"{strFolder}\\{rec.o43MessageID}.eml", Key = "eml" });
            }
            if (tryimport_ifempty && ret.Count() == 0)
            {
                TryImportAsFile(pid, "eml");
            }

            if (rec.o43AttachmentsCount > 0)
            {
                var lis = BO.Code.Bas.ConvertString2List(rec.o43Attachments, ",");
                foreach (string s in lis)
                {                    
                    if (System.IO.File.Exists($"{strFolder}\\{rec.o43MessageID}-{s.Trim()}"))
                    {
                        ret.Add(new BO.StringPair() { Value = $"{strFolder}\\{rec.o43MessageID}-{s.Trim()}", Key = s.Trim() });                        
                    }                    
                }
            }

            return ret;
        }
        public BO.Result TryImportAsFile(int pid, string format)
        {
            var rec = Load(pid);
            var recJ40 = _mother.j40MailAccountBL.Load(rec.j40ID);
            var client = ConnectToImapServer(recJ40, rec.o43ImapFolder);
            var mail = client.GetMailMessage(rec.o43InfoID);

            SaveMessage2File(mail);

            return new BO.Result(false, $"{mail.MessageId.Id}.{format}");


        }

        public bool TryReloadOneMessage(int pid,string strNahazovatZpravamPriznak)
        {
            var rec = Load(pid);
            var recJ40 = _mother.j40MailAccountBL.Load(rec.j40ID);
            var client = ConnectToImapServer(recJ40, rec.o43ImapFolder);
            client.Settings.UsePeekForGetMessage = true;    //nenahazovat IsSeen flag na načtené zprávy!!

            if (client == null || !client.IsConnected)
            {
                this.AddMessage("Připojení k poštovnímu IMAP serveru se nezdařilo.");
                client.Disconnect();
                return false;
            }

            var set = new ImapMessageSet();
            set.Add(rec.o43InfoID);
            
            ImapMessageCollection lis = client.GetMessageList(set);
            if (lis == null || lis.Count() == 0)
            {
                this.AddMessage("Na poštovním serveru se nedaří najít zprávu. Je pravděpodobé, že byla přesunuta do jiné složky nebo odstraněna.");
                client.Disconnect();
                return false;
            }
            SaveMessages2Inbox(client, rec.o43ImapFolder, lis, recJ40.pid,true, strNahazovatZpravamPriznak);

            client.Disconnect();
            return true;
        }

        public bool TryImportMessages(int j40id, int intTakelastTopMessages, string strImapFolder, string strSeenFlag, string strFlaggedFlag, string strNahazovatZpravamPriznak)
        {
            var recJ40 = _mother.j40MailAccountBL.Load(j40id);
            var client = ConnectToImapServer(recJ40, strImapFolder);
           
            if (strNahazovatZpravamPriznak != "Seen" && strNahazovatZpravamPriznak != "SeenFlagged")
            {
                client.Settings.UsePeekForGetMessage = true;    //nenahazovat IsSeen flag na načtené zprávy!!
            }
            

            if (client == null || !client.IsConnected)
            {
                this.AddMessage("Připojení k poštovnímu IMAP serveru se nezdařilo.");
                return false;
            }

            var lis = GetMessageList(client, intTakelastTopMessages, strSeenFlag, strFlaggedFlag);
            if (lis == null || lis.Count() == 0)
            {
                //this.AddMessage("Na poštovním serveru se nedaří najít zprávu. Je pravděpodobé, že byla přesunuta do jiné složky nebo odstraněna.");
                this.AddMessageTranslated("Žádné nové zprávy", "info");
                return false;
            }
            
            SaveMessages2Inbox(client, strImapFolder, lis, recJ40.pid, false, strNahazovatZpravamPriznak);


            return true;
        }
        public IEnumerable<ImapMessageInfo> GetMessageList(Imap client, int intTakelastTopMessages, string strSeenFlag, string strFlaggedFlag)
        {
            //intTakelastTopMessages: kolik načíst najednou posledních zpráv
            int intTo = client.CurrentFolder.TotalMessageCount;
            int intFrom = intTo - intTakelastTopMessages;
            if (intFrom <= 0)
            {
                intFrom = 1;
            }
            if (intFrom > intTo)
            {
                return null;
            }

            var set = new ImapMessageSet();
            set.AddRange(intFrom, intTo);
            

            ImapMessageCollection lis = client.GetMessageList(set);
            var ret = lis.Take(lis.Count);


            if (strSeenFlag == "true")
            {
                ret = lis.Where(p => p.IsSeen == true);
            }
            if (strSeenFlag == "false")
            {
                ret = lis.Where(p => p.IsSeen == false);
            }
            if (strFlaggedFlag == "true")
            {
                ret = ret.Where(p => p.IsFlagged == true);
            }
            if (strFlaggedFlag == "false")
            {
                ret = ret.Where(p => p.IsFlagged == false);
            }

            return ret;



        }

        public BO.o43Inbox GetRecordFromMessage(Imap client,ImapMessageInfo c)
        {
            Rebex.Mail.MailMessage mail = client.GetMailMessage(c.UniqueId);
            var rec = new BO.o43Inbox() { j02ID_Owner = _mother.CurrentUser.pid, o43MessageID = c.MessageId.Id };
                        
            var cc = mail.AlternateViews[0];
            var ii = cc.GetContentLength;
            var ss = cc.ContentString;

            if (mail.ReceivedDate != null)
            {
                rec.o43DateReceived = mail.ReceivedDate.LocalTime;
            }
            if (mail.Date != null)
            {
                rec.o43DateMessage = mail.Date.LocalTime;
            }

            if (mail.ReceivedDate != null)
            {
                rec.o43DateReceived = mail.ReceivedDate.LocalTime;
            }
            if (mail.Date != null)
            {
                rec.o43DateMessage = mail.Date.LocalTime;
            }


            rec.o43Subject = mail.Subject;

            if (mail.To != null && mail.To.Count > 0)
            {
                rec.o43To = String.Join(", ", mail.To.Select(p => p.Address));

            }
            if (mail.CC != null && mail.CC.Count > 0)
            {
                rec.o43Cc = String.Join(", ", mail.CC.Select(p => p.Address));

            }
            if (mail.Bcc != null && mail.Bcc.Count > 0)
            {
                rec.o43Bcc = String.Join(", ", mail.Bcc.Select(p => p.Address));

            }

            rec.o43AttachmentsCount = mail.Attachments.Count;
            if (mail.Attachments.Count > 0)
            {
                rec.o43Attachments = BO.Code.Bas.OM2(string.Join(", ", mail.Attachments.Select(p => p.FileName)), 490);
            }
            else
            {
                rec.o43Attachments = null;
            }
            rec.o43IsDraft = mail.IsDraft;

            if (mail.HasBodyHtml)
            {
                rec.o43IsBodyHtml = true;
                rec.o43BodyHtml = mail.BodyHtml;
            }
            else
            {
                rec.o43IsBodyHtml = false;
            }
            if (mail.HasBodyText)
            {
                rec.o43BodyText = mail.BodyText;
            }

            rec.o43InfoID = c.UniqueId;
            rec.o43IsSeen = c.IsSeen;
            rec.o43IsDeleted = c.IsDeleted;
            rec.o43IsFlagged = c.IsFlagged;
            rec.o43IsDraft = c.IsDraft;
            
            rec.o43SenderName = c.Sender.DisplayName;
            rec.o43SenderAddress = c.Sender.Address;
            
            rec.o43Length = Convert.ToInt32(c.Length);


            return rec;
        }
        private void SaveMessages2Inbox(Imap client, string strImapFolder, IEnumerable<ImapMessageInfo> lis, int intJ40ID, bool bolUpdateSubjects,string strNahazovatZpravamPriznak) //zpracování načtených zpráv z poštovního serveru
        {
            int intNewRecs = 0;
            foreach (var c in lis)
            {
                var rec = LoadByMessageId(c.MessageId.Id);
                if (rec == null)
                {                                        
                    intNewRecs += 1;
                    
                    rec = GetRecordFromMessage(client,c);                                        

                }
                rec.j40ID = intJ40ID;
                rec.o43InfoID = c.UniqueId;
                rec.o43IsSeen = c.IsSeen;
                rec.o43IsDeleted = c.IsDeleted;
                rec.o43IsFlagged = c.IsFlagged;
                rec.o43IsDraft = c.IsDraft;

                rec.o43ArchiveFolder = $"o43\\{c.Date.LocalTime.Year}\\{c.Date.LocalTime.Month}";
                rec.o43ImapFolder = strImapFolder;

                rec.o43SenderName = c.Sender.DisplayName;
                rec.o43SenderAddress = c.Sender.Address;

                if (bolUpdateSubjects)
                {
                    rec.o43Subject = c.Subject;
                }

                rec.o43Length = Convert.ToInt32(c.Length);


                int intPID = Save(rec);
                if (intPID > 0 && strNahazovatZpravamPriznak=="Deleted")
                {
                    try
                    {
                        var mail = client.GetMailMessage(c.UniqueId);
                        SaveMessage2File(mail);

                        
                    }
                    catch
                    {
                        //nic
                    }
                    
                    
                    client.CopyMessage(c.UniqueId, "Trash");
                    client.DeleteMessage(c.UniqueId);
                    client.Purge();
                    rec = Load(intPID);
                    rec.o43ImapFolder = "Trash";
                    Save(rec);
                    
                }
                if (intPID>0 && (strNahazovatZpravamPriznak== "Flagged" || strNahazovatZpravamPriznak== "SeenFlagged"))
                {
                    client.SetMessageFlags(c.UniqueId, ImapFlagAction.Add, ImapMessageFlags.Flagged);
                }
            }
            if (intNewRecs > 0)
            {
                
                this.AddMessageTranslated(_mother.tra("Počet nových zpráv") + ": " + intNewRecs.ToString(), "info");
            }
        }

        public void SaveMessage2File(Rebex.Mail.MailMessage mail)
        {            
            string strFolder = $"{_mother.UploadFolder}\\o43\\{mail.Date.LocalTime.Year}\\{mail.Date.LocalTime.Month}";
            if (!System.IO.Directory.Exists(strFolder))
            {
                System.IO.Directory.CreateDirectory(strFolder);
            }
            if (!System.IO.File.Exists($"{strFolder}\\{mail.MessageId.Id}.msg"))
            {
                mail.Save($"{strFolder}\\{mail.MessageId.Id}.msg", MailFormat.OutlookMsg);
            }
            if (!System.IO.File.Exists($"{strFolder}\\{mail.MessageId.Id}.eml"))
            {
                mail.Save($"{strFolder}\\{mail.MessageId.Id}.eml", MailFormat.Mime);
            }
            if (mail.Attachments.Count() > 0)
            {
                foreach(var c in mail.Attachments)
                {
                    c.Save($"{strFolder}\\{mail.MessageId.Id}-{c.FileName}");
                }
            }
        }

        public List<string> GetFolderList(int j40id)
        {
            var config = _mother.j40MailAccountBL.Load(j40id);
            var client = ConnectToImapServer(config, "Inbox");
            return client.GetFolderList().Select(p => p.Name).ToList();
        }

        public Imap ConnectToImapServer(BO.j40MailAccount config, string strFolder)
        {
            if (string.IsNullOrEmpty(strFolder))
            {
                strFolder = "Inbox";
            }
            var client = new Imap();
            try
            {
                switch (config.j40SslModeFlag)
                {
                    case BO.SslModelFlagEnum.Implicit:
                        if (config.j40ImapPort > 0)
                        {
                            client.Connect(config.j40ImapHost, config.j40ImapPort, SslMode.Implicit);
                        }
                        else
                        {
                            client.Connect(config.j40ImapHost, SslMode.Implicit);
                        }
                        break;
                    case BO.SslModelFlagEnum.Explicit:
                        if (config.j40ImapPort > 0)
                        {
                            client.Connect(config.j40ImapHost, config.j40ImapPort, SslMode.Explicit);
                        }
                        else
                        {
                            client.Connect(config.j40ImapHost, SslMode.Explicit);
                        }
                        break;
                    default:
                        if (config.j40ImapPort > 0)
                        {
                            client.Connect(config.j40ImapHost, config.j40ImapPort);
                        }
                        else
                        {
                            client.Connect(config.j40ImapHost);
                        }
                        break;
                }
            }
            catch(Exception e)
            {
                _mother.CurrentUser.AddMessage(e.Message);
                return null;
            }
            

            if (config.j40ImapEnableSsl && !client.IsSecured)
            {
                client.Secure();
                
            }
            var strPassword = new BO.Code.Cls.Crypto().Decrypt(config.j40ImapPassword);

            client.Login(config.j40ImapLogin, strPassword);
            client.SelectFolder(strFolder);

            return client;
        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,p28.p28Name,j02.j02LastName+' '+j02.j02FirstName as Person,");
            sb(_db.GetSQL1_Ocas("o43"));
            sb(" FROM o43Inbox a LEFT OUTER JOIN p28Contact p28 ON a.p28ID=p28.p28ID LEFT OUTER JOIN j02User j02 ON a.j02ID=j02.j02ID");
            
            sb(strAppend);
            return sbret();
        }
        public BO.o43Inbox Load(int pid)
        {
            return _db.Load<BO.o43Inbox>(GetSQL1(" WHERE a.o43ID=@pid"), new { pid = pid });
        }
        public BO.o43Inbox LoadByGuid(string guid)
        {
            return _db.Load<BO.o43Inbox>(GetSQL1(" WHERE a.o43Guid=@guid"), new { guid = guid });
        }
        public BO.o43Inbox LoadByMessageId(string messageid)
        {
            return _db.Load<BO.o43Inbox>(GetSQL1(" WHERE a.o43MessageID=@messageid"), new { messageid = messageid });
        }
        public int TestIfExistLogRecord(string messageid)
        {            
            var c = _db.Load<BO.GetInteger>("select TOP 1 o43ID as Value FROM o43Inbox_Log WHERE o43MessageID=@messageid", new { messageid = messageid });
            if (c == null) return 0;

            return c.Value;
        }
        public IEnumerable<BO.o43Inbox> GetList(BO.myQueryO43 mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.o43Inbox>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.o43Inbox rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            p.AddInt("j02ID_Owner", rec.j02ID_Owner, true);
            p.AddInt("j40ID", rec.j40ID, true);
            p.AddInt("j02ID", rec.j02ID, true);
            p.AddInt("p28ID", rec.p28ID, true);
            p.AddInt("p41ID", rec.p41ID, true);
            p.AddInt("p91ID", rec.p91ID, true);
            p.AddInt("p90ID", rec.p90ID, true);
            p.AddInt("p56ID", rec.p56ID, true);
            p.AddInt("o22ID", rec.o22ID, true);
            p.AddInt("o23ID", rec.o23ID, true);
            p.AddInt("b05ID", rec.b05ID, true);

            p.AddDateTime("o43DateReceived", rec.o43DateReceived);
            p.AddDateTime("o43DateMessage", rec.o43DateMessage);
            p.AddString("o43MessageID", rec.o43MessageID);
            p.AddString("o43ImapFolder", rec.o43ImapFolder);
            p.AddString("o43InfoID", rec.o43InfoID);
            p.AddString("o43Subject", rec.o43Subject);
            p.AddString("o43BodyHtml", rec.o43BodyHtml);
            p.AddString("o43BodyText", rec.o43BodyText);

            p.AddString("o43SenderAddress", rec.o43SenderAddress);
            p.AddString("o43SenderName", rec.o43SenderName);
            p.AddString("o43To", rec.o43To);
            p.AddString("o43Cc", rec.o43Cc);
            p.AddString("o43Bcc", rec.o43Bcc);

            p.AddInt("o43AttachmentsCount", rec.o43AttachmentsCount);
            p.AddBool("o43IsBodyHtml", rec.o43IsBodyHtml);
            p.AddString("o43Attachments", rec.o43Attachments);
            p.AddString("o43ArchiveFolder", rec.o43ArchiveFolder);
            p.AddInt("o43Length", rec.o43Length);

            p.AddBool("o43IsSeen", rec.o43IsSeen);
            p.AddBool("o43IsDeleted", rec.o43IsDeleted);
            p.AddBool("o43IsDraft", rec.o43IsDraft);
            p.AddBool("o43IsFlagged", rec.o43IsFlagged);

            if (rec.o43Guid == Guid.Empty)
            {
                rec.o43Guid = Guid.NewGuid();
            }

            p.AddString("o43Guid", rec.o43Guid.ToString());

            int intPID = _db.SaveRecord("o43Inbox", p, rec);

            if (intPID > 0)
            {
                _db.RunSql("exec dbo.o43_aftersave @pid,@j02id", new { pid = intPID,j02id=_mother.CurrentUser.pid });
            }

            return intPID;

        }
        private bool ValidateBeforeSave(BO.o43Inbox rec)
        {
            
            //projde všechno
            if (rec.j02ID_Owner == 0)
            {
                rec.j02ID_Owner = _mother.CurrentUser.pid;
            }


            return true;
        }
    }
}
