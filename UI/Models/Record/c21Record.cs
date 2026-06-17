using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Record
{
    public class c28Repeater
    {
        public int c21ID { get; set; }
        public string ComboC21 { get; set; }
        public DateTime? c28ValidFrom { get; set; }
        public DateTime? c28ValidUntil { get; set; }

        public bool IsTempDeleted { get; set; }
        public string TempGuid { get; set; }
        public string CssTempDisplay
        {
            get
            {
                if (this.IsTempDeleted == true)
                {
                    return "display:none;";
                }
                else
                {
                    return "display:table-row;";
                }
            }
        }
    }

    public class c21Record:BaseRecordViewModel
    {
        public BO.c21FondCalendar Rec { get; set; }


        public List<c28Repeater> lisC28 { get; set; }
    }
}
