
using System.ComponentModel.DataAnnotations;

namespace BO
{
    public enum MailUsageFlag
    {
        SmtpPrivate=1,
        SmtpGlobal=2,
        ImapPrivate=3,
        ImapGlobal=4
    }
    public enum SslModelFlagEnum
    {
        None=0,
        Implicit=1,
        Explicit=2
    }
    public class j40MailAccount:BaseBO
    {
        [Key]
        public int j40ID { get; set; }
        public int x01ID { get; set; }
        public int j02ID { get; set; }
        public string j40Name { get; set; }
        public MailUsageFlag j40UsageFlag { get; set; }
        public string j40SmtpHost { get; set; }
        public int j40SmtpPort { get; set; }
        public string j40SmtpName { get; set; }
        public string j40SmtpEmail { get; set; }
        public bool j40SmtpUsePersonalReply { get; set; } = true;
        public string j40SmtpLogin { get; set; }
        public string j40SmtpPassword { get; set; }
        public bool j40SmtpUseDefaultCredentials { get; set; }
        public SslModelFlagEnum j40SslModeFlag { get; set; }
        public bool j40SmtpEnableSsl { get; set; }
        public string j40ImapHost { get; set; }
        public string j40ImapFolders { get; set; }
        public string j40ImapLogin { get; set; }
        public string j40ImapPassword { get; set; }
        public int j40ImapPort { get; set; }
        public bool j40ImapEnableSsl { get; set; }
        public int j40Ordinary { get; set; }
        public string Person { get; }
        public string ReadyImapFolders { get; }
        public string UsageAlias
        {
            get
            {
                switch (this.j40UsageFlag)
                {
                    case MailUsageFlag.SmtpPrivate:
                    case MailUsageFlag.SmtpGlobal:
                        return "SMTP účet (OUTBOX: odeslaná pošta)";
                   
                    case MailUsageFlag.ImapGlobal:
                    case MailUsageFlag.ImapPrivate:
                        return "IMAP účet (INBOX: došlá pošta)";
                    default:
                        return null;
                }

                
            }
        }

        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(this.j40Name))
                {
                    return this.j40Name;
                }

                if (this.j40UsageFlag == BO.MailUsageFlag.ImapGlobal || this.j40UsageFlag == BO.MailUsageFlag.ImapPrivate)
                {
                    return this.j40ImapLogin;
                }
                else
                {
                    return this.j40SmtpEmail;
                }
                
            }
        }
    }
}
