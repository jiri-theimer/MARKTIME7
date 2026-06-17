
namespace BO
{
    public class p91InvoiceSum
    {
        public string p91Code { get; set; }
        public string p92Name { get; set; }
        public string j27Code { get; set; }
        public int p91LockFlag { get; set; }

        public int p31_count { get; set; }
        public int p31_time_count { get; set; }
        public int p31_expense_count { get; set; }
        public int p31_fee_count { get; set; }
        public int p31_kusovnik_count { get; set; }
        public int p41_count { get; set; }
        public int p90_count { get; set; }
        
        public int j02_count { get; set; }
        public int o43_count { get; set; }
        
        public int b05_count { get; set; }
    }
}
