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

            return RedirectToAction("Index","Home");
        }

    }
}
