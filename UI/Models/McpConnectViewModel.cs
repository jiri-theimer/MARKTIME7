using BL.Mcp;

namespace UI.Models
{
    /// <summary>Stránka souhlasu: AI klient žádá o přístup k datům přihlášeného uživatele.</summary>
    public class McpConsentViewModel
    {
        public string ClientName { get; set; }
        /// <summary>Server, na který se uživatel po rozhodnutí vrátí – jediný údaj o klientovi, který nelze podvrhnout.</summary>
        public string RedirectHost { get; set; }
        public bool IsLocalClient { get; set; }
        public string UserName { get; set; }
        public string Login { get; set; }
        public string LicenseName { get; set; }
        public bool IsAllowed { get; set; }
        public int Days { get; set; }
        public List<int> DayChoices { get; set; }
        /// <summary>Parametry žádosti klienta – stránka je pošle zpět s rozhodnutím.</summary>
        public Dictionary<string, string> Query { get; set; }
    }

    /// <summary>Přehled připojených AI klientů uživatele.</summary>
    public class McpConnectViewModel : BaseViewModel
    {
        public bool IsEnabled { get; set; }
        public bool IsAllowed { get; set; }
        public string McpUrl { get; set; }
        public DateTime Now { get; set; }
        public List<McpGrant> Active { get; set; } = new();
        public List<McpGrant> Ended { get; set; } = new();
    }
}
