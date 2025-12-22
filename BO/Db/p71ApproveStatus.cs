using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public enum p71IdENUM
    {
        Schvaleno = 1,
        Neschvaleno = 2,
        Nic = 0
    }

    public class p71ApproveStatus : BaseBO
    {
        public string p71Name { get; set; }
        public string p71Code { get; set; }
        public string p71Name_BillingLang1 { get; set; }
        public string p71Name_BillingLang2 { get; set; }
        public string p71Name_BillingLang3 { get; set; }
        public string p71Name_BillingLang4 { get; set; }

        private p71IdENUM _p71ID { get; set; }
        public p71IdENUM p71ID
        {
            get
            {
                return _p71ID;
            }
        }
    }
}
