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
                PageTitle = Factory.tra("Úkony"),
                HideHeaderTitle = true,
                DateFrom = ParseDate(d1) ?? defaultMonthStart,
                DateTo = ParseDate(d2) ?? defaultMonthEnd,
                StateFilter = state,
                FormatFilter = format
            };

            var mq = new BO.myQueryP31
            {
                j02id = Factory.CurrentUser.pid,
                global_d1 = v.DateFrom,
                global_d2 = v.DateTo,
                p31statequery = v.StateFilter
            };

            var lis = Factory.p31WorksheetBL.GetList(mq).AsEnumerable();

            switch (v.FormatFilter)
            {
                case 1: lis = lis.Where(e => e.p33ID == BO.p33IdENUM.Cas); break;
                case 2: lis = lis.Where(e => e.p33ID == BO.p33IdENUM.PenizeBezDPH || e.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu); break;
                case 3: lis = lis.Where(e => e.p33ID == BO.p33IdENUM.Kusovnik); break;
            }

            var lisAll = lis.OrderByDescending(e => e.pid).ToList();

            v.TotalCount = lisAll.Count;
            v.TotalHours = lisAll.Where(e => e.p33ID == BO.p33IdENUM.Cas).Sum(e => e.p31Hours_Orig);

            const int maxRows = 300;
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