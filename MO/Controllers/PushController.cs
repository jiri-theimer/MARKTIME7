using Microsoft.AspNetCore.Mvc;

namespace MO.Controllers
{
    /// <summary>
    /// Web push v mobilní aplikaci: zapnutí a vypnutí upozornění v prohlížeči uživatele (BL.PushBL, tabulka u05PushSubscription).
    /// Klient: wwwroot/js/mo-push.js, příjem a zobrazení: wwwroot/service-worker.js.
    /// </summary>
    public class PushController : BaseController
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Subscribe(string endpoint, string p256dh, string auth)
        {
            if (!Factory.PushBL.IsAvailable()) return Json(new { ok = false, message = Factory.tra("Upozornění nejsou na serveru nastavená.") });
            bool b = Factory.PushBL.Subscribe(endpoint, p256dh, auth, Request.Headers["User-Agent"].ToString());
            return Json(new { ok = b });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Unsubscribe(string endpoint)
        {
            Factory.PushBL.Unsubscribe(endpoint);
            return Json(new { ok = true });
        }

        /// <summary>Pošle zkušební upozornění do všech zařízení přihlášeného uživatele.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Test()
        {
            int n = Factory.PushBL.SendTest();
            if (n == 0) return Json(new { ok = false, message = Factory.tra("Na žádném zařízení nemáte upozornění zapnutá.") });
            return Json(new { ok = true, count = n });
        }

        /// <summary>
        /// Jen pro admina: vygeneruje nový pár klíčů k vložení do appsettings.json (UI i MO, stejné hodnoty).
        /// Nic neukládá. Výměna klíčů zneplatní všechny dosavadní odběry - uživatelé si upozornění zapnou znovu.
        /// </summary>
        public IActionResult GenerateKeys()
        {
            if (!Factory.CurrentUser.IsAdmin) return Forbid();
            var keys = BL.PushKeys.Generate();
            string s = "\"Push\": {\n  \"Subject\": \"mailto:info@marktime.cz\",\n  \"PublicKey\": \"" + keys.PublicKey + "\",\n  \"PrivateKey\": \"" + keys.PrivateKey + "\"\n}";
            Response.Headers["Cache-Control"] = "no-store";
            return Content(s, "text/plain; charset=utf-8");
        }
    }
}
