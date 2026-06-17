using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace BO
{
    public class ThePeriod
    {
        public int pid { get; set; }        
        public string PeriodName { get; set; }
        public string PeriodInterval { get; set; }
        public DateTime? d1 { get; set; }
        public DateTime? d2 { get; set; }

        public bool IsUserPeriod { get; set; }
        

        public string Header
        {
            get
            {
                if (string.IsNullOrEmpty(this.PeriodInterval))
                {
                    return this.PeriodName;
                }
                else
                {
                    return this.PeriodName + ": " + this.PeriodInterval;
                }
               
            }
        }

        public int OrdinaryIndex
        {
            get
            {
                if (this.pid == 0 || this.pid == 1)
                {
                    return 0;   //bez barvy
                }
                if (this.pid > 60)
                {
                    return 1;   //vlastní období
                }
                if (DateTime.Today > this.d2)   //minulost
                {
                    return 2;
                }

                if (this.d1 > DateTime.Today)   //budoucnost
                {
                    return 10;
                }

                return 5;   //přítomnost
            }
        }

        public string Color
        {
            get
            {
                if (this.pid == 0)
                {
                    return "#FFFFFF";   //bez barvy
                }
                if (this.pid == 1)
                {
                    return "#fcf080";   //ručně
                }
                if (this.pid > 60)
                {
                    return "#98FB98";   //vlastní období
                }
                if (DateTime.Today>this.d2)   //minulost
                {
                    return "#E5E5E5";
                }

                if (this.d1>DateTime.Today)   //budoucnost
                {
                    return "#e6f2ff";
                }

                return "#FFFFFF";   //přítomnost
            }
        }
    }
}
