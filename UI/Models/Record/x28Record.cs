using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Record
{    
    public class x28Record:BaseRecordViewModel
    {
        public BO.x28EntityField Rec { get; set; }

        public string ComboX27Name { get; set; }

        public string SelectedJ04IDs { get; set; }
        public string SelectedJ04Names { get; set; }
        public string SelectedJ07IDs { get; set; }
        public string SelectedJ07Names { get; set; }

        public string SelectedP34IDs { get; set; }
        public string SelectedP34Names { get; set; }

        public List<BO.x26EntityField_Binding> lisX26 { get; set; }

        public bool IsTextArea { get; set; }
    }
}
