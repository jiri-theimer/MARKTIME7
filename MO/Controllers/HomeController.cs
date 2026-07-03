using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MO.Models;
using UAParser;

namespace MO.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            var v = new BaseViewModel { PageTitle = Factory.tra("Domů") };
            return View(v);
        }


        // ===== Úkony - seznam vykázaných úkonů uživatele s filtrem =====
        public IActionResult Ukony(string d1, string d2, int state = 0, int format = 0)
        {
            var todayNow = DateTime.Today;
            var defaultMonthStart = new DateTime(todayNow.Year, todayNow.Month, 1);
            var defaultMonthEnd = defaultMonthStart.AddMonths(1).AddDays(-1);

            var v = new UkonyListViewModel
            {
                PageTitle = format switch
                {
                    1 => Factory.tra("Hodiny"),
                    2 => Factory.tra("Peníze"),
                    3 => Factory.tra("Kusovník"),
                    _ => Factory.tra("Úkony")
                },
                HideHeaderTitle = true,
                DateFrom = ParseDate(d1) ?? defaultMonthStart,
                DateTo = ParseDate(d2) ?? defaultMonthEnd,
                StateFilter = state,
                FormatFilter = format
            };

            const int maxRows = 1000;

            var mq = new BO.myQueryP31
            {
                j02id = Factory.CurrentUser.pid,
                global_d1 = v.DateFrom,
                global_d2 = v.DateTo,
                p31statequery = v.StateFilter,
                explicit_orderby = "a.p31ID DESC"
            };

            var lis = Factory.p31WorksheetBL.GetList(mq).AsEnumerable();

            switch (v.FormatFilter)
            {
                case 1: lis = lis.Where(e => e.p33ID == BO.p33IdENUM.Cas); break;
                case 2: lis = lis.Where(e => e.p33ID == BO.p33IdENUM.PenizeBezDPH || e.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu); break;
                case 3: lis = lis.Where(e => e.p33ID == BO.p33IdENUM.Kusovnik); break;
            }

            var lisAll = lis.ToList();

            v.TotalCount = lisAll.Count;
            v.TotalHours = lisAll.Where(e => e.p33ID == BO.p33IdENUM.Cas).Sum(e => e.p31Hours_Orig);

            v.IsTruncated = lisAll.Count > maxRows;
            v.Entries = lisAll.Take(maxRows).ToList();

            return View(v);
        }


        private DateTime? ParseDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParseExact(s, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var dt))
                return dt;
            try { return BO.Code.Bas.String2Date(s); }
            catch { return null; }
        }


        // ===== Úkoly - projektové úkoly (p56Task) - zatím připravujeme =====
        // ===== Úkoly - přehled úkolů přidělených uživateli, s filtrem =====
        public IActionResult Tasks(string d1, string d2, int state = 0)
        {
            var v = new UkolyListViewModel
            {
                PageTitle = Factory.tra("Úkoly"),
                HideHeaderTitle = true,
                DateFrom = ParseDate(d1),
                DateTo = ParseDate(d2),
                StateFilter = state
            };

            var mq = new BO.myQueryP56
            {
                j02id = Factory.CurrentUser.pid,   // úkoly, kde má uživatel přidělenou libovolnou roli
                IsRecordValid = v.StateFilter switch
                {
                    1 => null,    // Vše
                    2 => false,   // Uzavřené
                    _ => true     // 0 = Otevřené (výchozí)
                }
            };

            if (v.DateFrom.HasValue || v.DateTo.HasValue)
            {
                mq.period_field = "p56PlanFrom_or_p56PlanUntil";
                mq.global_d1 = v.DateFrom;
                mq.global_d2 = v.DateTo;
            }

            var lisAll = Factory.p56TaskBL.GetList(mq)
                .OrderBy(t => t.p56PlanUntil ?? DateTime.MaxValue)
                .ThenBy(t => t.p56PlanFrom ?? DateTime.MaxValue)
                .ToList();

            v.TotalCount = lisAll.Count;

            const int maxRows = 300;
            v.IsTruncated = lisAll.Count > maxRows;
            v.Entries = lisAll.Take(maxRows).ToList();

            return View(v);
        }


        public IActionResult Reports()
        {
            var v = new BaseViewModel { PageTitle = Factory.tra("Tiskové sestavy") };
            return View(v);
        }


        public IActionResult Help()
        {
            var v = new BaseViewModel { PageTitle = Factory.tra("Nápověda"), HideHeaderTitle = true };
            return View(v);
        }


        // ===== Nastavení - zatím jen formát hodin, časem přibudou další parametry profilu =====
        public IActionResult Settings()
        {
            var v = new SettingsViewModel
            {
                PageTitle = Factory.tra("Nastavení"),
                HideHeaderTitle = true,
                HoursFormat = Factory.CurrentUser.j02DefaultHoursFormat == "T" ? "T" : "N"
            };
            return View(v);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Settings(SettingsViewModel v)
        {
            v.PageTitle = Factory.tra("Nastavení");
            v.HideHeaderTitle = true;

            var c = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
            if (c == null)
            {
                v.Message = Factory.tra("Nastavení se nepodařilo uložit.");
                return View(v);
            }

            c.j02DefaultHoursFormat = v.HoursFormat == "T" ? "T" : "N";

            if (Factory.j02UserBL.Save(c, null) <= 0)
            {
                var msg = Factory.CurrentUser.GetLastMessageNotify();
                v.Message = string.IsNullOrEmpty(msg) ? Factory.tra("Nastavení se nepodařilo uložit.") : msg;
                if (!string.IsNullOrEmpty(msg))
                {
                    // Zpráva je teď ve v.Message - vyprázdnit frontu, ať ji layout nevypíše ještě
                    // jednou přes centrální cyklus Messages4Notify.
                    Factory.CurrentUser.Messages4Notify = null;
                }
                return View(v);
            }

            v.MessageSuccess = Factory.tra("Nastavení bylo uloženo.");
            return View(v);
        }


        // ===== Změna přihlašovacího hesla =====
        public IActionResult ChangePassword()
        {
            var v = new ChangePasswordViewModel
            {
                PageTitle = Factory.tra("Změna přihlašovacího hesla"),
                HideHeaderTitle = true
            };

            if (Factory.CurrentUser.j02IsMustChangePassword)
            {
                v.Message = Factory.tra("Administrátor nastavil, že si musíte změnit přihlašovací heslo.");
            }

            return View(v);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel v)
        {
            v.PageTitle = Factory.tra("Změna přihlašovacího hesla");
            v.HideHeaderTitle = true;

            if (string.IsNullOrEmpty(v.NewPassword) || string.IsNullOrEmpty(v.VerifyPassword))
            {
                v.Message = Factory.tra("Vyplňte nové heslo a jeho ověření.");
                return View(v);
            }

            var cPwdSupp = new BL.Code.PasswordSupport();

            var res = cPwdSupp.CheckPassword(v.NewPassword);
            if (!res.issuccess)
            {
                v.Message = res.Message;
                return View(v);
            }

            if (v.NewPassword != v.VerifyPassword)
            {
                v.Message = Factory.tra("Heslo nesouhlasí s jeho ověřením.");
                return View(v);
            }

            res = cPwdSupp.VerifyUserPassword(v.CurrentPassword, Factory.CurrentUser.j02Login, Factory.CurrentUser);
            if (!res.issuccess)
            {
                v.Message = res.Message;
                return View(v);
            }

            var resSave = Factory.j02UserBL.SaveNewPassword(Factory.CurrentUser.pid, v.NewPassword, false);
            if (!resSave.issuccess)
            {
                v.Message = resSave.Message;
                return View(v);
            }

            var recJ02 = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
            if (recJ02 != null)
            {
                recJ02.j02IsMustChangePassword = false;
                Factory.j02UserBL.Save(recJ02, null);
            }

            v.CurrentPassword = null;
            v.NewPassword = null;
            v.VerifyPassword = null;
            v.MessageSuccess = Factory.tra("Heslo bylo změněno.");
            return View(v);
        }


        public IActionResult More()
        {
            var v = new BaseViewModel { PageTitle = Factory.tra("Více") };
            return View(v);
        }


        public IActionResult MyProfile()
        {
            var rec = Factory.j02UserBL.Load(Factory.CurrentUser.pid);

            var v = new MyProfileViewModel
            {
                PageTitle = Factory.tra("Můj profil"),
                RecJ02 = rec,
                userAgent = Request.Headers["User-Agent"].ToString()
            };

            try
            {
                v.client_info = Parser.GetDefault().Parse(v.userAgent);
            }
            catch
            {
                // tichá chyba - UA parser nesmí blokovat stránku
            }

            return View(v);
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Identity.Application");
            return View(new BaseViewModel());
        }


        // Error handler - nasměrováno v Startup.cs UseExceptionHandler("/Home/Error")
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult Error()
        {
            return View(new BaseViewModel());
        }
    }
}