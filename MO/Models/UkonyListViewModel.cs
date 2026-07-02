namespace MO.Models
{
    public class UkonyListViewModel : BaseViewModel
    {
        // ===== Filtr =====
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        /// <summary>0 = vše, jinak hodnota BO.myQueryP31.p31statequery (viz UkonyStateFilter)</summary>
        public int StateFilter { get; set; }

        /// <summary>0 = vše, 1 = hodiny (Cas), 2 = peníze, 3 = kusovník</summary>
        public int FormatFilter { get; set; }

        // ===== Výsledek =====
        public List<BO.p31Worksheet> Entries { get; set; } = new List<BO.p31Worksheet>();
        public double TotalHours { get; set; }
        public int TotalCount { get; set; }

        /// <summary>true, pokud byl výsledek useknutý na maximální počet záznamů (příliš široký filtr)</summary>
        public bool IsTruncated { get; set; }
    }
}
