using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using UI.Code.DbCreate;
using UI.Models.Guru;

namespace UI.Controllers.guru
{
    /// <summary>
    /// Samoobslužná registrace v režimu TotalCloud (jedna databáze, každý zákazník má licenci v x01License).
    ///
    /// Průběh:
    ///  1. /CreateLicenseFree/Index        - zadání e-mailu, nebo přihlášení přes Google
    ///  2. SendLink                        - na e-mail odejde ověřovací odkaz (platí 24 hodin)
    ///  3. /CreateLicenseFree/Complete?t=  - jméno, firma, obor, heslo
    ///  4. Create                          - založení licence (TotalCloudRegisterEngine) a rovnou přihlášení
    ///
    /// Přihlašovacím jménem je vždy ověřený e-mail. Odkaz nese zašifrovaný, časově omezený token (Data Protection),
    /// na serveru se k němu nic neukládá.
    ///
    /// Nastavení v appsettings.json (sekce "DbCreate", vše volitelné):
    ///   "TurnstileSiteKey": "", "TurnstileSecret": ""  - Cloudflare Turnstile u zadání e-mailu; bez obou klíčů se ochrana proti robotům nepoužije
    ///   "AnonymousMaxPerHour": 10                      - kolik licencí lze celkem založit za hodinu
    ///   "AnonymousMaxPerIpPerDay": 5                   - kolik licencí lze založit z jedné IP adresy za 24 hodin
    ///   "PublicAppHost": ""                            - adresa aplikace do e-mailů a do licence, např. https://app.marktime.net; prázdné = adresa požadavku
    ///   "NotifyEmail": ""                              - kam chodí zpráva o každé registraci
    ///   "MailFrom": "noreply@marktime.net"
    /// </summary>
    public class CreateLicenseFreeController : Controller
    {
        private readonly BL.Factory _f;
        private readonly IDataProtectionProvider _dp;
        private const string _defaultNotifyEmail = "jiri.theimer@marktime.cz";
        private const int _linkHours = 24;

        public CreateLicenseFreeController(BL.Factory f, IDataProtectionProvider dp)
        {
            _f = f;
            _dp = dp;
        }

        // ── pomocné ─────────────────────────────────────────────────────────────────

        private Microsoft.Extensions.Configuration.IConfigurationSection Cfg
        {
            get
            {
                return _f.App.Configuration.GetSection("DbCreate");
            }
        }

        private bool IsTotalCloud
        {
            get
            {
                return _f.App.HostingMode == BL.Singleton.HostingModeEnum.TotalCloud;
            }
        }

        private string RemoteIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "?";
        }

        private string MailFrom()
        {
            string s = Cfg["MailFrom"];
            return string.IsNullOrEmpty(s) ? "noreply@marktime.net" : s;
        }

        private string AppHost()
        {
            string s = Cfg["PublicAppHost"];
            if (string.IsNullOrEmpty(s))
            {
                s = $"{Request.Scheme}://{Request.Host}";
            }
            return s.TrimEnd('/');
        }

        /// <summary>Veřejný klíč Cloudflare Turnstile, nebo NULL, pokud ochrana proti robotům není nastavená (chybí některý z klíčů).</summary>
        private string TurnstileSiteKey()
        {
            if (string.IsNullOrEmpty(Cfg["TurnstileSiteKey"]) || string.IsNullOrEmpty(Cfg["TurnstileSecret"]))
            {
                return null;
            }
            return Cfg["TurnstileSiteKey"];
        }

        private static string NormalizeCountry(string s)
        {
            s = (s ?? "").Trim().ToUpper();
            return s == "SK" ? "SK" : "CZ";
        }

