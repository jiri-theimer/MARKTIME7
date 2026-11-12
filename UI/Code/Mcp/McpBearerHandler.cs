using System.Security.Claims;
using System.Text.Encodings.Web;
using BL.Mcp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace UI.Code.Mcp
{
    /// <summary>
    /// Přihlášení MCP klienta přístupovým tokenem (hlavička Authorization: Bearer …). Používá se jen pro adresu /mcp.
    /// Token → připojení (j96McpGrant) → uživatel. Login se zapíše do BO.RunningUser dřív, než vznikne BL.Factory –
    /// ta pak načte uživatele a jeho oprávnění stejně jako po přihlášení cookie (viz BaseController).
    /// Bez platného tokenu vrací 401 s odkazem na metadata – podle nich klient spustí přihlášení uživatele (OAuth).
    /// </summary>
    /// <summary>Údaje o právě obsluhovaném MCP požadavku (scoped) – plní je McpBearerHandler.</summary>
    public class McpCallInfo
    {
        public string ClientName { get; set; }
    }

    public class McpBearerHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "McpBearer";
        public const string PolicyName = "McpBearer";
        public const string ClaimClient = "mcp_client";
        public const string ClaimGrant = "mcp_grant";

        private readonly BL.Singleton.RunningApp _app;
        private readonly McpOptions _opt;
        private bool _tokenPresented;

        public McpBearerHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
            BL.Singleton.RunningApp app, McpOptions opt) : base(options, logger, encoder)
        {
            _app = app;
            _opt = opt;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string header = Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(AuthenticateResult.NoResult());

            _tokenPresented = true;
            string token = header.Substring(7).Trim();
            if (!_opt.Enabled || !McpSecret.TryParse(token, McpSecret.Access, out string tenantKey))
                return Task.FromResult(AuthenticateResult.Fail("Neplatný token."));

            var db = McpTenant.OpenDb(_app, tenantKey);
            if (db == null) return Task.FromResult(AuthenticateResult.Fail("Neplatný token."));

            var grant = new McpOAuthService(new McpSqlStore(db), _opt, tenantKey).ValidateAccess(token);
            if (grant == null) return Task.FromResult(AuthenticateResult.Fail("Token vypršel nebo bylo připojení odpojeno."));

            // uživatel pro BL.Factory – musí být nastaven dřív, než si někdo Factory z kontejneru vyžádá
            var ru = Context.RequestServices.GetRequiredService<BO.RunningUser>();
            ru.j02Login = grant.j02Login;
            var f = Context.RequestServices.GetRequiredService<BL.Factory>();
            if (f.CurrentUser == null || f.CurrentUser.pid != grant.j02ID || f.CurrentUser.isclosed)
                return Task.FromResult(AuthenticateResult.Fail("Uživatelský účet je uzavřený."));
            if (!f.CurrentUser.IsAdmin)
            {
                var recX01 = f.x01LicenseBL.Load(f.CurrentUser.x01ID);     // stejný test jako při přihlášení (LoginController)
                if (recX01 != null && BO.Code.Bas.bit_compare_or(recX01.x01LockFlag, 2))
                    return Task.FromResult(AuthenticateResult.Fail("Databáze je uzamknutá."));
            }

            Context.RequestServices.GetRequiredService<McpCallInfo>().ClientName = grant.j96ClientName;

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, grant.j02ID.ToString()),
                new Claim(ClaimTypes.Name, grant.j02Login),
                new Claim(ClaimGrant, grant.j96ID.ToString()),
                new Claim(ClaimClient, grant.j96ClientName ?? "")
            }, SchemeName);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            string value = $"Bearer resource_metadata=\"{McpUrls.ResourceMetadata(Request, _opt)}\"";
            if (_tokenPresented) value += ", error=\"invalid_token\"";
            Response.Headers.WWWAuthenticate = value;
            return Task.CompletedTask;
        }
    }
}
