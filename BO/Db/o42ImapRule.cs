using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public enum o42WhatToDoFlagENUM
    {
        _StayInQueue=0,
        CreateDoc=1,
        BindWithProject=2,
        BindWithDoc=3,
        BindWithContact=4,
        BindWithUser=5,
        InvoicePayment=6,
        ProformaPayment=7

    }
    public class o42ImapRule:BaseBO
    {
        public int j40ID { get; set; }
        public string o42Name { get; set; }
        public o42WhatToDoFlagENUM o42WhatToDoFlag { get; set; }
        public int o18ID_CreateBy { get; set; }
        public int p41ID_Default { get; set; }
        public int j02ID_Default { get; set; }
        public int p28ID_Default { get; set; }
        public string o42Condition_Sender { get; set; }
        public string o42Condition_Subject { get; set; }
        public string o42Condition_Cc { get; set; }
        public string o42Condition_Bcc { get; set; }
        public string o42Description { get; set; }

        public string j40Name { get; }
        public string j40ImapLogin { get; }

    }
}
