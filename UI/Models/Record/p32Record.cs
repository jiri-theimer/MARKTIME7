using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Record
{
    public class p32Record:BaseRecordViewModel
    {
        public BO.p32Activity Rec { get; set; }
       
        public string ComboP34Name { get; set; }
        public string ComboP95Name { get; set; }
        public string ComboP38Name { get; set; }

        public ProjectComboViewModel ProjectCombo { get; set; }
    }
}
