using BL;

namespace UI.Models.banka
{
    public class BankaImportViewModel : BaseViewModel
    {
        public string Guid { get; set; }
        public string TypVypisu { get; set; }
        public string FioApiKey { get; set; }
        public List<Polozka> lisPolozky { get; set; }

        public List<BO.GpcRecord> lisGpcPolozky { get; set; }
        public bool ZobrazovatPouzeNesparovane { get; set; }
        public IFormFile file1 { get; set; }

        public BO.p91Invoice NajitFakturu(BL.Factory f,Polozka pol)
        {
            if (pol.p94Code != null)
            {
                var c = f.p91InvoiceBL.LoadByNumericCode(pol.p94Code);
                if (c != null)
                {
                    return c;
                }
            }
            if (pol.Protiucet != null)
            {
                var recP28 = f.p28ContactBL.LoadByBankAccount(pol.Protiucet);
                if (recP28 != null)
                {
                    var lisP91 = f.p91InvoiceBL.GetList(new BO.myQueryP91() { p28id = recP28.pid }).Where(p => p.p91Date <= pol.p94Date);
                    if (lisP91.Count() > 0)
                    {
                        return lisP91.First();
                    }
                }
            }

            return null;

        }

    }

    public class Polozka : BO.p94Invoice_Payment
    {
        public string p91Code { get; set; }
        public string Protiucet { get; set; }
        public double Dluh { get; set; }
    }



}
