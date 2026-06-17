

namespace BO
{
    public class p59TaskRecurrence_Plan
    {
        public int p59ID { get; set; }
        public int p58ID { get; set; }
        public string p59Name { get; set; }
        public DateTime? p59BaseDate { get; set; }
        public DateTime? p59PlanFrom { get; set; }
        public DateTime? p59PlanUntil { get; set; }
        public DateTime p59DateCreate { get; set; }
        public int p56ID_NewInstance { get; set; }
        public string p59ErrorMessage_NewInstance { get; set; }

        public string p56Name { get; }
        public DateTime? p56PlanUntil { get; }
        public DateTime? p56DateInsert { get; }
        public string p58Name { get; }
    }
}
