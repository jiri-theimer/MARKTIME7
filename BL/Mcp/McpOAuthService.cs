using System.Security.Cryptography;
using System.Text;

namespace BL.Mcp
{
    /// <summary>
    /// Tajné řetězce OAuth (autorizační kód, přístupový a obnovovací token). Tvar: mt{druh}_{databáze}_{náhodná část},
    /// např. mta_5f0c…_Qx7… Část „databáze“ říká, ve které databázi zákazníka se má otisk hledat (režim SharedApp) –
    /// sama o sobě nic neodemyká. V databázi se ukládá jen otisk SHA-256 celého řetězce.
    /// </summary>
    public static class McpSecret
    {
        public const char Code = 'c';
        public const char Access = 'a';
        public const char Refresh = 'r';

        public static string New(char kind, string tenantKey) => $"mt{kind}_{tenantKey}_{Base64Url(RandomNumberGenerator.GetBytes(32))}";

        public static bool TryParse(string secret, char kind, out string tenantKey)
        {
            tenantKey = null;
            if (string.IsNullOrEmpty(secret) || secret.Length > 200) return false;
            var arr = secret.Split('_', 3);     // náhodná část (base64url) může podtržítko obsahovat
            if (arr.Length != 3 || arr[0] != "mt" + kind || arr[2].Length < 40) return false;
            if (arr[1].Length == 0 || arr[1].Length > 40 || !arr[1].All(c => char.IsAsciiLetterOrDigit(c))) return false;
            tenantKey = arr[1];
            return true;
        }

