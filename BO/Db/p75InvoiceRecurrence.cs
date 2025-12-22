

namespace BO
{
    public class p75InvoiceRecurrence:BaseBO
    {
        public int p41ID { get; set; }
        public int p28ID { get; set; }
        public BO.Code.RecurrenceTypeENUM p75RecurrenceType { get; set; }
       
        public int j02ID_Owner { get; set; }
        public string p75Name { get; set; }
        
        public int p75Ordinary { get; set; }
        public bool p75IsDraft { get; set; }
        public DateTime? p75BaseDateStart { get; set; }
        public DateTime? p75BaseDateEnd { get; set; }
        public int p75Generate_DaysToBase_D { get; set; }
        
        public int p75DateSupplyFlag { get; set; }  //1:datum RD,2:konec předchozího měsíce

        public int p75DateMaturityDaysAfter { get; set; }
        public string p75InvoiceText { get; set; }


        public string Owner { get; }

        
        public int p75PeriodFlag { get; set; }  //0:bez období, 1:měsíc,2:kvartál,3:rok


    }
}
