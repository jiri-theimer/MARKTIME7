using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using UI.Code.DbCreate;
using UI.Models.Guru;

namespace UI.Controllers.guru
{
    /// <summary>
    /// Průvodce založením nové zákaznické databáze: /CreateLicense/Index
    ///
    /// Jedním formulářem vznikne SQL databáze, tabulky, SQL objekty, výchozí data, licence, první uživatel, složky a sestavy.
    /// Vlastní práci dělá UI.Code.DbCreate.DbCreateEngine po krocích - stránka volá RunStep pro každý krok zvlášť a ukazuje průběh.
    ///
    /// Přístup:
    ///  - guru (přihlášení účtem Guru:Login z appsettings.json) - všechna nastavení
    ///  - anonym - pouze pokud je v appsettings.json "DbCreate": { "AllowAnonymous": true } a aplikace běží v režimu SharedApp
    ///
    /// Volitelné nastavení v appsettings.json:
    ///  "ConnectionStrings": { "DbCreateMaster": "..." }   - jiný login pro CREATE DATABASE (výchozí je AppConnection)
    ///  "DbCreate": {
    ///     "AllowAnonymous": false,          - veřejná samoobsluha
    ///     "TrustLocalhost": false,          - požadavky z localhostu berou se jako guru (vývojářský stroj)
    ///     "AnonymousLimitUsers": 10,        - limit uživatelů licence založené anonymem
    ///     "AnonymousMaxPerHour": 10,        - kolik databází smí anonymové založit za hodinu (celkem)
    ///     "PublicAppHost": "https://app.marktime.net",
    ///     "RecaptchaSiteKey": "", "RecaptchaSecret": "",
    ///     "NotifyEmail": "",                - komu poslat informaci o každé založené databázi
    ///     "MailFrom": "noreply@marktime.net",
    ///     "CreateDbScriptPath": ""          - jiná cesta ke create_db.sql
    ///  }
    /// </summary>
    public class CreateLicenseController : Controller
    {
        private readonly BL.Factory _f;
        private static readonly string[] _countries = new[] { "CZ", "SK", "AT", "DE", "PL", "GB", "CH", "US" };
        private static readonly int[] _currencies = new[] { 2, 3, 58, 59, 56, 50 };   //CZK, EUR, USD, GBP, CHF, PLN
        private static readonly int[] _langs = new[] { 0, 1, 4 };

        public CreateLicenseController(BL.Factory f)
        {
            _f = f;
        }

        // ── přístup ─────────────────────────────────────────────────────────────────

        private Microsoft.Extensions.Configuration.IConfigurationSection Cfg
        {
            get
            {
                return _f.App.Configuration.GetSection("DbCreate");
            }
        }
        private bool IsSharedApp
        {
            get
            {
                return _f.App.HostingMode == BL.Singleton.HostingModeEnum.SharedApp;
            }
        }

