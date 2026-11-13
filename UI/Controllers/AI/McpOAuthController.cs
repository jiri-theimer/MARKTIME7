using System.Text.Json;
using System.Text.Json.Nodes;
using BL.Mcp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using UI.Code.Mcp;

namespace UI.Controllers.AI
{
    /// <summary>
    /// OAuth 2.1 pro MCP – adresy, které volá AI klient (Claude, ChatGPT…) bez přihlášeného uživatele:
    /// metadata, registrace klienta, začátek přihlášení a vydání tokenů. Souhlas uživatele je v McpConnectController.
    /// Průběh: klient dostane z /mcp odpověď 401 → přečte metadata → zaregistruje se → pošle prohlížeč uživatele na /oauth/authorize
    /// → uživatel se přihlásí do MARKTIME a povolí přístup → klient si za jednorázový kód vyzvedne tokeny na /oauth/token.
    /// </summary>
    [AllowAnonymous]
    public class McpOAuthController : Controller
    {
        private readonly BL.Singleton.RunningApp _app;
        private readonly McpClientCodec _codec;
        private readonly McpOptions _opt;

        public McpOAuthController(BL.Singleton.RunningApp app, McpClientCodec codec, McpOptions opt)
        {
            _app = app; _codec = codec; _opt = opt;
        }

        private string BaseUrl => McpUrls.Base(Request, _opt);

        /// <summary>Rozsahy, které server nabízí. O zápisu rozhoduje uživatel na stránce souhlasu – vydaný rozsah vrací /oauth/token.</summary>
        private JsonArray Scopes() => _opt.WriteEnabled ? new JsonArray { McpOptions.ScopeRead, McpOptions.ScopeWrite } : new JsonArray { McpOptions.ScopeRead };

        // ─── metadata ────────────────────────────────────────────────────────────

        /// <summary>Metadata chráněného zdroje (RFC 9728): kde je MCP server a kdo pro něj vydává tokeny.</summary>
        [HttpGet("/.well-known/oauth-protected-resource")]
        [HttpGet("/.well-known/oauth-protected-resource/mcp")]
        public IActionResult ProtectedResource()
        {
            if (!_opt.Enabled) return NotFound();
            return Json(new JsonObject
            {
                ["resource"] = McpUrls.Mcp(Request, _opt),
                ["authorization_servers"] = new JsonArray { BaseUrl },
                ["scopes_supported"] = Scopes(),
                ["bearer_methods_supported"] = new JsonArray { "header" },
                ["resource_name"] = McpToolRunner.ServerName
            });
        }

        /// <summary>Metadata autorizačního serveru (RFC 8414).</summary>
        [HttpGet("/.well-known/oauth-authorization-server")]
        [HttpGet("/.well-known/oauth-authorization-server/mcp")]
        [HttpGet("/.well-known/openid-configuration")]
        public IActionResult AuthorizationServer()
        {
            if (!_opt.Enabled) return NotFound();
            return Json(new JsonObject
            {
                ["issuer"] = BaseUrl,
                ["authorization_endpoint"] = BaseUrl + "/oauth/authorize",
                ["token_endpoint"] = BaseUrl + "/oauth/token",
                ["registration_endpoint"] = BaseUrl + "/oauth/register",
                ["revocation_endpoint"] = BaseUrl + "/oauth/revoke",
                ["response_types_supported"] = new JsonArray { "code" },
                ["response_modes_supported"] = new JsonArray { "query" },
                ["grant_types_supported"] = new JsonArray { "authorization_code", "refresh_token" },
                ["code_challenge_methods_supported"] = new JsonArray { "S256" },
                ["token_endpoint_auth_methods_supported"] = new JsonArray { "none" },
                ["revocation_endpoint_auth_methods_supported"] = new JsonArray { "none" },
                ["scopes_supported"] = Scopes(),
                ["authorization_response_iss_parameter_supported"] = true
            });
        }

        // ─── registrace klienta (RFC 7591) ───────────────────────────────────────

