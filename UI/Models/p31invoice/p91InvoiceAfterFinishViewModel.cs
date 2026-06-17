namespace UI.Models.p31invoice
{
    public class p91InvoiceAfterFinishViewModel:BaseViewModel
    {
        public int p91id { get; set; }
        public string GridUrl { get; set; }

        public BO.p91Invoice Rec { get; set; }
    }
}
