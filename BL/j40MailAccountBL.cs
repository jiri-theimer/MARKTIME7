

namespace BL
{
    public interface Ij40MailAccountBL
    {
        public BO.j40MailAccount Load(int pid);
        public BO.j40MailAccount LoadDefaultSmtpAccount();
        public IEnumerable<BO.j40MailAccount> GetList(BO.myQueryJ40 mq);
        public int Save(BO.j40MailAccount rec);
        public bool UpdateFolders(int j40id, string strFoldersInLine);
        public BO.Result TestConnection(BO.j40MailAccount rec, string strPlainPassword, List<string> steps);


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

        //test připojení k poštovnímu serveru podle (i neuloženého) nastavení účtu – stejný postup jako odesílání (MailBL) / čtení IMAP (o27AttachmentBL)
        //steps = průběh pro uživatele; nic se neodesílá ani neukládá
        public BO.Result TestConnection(BO.j40MailAccount rec, string strPlainPassword, List<string> steps)
        {
            Rebex.Licensing.Key = "==FmUGVeH5TmvcatEzt0Z4rBvjoahsK0e0albLXKX2bgBB+JxAEBiGKcZlB73B+U5q3G5YP==";
            bool bolImap = rec.j40UsageFlag == BO.MailUsageFlag.ImapPrivate || rec.j40UsageFlag == BO.MailUsageFlag.ImapGlobal;
            string strHost = bolImap ? rec.j40ImapHost : rec.j40SmtpHost;
            int intPort = bolImap ? rec.j40ImapPort : rec.j40SmtpPort;
            bool bolSecure = bolImap ? rec.j40ImapEnableSsl : rec.j40SmtpEnableSsl;
            string strLogin = bolImap ? rec.j40ImapLogin : rec.j40SmtpLogin;
            bool bolAuth = bolImap || !rec.j40SmtpUseDefaultCredentials;

            if (string.IsNullOrWhiteSpace(strHost))
            {
                return new BO.Result(true, _mother.tra("Chybí adresa serveru."));
            }
            strHost = strHost.Trim();
            if (strHost.ToLower() == "smtp.mycorecloud.net" && !bolImap)
            {
                strLogin = _mother.App.DefaultSmtpSetting.Username;
                strPlainPassword = _mother.App.DefaultSmtpSetting.Password;
            }
            if (bolAuth && (string.IsNullOrEmpty(strLogin) || string.IsNullOrEmpty(strPlainPassword)))
            {
                return new BO.Result(true, _mother.tra("Vyplňte login a heslo."));
            }

            Rebex.Net.SslMode mode = rec.j40SslModeFlag == BO.SslModelFlagEnum.Implicit ? Rebex.Net.SslMode.Implicit : (rec.j40SslModeFlag == BO.SslModelFlagEnum.Explicit ? Rebex.Net.SslMode.Explicit : Rebex.Net.SslMode.None);
            string strWhere = intPort > 0 ? $"{strHost}:{intPort}" : strHost;

            if (bolImap)
            {
                var client = new Rebex.Net.Imap();
                client.Timeout = 15000;
                try
                {
                    if (intPort > 0) client.Connect(strHost, intPort, mode); else client.Connect(strHost, mode);
                    steps.Add(_mother.tra("Spojení se serverem") + $" {strWhere}" + (client.IsSecured ? " (" + _mother.tra("šifrované") + ")" : ""));
                    if (bolSecure && !client.IsSecured)
                    {
                        client.Secure();
                        steps.Add(_mother.tra("Zabezpečení spojení (TLS)"));
                    }
                    client.Login(strLogin, strPlainPassword);
                    steps.Add(_mother.tra("Přihlášení") + $" ({strLogin})");
                    client.SelectFolder("Inbox");
                    steps.Add(_mother.tra("Otevření složky Inbox"));
                    client.Disconnect();
                    return new BO.Result(false, _mother.tra("Připojení k IMAP serveru funguje."));
                }
                catch (Exception e)
                {
                    return new BO.Result(true, e.Message);
                }
                finally
                {
                    client.Dispose();
                }
            }
            else
            {
                var client = new Rebex.Net.Smtp();
                client.Timeout = 15000;
                try
                {
                    if (intPort > 0) client.Connect(strHost, intPort, mode); else client.Connect(strHost, mode);
                    steps.Add(_mother.tra("Spojení se serverem") + $" {strWhere}" + (client.IsSecured ? " (" + _mother.tra("šifrované") + ")" : ""));
                    if (bolSecure && !client.IsSecured)
                    {
                        client.Secure();
                        steps.Add(_mother.tra("Zabezpečení spojení (TLS)"));
                    }
                    if (bolAuth)
                    {
                        client.Login(strLogin, strPlainPassword);
                        steps.Add(_mother.tra("Přihlášení") + $" ({strLogin})");
                    }
                    client.Disconnect();
                    return new BO.Result(false, _mother.tra("Připojení k SMTP serveru funguje. Zprávy půjde odesílat."));
                }
                catch (Exception e)
                {
                    return new BO.Result(true, e.Message);
                }
                finally
                {
                    client.Dispose();
                }
            }
        }
    }
}
