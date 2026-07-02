namespace MO.Models
{
    public class UkolyListViewModel : BaseViewModel
    {
        // ===== Filtr =====
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        /// <summary>0 = otevřené (výchozí), 1 = vše, 2 = dokončené</summary>
        public int StateFilter { get; set; }

        // ===== Výsledek =====
        public List<BO.p56Task> Entries { get; set; } = new List<BO.p56Task>();
        public int TotalCount { get; set; }

        /// <summary>true, pokud byl výsledek useknutý na maximální počet záznamů (příliš široký filtr)</summary>
        public bool IsTruncated { get; set; }
    }
}
