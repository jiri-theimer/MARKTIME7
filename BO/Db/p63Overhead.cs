
namespace BO
{
    public class p63Overhead:BaseBO
    {
        public int x01ID { get; set; }
        public string p63Name { get; set; }
        public int p32ID { get; set; }
        public double p63PercentRate { get; set; }
        public bool p63IsIncludeTime { get; set; }
        public bool p63IsIncludeFees { get; set; }
        public bool p63IsIncludeExpense { get; set; }
        public string p32Name { get; }
        public string NameWithRate {
            get
            {
                return this.p63Name + " [" + this.p63PercentRate.ToString() + "]";
            }
        }
    }
}
