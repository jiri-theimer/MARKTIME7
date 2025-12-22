using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class CapacityProjectGroupBy
    {
        public int p41ID { get; set; }
        public string Project { get; set; }
        public DateTime D1 { get; set; }      //pro agregaci měsíc je první den v měsíci, pro agregaci rok je první den v roce apod.
        public double HoursFa { get; set; }
        public double HoursNeFa { get; set; }
        public double HoursTotal { get; set; }
        public int Krat { get; set; }
    }
}
