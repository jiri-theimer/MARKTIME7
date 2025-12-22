

namespace BO
{
    public class p41MyTop10
    {
        public int p41ID { get; set; }
        public string Project { get; set; }
        public DateTime LastDate { get; set; }
    }

    public class p41BoxRec
    {
        public int p41ID { get; set; }
        public int p41ParentID { get; set; }
        public int p07Level { get; set; }
        public int p28ID_Client { get; set; }
        public string Client { get; set; }
        public string Project { get; set; }
        public string p41Code { get; set; }
        public string p42Name { get; set; }
        public int p41TreeIndex { get; set; }
        public int p41TreeLevel { get; set; }
        public int p41TreePrev { get; set; }
        public int p41TreeNext { get; set; }
        public int p41ID_P07Level1 { get; set; }
        public int p41ID_P07Level2 { get; set; }
        public int p41ID_P07Level3 { get; set; }
        public int p41ID_P07Level4 { get; set; }
        public DateTime p41ValidUntil { get; set; }
    }
}
