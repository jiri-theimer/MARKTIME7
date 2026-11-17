namespace BO
{
    /// <summary>
    /// Časový blok, do kterého je rozplánovaná část stropu r11 (tabulka r12CapacityPlan).
    /// Hodiny bloku se při zobrazení rozloží do pracovních dnů osoby podle jejího pracovního fondu; rozpad na dny se neukládá.
    /// </summary>
    public class r12CapacityPlan : BaseBO
    {
        public int r11ID { get; set; }
        public DateTime r12Start { get; set; }
        public DateTime r12End { get; set; }
        public double r12Hours { get; set; }
        public string r12Text { get; set; }

        public int p41ID { get; }
        public int j02ID { get; }
    }
}