        private bool IsGuru()
        {
            string strGuruLogin = _f.App.GuruLogin;
            if (!string.IsNullOrEmpty(strGuruLogin) && User?.Identity != null && User.Identity.IsAuthenticated && string.Equals(User.Identity.Name, strGuruLogin, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (Cfg.GetValue<bool>("TrustLocalhost"))
            {
                var ip = HttpContext.Connection.RemoteIpAddress;
                if (ip != null && System.Net.IPAddress.IsLoopback(ip))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsAnonymousAllowed()
        {
            return Cfg.GetValue<bool>("AllowAnonymous") && IsSharedApp;
        }

        /// <summary>Vrací NULL, pokud volající smí stránku používat, jinak důvod odmítnutí.</summary>
        private string GetDeniedMessage(bool bolGuru)
        {
            if (_f.App.HostingMode == BL.Singleton.HostingModeEnum.TotalCloud)
            {
                return "Aplikace běží v režimu TotalCloud (všechny licence v jedné databázi). Novou licenci založíte na stránce /CreateLicenseFree/Index.";
            }
            if (bolGuru || IsAnonymousAllowed())
            {
                return null;
            }
            return "Zakládat nové databáze může pouze správce systému. Přihlaste se prosím účtem Guru.";
        }

        private string RemoteIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "?";
        }

        private JsonResult JsonDenied(string strMessage)
        {
            Response.StatusCode = 403;
            return Json(new { ok = false, message = strMessage });
        }

        // ── stránka ─────────────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Index()
        {
            bool bolGuru = IsGuru();
            var v = new DbCreateViewModel() { PageTitle = "Nová databáze", IsGuru = bolGuru, IsSharedApp = IsSharedApp };

            v.DeniedMessage = GetDeniedMessage(bolGuru);
            if (v.DeniedMessage != null)
            {
                v.IsAccessDenied = true;
                return View(v);
            }

            var ps = new BL.Code.PasswordSupport();
            v.PwdMinLength = ps.MinLength;
            v.PwdMaxLength = ps.MaxLength;
            v.PwdRequireDigit = ps.RequireDigit;
            v.PwdRequireLowercase = ps.RequireLowercase;
            v.PwdRequireUppercase = ps.RequireUppercase;
            v.PwdRequireNonAlphanumeric = ps.RequireNonAlphanumeric;
            v.DefaultAppHost = GetDefaultAppHost(bolGuru);

            if (bolGuru)
            {
                v.GeneratedPassword = GeneratePassword(ps);
                v.ServerName = DL.DbCreateTasks.GetServerName(GetBaseConnectString());
                if (!IsSharedApp)
                {
                    v.DefaultDbName = _f.App.ParseDbNameFromConnectString();
                }
            }
            else
            {
                v.RecaptchaSiteKey = Cfg["RecaptchaSiteKey"];
            }

            return View(v);
        }

        private string GetDefaultAppHost(bool bolGuru)
        {
            string s = Cfg["PublicAppHost"];
            if (string.IsNullOrEmpty(s))
            {
                s = $"{Request.Scheme}://{Request.Host}";
            }
            return s;
        }

        private string GetBaseConnectString()
        {
            string s = _f.App.Configuration.GetSection("ConnectionStrings")["DbCreateMaster"];
            if (string.IsNullOrEmpty(s))
            {
                s = _f.App.ConnectString;
            }
            return s;
        }

        private static string GeneratePassword(BL.Code.PasswordSupport ps)
        {
            const string LOWER = "abcdefghijkmnpqrstuvwxyz";
            const string UPPER = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string DIGITS = "23456789";
            const string SPECIALS = "!.$_*#";

            int intLen = Math.Max(ps.MinLength, 12);
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
            //zamíchat
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

        // ── ajax: rejstřík, doména, heslo ───────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LoadRejstrik(string ico, string country)
        {
            if (GetDeniedMessage(IsGuru()) != null) return JsonDenied("Přístup odepřen.");

            ico = Regex.Replace(ico ?? "", @"\s", "");
            if (!Regex.IsMatch(ico, "^[0-9A-Za-z]{4,14}$"))
            {
                return Json(new { ok = false, message = "Zadejte platné IČO." });
            }
            if (!_countries.Contains(country)) country = "CZ";

            try
            {
                var viesClient = HttpContext.RequestServices.GetRequiredService<BL.Code.IViesClient>();
                var engine = new BL.Code.RejstrikySupport(viesClient);
                var c = engine.LoadDefaultZaznam("ico", ico, country, new HttpClient()).Result;
                if (c == null || string.IsNullOrEmpty(c.name))
                {
                    return Json(new { ok = false, message = $"Subjekt s IČO {ico} se v rejstříku nepodařilo najít." });
                }
                return Json(new { ok = true, name = c.name, street = c.street, city = c.city, zip = c.zipcode, dic = c.dic, ico = c.ico });
            }
            catch
            {
                return Json(new { ok = false, message = "Rejstřík teď neodpovídá, vyplňte prosím údaje ručně." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckDomain(string domain, string dbname)
        {
            bool bolGuru = IsGuru();
            if (GetDeniedMessage(bolGuru) != null) return JsonDenied("Přístup odepřen.");

            domain = (domain ?? "").Trim().ToLower();
            if (!IsValidDomain(domain))
            {
                return Json(new { ok = false, message = "Doména má mít tvar nasefirma.cz (malá písmena bez diakritiky a číslice)." });
            }

            string strDbName = ResolveDbName(domain, dbname, bolGuru);
            if (!DL.DbCreateTasks.IsValidDbName(strDbName))
            {
                return Json(new { ok = false, message = "Neplatný název databáze." });
            }

            try
            {
                var master = new DL.DbCreateTasks(DL.DbCreateTasks.BuildConnectString(GetBaseConnectString(), "master"), 15);
                if (IsSharedApp)
                {
                    string strPrefix = domain.Split('.')[0];
                    int x = master.Scalar<int>("SELECT COUNT(*) FROM a7_cloudheader.dbo.x01License WHERE x01LoginDomain=@dom OR x01LoginDomain LIKE @prefix", new { dom = domain, prefix = strPrefix + ".%" });
                    if (x > 0)
                    {
                        return Json(new { ok = false, dbname = strDbName, message = $"Doména [{strPrefix}] je už obsazená." });
                    }
                }
                if (master.DatabaseExists(strDbName))
                {
                    if (bolGuru)
                    {
                        return Json(new { ok = true, exists = true, dbname = strDbName, message = $"Databáze {strDbName} už na serveru existuje - pokud v ní není licence, průvodce ji použije." });
                    }
                    return Json(new { ok = false, dbname = strDbName, message = "Tato doména je už obsazená." });
                }
                return Json(new { ok = true, exists = false, dbname = strDbName, message = "Doména je volná." });
            }
            catch (Exception e)
            {
                BO.Code.File.LogError(e.Message, "dbcreate", "CheckDomain");
                return Json(new { ok = false, dbname = strDbName, message = bolGuru ? "Nelze ověřit: " + e.Message : "Dostupnost domény se teď nepodařilo ověřit." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NewPassword()
        {
            if (GetDeniedMessage(IsGuru()) != null) return JsonDenied("Přístup odepřen.");

            return Json(new { ok = true, password = GeneratePassword(new BL.Code.PasswordSupport()) });
        }

        // ── ajax: start + kroky ─────────────────────────────────────────────────────

        private static bool IsValidDomain(string s)
        {
            return Regex.IsMatch(s ?? "", @"^[a-z0-9]{2,40}(\.[a-z0-9]{2,20})+$");
        }

        private string ResolveDbName(string strDomain, string strExplicitDbName, bool bolGuru)
        {
            if (IsSharedApp)
            {
                return "a7" + strDomain.Split('.')[0];  //v cloudu se databáze odvozuje z loginu: a7 + část domény před tečkou
            }
            if (bolGuru && !string.IsNullOrWhiteSpace(strExplicitDbName))
            {
                return strExplicitDbName.Trim();
            }
            return _f.App.ParseDbNameFromConnectString();
        }

        private static string T(string s)
        {
            if (s == null) return null;
            s = s.Trim();
            return s == "" ? null : s;
        }

        private void Normalize(DbCreateRequest r, bool bolGuru)
        {
            r.CompanyName = T(r.CompanyName); r.RegID = T(r.RegID); r.VatID = T(r.VatID); r.IcDphSk = T(r.IcDphSk);
            r.Street = T(r.Street); r.City = T(r.City); r.Zip = T(r.Zip); r.Registration = T(r.Registration);
            r.FirstName = T(r.FirstName); r.LastName = T(r.LastName); r.Email = T(r.Email);
            r.LoginName = T(r.LoginName)?.ToLower();
            r.LoginDomain = T(r.LoginDomain)?.ToLower();
            r.LicenseName = T(r.LicenseName); r.AppName = T(r.AppName); r.AppHost = T(r.AppHost); r.DbName = T(r.DbName);
            r.ClientCompanyName = T(r.ClientCompanyName); r.ClientRegID = T(r.ClientRegID); r.ClientVatID = T(r.ClientVatID); r.ClientIcDphSk = T(r.ClientIcDphSk);
            r.ClientStreet = T(r.ClientStreet); r.ClientCity = T(r.ClientCity); r.ClientZip = T(r.ClientZip);
            r.Project1 = T(r.Project1); r.Project2 = T(r.Project2);

            if (!_countries.Contains(r.CountryCode)) r.CountryCode = "CZ";
            if (!_langs.Contains(r.LangIndex)) r.LangIndex = 0;

            if (!bolGuru)
            {
                //anonym nesmí ovlivnit technická nastavení licence
                r.LicenseName = null; r.AppName = null; r.DbName = null;
                r.AppHost = GetDefaultAppHost(false);
                r.CurrencyID = 0;
                r.Round2Minutes = 15;
                r.LimitUsers = Cfg.GetValue<int>("AnonymousLimitUsers", 10);
                r.SendWelcomeMail = true;
                if (r.CountryCode != "CZ" && r.CountryCode != "SK") r.CountryCode = "CZ";

                //ukázkový klient jako u /CreateLicenseFree
                r.CreateClient = true;
                r.ClientCompanyName = "Marktime Software s.r.o."; r.ClientRegID = "25722034"; r.ClientVatID = "CZ25722034"; r.ClientIcDphSk = null;
                r.ClientStreet = "Slezská 2232/144"; r.ClientCity = "Praha 3 Vinohrady"; r.ClientZip = "13000";
                r.Project1 = "General"; r.Project2 = $"Účetnictví {DateTime.Now.Year}";
            }

            if (r.LicenseName == null) r.LicenseName = r.CompanyName;
            if (r.AppName == null && r.CompanyName != null)
            {
                //do hlavního menu krátký název bez právní formy
                string s = Regex.Replace(r.CompanyName, @",?\s*(spol\.\s*s\s*r\.\s*o\.|s\.\s*r\.\s*o\.|a\.\s*s\.|v\.\s*o\.\s*s\.|k\.\s*s\.|z\.\s*s\.)", "", RegexOptions.IgnoreCase).Trim();
                if (s == "") s = r.CompanyName;
                r.AppName = s.Length > 20 ? s.Substring(0, 20).Trim() : s;
            }
            if (r.AppHost == null) r.AppHost = GetDefaultAppHost(bolGuru);
            if (r.AppHost.EndsWith("/")) r.AppHost = r.AppHost.TrimEnd('/');
            if (!_currencies.Contains(r.CurrencyID))
            {
                r.CurrencyID = (r.CountryCode == "CZ" ? 2 : 3);     //CZK, jinak EUR
            }
            if (r.Round2Minutes <= 0) r.Round2Minutes = 5;
            if (r.LimitUsers <= 0) r.LimitUsers = 99;
        }

        private List<DbCreateFieldError> Validate(DbCreateRequest r, bool bolGuru)
        {
            var lis = new List<DbCreateFieldError>();
            void err(string field, string message) { lis.Add(new DbCreateFieldError() { field = field, message = message }); }

            if (r.CompanyName == null) err("CompanyName", "Vyplňte název firmy.");
            if (r.CompanyName != null && r.CompanyName.Length > 200) err("CompanyName", "Název firmy je příliš dlouhý.");
            if (r.FirstName == null) err("FirstName", "Vyplňte jméno.");
            if (r.LastName == null) err("LastName", "Vyplňte příjmení.");
            if (r.Email == null || !BO.Code.Bas.IsValidEmail(r.Email)) err("Email", "Vyplňte platnou e-mail adresu.");

            if (r.LoginDomain == null || !IsValidDomain(r.LoginDomain))
            {
                err("LoginDomain", "Doména má mít tvar nasefirma.cz (malá písmena bez diakritiky a číslice).");
            }
            if (r.LoginName == null || !Regex.IsMatch(r.LoginName, @"^[a-z0-9][a-z0-9._-]{0,39}$"))
            {
                err("LoginName", "Přihlašovací jméno smí obsahovat malá písmena bez diakritiky, číslice, tečku, pomlčku a podtržítko.");
            }

            if (string.IsNullOrEmpty(r.Password))
            {
                err("Password", "Vyplňte heslo.");
            }
            else
            {
                var res = new BL.Code.PasswordSupport().CheckPassword(r.Password);
                if (res.Flag == BO.ResultEnum.Failed) err("Password", res.Message);
            }

            if (r.AppName != null && r.AppName.Length > 50) err("AppName", "Název aplikace smí mít nejvýše 50 znaků.");
            if (r.AppHost != null && !Regex.IsMatch(r.AppHost, @"^https?://[^\s/]+(/[^\s]*)?$")) err("AppHost", "Adresa aplikace má začínat http:// nebo https://.");
            if (r.Round2Minutes > 60) err("Round2Minutes", "Zaokrouhlení může být nejvýše 60 minut.");
            if (r.LimitUsers > 100000) err("LimitUsers", "Limit uživatelů je příliš vysoký.");

            if (r.CreateClient && bolGuru && r.ClientCompanyName == null)
            {
                err("ClientCompanyName", "Vyplňte název prvního klienta, nebo jeho založení vypněte.");
            }

            return lis;
        }

        private async Task<bool> VerifyCaptcha()
        {
            string strSecret = Cfg["RecaptchaSecret"];
            if (string.IsNullOrEmpty(strSecret) || string.IsNullOrEmpty(Cfg["RecaptchaSiteKey"]))
            {
                return true;    //captcha není nastavená
            }
            string strResponse = Request.Form["g-recaptcha-response"];
            if (string.IsNullOrEmpty(strResponse)) return false;

            try
            {
                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(new Dictionary<string, string> { { "secret", strSecret }, { "response", strResponse } });
                    var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                    string strRet = await response.Content.ReadAsStringAsync();
                    var jo = Newtonsoft.Json.Linq.JObject.Parse(strRet);
                    return jo.Value<bool?>("success") == true;
                }
            }
            catch (Exception e)
            {
                BO.Code.File.LogError(e.Message, "anonym", "DbCreate.VerifyCaptcha");
                return false;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(DbCreateRequest r)
        {
            bool bolGuru = IsGuru();
            if (GetDeniedMessage(bolGuru) != null) return JsonDenied("Přístup odepřen.");
            if (r == null) return Json(new { ok = false, message = "Chybí zadání." });

            if (!bolGuru)
            {
                if (!string.IsNullOrEmpty(Request.Form["Website"]))
                {
                    return Json(new { ok = false, message = "Požadavek se nepodařilo zpracovat." });    //past na roboty: skryté pole má zůstat prázdné
                }
                if (DbCreateJobStore.CountAnonymousByIp(RemoteIp(), 24) >= 2 || DbCreateJobStore.CountAnonymous(1) >= Cfg.GetValue<int>("AnonymousMaxPerHour", 10))
                {
                    return Json(new { ok = false, message = "Z kapacitních důvodů teď nelze založit další databázi. Zkuste to prosím později nebo nás kontaktujte." });
                }
                if (!await VerifyCaptcha())
                {
                    return Json(new { ok = false, message = "Potvrďte prosím, že nejste robot." });
                }
            }

            Normalize(r, bolGuru);
            var errors = Validate(r, bolGuru);
            if (errors.Count > 0)
            {
                return Json(new { ok = false, message = "Opravte prosím označená pole.", errors = errors });
            }

            var job = new DbCreateJob() { Request = r, IsGuru = bolGuru, RemoteIp = RemoteIp() };
            job.DbName = ResolveDbName(r.LoginDomain, r.DbName, bolGuru);
            if (!DL.DbCreateTasks.IsValidDbName(job.DbName))
            {
                return Json(new { ok = false, message = $"Neplatný název databáze [{job.DbName}].", errors = new[] { new DbCreateFieldError() { field = IsSharedApp ? "LoginDomain" : "DbName", message = "Neplatný název databáze." } } });
            }
            var prev = DbCreateJobStore.FindUnfinished(job.DbName);
            if (prev != null)
            {
                if (prev.IsGuru != bolGuru || (!bolGuru && prev.RemoteIp != job.RemoteIp))
                {
                    return Json(new { ok = false, message = bolGuru ? $"Databázi [{job.DbName}] právě zakládá někdo jiný." : "Tato doména je už obsazená." });
                }
                lock (prev.Locker)
                {
                    job.TakeStateFrom(prev);    //návrat z chyby zpět do formuláře: navázat na hotové kroky
                    prev.Finished = true;
                }
                DbCreateJobStore.Remove(prev.Id);
            }

            job.MasterConnectString = DL.DbCreateTasks.BuildConnectString(GetBaseConnectString(), "master");
            job.TargetConnectString = DL.DbCreateTasks.BuildConnectString(GetBaseConnectString(), job.DbName);
            job.Steps = DbCreateEngine.BuildSteps(r);
            DbCreateJobStore.Add(job);

            return Json(new
            {
                ok = true,
                job = job.Id,
                dbname = job.DbName,
                login = r.UserLogin,
                apphost = r.AppHost,
                steps = job.Steps.Select(p => new { key = p.Key, title = p.Title })
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RunStep(string job, int step)
        {
            bool bolGuru = IsGuru();
            if (GetDeniedMessage(bolGuru) != null) return JsonDenied("Přístup odepřen.");

            var c = DbCreateJobStore.Get(job);
            if (c == null)
            {
                return Json(new { ok = false, status = DbCreateStepStatus.Error, message = "Zakládání už není v paměti serveru (aplikace byla nejspíš restartována). Odešlete formulář znovu se stejnou doménou a přihlašovacím jménem - průvodce naváže tam, kde skončil." });
            }
            if (c.IsGuru != bolGuru || (!bolGuru && c.RemoteIp != RemoteIp()))
            {
                return JsonDenied("Přístup odepřen.");
            }
            if (step < 0 || step >= c.Steps.Count)
            {
                return Json(new { ok = false, status = DbCreateStepStatus.Error, message = "Neznámý krok." });
            }

            lock (c.Locker)
            {
                if (c.Finished)
                {
                    return Json(new { ok = false, status = DbCreateStepStatus.Error, message = "Databáze je už založená." });
                }
                if (c.Steps.Take(step).Any(p => !p.IsDone))
                {
                    return Json(new { ok = false, status = DbCreateStepStatus.Error, message = "Nejdříve musí doběhnout předchozí kroky." });
                }

                var s = c.Steps[step];
                if (!s.IsDone)
                {
                    new DbCreateEngine(_f, c).RunStep(s);
                }

                string strMessage = s.Message;
                var details = s.Details;
                if (!bolGuru && s.Status == DbCreateStepStatus.Error && s.Key != "preflight")
                {
                    //anonymovi neukazovat technické detaily chyb
                    strMessage = "Při zakládání došlo k chybě. Zkuste to prosím znovu, případně kontaktujte naši podporu.";
                }
                if (!bolGuru) details = new List<string>();

                return Json(new
                {
                    ok = s.IsDone,
                    status = s.Status,
                    message = strMessage,
                    details = details,
                    ms = s.DurationMs,
                    finished = c.Finished
                });
            }
        }
    }
}
