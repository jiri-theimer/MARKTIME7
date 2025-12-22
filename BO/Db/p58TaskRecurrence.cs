
namespace BO
{
    public class p58TaskRecurrence:BaseBO
    {
        public int p57ID { get; set; }
        public BO.Code.RecurrenceTypeENUM p58RecurrenceType { get; set; }
        public int p41ID { get; set; }       
        public int j02ID_Owner { get; set; }

        public string p58Name { get; set; }
        public string p58Notepad { get; set; }
        public int x04ID { get; set; }
        public int p58Ordinary { get; set; }

        public DateTime? p58BaseDateStart { get; set; }
        public DateTime? p58BaseDateEnd { get; set; }
        public int p58Generate_DaysToBase_D { get; set; }        
        public int p58Generate_DaysToBase_M { get; set; }


        public bool p58IsPlanUntil { get; set; }
        public int p58PlanUntil_D2Base { get; set; }
        public int p58PlanUntil_M2Base { get; set; }


        public bool p58IsPlanFrom { get; set; }
        public int p58PlanFrom_D2Base { get; set; }
        public int p58PlanFrom_M2Base { get; set; }


        
        public double p58Plan_Hours { get; set; }
        public double p58Plan_Expenses { get; set; }
        public double p58Plan_Revenue { get; set; }
        public p56PlanFlagEnum p58PlanFlag { get; set; }
      
        public bool p58IsStopNotify { get; set; }

        
        public string Owner { get; }

        public string p57Name { get; }

        public string p41Name { get; }
        public string p41Code { get; }
        public string ProjectClient { get; }
        public int b05RecordPid { get; }
        public string b05RecordEntity { get; }




    }
}
