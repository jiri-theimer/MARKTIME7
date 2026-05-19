
namespace BO
{
    public class p84Upominka:BaseBO
    {
        public string p84Name { get; set; }
        public double p84AmountDebt { get; set; }
        public int p84Index { get; set; }
        public int p83ID { get; set; }
        public int p91ID { get; set; }
        public int j02ID_Owner { get; set; }
       
        public string p84TextA { get; set; }
        public string p84TextB { get; set; }
        public DateTime p84Date { get; set; }
        public string p84Code { get; set; }

        public string p83Name { get; }
        public string p91Code { get; }
        public string Owner { get; }
        public int p28ID { get; }
        public int p92ID { get; }
        public int x31ID_Index1 { get; }
        public int x31ID_Index2 { get; }
        public int x31ID_Index3 { get; }
        public int j61ID_Index1 { get; }
        public int j61ID_Index2 { get; }
        public int j61ID_Index3 { get; }
        public int p83Days_Index1 { get; }
        public int p83Days_Index2 { get; }
        public int p83Days_Index3 { get; }
        public int j27ID { get; }
        public string j27Code { get; }

        public string p91CodeWithAmount
        {
            get
            {
                return $"{this.p91Code}: {BO.Code.Bas.Number2String(this.p84AmountDebt)} {this.j27Code}";
            }
        }
    }
}
