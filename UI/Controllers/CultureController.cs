using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers
{
    public class CultureController : BaseController
    {
        [HttpPost]
        public IActionResult SaveLang(string culture)
        {
            var allowed = new HashSet<string> { "cs-CZ", "en-US", "sk-SK" };
            if (!allowed.Contains(culture)) culture = "cs-CZ";

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                });

            return RedirectToAction("Index","Home");
        }

    }
}
