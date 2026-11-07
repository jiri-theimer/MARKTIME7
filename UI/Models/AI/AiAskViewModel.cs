namespace UI.Models.AI
{
    /// <summary>Okno „Zeptej se MARKTIME“ (AiAskController.Index).</summary>
    public class AiAskViewModel : BaseViewModel
    {
        /// <summary>Kód funkce pro historii dotazů (j98Feature).</summary>
        public string Feature { get; set; }
    }

    /// <summary>Pivot-Explorer nad výsledkem agenta (AiAskController.Pivot).</summary>
    public class AiAskPivotViewModel : BaseViewModel
    {
        public string Guid { get; set; }
        public string Title { get; set; }
        /// <summary>Použité období a filtry.</summary>
        public string Description { get; set; }
        /// <summary>Výchozí rozložení pivotu (formát reportu WebDataRocks).</summary>
        public string ReportJson { get; set; }
        /// <summary>Kliknutí na hodnotu zobrazí zdrojové úkony (jen p31).</summary>
        public bool CanDrill { get; set; }
    }
}
