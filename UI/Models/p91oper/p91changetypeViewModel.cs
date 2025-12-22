namespace UI.Models.p91oper
{
    public class p91changetypeViewModel:BaseViewModel
    {
        public string pids { get; set; }
        public IEnumerable<BO.p91Invoice> lisP91 { get; set; }
        
        public int SelectedP92ID { get; set; }
        public BO.x15IdEnum SelectedX15ID { get; set; }
        public BO.p92InvoiceType RecP92 { get; set; }
        public BO.p93InvoiceHeader RecP93 { get; set; }
        public BO.x38CodeLogic RecX38 { get; set; }
        public IEnumerable<BO.p92InvoiceType> lisP92 { get; set; }
        public bool IsUpdateIssuer { get; set; }
        public bool IsUpdateJ27ID { get; set; }
        public bool IsUpdateP91Code { get; set; }
        
    }
}
