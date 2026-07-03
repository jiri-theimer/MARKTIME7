namespace MO.Models
{
    public class StatsViewModel : BaseViewModel
    {
        public DateTime d1 { get; set; }
        public DateTime d2 { get; set; }
        public string PeriodLabel { get; set; }

        // Vykázané hodiny
        public double Hodiny { get; set; }
        public double HodinyFa { get; set; }

        // Rozpracované (čeká na schválení) - stav Nic, ještě nefakturováno
        public double HodinyWip { get; set; }
        public double HodinyFaWip { get; set; }
        public double HodinyNeFaWip { get; set; }

        // Schváleno a čeká na vyúčtování
        public double Hodiny4Approve { get; set; }

        // Vyúčtováno
        public double HodinyVyfa4 { get; set; }        // sazbou
        public double HodinyVyfa6 { get; set; }        // zahrnuto do paušálu
        public double HodinyVyfa23 { get; set; }        // odepsáno
        public bool HasRatesAccess { get; set; }

        // Fond a utilizace
        public double Fond { get; set; }
        public double UtilTotal { get; set; }
        public double UtilFa { get; set; }

        // Top 15 naposledy vykazovaných projektů
        public IEnumerable<BO.p41MyTop10> Top15Projects { get; set; } = new List<BO.p41MyTop10>();
    }
}