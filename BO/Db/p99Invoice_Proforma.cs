using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class p99Invoice_Proforma:BaseBO
    {
        public int p91ID { get; set; }
        public int p82ID { get; set; }
        public double p99Amount { get; set; }
        public double p99Amount_WithoutVat { get; set; }
        public double p99Amount_Vat { get; set; }

        public string p82Code { get; }
        
        public string p90Code { get; }
       
        public string p91Code { get; }
        
        public int p90ID { get; }
        public int x31ID_Invoice { get; }   //tisková sestava daňové faktury
        public int x31ID_Payment { get; }   //tisková sestava úhrady zálohy


    }
}
