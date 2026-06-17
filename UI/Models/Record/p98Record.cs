using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Record
{
    public class p97Repeater : BO.p97Invoice_Round_Setting
    {
        public string ComboJ27 { get; set; }
        
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
    public class p98Record: BaseRecordViewModel
    {
        public BO.p98Invoice_Round_Setting_Template Rec { get; set; }
        

        public List<p97Repeater> lisP97 { get; set; }
    }
}
