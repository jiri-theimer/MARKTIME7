namespace BO
{
    /// <summary>
    /// Strop hodin osoby na projektu (tabulka r11CapacityLimit). Hlídá se součet r11HoursTotal = Fa + Nefa,
    /// rozdělení na fakturovatelné a nefakturovatelné hodiny je jen orientační (porovnává se se skutečností).
    /// </summary>
    public class r11CapacityLimit : BaseBO
    {
        public int p41ID { get; set; }
        public int j02ID { get; set; }
        public double r11HoursFa { get; set; }
        public double r11HoursNeFa { get; set; }
        public double r11HoursTotal { get; set; }
        public string r11Text { get; set; }

        public string Person { get; }
        public int c21ID { get; }
        public string j02CountryCode { get; }
        public string Project { get; }
        public string p41Code { get; }
        public string Client { get; }
        public DateTime? p41PlanFrom { get; }
        public DateTime? p41PlanUntil { get; }
        public int j02ID_ProjectOwner { get; }
    }

    /// <summary>Vykázané hodiny osoby na projektu (časové úkony), rozdělené podle fakturovatelnosti aktivity.</summary>
    public class r11Actual
    {
        public int p41ID { get; set; }
        public int j02ID { get; set; }
        public double HoursFa { get; set; }
        public double HoursNeFa { get; set; }
    }
}
