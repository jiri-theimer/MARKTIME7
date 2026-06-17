

namespace UI.Models.p31invoice
{
    public class Append2InvoiceViewModel: BaseViewModel
    {
        public int SelectedInvoiceP91ID { get; set; }
        public string SelectedInvoiceText { get; set; }

        public string pids { get; set; }
        
        public BO.p91Invoice Rec { get; set; }
        public IEnumerable<BO.p31Worksheet> lisP31 { get; set; }
    }
}
