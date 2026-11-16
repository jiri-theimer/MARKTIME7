using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace UI.Code.OutlookAddin
{
    /// <summary>
    /// Token doplňku pro Outlook. Panel doplňku běží uvnitř Outlooku (na webu v iframe cizího webu), kde se přihlašovací cookie
    /// MARKTIME neposílá (SameSite=Strict). Uživatel proto doplněk jednou připojí (OutlookAddinConnectController) a panel pak
    /// posílá hlavičku Authorization: Bearer mto_…
    /// Token není v databázi: je to zašifrovaný a časově omezený záznam (login, ID uživatele) chráněný klíči ASP.NET Data Protection
    /// (appsettings.json: Authentication:KeyPath) – stejnými, které chrání přihlašovací cookie. Při každém požadavku se uživatel
    /// načte znovu, takže uzavřený účet nebo uzamknutá databáze token okamžitě znefunkční.
    /// </summary>
    public class OutlookAddinTokenCodec
    {
        public const int ValidDays = 90;
        private const string Prefix = "mto_";
        private readonly ITimeLimitedDataProtector _protector;

        public OutlookAddinTokenCodec(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("MARKTIME.OutlookAddin.Token.v1").ToTimeLimitedDataProtector();
        }

        private class Payload
        {
            public string l { get; set; }
            public int u { get; set; }
        }

        public string Issue(string login, int j02id)
        {
            return Prefix + _protector.Protect(JsonSerializer.Serialize(new Payload { l = login, u = j02id }), TimeSpan.FromDays(ValidDays));
        }

        /// <returns>false = token nevydal tento server, vypršel, nebo se změnily šifrovací klíče</returns>
        public bool TryParse(string token, out string login, out int j02id)
        {
            login = null; j02id = 0;
            if (string.IsNullOrEmpty(token) || token.Length > 2000 || !token.StartsWith(Prefix)) return false;
            try
            {
                var p = JsonSerializer.Deserialize<Payload>(_protector.Unprotect(token.Substring(Prefix.Length)));
                if (p == null || string.IsNullOrEmpty(p.l) || p.u <= 0) return false;
                login = p.l; j02id = p.u;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Přihlášení panelu doplňku tokenem (jen akce OutlookAddinController označené tímto schématem).
    /// Login se zapíše do BO.RunningUser dřív, než vznikne BL.Factory – stejně jako u MCP (McpBearerHandler).
    /// </summary>
    public class OutlookAddinBearerHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "OutlookAddin";
        private readonly OutlookAddinTokenCodec _codec;

        public OutlookAddinBearerHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
            OutlookAddinTokenCodec codec) : base(options, logger, encoder)
        {
            _codec = codec;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string header = Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(AuthenticateResult.NoResult());

            if (!_codec.TryParse(header.Substring(7).Trim(), out string login, out int j02id))
                return Task.FromResult(AuthenticateResult.Fail("Neplatný token."));

            var ru = Context.RequestServices.GetRequiredService<BO.RunningUser>();
            ru.j02Login = login;
            var f = Context.RequestServices.GetRequiredService<BL.Factory>();
            if (f.CurrentUser == null || f.CurrentUser.pid != j02id || f.CurrentUser.isclosed)
                return Task.FromResult(AuthenticateResult.Fail("Uživatelský účet je uzavřený."));
            if (f.CurrentUser.j02IsMustChangePassword || f.CurrentUser.j02SmsVerifyCode != null)
                return Task.FromResult(AuthenticateResult.Fail("Účet vyžaduje přihlášení v MARKTIME."));
            if (!f.CurrentUser.IsAdmin)
            {
                var recX01 = f.x01LicenseBL.Load(f.CurrentUser.x01ID);     // stejný test jako při přihlášení (LoginController)
                if (recX01 != null && BO.Code.Bas.bit_compare_or(recX01.x01LockFlag, 2))
                    return Task.FromResult(AuthenticateResult.Fail("Databáze je uzamknutá."));
            }

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, j02id.ToString()),
                new Claim(ClaimTypes.Name, login)
            }, SchemeName);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;    // panel podle 401 nabídne nové připojení; žádné přesměrování na login
            return Task.CompletedTask;
        }
    }

    public static class OutlookAddinSetup
    {
        /// <summary>Volá Startup.ConfigureServices.</summary>
        public static void AddServices(IServiceCollection services)
        {
            services.AddSingleton<OutlookAddinTokenCodec>();
            services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, OutlookAddinBearerHandler>(OutlookAddinBearerHandler.SchemeName, null);
        }
    }
}
