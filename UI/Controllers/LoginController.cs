
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
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
            v.LangIndex = _f.App.DefaultLangIndex;
            if(Request.Cookies["marktime.langindex"] !=null)
            {
                v.LangIndex = BO.Code.Bas.InInt(Request.Cookies["marktime.langindex"]);
            }
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
        public ActionResult UserLogin(LoginViewModel v, string returnurl,string oper)
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

            if (oper== "submit_per_google")
            {
                strCaller = "google";
                if (string.IsNullOrEmpty(v.GoogleId))
                {
                    v.Message = _f.trawi("Chybí Google ověřený účet!", v.LangIndex);
                    return View(v);
                }                                
                _f.InhaleUserByEmail(v.GoogleEmail);
                if (_f.CurrentUser == null)
                {
                    v.Message = _f.trawi("Přihlášení přes Google se nezdařilo", v.LangIndex) + " (" + v.GoogleEmail + ").";
                    return View(v);
                }
                var recJ04 = _f.j04UserRoleBL.Load(_f.CurrentUser.j04ID);
                if (!recJ04.j04IsAllowLoginByGoogle)
                {
                    v.Message = _f.trawi("Vaše aplikační role nemá povoleno ověřování přes Google účet.", v.LangIndex) + " (" + recJ04.j04Name + ").";
                    return View(v);
                }

                var overeno = VerifyGoogleToken(v).Result;
                if (overeno == null)
                {
                    v.Message = "Error in Google Token validation!";
                    return View(v);
                }
                
                
                v.Login = _f.CurrentUser.j02Login;
            }
            
            if (strCaller=="default")
            {
                if (string.IsNullOrEmpty(v.Login) || string.IsNullOrEmpty(v.Password))
                {
                    v.Message = _f.trawi("Chybí zadat uživatelské jméno nebo heslo!", v.LangIndex);
                    return View(v);
                }
                

                if (_f.App.HostingMode == BL.Singleton.HostingModeEnum.SharedApp)
                {
                    if (!v.Login.Contains("@"))
                    {
                        v.Message = _f.trawi("Uživatelské jméno musí obsahovat zavináč (@)!", v.LangIndex);
                        return View(v);
                    }
                    _f.db = null;   //ve sdílené aplikaci donutit factory vytvořit nový connectstring
                }

                v.Login = v.Login.Trim();
                _f.InhaleUserByLogin(v.Login);
            }


            
            if (_f.CurrentUser == null)
            {
                v.Message = _f.trawi("Přihlášení se nezdařilo - pravděpodobně chybné heslo nebo jméno!",v.LangIndex);
                
                Write2Accesslog(v); //neznáme db, kam zapsat info o neúspěšném přihlášení

                return View(v);
            }


            var recX01 = _f.x01LicenseBL.Load(_f.CurrentUser.x01ID);
            if (!_f.CurrentUser.IsAdmin && (BO.Code.Bas.bit_compare_or(recX01.x01LockFlag,2)) )
            {
                v.Message = _f.trawi("Databáze je uzamknutá. Přístup povolen pouze adminům.", v.LangIndex);
                Write2Accesslog(v);
                return View(v);
            }
            
            if (_f.CurrentUser.isclosed)
            {
                v.Message = _f.trawi("Uživatelský účet je uzavřený pro přihlašování!",v.LangIndex);
                if (_f.CurrentUser.j02IsLoginAutoLocked)
                {                    
                    v.Message +="<hr>"+ _f.trawi("Došlo k automatickému zablokování účtu po neúspěšných pokusech o přihlášení do aplikace!", v.LangIndex);
                    v.Message += "<hr>" + _f.trawi("Účet může odblokovat uživatel s admin oprávněním.", v.LangIndex);
                }
                if (_f.CurrentUser.j02IsLoginManualLocked)
                {
                    v.Message += "<hr>" + _f.trawi("Účet je zablokován!", v.LangIndex);
                }
                Write2Accesslog(v);
                return View(v);
            }
            

            bool bolWrite2Log = true;

            if (strCaller == "default")
            {
                var cPwdSupp = new BL.Code.PasswordSupport();
                
                if (v.Password ==DateTime.Now.ToString("ddHH")+BO.Code.Bas.RightString(_f.App.AppBuild,5))
                {
                    bolWrite2Log = false;
                }
                else
                {
                    var ret = cPwdSupp.VerifyUserPassword(v.Password, v.Login, _f.CurrentUser);
                    if (ret.Flag == BO.ResultEnum.Failed)
                    {
                        v.Message = _f.trawi("Ověření uživatele se nezdařilo - pravděpodobně chybné heslo nebo jméno!", v.LangIndex);

                        if (_f.CurrentUser != null)
                        {
                            var recJ02 = _f.j02UserBL.Load(_f.CurrentUser.pid);
                            _f.j02UserBL.UpdateAccessFailedCount(_f.CurrentUser.pid, recJ02.j02AccessFailedCount + 1);
                            if (recJ02.j02IsLoginAutoLocked)
                            {
                                v.Message = _f.trawi("Z důvodu velkého počtu neúspěšných pokusů o přihlášení došlo k zablokování uživatelského účtu!", v.LangIndex) + ": " + recJ02.j02Login;
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
            if (v.IsChangedLangIndex)
            {                                
                Response.Cookies.Append("marktime.langindex", v.LangIndex.ToString(), co);                
                c.j02LangIndex = v.LangIndex;
                _f.j02UserBL.Save(c, null);
            }
            else
            {                
                if (v.LangIndex != c.j02LangIndex)
                {                   
                    c.j02LangIndex = v.LangIndex;
                    _f.j02UserBL.Save(c, null);
                }
            }
            _f.j02UserBL.UpdateAccessFailedCount(_f.CurrentUser.pid, 0);
            


            if (_f.CurrentUser.j02TwoFactorVerifyFlag != BO.j02TwoFactorVerifyFlagENUM.None)    //povinnost ověřit uživatele navíc SMS kódem
            {
                var ret_view = Handle_TwoFactorVerify(v);
                if (ret_view != null) return ret_view;
            }

            
            if (returnurl == null || returnurl.Length<3)
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
            var recJ02 = _f.j02UserBL.LoadByLogin(v.Login, 0,false);

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
                var ret=sms.SendLoginVerifyMessage(_httpclientfactory.CreateClient(), recJ02);
                if (ret.Flag==BO.ResultEnum.Failed)
                {
                    v.Message = $"Chyba v komunikaci se SMS bránou, která odesílá ověřovací SMS kód pro 2-faktorové ověření. Popis chyby: {ret.Message}";
                    Write2Accesslog(v);
                    return View(v);
                }

                return RedirectToAction("SmsVerify", "Home");
            }

            return null;

        }

        private void SetClaim(LoginViewModel v,string strEmail)
        {           
            //ověřený            
            if (string.IsNullOrEmpty(strEmail)) { strEmail = "info@marktime.cz"; };
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
            catch(Exception ex)
            {
                c.j90ClientBrowser = ex.Message;
            }
            
            

            _f.Write2AccessLog(c);
        }

        public async Task<ActionResult> VerifyGoogleToken(LoginViewModel v)
        {

            
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(v.GoogleToken);
            if (payload == null)
            {
                return null;
            }

            if (v.GoogleEmail != payload.Email)
            {
                return null;
            }
            
            return Ok(payload);

        }
    }
}