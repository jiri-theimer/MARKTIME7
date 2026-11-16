using Microsoft.AspNetCore.Mvc;
using UI.Code.OutlookAddin;

namespace UI.Controllers
{
    /// <summary>
    /// Doplněk pro Outlook – stránky přihlášeného uživatele (běžné přihlášení MARKTIME):
    ///   Index – okno „Připojit“ otevřené z panelu doplňku: vydá token a předá ho panelu (Office.context.ui.messageParent)
    ///   Info  – návod a adresa manifestu (Můj profil → Doplněk pro Outlook)
    /// </summary>
    public class OutlookAddinConnectController : BaseController
    {
        private readonly OutlookAddinTokenCodec _codec;

        public OutlookAddinConnectController(OutlookAddinTokenCodec codec)
        {
            _codec = codec;
        }

        public IActionResult Index()
        {
            var v = new UI.Models.OutlookAddinConnectViewModel()
            {
                Token = _codec.Issue(Factory.CurrentUser.j02Login, Factory.CurrentUser.pid),
                UserName = Factory.CurrentUser.FullNameAsc,
                ValidDays = OutlookAddinTokenCodec.ValidDays
            };
            return View(v);
        }

        public IActionResult Info()
        {
            string b = $"{Request.Scheme}://{Request.Host}{Request.PathBase}".TrimEnd('/');
            return View(new UI.Models.OutlookAddinConnectViewModel() { ManifestUrl = b + "/OutlookAddin/Manifest", ValidDays = OutlookAddinTokenCodec.ValidDays });
        }
    }
}
