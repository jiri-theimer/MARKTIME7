using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.p91oper
{
    public class proformaViewModel:BaseViewModel
    {
        public int p91ID { get; set; }
        public BO.p91Invoice RecP91 { get; set; }
        public bool FilterCustomerOnly { get; set; }
        public int SelectedP90ID { get; set; }
        public string SelectedP90Alias { get; set; }
        public int SelectedP82ID { get; set; }          
        public IEnumerable<BO.p82Proforma_Payment> lisP82 { get; set; }
        public IEnumerable<BO.p99Invoice_Proforma> lisP99 { get; set; }

        public double ZbyvaUhraditBezDph { get; set; }

        public int PodilProcento { get; set; }
        public double PodilBezDph { get; set; }
        public double PodilDph { get; set; }
        public double PodilCelkem { get; set; }
    }
}
