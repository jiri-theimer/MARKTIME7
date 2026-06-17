using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public enum p70IdENUM
    {
        ViditelnyOdpis = 2,
        SkrytyOdpis = 3,
        Vyfakturovano = 4,
        ZahrnutoDoPausalu = 6,
        Nic = 0
    }

    public class p70BillingStatus : BaseBO
    {
        public string p70Name { get; set; }
        public string p70Code { get; set; }
        public string p70Name_BillingLang1 { get; set; }
        public string p70Name_BillingLang2 { get; set; }
        public string p70Name_BillingLang3 { get; set; }
        public string p70Name_BillingLang4 { get; set; }

        private p70IdENUM _p70ID { get; set; }
        public p70IdENUM p70ID
        {
            get
            {
                return _p70ID;
            }
        }
        public void SetStatus(p70IdENUM status)
        {
            _p70ID = status;
        }
       

        public string Color
        {
            get
            {
                switch (this.p70ID)
                {
                    case p70IdENUM.ViditelnyOdpis:
                        {
                            return "red";
                        }

                    case p70IdENUM.SkrytyOdpis:
                        {
                            return "brown";
                        }

                    case p70IdENUM.Vyfakturovano:
                        {
                            return "green";
                        }

                    case p70IdENUM.ZahrnutoDoPausalu:
                        {
                            return "pink";
                        }

                    default:
                        {
                            return "";
                        }
                }
            }
        }
    }
}
