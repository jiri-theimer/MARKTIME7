namespace MO.Models
{
    public class EntryViewModel : BaseViewModel
    {
        public int pid { get; set; }                     // 0 = nový záznam
        public DateTime Date { get; set; }

        // Projekt - povinné
        public int p41ID { get; set; }
        public IEnumerable<ComboItem> ProjectComboItems { get; set; } = new List<ComboItem>();
        public string SelectedProjectText { get; set; }

        // Úkol - volitelné (po výběru projektu)
        public int p56ID { get; set; }
        public IEnumerable<BO.p56Task> TaskList { get; set; } = new List<BO.p56Task>();

        // Hodiny - povinné (text input: "1.5", "1,5", "01:30")
        public string Hours { get; set; }

        // Čas od / do - volitelné (dobrovolné u některých)
        public string TimeFrom { get; set; }
        public string TimeUntil { get; set; }

        // Popis - povinné
        public string Description { get; set; }

        
    }
}
