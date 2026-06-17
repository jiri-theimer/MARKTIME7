using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Record
{
    public class p89Record:BaseRecordViewModel
    {
        public BO.p89ProformaType Rec { get; set; }
        public string ComboX38Name { get; set; }
        public string ComboX38Name_Payment { get; set; }
        public string ComboX31Name { get; set; }
        public string ComboJ61Name { get; set; }
        public string ComboX31Name_Payment { get; set; }
        public string ComboP93Name { get; set; }

        public CreateAssignViewModel creates { get; set; }

    }
}
