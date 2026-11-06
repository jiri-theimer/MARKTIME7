namespace UI.Models.AI
{
    /// <summary>Nastavení společného chatu AI agenta (partial Views/Shared/AI/_AiChat.cshtml).</summary>
    public class AiChatSettings
    {
        /// <summary>Úvodní zpráva agenta (co umí, příklady dotazů).</summary>
        public string Intro { get; set; }
        public string Placeholder { get; set; }
        /// <summary>Text zobrazený během čekání na odpověď.</summary>
        public string Thinking { get; set; }
    }
}
