

namespace BO
{
    public class p39WorkSheet_Recurrence_Plan
    {
        public int p39ID { get; set; }
        public int p40ID { get; set; }        
        public string p39Text { get; set; }
        public string p39TextInternal { get; set; }
        public DateTime p39Date { get; set; }
        public DateTime p39DateCreate { get; set; }
        public int p31ID_NewInstance { get; set; }
        public string p39ErrorMessage_NewInstance { get; set; }

        public DateTime? p31Date { get; }
        public DateTime? p31DateInsert { get; }
        public string p31Text { get; set; }
        public double p31Value_Orig { get; }

        public double p40Value { get; }
        public string p41Name { get; }
        public string Client { get; }
    }
}
