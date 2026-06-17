using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.p91oper
{
    public class j27ViewModel:BaseViewModel
    {
        public int p91ID { get; set; }

        public int SelectedJ27ID { get; set; }
        
        public BO.p91Invoice RecP91 { get; set; }

        public IEnumerable<BO.j27Currency> lisJ27 { get; set; }
    }
}