        public static string Hash(string s) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s ?? ""))).ToLowerInvariant();

        public static string Base64Url(byte[] data) => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        /// <summary>PKCE (S256): otisk code_verifier se musí rovnat code_challenge z žádosti o přihlášení.</summary>
        public static bool VerifyPkce(string verifier, string challenge)
        {
            if (string.IsNullOrEmpty(verifier) || string.IsNullOrEmpty(challenge) || verifier.Length < 43 || verifier.Length > 128) return false;
            byte[] a = Encoding.ASCII.GetBytes(Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier))));
            byte[] b = Encoding.ASCII.GetBytes(challenge);
            return CryptographicOperations.FixedTimeEquals(a, b);
        }
    }

    /// <summary>Databáze zákazníka pro OAuth požadavky, které přicházejí bez přihlášeného uživatele (token, obnovení, ověření tokenu).</summary>
    public static class McpTenant
    {
        /// <summary>Klíč databáze do tokenů = x01Guid licence přihlášeného uživatele.</summary>
        public static string Key(BL.Factory f) => f.Lic.x01Guid.ToString("N");

        /// <returns>null = neznámý klíč</returns>
        public static DL.DbHandler OpenDb(BL.Singleton.RunningApp app, string tenantKey)
        {
            var lic = app.lisX01?.FirstOrDefault(p => p.x01Guid.ToString("N") == tenantKey);
            if (lic == null) return null;
            string con = app.ConnectString;
            if (app.HostingMode == BL.Singleton.HostingModeEnum.SharedApp)
            {
                if (string.IsNullOrEmpty(lic.x01LoginDomain)) return null;
                con = app.ParseConnectStringFromLogin("x@" + lic.x01LoginDomain);
            }
            return new DL.DbHandler(con, new BO.RunningUser(), app.LogFolder);
        }
    }

    public class McpTokenResult
    {
        public bool Ok => Error == null;
        /// <summary>Kód chyby OAuth (invalid_grant, invalid_request, server_error).</summary>
        public string Error { get; set; }
        public string ErrorDescription { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public string Scope { get; set; }

        public static McpTokenResult Fail(string error, string description) => new() { Error = error, ErrorDescription = description };
    }

    /// <summary>
    /// Logika OAuth 2.1 pro MCP nad jednou databází zákazníka: autorizační kód s PKCE, přístupový token s krátkou platností,
    /// obnovovací token s výměnou při každém použití. Připojení (souhlas uživatele) má pevný konec – po něm se uživatel přihlásí znovu.
    /// </summary>
    public class McpOAuthService
    {
        private readonly IMcpStore _store;
        private readonly McpOptions _opt;
        private readonly string _tenantKey;
        private readonly Func<DateTime> _now;

        public McpOAuthService(IMcpStore store, McpOptions opt, string tenantKey, Func<DateTime> now = null)
        {
            _store = store; _opt = opt; _tenantKey = tenantKey; _now = now ?? (() => DateTime.Now);
        }

        /// <summary>Uživatel povolil přístup: založí připojení a vrátí autorizační kód pro klienta (null při chybě zápisu).</summary>
        public string CreateCode(int j02id, string clientName, string clientKey, string redirectUri, string codeChallenge, string scope, int days)
        {
            var now = _now();
            string code = McpSecret.New(McpSecret.Code, _tenantKey);
            var g = new McpGrant
            {
                j02ID = j02id,
                j96ClientName = Cut(clientName, 200),
                j96ClientKey = clientKey,
                j96RedirectUri = redirectUri,
                j96Scope = scope,
                j96CodeHash = McpSecret.Hash(code),
                j96CodeChallenge = codeChallenge,
                j96CodeExpires = now.AddMinutes(_opt.CodeMinutes),
                j96DateInsert = now,
                j96DateExpires = now.AddDays(Math.Clamp(days, 1, _opt.MaxConnectionDays))
            };
            return _store.InsertGrant(g) > 0 ? code : null;
        }

        /// <summary>Klient vyzvedává tokeny za autorizační kód.</summary>
        public McpTokenResult RedeemCode(string code, string clientKey, string redirectUri, string codeVerifier)
        {
            var now = _now();
            var g = _store.LoadGrantByCode(McpSecret.Hash(code));
            if (g == null) return Invalid("Neplatný autorizační kód.");
            if (g.j96CodeUsed != null)
            {
                // kód smí být použit jednou – druhé použití znamená, že ho má i někdo jiný
                _store.RevokeGrant(g.j96ID, now, "code-reuse");
                return Invalid("Autorizační kód už byl použit.");
            }
            if (!g.IsActive(now) || g.j96CodeExpires < now) return Invalid("Platnost autorizačního kódu vypršela.");
            if (g.j96ClientKey != clientKey) return Invalid("Autorizační kód byl vydán jinému klientovi.");
            if (g.j96RedirectUri != redirectUri) return Invalid("Parametr redirect_uri se liší od žádosti o přihlášení.");
            if (!McpSecret.VerifyPkce(codeVerifier, g.j96CodeChallenge)) return Invalid("Ověření PKCE (code_verifier) selhalo.");
            if (!_store.MarkCodeUsed(g.j96ID, now)) return Invalid("Autorizační kód už byl použit.");

            return Issue(g, now);
        }

        /// <summary>Klient obnovuje přístupový token. Obnovovací token se při tom vymění za nový.</summary>
        public McpTokenResult Refresh(string refreshToken, string clientKey)
        {
            var now = _now();
            var t = _store.LoadToken(McpSecret.Hash(refreshToken));
            if (t == null || t.j99Kind != "R") return Invalid("Neplatný obnovovací token.");
            var g = _store.LoadGrant(t.j96ID);
            if (g == null || !g.IsActive(now) || t.j99DateExpires < now) return Invalid("Připojení vypršelo nebo bylo odpojeno.");
            if (g.j96ClientKey != clientKey) return Invalid("Obnovovací token byl vydán jinému klientovi.");

            if (t.j99DateUsed != null && t.j99DateUsed.Value.AddSeconds(_opt.RefreshGraceSeconds) < now)
            {
                // už vyměněný token použitý znovu po delší době = token unikl → připojení končí
                _store.RevokeGrant(g.j96ID, now, "refresh-reuse");
                return Invalid("Obnovovací token už byl použit. Připojení bylo z bezpečnostních důvodů ukončeno.");
            }
            if (t.j99DateUsed == null) _store.MarkTokenUsed(t.j99ID, now);

            _store.Cleanup(g.j96ID, now);
            return Issue(g, now);
        }

        /// <summary>Ověří přístupový token z hlavičky Authorization. Vrací připojení (včetně j02Login), nebo null.</summary>
        public McpGrant ValidateAccess(string accessToken)
        {
            var now = _now();
            var t = _store.LoadToken(McpSecret.Hash(accessToken));
            if (t == null || t.j99Kind != "A" || t.j99DateExpires < now) return null;
            var g = _store.LoadGrant(t.j96ID);
            if (g == null || !g.IsActive(now) || string.IsNullOrEmpty(g.j02Login)) return null;

            if (g.j96DateLastUsed == null || g.j96DateLastUsed.Value.AddMinutes(5) < now) _store.TouchGrant(g.j96ID, now);
            return g;
        }

        /// <summary>Odvolání tokenem (RFC 7009): klient se odpojuje sám. Ukončí celé připojení.</summary>
        public void RevokeByToken(string token, string clientKey)
        {
            var t = _store.LoadToken(McpSecret.Hash(token));
            if (t == null) return;
            var g = _store.LoadGrant(t.j96ID);
            if (g == null || g.j96ClientKey != clientKey) return;
            _store.RevokeGrant(g.j96ID, _now(), "client");
        }

        /// <summary>Uživatel odpojuje klienta ze svého profilu.</summary>
        public bool RevokeByUser(int j96id, int j02id)
        {
            var g = _store.LoadGrant(j96id);
            if (g == null || g.j02ID != j02id) return false;
            _store.RevokeGrant(j96id, _now(), "user");
            return true;
        }

        private McpTokenResult Issue(McpGrant g, DateTime now)
        {
            var accessExpires = now.AddMinutes(_opt.AccessTokenMinutes);
            if (accessExpires > g.j96DateExpires) accessExpires = g.j96DateExpires;

            string access = McpSecret.New(McpSecret.Access, _tenantKey);
            string refresh = McpSecret.New(McpSecret.Refresh, _tenantKey);
            bool ok = _store.InsertToken(new McpToken { j96ID = g.j96ID, j99Kind = "A", j99Hash = McpSecret.Hash(access), j99DateInsert = now, j99DateExpires = accessExpires })
                   && _store.InsertToken(new McpToken { j96ID = g.j96ID, j99Kind = "R", j99Hash = McpSecret.Hash(refresh), j99DateInsert = now, j99DateExpires = g.j96DateExpires });
            if (!ok) return McpTokenResult.Fail("server_error", "Token se nepodařilo uložit.");

            return new McpTokenResult
            {
                AccessToken = access,
                RefreshToken = refresh,
                ExpiresIn = Math.Max(1, (int)(accessExpires - now).TotalSeconds),
                Scope = g.j96Scope
            };
        }

        private static McpTokenResult Invalid(string description) => McpTokenResult.Fail("invalid_grant", description);
        private static string Cut(string s, int len) => s == null ? null : (s.Length > len ? s.Substring(0, len) : s);
    }
}
