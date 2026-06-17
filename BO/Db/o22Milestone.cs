using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public enum o22DurationCalcFlagEnum
    {
        Pocitat=1,
        Rucne=2
    }


    public class o22Milestone:BaseBO
    {
        public int o21ID { get; set; }
        public int p41ID { get; set; }
        public string o22Notepad { get; set; }
        public int x04ID { get; set; }
        public bool o22IsAllDay { get; set; }
        public DateTime? o22PlanFrom { get; set; }
        public DateTime? o22PlanUntil { get; set; }
        public int o22DurationCount { get; set; }
        public string o22DurationUnit { get; set; }     //hodnoty: d/w/m/y
        public o22DurationCalcFlagEnum o22DurationCalcFlag { get; set; } = o22DurationCalcFlagEnum.Pocitat;

        public string o22Name { get; set; }
        public string o22Location { get; set; }

        public string o22ExternalCode { get; set; }

        public int b05RecordPid { get; }
        public string b05RecordEntity { get; }
        public int j02ID_Owner { get; set; }
        public string Owner { get; }
        public string o21Name { get; set; } //kvůli dayline je nutný i SET
        public BO.o21TypeFlagEnum o21TypeFlag { get; set; } //kvůli dayline je nutný i SET
        public string o21Color { get; set; } //kvůli dayline je nutný i SET
        public string p41Name { get; }
        public string p41Code { get; }
        public string ProjectClient { get; }

        public string Duration
        {
            get
            {
                if (this.o22DurationCount <= 0) return null;

                switch (this.o22DurationUnit)
                {
                    case "d":
                        
                        return this.o22DurationCount.ToString() + " (dny)";
                    case "e":
                        return this.o22DurationCount.ToString() + " (prac.dny)";
                    case "w":
                        return this.o22DurationCount.ToString() + " (týdny)";
                    case "m":
                        return this.o22DurationCount.ToString() + " (měsíce)";
                    case "y":
                        return this.o22DurationCount.ToString() + " (roky)";
                    default:
                        return null;
                }
            }
        }
        public string ProjectCodeAndName
        {
            get
            {
                return this.p41Code + " - " + this.p41Name;
            }
        }
        public string ProjectWithClient
        {
            get
            {
                if (this.ProjectClient == null)
                    return this.p41Name;
                else
                    return this.ProjectClient + " - " + this.p41Name;
            }
        }



        public string FullName
        {
            get
            {
                if (this.ProjectClient != null)
                    return this.o22Name + " [" + this.ProjectClient + " - " + this.p41Name + "]";
                else
                    return this.o22Name + " [" + this.p41Name + "]";
            }
        }

        public string ForeColor
        {
            get
            {
                if (this.o21Color == null)
                {
                    return "steelblue";
                }
                else
                {
                    return this.o21Color;
                }
            }
        }
    }
}
