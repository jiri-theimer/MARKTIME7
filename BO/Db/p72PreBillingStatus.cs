
namespace BO
{
    public enum p72IdENUM
    {
        ViditelnyOdpis = 2,
        SkrytyOdpis = 3,
        Fakturovat = 4,
        ZahrnoutDoPausalu = 6,
        FakturovatPozdeji = 7,
        _NotSpecified = 0
    }

    public class p72PreBillingStatus : BaseBO
    {
        public string p72Name { get; set; }
        public string p72Code { get; set; }
        public string p72Name_BillingLang1 { get; set; }
        public string p72Name_BillingLang2 { get; set; }
        public string p72Name_BillingLang3 { get; set; }
        public string p72Name_BillingLang4 { get; set; }

        private p72IdENUM _p72ID { get; set; }
        public p72IdENUM p72ID
        {
            get
            {
                return _p72ID;
            }
        }
        
        public void SetStatus(p72IdENUM status)
        {
            _p72ID = status;
        }
        public string ImageUrl
        {
            get
            {
                switch (this.p72ID)
                {
                    case p72IdENUM.ViditelnyOdpis:
                        {
                            return "/images/a12.gif";
                        }

                    case p72IdENUM.SkrytyOdpis:
                        {
                            return "/images/a13.gif";
                        }

                    case p72IdENUM.Fakturovat:
                        {
                            return "/images/a14.gif";
                        }

                    case p72IdENUM.ZahrnoutDoPausalu:
                        {
                            return "/images/a16.gif";
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
