namespace UI.Models.Tab1
{
    public class p75Tab1: BaseTab1ViewModel
    {
        public BO.p75InvoiceRecurrence Rec { get; set; }

        public BO.p41Project RecP41 { get; set; }
        public BO.p28Contact RecP28 { get; set; }

        public IEnumerable<BO.p76InvoiceRecurrence_Plan> lisP76 { get; set; }
    }
}
