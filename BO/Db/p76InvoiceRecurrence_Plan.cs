

namespace BO
{
    public class p76InvoiceRecurrence_Plan
    {
        public int p76ID { get; set; }
        public int p75ID { get; set; }
       public string p76Name { get; set; }
        public DateTime? p76BaseDate { get; set; }
        public DateTime? p76DateMaturity { get; set; }
        public DateTime? p76DateSupply { get; set; }
        public DateTime p76DateCreate { get; set; }
        public int p91ID_NewInstance { get; set; }
        public string p76ErrorMessage_NewInstance { get; set; }

        public string p91Code { get; }
        public DateTime? p91DateInsert { get; }
        public string p91Text1 { get; }
        public string p92Name { get; }
        
    }
}
