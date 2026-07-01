namespace MO.Models
{
    public class EntryHoursViewModel : BaseViewModel
    {
        public int pid { get; set; }                     // 0 = nový záznam
        public DateTime Date { get; set; }

        // Sešit (p34) - PRVNÍ volba, určuje typ úkonu (p33id) a nabídku aktivit
        public int p34ID { get; set; }
        public IEnumerable<ComboItem> SesitComboItems { get; set; } = new List<ComboItem>();
        public string SelectedSesitText { get; set; }
        // Režim zadávání aktivity dle vybraného sešitu (1=nezadává, 2=nepovinná, 3=povinná)
        public int ActivityEntryFlag { get; set; }

        // Projekt (p41) - povinné
        public int p41ID { get; set; }
        public IEnumerable<ComboItem> ProjectComboItems { get; set; } = new List<ComboItem>();
        public string SelectedProjectText { get; set; }

        // Aktivita (p32) - dle sešitu + projektu
        public int p32ID { get; set; }
        public IEnumerable<ComboItem> ActivityComboItems { get; set; } = new List<ComboItem>();
        public string SelectedActivityText { get; set; }

        // Úkol (p56) - volitelné
        public int p56ID { get; set; }
        public IEnumerable<BO.p56Task> TaskList { get; set; } = new List<BO.p56Task>();
        public string SelectedTaskText { get; set; }

        // Hodiny - povinné
        public string Hours { get; set; }

        // Čas od / do - volitelné
        public string TimeFrom { get; set; }
        public string TimeUntil { get; set; }

        // Popis - povinné
        public string Description { get; set; }

        // K hodinám vykázat i kusovníkové úkony ("navíc kusovník")
        public bool IsOfferNavicKusovnik { get; set; }
        public bool IsNavicKusovnik { get; set; }
        public int p34ID_Kusovnik { get; set; }
        public IEnumerable<ComboItem> KusovnikSesitComboItems { get; set; } = new List<ComboItem>();
        public IEnumerable<ComboItem> KusovnikActivityComboItems { get; set; } = new List<ComboItem>();
        public List<KusovnikRowViewModel> KusovnikRows { get; set; } = new List<KusovnikRowViewModel>();

        /// <summary>Již uložené kusovníkové úkony navázané na tento hodinový úkon (p31MasterID) - jen pro čtení.</summary>
        public IEnumerable<BO.p31Worksheet> ExistingKusovnikEntries { get; set; } = new List<BO.p31Worksheet>();



        /// <summary>Uživatelská pole (freefields) - plní se v controlleru, ne z formuláře</summary>
        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        public FreeFieldsViewModel ff1 { get; set; } = new FreeFieldsViewModel();

        /// <summary>Záznam je pouze pro čtení (nemá OwnerAccess nebo stav != Editing)</summary>
        public bool IsReadOnly { get; set; }

        /// <summary>Popis stavu záznamu pro zobrazení v ReadOnly hlavičce</summary>
        public string RecordStateLabel { get; set; }

        /// <summary>Odkud přišel uživatel - "week" nebo null (Day). Řídí kam vede Zpět/Uložit.</summary>
        public string Ret { get; set; }

        /// <summary>Kotevní datum pro návrat (u "week" pondělí daného týdne)</summary>
        public string RetD { get; set; }
    }
}