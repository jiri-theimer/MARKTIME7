
namespace BO
{
    public class p78UpominkaSdruzena:BaseBO
    {
        public int p28ID { get; set; }
        public int j02ID_Owner { get; set; }
        public string p78Code { get; set; }
        public string p78Name { get; set; }
        public string p78TextA { get; set; }
        public string p78TextB { get; set; }
        public DateTime p78Date { get; set; }

        public string p28Name { get; }
        public string Owner { get; }
    }
}
