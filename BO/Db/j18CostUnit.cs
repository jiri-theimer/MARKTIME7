

namespace BO
{
    public class j18CostUnit:BaseBO
    {
        public int x01ID { get; set; }
        public string j18Name { get; set; }
        public string j18Code { get; set; }
        
        public int j18Ordinary { get; set; }
        public string j18CountryCode { get; set; }  //pro rozlišení dph sazby projekty
        
    }
}
