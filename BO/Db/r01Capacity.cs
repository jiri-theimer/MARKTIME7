using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class r01Capacity:BaseBO
    {
        public int r02ID { get; set; }
        public int j02ID { get; set; }
        public int p41ID { get; set; }
        public string r01Text { get; set; }
        public double r01HoursFa { get; set; }
        public double r01HoursNeFa { get; set; }
        public double r01HoursTotal { get; set; }
       public double r01UnitFa { get; set; }
        public double r01UnitNeFa { get; set; }
        public double r01UnitTotal { get; set; }
        public DateTime r01Start { get; set; }
        public DateTime r01End { get; set; }
        public int r01DaysAll { get; }
        public int r01DaysPlan { get; }
        public bool r01IsIncludeWeekend { get; set; }
        public string r01Color { get; set; }
        public string Person { get; }
        public string Person_Inicialy { get; set; }
        public string PersonAsc { get; set; }
        public string Project { get; }
        public string Client { get; }
        public DateTime? p41PlanFrom { get; }
        public DateTime? p41PlanUntil { get; }
        public string r02Name { get; }

        public bool Rezerva { get; set; }
        
    }

   
}
