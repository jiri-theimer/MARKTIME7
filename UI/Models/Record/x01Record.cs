namespace UI.Models.Record
{
    public class x01Record:BaseRecordViewModel
    {
        public BO.x01License Rec { get; set; }
        public string ComboX04 { get; set; }

        /// <summary>Nově zadaný API klíč AI; prázdné = ponechat uložený (uložený klíč se do stránky neposílá).</summary>
        public string AiApiKeyNew { get; set; }
        /// <summary>Odstranit uložený API klíč AI.</summary>
        public bool AiApiKeyClear { get; set; }
        /// <summary>Konec uloženého klíče pro zobrazení („…AbCd“), null = klíč není uložen.</summary>
        public string AiApiKeyMasked { get; set; }
        /// <summary>Globální konfigurace AI ze serveru (appsettings) – pro nápovědu a výchozí hodnoty.</summary>
        public BL.AI.Core.AiOptions AiGlobal { get; set; }
        /// <summary>Výsledek testu připojení k AI (null = test neproběhl).</summary>
        public string AiTestResult { get; set; }
        public bool AiTestOk { get; set; }
        
    }
}
