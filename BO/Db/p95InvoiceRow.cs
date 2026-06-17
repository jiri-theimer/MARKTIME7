

namespace BO
{
    public class p95InvoiceRow:BaseBO
    {
        public int x01ID { get; set; }
        public string p95Name { get; set; }
        public string p95Code { get; set; }
        public int p95Ordinary { get; set; }

        public string p95Name_BillingLang1 { get; set; }
        public string p95Name_BillingLang2 { get; set; }
        public string p95Name_BillingLang3 { get; set; }
        public string p95Name_BillingLang4 { get; set; }

        
    }
}
