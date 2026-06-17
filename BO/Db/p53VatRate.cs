using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public enum x15IdEnum
    {
        Nic = 0,
        BezDPH = 1,
        SnizenaSazba = 2,
        ZakladniSazba = 3,
        SpecialniSazba = 4
    }

    public class p53VatRate:BaseBO
    {
        public x15IdEnum x15ID { get; set; }
        public int x01ID { get; set; }
        public double p53Value { get; set; }
        public int j27ID { get; set; }
        public string p53CountryCode { get; set; }

        public string x15Name { get; }
       
        public string j27Code { get; }
        
        
        
    }
}
