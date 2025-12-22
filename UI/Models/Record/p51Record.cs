using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Record
{
    public class p51Record:BaseRecordViewModel
    {
        public BO.p51PriceList Rec { get; set; }
        public string ComboJ27Code { get; set; }
        public List<p52Repeater> lisP52 { get; set; }
        public string TempGuid { get; set; }    //kvůli zakládání ceníku na míru
        public DateTime? d1 { get; set; }
        public DateTime? d2 { get; set; }
        public DateTime? recalc_d1 { get; set; }
        public DateTime? recalc_d2 { get; set; }
    }

    public class p52Repeater : BO.p52PriceList_Item
    {
        public string ComboPerson { get; set; }
        public string ComboJ07Name { get; set; }       
        public string ComboP32Name { get; set; }
        public string ComboP34Name { get; set; }

        public string RowPrefixWho { get; set; }
        public string RowPrefixActivity { get; set; }
        public bool IsTempDeleted { get; set; }
        public string TempGuid { get; set; }        
        public string CssTempDisplay
        {
            get
            {
                if (this.IsTempDeleted)
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
    
}
