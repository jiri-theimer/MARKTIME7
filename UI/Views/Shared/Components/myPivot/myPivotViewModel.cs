namespace UI.Views.Shared.Components.myPivot
{
    /// <summary>Nastavení MARKTIME pivotu (myPivot) na stránce.</summary>
    public class myPivotViewModel
    {
        /// <summary>id elementu kontejneru; instance pivotu je v JS proměnné stejného jména jako JsVariable.</summary>
        public string ElementId { get; set; } = "mtPivot";
        /// <summary>Globální JS proměnná s instancí MtPivot (např. "pivot1").</summary>
        public string JsVariable { get; set; } = "pivot1";
        /// <summary>URL s daty (JSON z myPivotData.ToJson).</summary>
        public string DataUrl { get; set; }
        /// <summary>Rozložení ve formátu reportu WebDataRocks (JSON) nebo null.</summary>
        public string ReportJson { get; set; }
        /// <summary>Klíč v localStorage: rozložení uživatele má přednost před ReportJson a ukládá se při každé změně (null = neukládat).</summary>
        public string StorageKey { get; set; }
        /// <summary>Titulek pro export a tisk.</summary>
        public string Title { get; set; }
        /// <summary>Název JS funkce volané po změně rozložení: function(report) – např. uložení do localStorage.</summary>
        public string OnReportChange { get; set; }
        /// <summary>Výška tabulky v px (0 = dopočítat do konce okna).</summary>
        public int Height { get; set; }
    }
}