        private static string T(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        private static string H(string s)
        {
            return WebUtility.HtmlEncode(s ?? "");
        }

        /// <summary>E-mail malými písmeny, nebo NULL, pokud adresa není platná. E-mail je zároveň login a název složky licence.</summary>
        private static string NormalizeEmail(string s)
        {
            s = (s ?? "").Trim().ToLowerInvariant();
            if (s.Length < 6 || s.Length > 100 || s.Contains("..")) return null;
            if (!Regex.IsMatch(s, @"^[a-z0-9][a-z0-9._+\-]*@[a-z0-9][a-z0-9.\-]*\.[a-z]{2,}$")) return null;
            if (!BO.Code.Bas.IsValidEmail(s)) return null;
            return s;
        }

        private bool AccountExists(string strEmail)
        {
            return new DL.DbCreateTasks(_f.App.ConnectString).Scalar<int>("SELECT COUNT(*) FROM j02User WHERE j02Login=@e OR j02Email=@e", new { e = strEmail }) > 0;
        }

        private ITimeLimitedDataProtector Protector()
        {
            return _dp.CreateProtector("MARKTIME.TotalCloud.Register.v1").ToTimeLimitedDataProtector();
        }

        private string MakeToken(TotalCloudRegisterToken c, int hours)
        {
            return Protector().Protect(Newtonsoft.Json.JsonConvert.SerializeObject(c), TimeSpan.FromHours(hours));
        }

        /// <summary>Vrací NULL, pokud odkaz není náš, někdo ho pozměnil nebo už vypršel.</summary>
        private TotalCloudRegisterToken ReadToken(string t)
        {
            if (string.IsNullOrEmpty(t) || t.Length > 4000) return null;
            try
            {
                string s = Protector().Unprotect(t, out DateTimeOffset expiration);
                var c = Newtonsoft.Json.JsonConvert.DeserializeObject<TotalCloudRegisterToken>(s);
                if (c == null || NormalizeEmail(c.Email) != c.Email) return null;
                return c;
            }
            catch
            {
                return null;
            }
        }

        private bool SendMail(string strTo, string strSubject, string strHtml)
        {
            try
            {
                var res = _f.MailBL.SendMessageWithoutFactory(strHtml, strSubject, strTo, null, MailFrom(), "MARKTIME");
                if (!res.issuccess)
                {
                    BO.Code.File.LogError($"{strTo}: {res.Message}", "anonym", "CreateLicenseFree.SendMail");
                }
                return res.issuccess;
            }
            catch (Exception e)
            {
                BO.Code.File.LogError($"{strTo}: {e.Message}", "anonym", "CreateLicenseFree.SendMail");
                return false;
            }
        }

        private static string MailLayout(string strBody)
        {
            return "<div style=\"font-family:Segoe UI,Arial,sans-serif;font-size:15px;color:#1f2328;max-width:560px;\">" + strBody + "<p style=\"color:#626b74;font-size:13px;margin-top:28px;\">MARKTIME</p></div>";
        }

        private static string MailButton(string strUrl, string strText)
        {
            return $"<p style=\"margin:22px 0;\"><a href=\"{H(strUrl)}\" style=\"background:#1f87cf;color:#ffffff;text-decoration:none;padding:11px 22px;border-radius:8px;font-weight:600;display:inline-block;\">{H(strText)}</a></p>";
        }

        /// <summary>Ověří u Cloudflare token, který Turnstile přidal do formuláře. Bez nastavených klíčů vrací true.</summary>
        private async Task<bool> VerifyTurnstile()
        {
            if (TurnstileSiteKey() == null)
            {
                return true;    //ochrana proti robotům není nastavená
            }
            string strResponse = Request.Form["cf-turnstile-response"];
            if (string.IsNullOrEmpty(strResponse) || strResponse.Length > 4000) return false;

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var pars = new Dictionary<string, string>
                    {
                        { "secret", Cfg["TurnstileSecret"] },
                        { "response", strResponse },
                        { "remoteip", RemoteIp() }
                    };
                    var response = await client.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", new FormUrlEncodedContent(pars));
                    string strRet = await response.Content.ReadAsStringAsync();
                    var jo = Newtonsoft.Json.Linq.JObject.Parse(strRet);
                    if (jo.Value<bool?>("success") == true) return true;

                    //odmítnutí si zaslouží stopu: špatný tajný klíč nebo nepovolená doména by jinak zablokovaly všechny registrace potichu
                    BO.Code.File.LogError(strRet, "anonym", "CreateLicenseFree.VerifyTurnstile");
                    return false;
                }
            }
            catch (Exception e)
            {
                BO.Code.File.LogError(e.Message, "anonym", "CreateLicenseFree.VerifyTurnstile");
                return false;
            }
        }

        private static string GeneratePassword(BL.Code.PasswordSupport ps)
        {
            const string LOWER = "abcdefghijkmnpqrstuvwxyz";
            const string UPPER = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string DIGITS = "23456789";
            const string SPECIALS = "!.$_*#";

            int intLen = Math.Max(ps.MinLength, 16);
            if (ps.MaxLength > 0 && intLen > ps.MaxLength) intLen = ps.MaxLength;

            var chars = new List<char>();
            chars.Add(Pick(LOWER));
            chars.Add(Pick(UPPER));
            chars.Add(Pick(DIGITS));
            if (ps.RequireNonAlphanumeric) chars.Add(Pick(SPECIALS));
            while (chars.Count < intLen)
            {
                chars.Add(Pick(LOWER + UPPER + DIGITS));
            }
            for (int i = chars.Count - 1; i > 0; i--)
            {
                int j = System.Security.Cryptography.RandomNumberGenerator.GetInt32(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }
            return new string(chars.Take(intLen).ToArray());
        }
        private static char Pick(string s)
        {
            return s[System.Security.Cryptography.RandomNumberGenerator.GetInt32(s.Length)];
        }

        private static string GetPasswordHint(BL.Code.PasswordSupport ps)
        {
            var lis = new List<string>();
            lis.Add($"alespoň {ps.MinLength} znaků");
            if (ps.RequireUppercase) lis.Add("velké písmeno");
            if (ps.RequireLowercase) lis.Add("malé písmeno");
            if (ps.RequireDigit) lis.Add("číslici");
            if (ps.RequireNonAlphanumeric) lis.Add($"zvláštní znak ({BL.Code.PasswordSupport.SpecialChars})");
            return "Heslo musí mít " + string.Join(", ", lis) + ".";
        }

        // ── 1. zadání e-mailu ───────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Index(string countrycode)
        {
            var v = new TotalCloudRegisterViewModel() { PageTitle = "Registrace", CountryCode = NormalizeCountry(countrycode) };
            if (!IsTotalCloud)
            {
                v.IsClosed = true;
                v.Message = "Aplikace není v režimu TotalCloud.";
                return View(v);
            }
            v.TurnstileSiteKey = TurnstileSiteKey();
            v.AllowGoogle = _f.App.AllowGoogleLogin;
            if (TempData["GRegError"] is string strErr)
            {
                v.Message = strErr;
            }
            return View(v);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendLink(string email, string countrycode)
        {
            if (!IsTotalCloud)
            {
                return Json(new { ok = false, message = "Aplikace není v režimu TotalCloud." });
            }
            string strEmail = NormalizeEmail(email);
            if (strEmail == null)
            {
                return Json(new { ok = false, message = "Zadejte platnou e-mailovou adresu." });
            }
            string strIP = RemoteIp();
            int intMaxPerHour = Cfg.GetValue<int>("AnonymousMaxPerHour", 10);
            if (TotalCloudRegisterGuard.Count(TotalCloudRegisterGuard.KindLink, 1, strIP) >= 10
                || TotalCloudRegisterGuard.Count(TotalCloudRegisterGuard.KindLink, 1, null, strEmail) >= 3
                || TotalCloudRegisterGuard.Count(TotalCloudRegisterGuard.KindLink, 1) >= intMaxPerHour * 5)
            {
                return Json(new { ok = false, message = "Příliš mnoho pokusů. Zkuste to prosím za hodinu." });
            }
            if (!await VerifyTurnstile())
            {
                return Json(new { ok = false, captcha = true, message = "Ověření, že nejste robot, se nezdařilo. Zkuste to prosím znovu." });
            }
            TotalCloudRegisterGuard.Add(TotalCloudRegisterGuard.KindLink, strIP, strEmail);

            //odpověď je stejná, ať účet existuje nebo ne - z formuláře nesmí jít zjistit, kdo je u nás registrovaný
            bool bolSent;
            if (AccountExists(strEmail))
            {
                string s = "<p>Dobrý den,</p><p>někdo, nejspíš vy, se právě pokusil zaregistrovat do MARKTIME s touto e-mailovou adresou. Účet s ní už ale existuje.</p>";
                s += MailButton(AppHost() + "/Login/UserLogin", "Přihlásit se");
                s += $"<p>Přihlašovací jméno: <b>{H(strEmail)}</b><br>Pokud si nepamatujete heslo, použijte <a href=\"{H(AppHost())}/Password/Index\">obnovu hesla</a>.</p>";
                s += "<p>Pokud jste o registraci nežádali, tuto zprávu ignorujte.</p>";
                bolSent = SendMail(strEmail, "Váš účet MARKTIME už existuje", MailLayout(s));
            }
            else
            {
                string strToken = MakeToken(new TotalCloudRegisterToken() { Email = strEmail, CountryCode = NormalizeCountry(countrycode) }, _linkHours);
                string strUrl = AppHost() + "/CreateLicenseFree/Complete?t=" + WebUtility.UrlEncode(strToken);
                string s = "<p>Dobrý den,</p><p>děkujeme za zájem o MARKTIME. Registraci dokončíte kliknutím na tlačítko:</p>";
                s += MailButton(strUrl, "Dokončit registraci");
                s += $"<p>Odkaz platí {_linkHours} hodin. Pokud tlačítko nefunguje, zkopírujte do prohlížeče tuto adresu:<br><span style=\"font-size:12px;color:#626b74;word-break:break-all;\">{H(strUrl)}</span></p>";
                s += "<p>Pokud jste o registraci nežádali, tuto zprávu ignorujte. Žádný účet bez kliknutí na odkaz nevznikne.</p>";
                bolSent = SendMail(strEmail, "Dokončete registraci do MARKTIME", MailLayout(s));
            }
            if (!bolSent)
            {
                return Json(new { ok = false, message = "E-mail se nepodařilo odeslat. Zkuste to prosím za chvíli znovu." });
            }

            return Json(new { ok = true, email = strEmail });
        }

        // ── Google: e-mail ověřil Google ────────────────────────────────────────────

        [HttpGet]
        public IActionResult GoogleSignIn(string countrycode)
        {
            if (!IsTotalCloud)
            {
                return RedirectToAction("Index");
            }
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback", "CreateLicenseFree", new { countrycode = NormalizeCountry(countrycode) })
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback(string countrycode)
        {
            //Google middleware uložil claims do "ExternalCookie" (SignInScheme ve Startup.cs)
            var result = await HttpContext.AuthenticateAsync("ExternalCookie");
            if (result?.Succeeded != true)
            {
                TempData["GRegError"] = "Přihlášení přes Google se nepodařilo. Zkuste to prosím znovu.";
                return RedirectToAction("Index");
            }

            string strEmail = NormalizeEmail(result.Principal.FindFirst(ClaimTypes.Email)?.Value);
            string strFirstName = result.Principal.FindFirst(ClaimTypes.GivenName)?.Value;
            string strLastName = result.Principal.FindFirst(ClaimTypes.Surname)?.Value;
            await HttpContext.SignOutAsync("ExternalCookie");

            if (!IsTotalCloud)
            {
                return RedirectToAction("Index");
            }
            if (strEmail == null)
            {
                TempData["GRegError"] = "Google účet neposkytl použitelnou e-mailovou adresu.";
                return RedirectToAction("Index");
            }
            if (AccountExists(strEmail))
            {
                return Redirect("/Login/GoogleLogin");  //účet už má - rovnou ho přihlásit
            }

            string strToken = MakeToken(new TotalCloudRegisterToken() { Email = strEmail, IsGoogle = true, FirstName = T(strFirstName), LastName = T(strLastName), CountryCode = NormalizeCountry(countrycode) }, 2);
            return RedirectToAction("Complete", new { t = strToken });
        }

        // ── 3. dokončení registrace ─────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Complete(string t)
        {
            Response.Headers["Referrer-Policy"] = "no-referrer";    //adresa stránky obsahuje ověřovací token
            Response.Headers["Cache-Control"] = "no-store";

            var v = new TotalCloudRegisterViewModel() { PageTitle = "Dokončení registrace" };
            if (!IsTotalCloud)
            {
                v.IsClosed = true;
                v.Message = "Aplikace není v režimu TotalCloud.";
                return View(v);
            }
            var c = ReadToken(t);
            if (c == null)
            {
                v.IsLinkInvalid = true;
                return View(v);
            }
            v.Email = c.Email;
            if (AccountExists(c.Email))
            {
                v.IsAccountExists = true;
                return View(v);
            }

            v.Token = t;
            v.IsGoogle = c.IsGoogle;
            v.FirstName = c.FirstName;
            v.LastName = c.LastName;
            v.CountryCode = NormalizeCountry(c.CountryCode);
            v.Profiles = TotalCloudProfile.GetList();
            v.PasswordHint = GetPasswordHint(new BL.Code.PasswordSupport());
            return View(v);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoadRejstrik(string t, string regid, string country)
        {
            if (!IsTotalCloud || ReadToken(t) == null)
            {
                return Json(new { ok = false, message = "Platnost odkazu vypršela." });
            }
            string ico = Regex.Replace(regid ?? "", @"\s", "");
            if (!Regex.IsMatch(ico, "^[0-9]{6,10}$"))
            {
                return Json(new { ok = false, message = "Zadejte platné IČO." });
            }
            if (TotalCloudRegisterGuard.Count("rejstrik", 1, RemoteIp()) >= 20)
            {
                return Json(new { ok = false, message = "Příliš mnoho dotazů do rejstříku, vyplňte prosím název ručně." });
            }
            TotalCloudRegisterGuard.Add("rejstrik", RemoteIp(), null);

            var c = await LoadFromRegistry(ico, NormalizeCountry(country));
            if (c == null || string.IsNullOrEmpty(c.name))
            {
                return Json(new { ok = false, message = $"Subjekt s IČO {ico} se v rejstříku nepodařilo najít. Vyplňte prosím název ručně." });
            }
            return Json(new { ok = true, name = c.name, ico = c.ico });
        }

        /// <summary>Vrací NULL, pokud rejstřík neodpoví do 8 sekund nebo subjekt nezná.</summary>
        private async Task<BO.Rejstrik.DefaultZaznam> LoadFromRegistry(string strICO, string strCountry)
        {
            try
            {
                var viesClient = HttpContext.RequestServices.GetRequiredService<BL.Code.IViesClient>();
                var engine = new BL.Code.RejstrikySupport(viesClient);
                var task = engine.LoadDefaultZaznam("ico", strICO, strCountry, new HttpClient());
                if (await Task.WhenAny(task, Task.Delay(8000)) != task)
                {
                    return null;
                }
                return await task;
            }
            catch
            {
                return null;
            }
        }

        // ── 4. založení licence ─────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string t, string firstname, string lastname, string company, string regid, string country, string profile, string password)
        {
            if (!IsTotalCloud)
            {
                return Json(new { ok = false, message = "Aplikace není v režimu TotalCloud." });
            }
            var c = ReadToken(t);
            if (c == null)
            {
                return Json(new { ok = false, expired = true, message = "Platnost odkazu vypršela. Požádejte prosím o nový." });
            }

            var ps = new BL.Code.PasswordSupport();
            var r = new TotalCloudRegisterRequest()
            {
                Email = c.Email,
                IsGoogle = c.IsGoogle,
                FirstName = T(firstname),
                LastName = T(lastname),
                CompanyName = T(company),
                RegID = T(Regex.Replace(regid ?? "", @"\s", "")),
                CountryCode = NormalizeCountry(country),
                ProfileKey = profile,
                Password = password,
                AppHost = AppHost(),
                RemoteIp = RemoteIp()
            };

            //kontrola zadání
            var errors = new List<object>();
            if (r.FirstName == null) errors.Add(new { field = "firstname", message = "Vyplňte jméno." });
            if (r.LastName == null) errors.Add(new { field = "lastname", message = "Vyplňte příjmení." });
            if (r.CompanyName == null) errors.Add(new { field = "company", message = "Vyplňte název firmy." });
            if (r.RegID != null && !Regex.IsMatch(r.RegID, "^[0-9]{6,10}$")) errors.Add(new { field = "regid", message = "IČO tvoří jen číslice. Pole můžete nechat prázdné." });
            if (r.IsGoogle)
            {
                r.Password = GeneratePassword(ps);  //přihlašuje se přes Google; vlastní heslo si může nastavit přes obnovu hesla
            }
            else if (string.IsNullOrEmpty(r.Password))
            {
                errors.Add(new { field = "password", message = "Zvolte si heslo." });
            }
            else
            {
                var res = ps.CheckPassword(r.Password);
                if (res.Flag == BO.ResultEnum.Failed) errors.Add(new { field = "password", message = res.Message });
            }
            if (errors.Count > 0)
            {
                return Json(new { ok = false, errors = errors });
            }

            if (TotalCloudRegisterGuard.Count(TotalCloudRegisterGuard.KindCreate, 24, r.RemoteIp) >= Cfg.GetValue<int>("AnonymousMaxPerIpPerDay", 5)
                || TotalCloudRegisterGuard.Count(TotalCloudRegisterGuard.KindCreate, 1) >= Cfg.GetValue<int>("AnonymousMaxPerHour", 10))
            {
                return Json(new { ok = false, message = "Z kapacitních důvodů teď nový účet založit nelze. Zkuste to prosím za hodinu, nebo nám napište." });
            }

            //rejstřík jen doplňuje adresu a DIČ; když neodpoví, zakládá se bez nich
            if (r.RegID != null)
            {
                var z = await LoadFromRegistry(r.RegID, r.CountryCode);
                if (z != null && !string.IsNullOrEmpty(z.name))
                {
                    r.VatID = T(z.dic);
                    r.Street = T(z.street);
                    r.City = T(z.city);
                    r.Zip = T(z.zipcode);
                }
            }

            if (!await TotalCloudRegisterGuard.EngineLock.WaitAsync(TimeSpan.FromSeconds(60)))
            {
                return Json(new { ok = false, message = "Server je teď vytížený. Zkuste to prosím za chvíli znovu." });
            }
            TotalCloudRegisterResult ret;
            try
            {
                var engine = new TotalCloudRegisterEngine(_f, r);
                string strInvalid = engine.Validate();
                if (strInvalid != null)
                {
                    return Json(new { ok = false, exists = AccountExists(r.Email), message = strInvalid });
                }
                ret = engine.Run();
            }
            catch (Exception e)
            {
                ret = new TotalCloudRegisterResult() { Ok = false, Error = e.Message, Login = r.Login };
                BO.Code.File.LogError(e.ToString(), r.Login, "CreateLicenseFree.Create");
            }
            finally
            {
                TotalCloudRegisterGuard.EngineLock.Release();
            }

            SendNotifyMail(r, ret);
            if (!ret.Ok)
            {
                return Json(new { ok = false, message = "Účet se teď nepodařilo založit. Chybu jsme zaznamenali a ozveme se vám na e-mail. Můžete to také zkusit znovu." });
            }
            TotalCloudRegisterGuard.Add(TotalCloudRegisterGuard.KindCreate, r.RemoteIp, r.Email);

            //rovnou přihlásit, stejně jako LoginController.SetClaim
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, r.Login),
                new Claim("access_token", "inspis_core_token"),
                new Claim(ClaimTypes.Email, r.Email)
            };
            var principal = new ClaimsPrincipal(new[] { new ClaimsIdentity(claims, "User Identity") });
            await HttpContext.SignInAsync(principal, new AuthenticationProperties() { IsPersistent = true, ExpiresUtc = DateTime.Now.AddHours(168) });
            Response.Cookies.Append("marktime.cookieexpiresinhours", "168", new CookieOptions() { Expires = DateTime.Now.AddDays(100) });

            SendWelcomeMail(r);

            return Json(new { ok = true, redirect = Url.Action("Index", "Home") });
        }

        private void SendWelcomeMail(TotalCloudRegisterRequest r)
        {
            string strPassword = r.IsGoogle ? "přihlašujete se tlačítkem Přihlásit se přes Google" : "heslo jste si zvolili při registraci";
            string strHtml = null;
            try
            {
                string strPath = _f.App.RootUploadFolder + "\\_distribution\\mail\\send_message_template_cloud_confirm.html";
                if (System.IO.File.Exists(strPath))
                {
                    strHtml = BO.Code.File.GetFileContent(strPath);
                }
            }
            catch
            {
                strHtml = null;
            }
            if (!string.IsNullOrEmpty(strHtml))
            {
                strHtml = strHtml.Replace("#username#", H(r.Login)).Replace("#password#", strPassword);
            }
            else
            {
                string s = $"<p>Dobrý den,</p><p>váš účet MARKTIME pro <b>{H(r.CompanyName)}</b> je připravený.</p>";
                s += MailButton(r.AppHost + "/Login/UserLogin", "Otevřít MARKTIME");
                s += $"<p>Přihlašovací jméno: <b>{H(r.Login)}</b><br>Heslo: {strPassword}.</p>";
                strHtml = MailLayout(s);
            }
            SendMail(r.Email, $"MARKTIME účet [{r.Login}] založen", strHtml);
        }

        /// <summary>Zpráva správci o výsledku každé registrace, včetně varování a chyb.</summary>
        private void SendNotifyMail(TotalCloudRegisterRequest r, TotalCloudRegisterResult ret)
        {
            string strTo = Cfg["NotifyEmail"];
            if (string.IsNullOrEmpty(strTo)) strTo = _defaultNotifyEmail;

            var sb = new System.Text.StringBuilder();
            sb.Append($"<p><b>{(ret.Ok ? "Licence založena" : "ZALOŽENÍ SELHALO")}</b> ({(r.IsGoogle ? "Google" : "e-mail")})</p>");
            sb.Append($"<p>Firma: {H(r.CompanyName)}<br>IČO: {H(r.RegID)}<br>Jméno: {H(r.FirstName)} {H(r.LastName)}<br>E-mail / login: {H(r.Email)}<br>Země: {H(r.CountryCode)}<br>Obor: {H(TotalCloudProfile.Find(r.ProfileKey).Name)}<br>IP: {H(r.RemoteIp)}<br>x01ID: {ret.x01ID}, tabulek: {ret.Tables}, řádků: {ret.Rows}</p>");
            if (!ret.Ok)
            {
                sb.Append($"<p><b>Chyba:</b> {H(ret.Error)}</p><p>Rozpracovaná licence byla uzavřena a přejmenována (failed-…), e-mail je volný pro další pokus. SQL log je ve složce Logs (register-failed-…).</p>");
            }
            if (ret.Warnings.Count > 0)
            {
                sb.Append("<p><b>Varování:</b></p><ul>");
                foreach (string s in ret.Warnings.Take(40))
                {
                    sb.Append($"<li>{H(s)}</li>");
                }
                sb.Append("</ul>");
            }
            string strSubject = ret.Ok ? $"Nová MARKTIME licence: {r.CompanyName}" : $"CHYBA registrace MARKTIME: {r.Email}";
            if (ret.Ok && ret.Warnings.Count > 0) strSubject += $" ({ret.Warnings.Count} varování)";
            SendMail(strTo, strSubject, sb.ToString());
        }
    }
}
