using System;

namespace BO
{
    public class p36LockPeriod:BaseBO
    {
        public int x01ID { get; set; }
        public int j02ID { get; set; }
        public int j11ID { get; set; }
        public DateTime? p36DateFrom { get; set; }
        public DateTime? p36DateUntil { get; set; }
        public bool p36IsAllSheets { get; set; }
        public bool p36IsAllPersons { get; set; }

        public string j11Name { get; set; }
        public string Person { get; set; }

        
       
    }
}
