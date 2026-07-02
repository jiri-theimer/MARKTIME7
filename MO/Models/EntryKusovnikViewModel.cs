namespace MO.Models
{
    public class EntryKusovnikViewModel : BaseViewModel
    {
        public int pid { get; set; }                     // 0 = nový záznam
        public DateTime Date { get; set; }

        // Sešit (p34) - předvybraný, určuje aktivity a je pořád typu Kusovník
        public int p34ID { get; set; }
        public IEnumerable<ComboItem> SesitComboItems { get; set; } = new List<ComboItem>();
        public string SelectedSesitText { get; set; }
        public int ActivityEntryFlag { get; set; }

        // Projekt (p41) - povinné
        public int p41ID { get; set; }
        public IEnumerable<ComboItem> ProjectComboItems { get; set; } = new List<ComboItem>();
        public string SelectedProjectText { get; set; }

        // Aktivita (p32) - dle sešitu
        public int p32ID { get; set; }
        public IEnumerable<ComboItem> ActivityComboItems { get; set; } = new List<ComboItem>();
        public string SelectedActivityText { get; set; }

        // Úkol (p56) - volitelné
        public int p56ID { get; set; }
        public IEnumerable<BO.p56Task> TaskList { get; set; } = new List<BO.p56Task>();
        public string SelectedTaskText { get; set; }

        // Počet - jediná "hodnota" kusovníkového úkonu
        public string Pocet { get; set; }

        // Popis - povinné
        public string Description { get; set; }

        /// <summary>Uživatelská pole (freefields) - plní se v controlleru, ne z formuláře</summary>
        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        public FreeFieldsViewModel ff1 { get; set; } = new FreeFieldsViewModel();

        public bool IsReadOnly { get; set; }
        public string RecordStateLabel { get; set; }

        public string Ret { get; set; }
        public string RetD { get; set; }
    }
}
