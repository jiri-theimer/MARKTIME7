using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class r05CapacityUnit
    {
        public int r05ID { get; set; }
        public int r01ID { get; set; }
        public DateTime r05Date { get; set; }
        public double r05HoursFa { get; set; }
        public double r05HoursNeFa { get; set; }
        public double r05HoursTotal { get; set; }
        public DateTime r05Week { get; set; }
        public DateTime r05Month { get; set; }
        public DateTime r05Year { get; set; }
    }

    
}
