

namespace BL
{
    public interface Ij40MailAccountBL
    {
        public BO.j40MailAccount Load(int pid);
        public BO.j40MailAccount LoadDefaultSmtpAccount();
        public IEnumerable<BO.j40MailAccount> GetList(BO.myQueryJ40 mq);
        public int Save(BO.j40MailAccount rec);
        public bool UpdateFolders(int j40id, string strFoldersInLine);

    }
    class j40MailAccountBL : BaseBL, Ij40MailAccountBL
    {
        public j40MailAccountBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null,string orderby=null)
        {
            sb("SELECT a.*,ISNULL(a.j40ImapFolders,'Inbox') as ReadyImapFolders,");
            sb(_db.GetSQL1_Ocas("j40"));
            sb(" FROM j40MailAccount a");
            sb(strAppend);
            if (orderby != null) sb($" ORDER BY {orderby}");
            return sbret();
        }
        public BO.j40MailAccount Load(int pid)
        {
            return _db.Load<BO.j40MailAccount>(GetSQL1(" WHERE a.j40ID=@pid"), new { pid = pid });
            
        }
        public BO.j40MailAccount LoadDefaultSmtpAccount()
        {
            string s = $" WHERE (a.j40UsageFlag = 2 OR (a.j40UsageFlag=1 AND j02ID={_mother.CurrentUser.j02ID}))  AND GETDATE() BETWEEN a.j40ValidFrom AND a.j40ValidUntil";
            if (_mother.CurrentUser.IsHostingModeTotalCloud)
            {
                s = $" WHERE a.x01ID={_mother.CurrentUser.x01ID} AND ((a.j40UsageFlag = 2 OR (a.j40UsageFlag=1 AND j02ID={_mother.CurrentUser.j02ID})) AND GETDATE() BETWEEN a.j40ValidFrom AND a.j40ValidUntil)";
            }
            return _db.Load<BO.j40MailAccount>(GetSQL1(s, "a.j40UsageFlag DESC,a.j40Ordinary"));
        }

        public IEnumerable<BO.j40MailAccount> GetList(BO.myQueryJ40 mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.j40MailAccount>(fq.FinalSql, fq.Parameters);
        }

        public bool UpdateFolders(int j40id,string strFoldersInLine)
        {
            if (string.IsNullOrEmpty(strFoldersInLine)) strFoldersInLine = "Inbox";
            return _db.RunSql($"UPDATE j40MailAccount set j40ImapFolders=@s WHERE j40ID=@pid", new {s= strFoldersInLine, pid = j40id });
        }

        public int Save(BO.j40MailAccount rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.j40ID);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            if (rec.j02ID == 0) rec.j02ID = _db.CurrentUser.pid;
            p.AddInt("j02ID", rec.j02ID, true);
            p.AddInt("j40UsageFlag", (int)rec.j40UsageFlag);
            p.AddString("j40Name", rec.j40Name);
            p.AddString("j40SmtpHost", rec.j40SmtpHost);
            p.AddInt("j40SmtpPort", rec.j40SmtpPort);
            p.AddString("j40SmtpName", rec.j40SmtpName);
            p.AddString("j40SmtpEmail", rec.j40SmtpEmail);
            p.AddString("j40SmtpLogin", rec.j40SmtpLogin);
            if (!string.IsNullOrEmpty(rec.j40SmtpPassword) || rec.j40SmtpUseDefaultCredentials)
            {
                p.AddString("j40SmtpPassword", rec.j40SmtpPassword);
            }
            p.AddEnumInt("j40SslModeFlag", rec.j40SslModeFlag);
            
            p.AddBool("j40SmtpUseDefaultCredentials", rec.j40SmtpUseDefaultCredentials);
            p.AddBool("j40SmtpEnableSsl", rec.j40SmtpEnableSsl);
            p.AddBool("j40SmtpUsePersonalReply", rec.j40SmtpUsePersonalReply);
            p.AddString("j40ImapHost", rec.j40ImapHost);
            p.AddString("j40ImapLogin", rec.j40ImapLogin);
            if (!string.IsNullOrEmpty(rec.j40ImapPassword))
            {
                p.AddString("j40ImapPassword", rec.j40ImapPassword);
            }
            
            p.AddInt("j40ImapPort", rec.j40ImapPort);
            p.AddString("j40ImapFolders", rec.j40ImapFolders);
            p.AddBool("j40ImapEnableSsl", rec.j40ImapEnableSsl);

            p.AddInt("j40Ordinary", rec.j40Ordinary);



            return _db.SaveRecord("j40MailAccount", p, rec);


        }
        private bool ValidateBeforeSave(BO.j40MailAccount rec)
        {
            if (rec.j02ID == 0)
            {
                this.AddMessage("Chybí Vlastník účtu");return false;
            }
            if (rec.j40UsageFlag == BO.MailUsageFlag.ImapPrivate || rec.j40UsageFlag == BO.MailUsageFlag.ImapGlobal)
            {
                if (string.IsNullOrEmpty(rec.j40ImapHost) || string.IsNullOrEmpty(rec.j40ImapLogin))
                {
                    this.AddMessage("Pro nastavení IMAP účtu musíte zadat IMAP server a IMAP Login."); return false;
                }
            }
            if (rec.j40UsageFlag == BO.MailUsageFlag.SmtpPrivate || rec.j40UsageFlag == BO.MailUsageFlag.SmtpGlobal)
            {
                if (string.IsNullOrEmpty(rec.j40SmtpEmail))
                {
                    this.AddMessage("U SMTP účtu chybí zadat e-mail odesílatele."); return false;
                }
                if (string.IsNullOrEmpty(rec.j40SmtpHost))
                {
                    this.AddMessage("Musíte zadat SMTP server."); return false;
                }
                if (rec.pid == 0 && !rec.j40SmtpUseDefaultCredentials && string.IsNullOrEmpty(rec.j40SmtpPassword))
                {
                    this.AddMessage("Zadané SMTP nastavení vyžaduje zadat heslo."); return false;
                }
            }
            

            


            return true;
        }

    }
}
