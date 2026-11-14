namespace MO.Models
{
    /// <summary>
    /// Společný základ formulářů úkonu (hodiny, peníze, kusovník): sešit, projekt, aktivita, úkol, popis, volná pole
    /// a návrat po uložení. Nabídky plní společné metody UkonController.Load*Core.
    /// </summary>
    public abstract class EntryBaseViewModel : BaseViewModel
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

        // Popis - povinné
        public string Description { get; set; }

        // Volná pole (freefields) - plní server, z formuláře se sbírají ručně
        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        public FreeFieldsViewModel ff1 { get; set; } = new FreeFieldsViewModel();

        // Záznam nejde upravit (schválený, vyúčtovaný, uzamčené období...)
        public bool IsReadOnly { get; set; }
        public string RecordStateLabel { get; set; }

        // Návrat po uložení / smazání
        public string Ret { get; set; }
        public string RetD { get; set; }
    }
}
