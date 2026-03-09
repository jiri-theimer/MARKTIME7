

namespace BO
{
    public enum o27CallerFlagENUM
    {
        _None = 0,
        Notepad = 1
    }
    public class o27Attachment : BaseBO
    {

        public int x01ID { get; set; }
        public int j02ID_Owner { get; set; }
        public string o27Entity { get; set; }
        public int o27RecordPid { get; set; }
        public string o27Name { get; set; }
        public string o27OriginalFileName { get; set; }
        public string o27FileExtension { get; set; }
        public string o27ArchiveFileName { get; set; }
        public string o27ArchiveFolder { get; set; }
        public string o27WwwRootFolder { get; set; }
        public int o27FileSize { get; set; }
        public string o27ContentType { get; set; }
        public string o27FullText { get; set; }
        public Guid o27Guid { get; set; }

        public string o27NotepadTempGuid { get; set; }
        public o27CallerFlagENUM o27CallerFlag { get; set; }
        public string FullPath { get; set; } //pracovní




        public DateTime? o27MailDateMessage { get; set; }
        public DateTime? o27MailDateReceived { get; set; }
        public string o27MailMessageID { get; set; }
        public string o27MailInfoID { get; set; }
        
        public int o27MailAttachmentsCount { get; set; }
        public bool o27MailIsBodyHtml { get; set; }
        public string o27MailSubject { get; set; }
        public string o27MailBodyText { get; set; }
        public string o27MailBodyHtml { get; set; }
        public string o27MailSenderAddress { get; set; }
        public string o27MailSenderName { get; set; }
        public string o27MailCc { get; set; }
        public string o27MailBcc { get; set; }
        public string o27MailToName { get; set; }
        public string o27MailToAddress { get; set; }
        public string o27MailAttachments { get; set; }

        public bool IsMail { get
            {
                if (this.o27FileExtension == ".msg" || this.o27FileExtension == ".eml" || this.o27MailMessageID != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        

    }
}
