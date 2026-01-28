
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Controllers;



namespace APARTMAN.Controllers
{
    public class CultureController : BaseController
    {
        [HttpPost]
        public IActionResult SaveLang(string culture, string returnUrl = "/")
        {
            var allowed = new HashSet<string> { "cs-CZ", "en-US", "de-DE", "ru-RU","sk-SK","es-ES" };
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

            

            return LocalRedirect(returnUrl);
        }

    }
}
