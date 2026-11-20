namespace BO
{
    /// <summary>Kontrola fakturační sazby úkonu: sazba uložená v úkonu proti sazbě, kterou pro úkon dnes určuje ceník.</summary>
    public class p31RateCheck
    {
        public int p31ID { get; set; }
        public DateTime p31Date { get; set; }
        public string Person { get; set; }
        public string Project { get; set; }
        public string p32Name { get; set; }
        public double Value { get; set; }           //hodiny nebo počet kusů
        public double RateStored { get; set; }
        public double RateExpected { get; set; }
        public string j27Code_Stored { get; set; }
        public string j27Code_Expected { get; set; }
        public double AmountStored { get; set; }
        public double AmountExpected { get; set; }
        /// <summary>Aktivita s ručně zadávaným honorářem: sazbu neurčuje ceník, úkon se nekontroluje ani nepřepočítává.</summary>
        public bool IsManualFee { get; set; }
        /// <summary>Schválený úkon: porovnává se schválená fakturační sazba (ta jde na fakturu), ne původní.</summary>
        public bool IsApproved { get; set; }
        public bool IsDifferent { get; set; }
        public double AmountDiff => AmountExpected - AmountStored;
    }
}
