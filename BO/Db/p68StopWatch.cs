
namespace BO
{
    public class p68StopWatch:BaseBO
    {
        public int j02ID { get; set; }
        public int p41ID { get; set; }
        public int p56ID { get; set; }
        public int p32ID { get; set; }
        public int p68Duration { get; set; }
        public DateTime? p68LastStart { get; set; }
        public DateTime? p68LastEnd { get; set; }
        public bool p68IsRunning { get; set; }
        public string p68Text { get; set; }
        public int p68Ordinary { get; set; }

        public string p41Name { get; }
        public string p28Name { get; }
        public string p32Name { get; }
    }
}
