
namespace BO
{
    //public enum p40RecurrenceTypeENUM
    //{
    //    Day = 1,
    //    Week = 2,
    //    Month = 3,
    //    Quarter = 4,
    //    Year = 5
    //}
    public class p40WorkSheet_Recurrence : BaseBO
    {
        public BO.Code.RecurrenceTypeENUM p40RecurrenceType { get; set; }
        public int p41ID { get; set; }
        public int j02ID { get; set; }
        public int p34ID { get; set; }
        public int p32ID { get; set; }
        public int j27ID { get; set; }
        public x15IdEnum x15ID { get; set; }
        public string p40Name { get; set; }
        public string p40Text { get; set; }
        public string p40TextInternal { get; set; }
        public DateTime? p40FirstSupplyDate { get; set; }
        public int p40GenerateDayAfterSupply { get; set; }
        public DateTime? p40LastSupplyDate { get; set; }
        public double p40Value { get; set; }
        public double p40FreeHours { get; set; }
        public double Cerpano_Hodiny { get; }
        public double p40FreeFee { get; set; }
        public double Cerpano_Honorar { get; }

        public string j27Code { get; }
        public string j02Name { get; }
        public string p34Name { get; }
        public string p32Name { get; }
        public string Project { get; }
        public string Client { get; }
        public int p33ID { get; set; }
        

        public string NamePlusCerpano
        {
            get
            {
                if (this.p40FreeHours > 0)
                {
                    return $"{this.p40Name} ** {p40FreeHours}/{BO.Code.Time.GetDecTimeFromSeconds(this.Cerpano_Hodiny*60*60)}";
                }
                if (this.p40FreeFee > 0)
                {
                    return $"{this.p40Name} ** {BO.Code.Bas.Number2String(p40FreeFee)}/{BO.Code.Bas.Number2String(this.Cerpano_Honorar)}";
                }

                return this.p40Name;
            }
        }

        public string RecurrenceAlias
        {
            get
            {
                switch (this.p40RecurrenceType)
                {
                    case Code.RecurrenceTypeENUM.Month: return "Měsíční";
                    case Code.RecurrenceTypeENUM.Year: return "Roční";
                    case Code.RecurrenceTypeENUM.Day: return "Denní";
                    case Code.RecurrenceTypeENUM.Week: return "Týdenní";
                    case Code.RecurrenceTypeENUM.Quarter: return "Čtvrtletní";
                }

                return null;
            }
        }
        
    }
}
