using BL.Mcp;
using Microsoft.AspNetCore.Mvc;
using UI.Code.Mcp;
using UI.Models;

namespace UI.Controllers.AI
{
    /// <summary>
    /// MCP – stránky pro přihlášeného uživatele: souhlas s připojením AI klienta (Consent) a přehled připojení s odpojením (Index).
    /// Adresy pro klienta samotného (metadata, registrace, tokeny) jsou v McpOAuthController.
    /// </summary>
    public class McpConnectController : BaseController
    {
        private readonly McpClientCodec _codec;
        private readonly McpOptions _opt;

        public McpConnectController(McpClientCodec codec, McpOptions opt)
        {
            _codec = codec; _opt = opt;
        }

        private McpOAuthService Service() => new McpOAuthService(new McpSqlStore(Factory.db), _opt, McpTenant.Key(Factory));
        private bool IsAllowed => Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_AI);

        // ─── souhlas s připojením klienta ───────────────────────────────────────

        /// <summary>Sem vede mezistránka z /oauth/authorize. Nepřihlášeného uživatele sem po přihlášení vrátí parametr returnurl.</summary>
        [HttpGet]
        public IActionResult Consent()
        {
            if (!_opt.Enabled) return NotFound();
            NoFrame();

            var q = McpAuthorizeRequest.Parse(k => Request.Query[k].FirstOrDefault(), _codec, _opt, McpUrls.Mcp(Request, _opt));
            if (q.FatalError != null) return View("~/Views/McpOAuth/Error.cshtml", q.FatalError);
            if (q.Error != null) return Redirect(q.BuildRedirect(McpUrls.Base(Request, _opt), null, q.Error, q.ErrorDescription));

            Uri.TryCreate(q.RedirectUri, UriKind.Absolute, out var redirect);
            var u = Factory.CurrentUser;
            return View(new McpConsentViewModel
            {
                ClientName = q.Client.Name,
                RedirectHost = redirect.Host,
                IsLocalClient = McpOptions.IsLoopback(redirect),
                UserName = $"{u.j02FirstName} {u.j02LastName}".Trim(),
                Login = u.j02Login,
                LicenseName = Factory.Lic.x01Name,
                IsAllowed = IsAllowed,
                Days = _opt.ConnectionDays,
                DayChoices = _opt.DayChoices(),
                IsWriteOffered = _opt.WriteEnabled,
                Query = q.ToQuery()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Consent(string decision, int days, bool write = false)
        {
            if (!_opt.Enabled) return NotFound();
            NoFrame();

            var q = McpAuthorizeRequest.Parse(k => Request.Form[k].FirstOrDefault(), _codec, _opt, McpUrls.Mcp(Request, _opt));
            if (q.FatalError != null) return View("~/Views/McpOAuth/Error.cshtml", q.FatalError);
            string issuer = McpUrls.Base(Request, _opt);
            if (q.Error != null) return Redirect(q.BuildRedirect(issuer, null, q.Error, q.ErrorDescription));

            if (decision != "allow" || !IsAllowed)
                return Redirect(q.BuildRedirect(issuer, null, "access_denied", "Uživatel přístup nepovolil."));

            if (!_opt.DayChoices().Contains(days)) days = _opt.ConnectionDays;
            string code = Service().CreateCode(Factory.CurrentUser.pid, q.Client.Name, q.Client.Key, q.RedirectUri, q.CodeChallenge,
                McpOptions.BuildScope(write && _opt.WriteEnabled), days);
            if (code == null)
                return Redirect(q.BuildRedirect(issuer, null, "server_error", "Připojení se nepodařilo uložit (chybí tabulky MCP v databázi?)."));

            return Redirect(q.BuildRedirect(issuer, code));
        }

        // ─── přehled připojení v profilu uživatele ──────────────────────────────

        public IActionResult Index()
        {
            var now = DateTime.Now;
            var v = new McpConnectViewModel
            {
                IsEnabled = _opt.Enabled,
                IsAllowed = IsAllowed,
                IsWriteEnabled = _opt.WriteEnabled,
                McpUrl = McpUrls.Mcp(Request, _opt),
                Now = now
            };
            if (_opt.Enabled)
            {
                var lis = new McpSqlStore(Factory.db).ListByUser(Factory.CurrentUser.pid);
                v.Active = lis.Where(p => p.IsActive(now)).ToList();
                v.Ended = lis.Where(p => !p.IsActive(now)).Take(10).ToList();
            }
            return View(v);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Disconnect(int j96id)
        {
            bool ok = _opt.Enabled && Service().RevokeByUser(j96id, Factory.CurrentUser.pid);
            return RedirectToAction("Index", new { done = ok ? 1 : 0 });
        }

        /// <summary>Stránka souhlasu se nesmí zobrazit v rámu cizího webu (podstrčené kliknutí).</summary>
        private void NoFrame()
        {
            Response.Headers["X-Frame-Options"] = "DENY";
            Response.Headers["Content-Security-Policy"] = "frame-ancestors 'none'";
            Response.Headers.CacheControl = "no-store";
        }
    }
}
