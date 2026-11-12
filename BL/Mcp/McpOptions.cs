using Microsoft.Extensions.Configuration;

namespace BL.Mcp
{
    /// <summary>
    /// Nastavení MCP serveru z appsettings.json, sekce "Mcp". Bez sekce je MCP vypnuté (Enabled = false).
    /// <code>
    /// "Mcp": {
    ///   "Enabled": true,
    ///   "ConnectionDays": 90,            // výchozí platnost připojení (uživatel ji volí při povolení přístupu)
    ///   "MaxConnectionDays": 365,        // nejdelší platnost, kterou lze zvolit
    ///   "AccessTokenMinutes": 60,
    ///   "RedirectHosts": [ "claude.ai", "claude.com", "chatgpt.com", "localhost" ],
    ///   "PublicUrl": "https://app.marktime.net"   // jen když aplikace nevidí vlastní veřejnou adresu (reverzní proxy)
    /// }
    /// </code>
    /// </summary>
    public class McpOptions
    {
        public const string ScopeRead = "marktime.read";

        public bool Enabled { get; set; }
        public int ConnectionDays { get; set; } = 90;
        public int MaxConnectionDays { get; set; } = 365;
        public int AccessTokenMinutes { get; set; } = 60;
        /// <summary>Platnost autorizačního kódu (mezi souhlasem uživatele a vyzvednutím tokenu klientem).</summary>
        public int CodeMinutes { get; set; } = 5;
        /// <summary>Jak dlouho po výměně refresh tokenu se ještě přijme ten předchozí (souběžné požadavky klienta).</summary>
        public int RefreshGraceSeconds { get; set; } = 60;
        /// <summary>
        /// Servery, na které smí MARKTIME po přihlášení vrátit uživatele (redirect_uri klienta). "localhost" povoluje i 127.0.0.1 a http
        /// (klienti běžící na počítači uživatele – Claude Code apod.). "*" povolí libovolný https server.
        /// </summary>
        public List<string> RedirectHosts { get; set; } = new() { "claude.ai", "claude.com", "chatgpt.com", "localhost" };
        public string PublicUrl { get; set; }

        public static McpOptions Load(IConfiguration config)
        {
            var o = new McpOptions();
            var sec = config?.GetSection("Mcp");
            if (sec == null || !sec.Exists()) return o;

            o.Enabled = sec.GetValue("Enabled", false);
            o.MaxConnectionDays = Math.Clamp(sec.GetValue("MaxConnectionDays", o.MaxConnectionDays), 1, 3650);
            o.ConnectionDays = Math.Clamp(sec.GetValue("ConnectionDays", o.ConnectionDays), 1, o.MaxConnectionDays);
            o.AccessTokenMinutes = Math.Clamp(sec.GetValue("AccessTokenMinutes", o.AccessTokenMinutes), 5, 24 * 60);
            o.PublicUrl = sec["PublicUrl"]?.Trim().TrimEnd('/');
            if (string.IsNullOrEmpty(o.PublicUrl)) o.PublicUrl = null;

            var hosts = sec.GetSection("RedirectHosts").GetChildren().Select(p => p.Value?.Trim().ToLowerInvariant()).Where(p => !string.IsNullOrEmpty(p)).ToList();
            if (hosts.Count > 0) o.RedirectHosts = hosts;
            return o;
        }

        /// <summary>Nabídka platnosti připojení na stránce souhlasu (dny), omezená MaxConnectionDays; obsahuje vždy ConnectionDays.</summary>
        public List<int> DayChoices()
        {
            var lis = new[] { 7, 30, 90, 180, 365 }.Where(p => p <= MaxConnectionDays).ToList();
            if (!lis.Contains(ConnectionDays)) lis.Add(ConnectionDays);
            lis.Sort();
            return lis;
        }

        /// <summary>Smí se uživatel po přihlášení vrátit na tuto adresu klienta?</summary>
        public bool IsRedirectAllowed(string redirectUri)
        {
            if (string.IsNullOrEmpty(redirectUri) || redirectUri.Length > 1000) return false;
            if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var u)) return false;
            if (!string.IsNullOrEmpty(u.Fragment) || !string.IsNullOrEmpty(u.UserInfo)) return false;

            if (IsLoopback(u))
                return (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps) && RedirectHosts.Contains("localhost");

            if (u.Scheme != Uri.UriSchemeHttps) return false;
            string host = u.IdnHost.ToLowerInvariant();
            return RedirectHosts.Contains("*") || RedirectHosts.Contains(host);
        }

        public static bool IsLoopback(Uri u) => u.IsLoopback && (u.Host == "localhost" || u.Host == "127.0.0.1" || u.Host == "[::1]");

        /// <summary>
        /// Tvar adresy pro porovnání s registrací klienta. U klienta na počítači uživatele se nebere v úvahu port –
        /// klient si ho volí až při přihlášení (RFC 8252).
        /// </summary>
        public static string NormalizeRedirect(string redirectUri)
        {
            if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var u)) return redirectUri ?? "";
            return IsLoopback(u) ? $"{u.Scheme}://{u.Host}{u.PathAndQuery}" : redirectUri;
        }
    }
}
