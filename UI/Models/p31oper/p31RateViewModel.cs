

namespace UI.Models.p31oper
{
    public class p31RateViewModel:BaseViewModel
    {
        public DateTime d { get; set; }
        public int p41ID { get; set; }
        public BO.p41Project RecP41 { get; set; }
        public int j02ID { get; set; }
        public int p32ID { get; set; }
        public double BillRate { get; set; }
        public string j27Code_BillingRate { get; set; }
        public double CostRate { get; set; }
        public string j27Code_CostRate { get; set; }

        public int p51ID_BillingRate { get; set; }
        public int p51ID_CostRate { get; set; }
    }
}
