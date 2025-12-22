using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.p91oper
{
    public class p31removeViewModel: BaseViewModel
    {
        public int p31ID { get; set; }
        public BO.p31Worksheet Rec { get; set; }

        public BO.p91Invoice RecP91 { get; set; }

        public int SelectedOper { get; set; }
    }
}
