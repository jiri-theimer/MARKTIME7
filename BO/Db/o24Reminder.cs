

namespace BO
{
    public enum o24MediumFlagEnum
    {
        Email = 1,
        Sms = 2,
        EmailPlusSms=3
    }
    public class o24Reminder : BaseBO
    {
        public int o24RecordPid { get; set; }
        public string o24RecordEntity { get; set; }
        public o24MediumFlagEnum o24MediumFlag { get; set; }
        public string o24Memo { get; set; }
        public string o24Unit { get; set; }
        public int o24Count { get; set; }
        public int j02ID { get; set; }
        public int j11ID { get; set; }
        public int p28ID { get; set; }
        public int p24ID { get; set; }
        public int x67ID { get; set; }
        public int x01ID { get; set; }
        public DateTime? o24StaticDate { get; set; }
        public DateTime? o24RecordDate { get; set; }
        public DateTime? o24DatetimeProcessed { get; set; }
        public string o24Flag { get; set; }

        public string Person { get; }
        public string j11Name { get; }
        public string p28Name { get; }
        public string p24Name { get; }
        public string x67Name { get; }

        public string CalcReminderDateString()
        {
            return BO.Code.Bas.ObjectDateTime2String(CalcReminderDate());
        }

        public DateTime? CalcReminderDate()
        {
            if (this.o24StaticDate != null)
            {
                return Convert.ToDateTime(this.o24StaticDate);
            }

            if (this.o24RecordDate != null)
            {
                DateTime d = Convert.ToDateTime(this.o24RecordDate);
                switch (this.o24Unit)
                {                   
                    case "h":
                        return d.AddHours(this.o24Count);
                    case "m":
                        return d.AddMonths(this.o24Count);
                    case "y":
                        return d.AddYears(this.o24Count);
                    default:
                        return d.AddDays(this.o24Count);
                }
            }

            return null;

        }
    }
}
