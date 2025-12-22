using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public enum CapacityGroupByEnum
    {
        Day=1,
        Week=2,
        Month=3,
        Year=4
    }
    public class CapacityResourceGroupBy
    {
        public int j02ID { get; set; }
        public string Person { get; set; }
        public DateTime D1 { get; set; }      //pro agregaci měsíc je první den v měsíci, pro agregaci rok je první den v roce apod.
        public double HoursFa { get; set; }
        public double HoursNeFa { get; set; }
        public double HoursTotal { get; set; }
        public int Krat { get; set; }
    }
}
