namespace UI.Models.AI
{
    public class AiGridViewModel : BaseViewModel
    {
        public int J72ID { get; set; }
        public string CallerPathName { get; set; }
        public string TableName { get; set; }
        public string EntityName { get; set; }
        public bool CanOverwrite { get; set; }
        /// <summary>Kód funkce pro historii dotazů (j98Feature), např. "ai-grid-p41".</summary>
        public string Feature { get; set; }
        public string CatalogJson { get; set; }
        /// <summary>Nabídky pevných filtrů gridu (období, stav, formát, archiv); null položka = filtr v gridu není.</summary>
        public string FixedJson { get; set; } = "{}";
        public string InitialJson { get; set; } = "null";
        public string InitialWarningsJson { get; set; } = "[]";
    }
}
