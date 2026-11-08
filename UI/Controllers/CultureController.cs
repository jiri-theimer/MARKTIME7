using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers
{
    public class CultureController : BaseController
    {
        [HttpPost]
        public IActionResult SaveLang(string culture, string returnUrl)
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

            var rec = Factory.j02UserBL.Load(this.Factory.CurrentUser.pid);
            switch (culture)
            {
                case "en-US":
                    rec.j02LangIndex = 1;
                    break;
                case "sk-SK":
                    rec.j02LangIndex = 4;
                    break;
                default:
                    rec.j02LangIndex = 0;
                    break;
            }
            Factory.j02UserBL.Save(rec, null);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && !returnUrl.StartsWith("/Menu/", StringComparison.OrdinalIgnoreCase))
            {
                return LocalRedirect(returnUrl);     //zpět na stránku, odkud uživatel jazyk přepnul
            }
            return RedirectToAction("Index","Home");
        }

    }
}
