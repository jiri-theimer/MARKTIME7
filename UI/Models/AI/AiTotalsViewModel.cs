namespace UI.Models.AI
{
    public class AiTotalsViewModel : BaseViewModel
    {
        public string CatalogJson { get; set; }
        public string PeriodsJson { get; set; }
        public string StatesJson { get; set; }
        public string TabsJson { get; set; }
        /// <summary>Název výchozí (systémové) šablony uživatele – pro tlačítko „Přepsat výchozí šablonu“.</summary>
        public string DefaultTemplateName { get; set; }
    }
}
