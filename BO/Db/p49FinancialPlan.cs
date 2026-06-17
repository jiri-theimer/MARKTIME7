

namespace BO
{
    public class p49FinancialPlan:BaseBO
    {
        public int p41ID { get; set; }
        public int j02ID { get; set; }
        public int p34ID { get; set; }
        public int p32ID { get; set; }
        public int j27ID { get; set; }
        public int p56ID { get; set; }
        public int p28ID_Supplier { get; set; }
        public byte p49StatusFlag { get; set; }    //1:překlopeno do skutečnosti

        public string p49Code { get; set; }
        public double p49PieceAmount { get; set; }
        public double p49Pieces { get; set; }
        public string p49Text { get; set; }
        public DateTime? p49Date { get; set; }

        public double p49Amount { get; set; }
        public double p49MarginHidden { get; set; }  
        public double p49MarginTransparent { get; set; }


        public string j27Code { get; }
        public string j02Name { get; }
        public string p34Name { get; }
        public string p32Name { get; }
        public string Project { get; }
        public string Client { get; }
        public string p56Name { get; }
        public int p33ID { get; set; }
        public string strana { get; }
    }
}
