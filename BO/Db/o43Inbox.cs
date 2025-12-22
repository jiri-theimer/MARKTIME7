using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class o43Inbox:BaseBO
    {
        public int x01ID { get; set; }
        public int j40ID { get; set; }
        public int j02ID_Owner { get; set; }
        public DateTime? o43DateMessage { get; set; }
        public DateTime? o43DateReceived { get; set; }
        public Guid o43Guid { get; set; }
        public string o43MessageID { get; set; }
        public string o43InfoID { get; set; }
        public string o43ImapFolder { get; set; }
        public int o43AttachmentsCount { get; set; }
        public bool o43IsBodyHtml { get; set; }
        public string o43Subject { get; set; }
        public string o43BodyText { get; set; }
        public string o43BodyHtml { get; set; }
        public string o43SenderAddress { get; set; }
        public string o43SenderName { get; set; }
        public string o43Cc { get; set; }
        public string o43Bcc { get; set; }
        public string o43To { get; set; }
        public string o43Attachments { get; set; }
        public int o43Length { get; set; }
        public string o43ArchiveFolder { get; set; }
      
        public string o43EmlFileName { get; set; }
        public int o42ID { get; set; }
        public int j02ID { get; set; }
        public int p28ID { get; set; }
        public int p41ID { get; set; }
        public int o23ID { get; set; }
        public int p90ID { get; set; }
        public int p91ID { get; set; }
        public int p56ID { get; set; }
        public int o22ID { get; set; }
        public int b05ID { get; set; }

        public bool o43IsSeen { get; set; }
        public bool o43IsDeleted { get; set; }
        public bool o43IsFlagged { get; set; }
        public bool o43IsDraft { get; set; }

        public string Person { get; }
        public string p28Name { get; }
        public string o42Name { get; }
        public string b05Name { get; }
    }
}
