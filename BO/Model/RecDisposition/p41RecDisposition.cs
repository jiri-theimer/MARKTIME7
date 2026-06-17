using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class p41RecDisposition: BaseRecDisposition
    {
        public bool p31_RecalcRates { get; set; }
        public bool p31_MoveToOtherProject { get; set; }
        public bool p91_Read { get; set; }
        public bool p91_DraftCreate { get; set; }
        public bool p91_Create { get; set; }

    }
}
