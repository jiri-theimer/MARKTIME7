

namespace BO
{
    public class p07ProjectLevel:BaseBO
    {
        public int x01ID { get; set; }
        public int p07Level { get; set; }
        public string p07Name { get; set; }
        public string p07NamePlural { get; set; }
        public string p07NameInflection { get; set; }
        public string x01LoginDomain { get; }
        public string TreeName { get
            {
                //return new string('-', this.p07Level)+"#"+ this.p07Level.ToString() + " " + this.p07Name;
                return $"{this.p07Name} (L{this.p07Level})";
                //return this.p07Name + " #" + this.p07Level.ToString();


            }
        }
    }
}