        [HttpPost("/oauth/register")]
        public async Task<IActionResult> Register()
        {
            if (!_opt.Enabled) return NotFound();
            NoStore();

            JsonNode body;
            try
            {
                using var reader = new StreamReader(Request.Body);
                string s = await reader.ReadToEndAsync();
                if (s.Length > 20000) return OAuthError("invalid_client_metadata", "Požadavek je příliš velký.");
                body = JsonNode.Parse(s);
            }
            catch
            {
                return OAuthError("invalid_client_metadata", "Tělo požadavku není platný JSON.");
            }

            var uris = new List<string>();
            if (body?["redirect_uris"] is JsonArray arr)
            {
                foreach (var x in arr)
                {
                    if (x is JsonValue v && v.TryGetValue<string>(out var u)) uris.Add(u);
                }
            }
            if (uris.Count == 0 || uris.Count > McpClientCodec.MaxRedirects) return OAuthError("invalid_redirect_uri", $"Uveďte 1 až {McpClientCodec.MaxRedirects} adres redirect_uris.");
            foreach (string u in uris)
            {
                if (!_opt.IsRedirectAllowed(u)) return OAuthError("invalid_redirect_uri", "Adresa redirect_uri není v MARKTIME povolená: " + McpAuthorizeRequest.SafeHost(u));
            }

            string name = body["client_name"] is JsonValue nv && nv.TryGetValue<string>(out var n) ? n : null;
            string clientId = _codec.Register(name, uris);

            Response.StatusCode = StatusCodes.Status201Created;
            return Json(new JsonObject
            {
                ["client_id"] = clientId,
                ["client_id_issued_at"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["client_name"] = _codec.Parse(clientId).Name,
                ["redirect_uris"] = new JsonArray(uris.Select(u => (JsonNode)u).ToArray()),
                ["grant_types"] = new JsonArray { "authorization_code", "refresh_token" },
                ["response_types"] = new JsonArray { "code" },
                ["token_endpoint_auth_method"] = "none",
                ["scope"] = McpOptions.BuildScope(_opt.WriteEnabled)
            });
        }

        // ─── začátek přihlášení ─────────────────────────────────────────────────

        /// <summary>
        /// Sem klient pošle prohlížeč uživatele. Přihlašovací cookie MARKTIME je SameSite=Strict – při příchodu z cizího webu ji prohlížeč
        /// nepošle, i když je uživatel přihlášený. Proto se tu jen ověří žádost a zobrazí mezistránka, která prohlížeč pošle dál
        /// na stránku souhlasu už jako požadavek z MARKTIME (cookie se pošle; nepřihlášený uživatel projde běžným přihlášením).
        /// </summary>
        [HttpGet("/oauth/authorize")]
        public IActionResult Authorize()
        {
            if (!_opt.Enabled) return NotFound();
            NoStore();

            var q = McpAuthorizeRequest.Parse(k => Request.Query[k].FirstOrDefault(), _codec, _opt, McpUrls.Mcp(Request, _opt));
            if (q.FatalError != null) return View("~/Views/McpOAuth/Error.cshtml", q.FatalError);
            if (q.Error != null) return Redirect(q.BuildRedirect(BaseUrl, null, q.Error, q.ErrorDescription));

            string url = QueryHelpers.AddQueryString(Url.Content("~/McpConnect/Consent"), q.ToQuery());
            return View("~/Views/McpOAuth/Authorize.cshtml", url);
        }

        // ─── tokeny ──────────────────────────────────────────────────────────────

        [HttpPost("/oauth/token")]
        [IgnoreAntiforgeryToken]
        public IActionResult Token()
        {
            if (!_opt.Enabled) return NotFound();
            NoStore();
            if (!Request.HasFormContentType) return OAuthError("invalid_request", "Očekává se application/x-www-form-urlencoded.");

            string F(string k) => Request.Form[k].FirstOrDefault();
            var client = _codec.Parse(F("client_id"));
            if (client == null) return OAuthError("invalid_client", "Neznámý klient (client_id).", StatusCodes.Status401Unauthorized);

            McpTokenResult res;
            switch (F("grant_type"))
            {
                case "authorization_code":
                    {
                        string code = F("code");
                        var svc = Service(code, McpSecret.Code);
                        res = svc == null ? McpTokenResult.Fail("invalid_grant", "Neplatný autorizační kód.") : svc.RedeemCode(code, client.Key, F("redirect_uri"), F("code_verifier"));
                        break;
                    }
                case "refresh_token":
                    {
                        string token = F("refresh_token");
                        var svc = Service(token, McpSecret.Refresh);
                        res = svc == null ? McpTokenResult.Fail("invalid_grant", "Neplatný obnovovací token.") : svc.Refresh(token, client.Key);
                        break;
                    }
                default:
                    return OAuthError("unsupported_grant_type", "Podporovány jsou authorization_code a refresh_token.");
            }

            if (!res.Ok) return OAuthError(res.Error, res.ErrorDescription, res.Error == "server_error" ? StatusCodes.Status500InternalServerError : StatusCodes.Status400BadRequest);

            return Json(new JsonObject
            {
                ["access_token"] = res.AccessToken,
                ["token_type"] = "Bearer",
                ["expires_in"] = res.ExpiresIn,
                ["refresh_token"] = res.RefreshToken,
                ["scope"] = res.Scope
            });
        }

        /// <summary>Odvolání tokenu klientem (RFC 7009) – ukončí celé připojení. Vždy odpovídá 200.</summary>
        [HttpPost("/oauth/revoke")]
        [IgnoreAntiforgeryToken]
        public IActionResult Revoke()
        {
            if (!_opt.Enabled) return NotFound();
            NoStore();
            if (!Request.HasFormContentType) return OAuthError("invalid_request", "Očekává se application/x-www-form-urlencoded.");

            string token = Request.Form["token"].FirstOrDefault();
            var client = _codec.Parse(Request.Form["client_id"].FirstOrDefault());
            if (client != null)
            {
                var svc = Service(token, McpSecret.Refresh) ?? Service(token, McpSecret.Access);
                svc?.RevokeByToken(token, client.Key);
            }
            return Ok();
        }

        /// <summary>Služba OAuth nad databází, do které řetězec (kód, token) patří. null = řetězec nemá platný tvar.</summary>
        private McpOAuthService Service(string secret, char kind)
        {
            if (!McpSecret.TryParse(secret, kind, out string tenantKey)) return null;
            var db = McpTenant.OpenDb(_app, tenantKey);
            return db == null ? null : new McpOAuthService(new McpSqlStore(db), _opt, tenantKey);
        }

        private void NoStore()
        {
            Response.Headers.CacheControl = "no-store";
            Response.Headers.Pragma = "no-cache";
        }

        private IActionResult OAuthError(string error, string description, int status = StatusCodes.Status400BadRequest)
        {
            Response.StatusCode = status;
            return Json(new JsonObject { ["error"] = error, ["error_description"] = description });
        }

        /// <summary>JSON přes System.Text.Json bez ohledu na to, jaký serializátor má MVC nastavený (aplikace používá i Newtonsoft).</summary>
        private ContentResult Json(JsonObject o) => Content(o.ToJsonString(_json), "application/json", System.Text.Encoding.UTF8);

        private static readonly JsonSerializerOptions _json = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()     // bez něj JsonNode.ToJsonString(options) v .NET 8 vyhodí výjimku
        };
    }
}
