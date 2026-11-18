namespace BO
{
    public enum p87StatusFlagENUM
    {
        Nesparovano = 0,
        SparovanoAutomaticky = 1,
        SparovanoRucne = 2,
        Ignorovano = 3
    }

    /// <summary>Bankovní transakce načtená z notifikačního e-mailu banky (jeden řádek = jeden pohyb v e-mailu).</summary>
    public class p87BankTransaction : BaseBO
    {
        public int o42ID { get; set; }
        public int o27ID { get; set; }
        public DateTime? p87Date { get; set; }
        public double p87Amount { get; set; }
        public int p87Ordinary { get; set; }
        public string p87Code { get; set; }
        public p87StatusFlagENUM p87StatusFlag { get; set; }
        public int p94ID { get; set; }
        public int p91ID { get; set; }
        public int p90ID { get; set; }
        public int p82ID { get; set; }
        public string p87Message { get; set; }
        public string p87BankAccount { get; set; }

        public string o42Name { get; }
        public string o27MailSubject { get; }
        public string o27MailSenderAddress { get; }
        public string p91Code { get; }
        public string p91Client { get; }
        public double p91Amount_Debt { get; }
        public string p90Code { get; }
        public double p90Amount_Debt { get; }
    }
}
