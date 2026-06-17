

namespace BO
{
    public enum p56PlanFlagEnum
    {
        _None=0,
        ZastropovatHodiny=1,
        ZastropovatVydaje=2,
        ZastropovatHodinyVydaje=3
    }
    public enum p56OutlookStatusEnum
    {        
        Nezahajeno=1,
        Probiha=2,
        Dokonceno=3,
        Ceka=4,
        Odlozeno=5
    }
    public class p56Task:BaseBO
    {        
        public int p57ID { get; set; }
        public int p41ID { get; set; }
        public int b02ID { get; set; }
        public int j02ID_Owner { get; set; }
        public int p55ID { get; set; }
        public int p15ID { get; set; }
        public string p56Name { get; set; }        
        public string p56Code { get; set; }
        public string p56Notepad { get; set; }
        public int x04ID { get; set; }

        public p56OutlookStatusEnum p56OutlookStatus { get; set; }
        public DateTime? p56PlanFrom { get; set; }
        public DateTime? p56PlanUntil { get; set; }

        public int p56Ordinary { get; set; }
        public double p56Plan_Hours { get; set; }
        public double p56Plan_Expenses { get; set; }
        public double p56Plan_Revenue { get; set; }
        public double p56Plan_Internal_Fee { get; set; }
        public p56PlanFlagEnum p56PlanFlag { get; set; } 
       

        public string p56ExternalCode { get; set; }
        public bool p56IsStopNotify { get; set; }
        public int p60ID { get; set; }

        public string b02Name { get; set; } //musí být SET kvůli zděděné třídě p56TaskDayline
        public string b02Color { get; set; }    //musí být SET kvůli zděděné třídě p56TaskDayline
        public int b01ID { get; }
        public string Owner { get; }
       
        public string p57Name { get; }
        public string p55Name { get; }
        public int b05RecordPid { get; }
        public string b05RecordEntity { get; }

        public int x38ID { get; }
        public string p41Name { get; }       
        public string p41Code { get; }
        public string ProjectClient { get; }

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
                    return $"{this.ProjectClient} - {this.p41Name}";
            }
        }

        
        
        public string FullName
        {
            get
            {
                if (this.p41ID == 0)
                {
                    return this.p56Name;
                }

                if (this.ProjectClient != null)
                    return this.p56Name + " [" + this.ProjectClient + " - " + this.p41Name + "]";
                else
                    return this.p56Name + " [" + this.p41Name + "]";
            }
        }
        public string NameWithTypeAndCode
        {
            get
            {
                return this.p57Name + ": " + this.p56Name + " (" + this.p56Code + ")";
            }
        }

        public string ForeColor
        {
            get
            {
                if (this.b02Color == null)
                {
                    return "black";
                }
                else
                {
                    return this.b02Color;
                }
            }
        }

    }
}
