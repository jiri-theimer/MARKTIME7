using BL.Mcp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace UI.Code.Mcp
{
    /// <summary>Veřejné adresy MCP serveru odvozené z požadavku (nebo z Mcp:PublicUrl).</summary>
    public static class McpUrls
    {
        public const string McpPath = "/mcp";

        public static string Base(HttpRequest r, McpOptions opt) => opt.PublicUrl ?? $"{r.Scheme}://{r.Host}{r.PathBase}".TrimEnd('/');
        public static string Mcp(HttpRequest r, McpOptions opt) => Base(r, opt) + McpPath;
        public static string ResourceMetadata(HttpRequest r, McpOptions opt) => Base(r, opt) + "/.well-known/oauth-protected-resource";
    }

    /// <summary>
    /// Žádost klienta o přihlášení uživatele (parametry /oauth/authorize). Ověřuje se dvakrát stejným kódem:
    /// při příchodu z klienta a znovu na stránce souhlasu – parametry mezi tím putují adresou prohlížeče.
    /// </summary>
    public class McpAuthorizeRequest
    {
        public string ClientId { get; set; }
        public string RedirectUri { get; set; }
        public string State { get; set; }
        public string CodeChallenge { get; set; }
        public string Scope { get; set; }
        public string Resource { get; set; }
        public McpClient Client { get; set; }

        /// <summary>Chyba, kterou nelze vrátit klientovi (neznámý klient nebo adresa návratu) – zobrazí se uživateli.</summary>
        public string FatalError { get; set; }
        /// <summary>Chyba, která se vrací klientovi na redirect_uri (kód chyby OAuth).</summary>
        public string Error { get; set; }
        public string ErrorDescription { get; set; }

        public bool IsValid => FatalError == null && Error == null;

        public static McpAuthorizeRequest Parse(Func<string, string> get, McpClientCodec codec, McpOptions opt, string mcpUrl)
        {
            var q = new McpAuthorizeRequest
            {
                ClientId = get("client_id"),
                RedirectUri = get("redirect_uri"),
                State = get("state"),
                CodeChallenge = get("code_challenge"),
                Scope = get("scope"),
                Resource = get("resource")
            };

            q.Client = codec.Parse(q.ClientId);
            if (q.Client == null) { q.FatalError = "Neznámý klient (client_id). Odeberte konektor MARKTIME ve svém AI klientovi a přidejte ho znovu."; return q; }
            if (string.IsNullOrEmpty(q.RedirectUri) || !q.Client.HasRedirect(q.RedirectUri)) { q.FatalError = "Adresa návratu (redirect_uri) neodpovídá registraci klienta."; return q; }
            if (!opt.IsRedirectAllowed(q.RedirectUri)) { q.FatalError = "Adresa návratu (redirect_uri) není v MARKTIME povolená: " + SafeHost(q.RedirectUri); return q; }

            if (q.State != null && q.State.Length > 1000) return q.Fail("invalid_request", "Parametr state je příliš dlouhý.");
            if (get("response_type") != "code") return q.Fail("unsupported_response_type", "Podporován je jen response_type=code.");
            if (string.IsNullOrEmpty(q.CodeChallenge) || q.CodeChallenge.Length < 43 || q.CodeChallenge.Length > 128) return q.Fail("invalid_request", "Chybí code_challenge (PKCE).");
            if ((get("code_challenge_method") ?? "plain") != "S256") return q.Fail("invalid_request", "Podporována je jen metoda PKCE S256.");
            if (!string.IsNullOrEmpty(q.Resource) && !IsOurResource(q.Resource, mcpUrl)) return q.Fail("invalid_target", "Parametr resource neodpovídá adrese MCP serveru.");

            // rozsah: zatím jen čtení; neznámé hodnoty se ignorují
            q.Scope = McpOptions.ScopeRead;
            return q;
        }

        private McpAuthorizeRequest Fail(string error, string description)
        {
            Error = error; ErrorDescription = description;
            return this;
        }

        /// <summary>Adresa, na kterou se prohlížeč vrátí ke klientovi s výsledkem.</summary>
        public string BuildRedirect(string issuer, string code, string error = null, string errorDescription = null)
        {
            var d = new Dictionary<string, string>();
            if (code != null) d["code"] = code;
            if (error != null) d["error"] = error;
            if (errorDescription != null) d["error_description"] = errorDescription;
            if (!string.IsNullOrEmpty(State)) d["state"] = State;
            d["iss"] = issuer;
            return QueryHelpers.AddQueryString(RedirectUri, d);
        }

        /// <summary>Parametry žádosti pro předání na stránku souhlasu.</summary>
        public Dictionary<string, string> ToQuery()
        {
            var d = new Dictionary<string, string>
            {
                ["response_type"] = "code",
                ["client_id"] = ClientId,
                ["redirect_uri"] = RedirectUri,
                ["code_challenge"] = CodeChallenge,
                ["code_challenge_method"] = "S256"
            };
            if (!string.IsNullOrEmpty(State)) d["state"] = State;
            if (!string.IsNullOrEmpty(Resource)) d["resource"] = Resource;
            return d;
        }

        public static string SafeHost(string url) => Uri.TryCreate(url, UriKind.Absolute, out var u) ? u.Host : "?";

        /// <summary>resource = adresa MCP serveru; někteří klienti posílají jen adresu aplikace bez /mcp.</summary>
        public static bool IsOurResource(string resource, string mcpUrl)
        {
            string r = resource.TrimEnd('/');
            return string.Equals(r, mcpUrl, StringComparison.OrdinalIgnoreCase)
                || string.Equals(r + McpUrls.McpPath, mcpUrl, StringComparison.OrdinalIgnoreCase);
        }
    }
}
