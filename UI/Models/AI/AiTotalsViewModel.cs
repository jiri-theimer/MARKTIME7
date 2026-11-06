namespace UI.Models.AI
{
    public class AiTotalsViewModel : BaseViewModel
    {
        public string CatalogJson { get; set; }
        public string PeriodsJson { get; set; }
        public string StatesJson { get; set; }
        public string TabsJson { get; set; }
        public string FiltersJson { get; set; }
        /// <summary>Šablona, kterou přepíše tlačítko „Přepsat“ (předaná, pokud ji uživatel smí upravit, jinak výchozí).</summary>
        public int OverwriteJ79ID { get; set; }
        public string OverwriteName { get; set; }
        /// <summary>Návrh načtený z předané šablony (JSON) – předvyplní formulář.</summary>
        public string InitialJson { get; set; } = "null";
        public string InitialWarningsJson { get; set; } = "[]";
    }
}
