using System;

namespace BO
{
    public class p52PriceList_Item:BaseBO
    {
        public int p51ID { get; set; }
        public int j02ID { get; set; }
        public int j07ID { get; set; }
        public int p34ID { get; set; }
        public int p32ID { get; set; }
        public string p52Name { get; set; }
        public double p52Rate { get; set; }
        public bool p52IsPlusAllTimeSheets { get; set; }
        public bool p52IsMaster { get; set; }
        public string Person { get; }
        public string j07Name { get; }
        public string p34Name { get; }
        public string p32Name { get; }

      
        
    }
}
