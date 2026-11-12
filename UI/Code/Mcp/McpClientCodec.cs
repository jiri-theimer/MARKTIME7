using System.Text.Json;
using BL.Mcp;
using Microsoft.AspNetCore.DataProtection;

namespace UI.Code.Mcp
{
    /// <summary>AI klient (Claude, ChatGPT…), který se zaregistroval přes /oauth/register.</summary>
    public class McpClient
    {
        public string Name { get; set; }
        /// <summary>Otisky povolených adres návratu (redirect_uri).</summary>
        public List<string> RedirectHashes { get; set; } = new();
        /// <summary>Otisk client_id – ukládá se k připojení (j96ClientKey).</summary>
        public string Key { get; set; }

        public bool HasRedirect(string redirectUri) => RedirectHashes.Contains(McpClientCodec.RedirectHash(redirectUri));
    }

    /// <summary>
    /// Registrace klientů bez tabulky v databázi: client_id je zašifrovaný a podepsaný záznam o klientovi (název, adresy návratu),
    /// který MARKTIME vydá při registraci a při každém použití si ho sám přečte. Důvod: klient se registruje dřív, než se uživatel přihlásí,
    /// a v režimu SharedApp v tu chvíli ještě není známá databáze zákazníka. Šifruje se klíči ASP.NET Data Protection
    /// (appsettings.json: Authentication:KeyPath) – stejnými, které chrání přihlašovací cookie.
    /// Registrace sama nic neodemyká: přístup k datům dává až souhlas přihlášeného uživatele.
    /// </summary>
    public class McpClientCodec
    {
        public const int MaxRedirects = 8;
        private const string Prefix = "mtc-";
        private readonly IDataProtector _protector;

        public McpClientCodec(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("MARKTIME.Mcp.Client.v1");
        }

        private class Payload
        {
            public string n { get; set; }
            public List<string> r { get; set; }
            public long t { get; set; }
        }

        public string Register(string name, IEnumerable<string> redirectUris)
        {
            name = string.IsNullOrWhiteSpace(name) ? "MCP klient" : name.Trim();
            if (name.Length > 80) name = name.Substring(0, 80);
            var p = new Payload { n = name, r = redirectUris.Select(RedirectHash).Distinct().ToList(), t = DateTimeOffset.UtcNow.ToUnixTimeSeconds() };
            return Prefix + _protector.Protect(JsonSerializer.Serialize(p));
        }

        /// <returns>null = client_id nevydal tento server (nebo se změnily šifrovací klíče)</returns>
        public McpClient Parse(string clientId)
        {
            if (string.IsNullOrEmpty(clientId) || clientId.Length > 2000 || !clientId.StartsWith(Prefix)) return null;
            try
            {
                var p = JsonSerializer.Deserialize<Payload>(_protector.Unprotect(clientId.Substring(Prefix.Length)));
                if (p?.r == null || p.r.Count == 0) return null;
                return new McpClient { Name = p.n, RedirectHashes = p.r, Key = McpSecret.Hash(clientId) };
            }
            catch
            {
                return null;
            }
        }

        public static string RedirectHash(string redirectUri) => McpSecret.Hash(McpOptions.NormalizeRedirect(redirectUri)).Substring(0, 22);
    }
}
