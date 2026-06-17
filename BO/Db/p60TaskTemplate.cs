

namespace BO
{
    public class p60TaskTemplate:BaseBO
    {
        public int p57ID { get; set; }
        public int p41ID { get; set; }
      
        public int j02ID_Owner { get; set; }
        public bool p60IsPublic { get; set; }
        public int p55ID { get; set; }
        public int p15ID { get; set; }
        public string p60Name { get; set; }
        public string p60Notepad { get; set; }
        public int x04ID { get; set; }

      
        public string p60PlanFrom_Unit { get; set; }
        public int p60PlanFrom_UC { get; set; }
        public int p60PlanUntil_UC { get; set; }
        public string p60PlanUntil_Unit { get; set; }

        public int p60Ordinary { get; set; }
        public double p60Plan_Hours { get; set; }
        public double p60Plan_Expenses { get; set; }
        public double p60Plan_Revenue { get; set; }
        public double p60Plan_Internal_Fee { get; set; }
        public p56PlanFlagEnum p60PlanFlag { get; set; }


        public bool p60IsStopNotify { get; set; }

        public string Owner { get; }

        public string p57Name { get; }
        public string p55Name { get; }

      
        public string p41Name { get; }
        public string p41Code { get; }
        public string ProjectClient { get; }

        public string ProjectCodeAndName
        {
            get
            {
                return $"{this.p41Code} {this.p41Name}";
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
                    return this.p60Name;
                }

                if (this.ProjectClient != null)
                    return this.p60Name + " [" + this.ProjectClient + " - " + this.p41Name + "]";
                else
                    return $"{this.p60Name} [{this.p41Name}]";
            }
        }
        
    }
}
