using Microsoft.AspNetCore.Mvc;
using MO.Models;

namespace MO.Controllers
{
    public class CalendarController : BaseController
    {
        // ===== Měsíční pohled =====
        public IActionResult Index(string d)
        {
            var v = new CalendarViewModel
            {
                PageTitle = Factory.tra("Kalendář"),
                HideHeaderTitle = true,
                d0 = ParseDate(d) ?? DateTime.Today,
                ShowWeekend = Factory.CBL.LoadUserParamBool("mo-calendar-showweekend", false)
            };

            // mřížka: první pondělí ≤ 1. den měsíce, poslední neděle ≥ poslední den měsíce
            var firstOfMonth = new DateTime(v.d0.Year, v.d0.Month, 1);
            var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);
            v.d1 = BO.Code.Bas.get_first_prev_monday(firstOfMonth);
            v.d2 = BO.Code.Bas.get_first_prev_monday(lastOfMonth).AddDays(6);

            LoadCalendarData(v);

            v.PageTitle = v.d0.ToString("MMMM yyyy",
                System.Globalization.CultureInfo.CurrentUICulture);
            v.PageTitle = char.ToUpper(v.PageTitle[0]) + v.PageTitle.Substring(1);

            return View(v);
        }


        [HttpPost]
        public IActionResult ToggleWeekend(string d, string view)
        {
            var current = Factory.CBL.LoadUserParamBool("mo-calendar-showweekend", false);
            Factory.CBL.SetUserParam("mo-calendar-showweekend", (!current).ToString().ToLower());
            return view == "week"
                ? RedirectToAction("Week", new { d })
                : RedirectToAction("Index", new { d });
        }


        // ===== Týdenní agenda =====
        public IActionResult Week(string d)
        {
            var v = new WeekViewModel
            {
                d0 = ParseDate(d) ?? DateTime.Today,
                ShowWeekend = Factory.CBL.LoadUserParamBool("mo-calendar-showweekend", false)
            };

            v.d1 = BO.Code.Bas.get_first_prev_monday(v.d0);
            v.d2 = v.ShowWeekend ? v.d1.AddDays(6) : v.d1.AddDays(4);

            var lisP31 = Factory.p31WorksheetBL.GetList(new BO.myQueryP31
            {
                j02id = Factory.CurrentUser.pid,
                global_d1 = v.d1,
                global_d2 = v.d2
            }).ToList();

            var lisC26 = Factory.c26HolidayBL.GetList(new BO.myQueryC26
            {
                global_d1 = v.d1,
                global_d2 = v.d2
            }).ToList();

            var dayCount = (v.d2 - v.d1).Days + 1;
            for (int i = 0; i < dayCount; i++)
            {
                var date = v.d1.AddDays(i);
                var holiday = lisC26.FirstOrDefault(h => h.c26Date.Date == date.Date);

                var wd = new WeekDay
                {
                    Date = date,
                    DayName = System.Globalization.CultureInfo.CurrentUICulture
                        .DateTimeFormat.GetDayName(date.DayOfWeek),
                    IsHoliday = holiday != null,
                    HolidayName = holiday?.c26Name,
                    Entries = lisP31.Where(p => p.p31Date.Date == date.Date)
                        .OrderBy(p => p.p31DateTimeFrom_Orig ?? p.p31Date.AddHours(23.99))
                        .ToList()
                };
                wd.TotalHours = wd.Entries.Sum(e => e.p31Hours_Orig);
                v.Days.Add(wd);
            }

            v.PageTitle = $"{v.d1:d. M.} – {v.d2:d. M. yyyy}";
            v.HideHeaderTitle = true;

            return View(v);
        }


        // ===== Detail dne =====
        public IActionResult Day(string d)
        {
            var date = ParseDate(d) ?? DateTime.Today;

            var v = new DayViewModel
            {
                Date = date,
                PageTitle = date.ToString("d. MMMM yyyy",
                    System.Globalization.CultureInfo.CurrentUICulture),
                HideHeaderTitle = true
            };

            var mq = new BO.myQueryP31
            {
                j02id = Factory.CurrentUser.pid,
                global_d1 = date,
                global_d2 = date
            };
            v.Entries = Factory.p31WorksheetBL.GetList(mq)
                .OrderByDescending(p => p.pid)
                .ToList();

            v.TotalHours = v.Entries.Sum(e => e.p31Hours_Orig);

            var holiday = Factory.c26HolidayBL.GetList(new BO.myQueryC26
            {
                global_d1 = date,
                global_d2 = date
            }).FirstOrDefault();
            if (holiday != null)
            {
                v.IsHoliday = true;
                v.HolidayName = holiday.c26Name;
            }

            v.SesitList = Factory.p34ActivityGroupBL
                .GetList_WorksheetEntry_InAllProjects(Factory.CurrentUser.pid)
                .ToList();

            return View(v);
        }


        // ===== Pomocné metody =====
        private void LoadCalendarData(CalendarViewModel v)
        {
            // Úkony za období
            v.lisP31 = Factory.p31WorksheetBL.GetList(new BO.myQueryP31
            {
                j02id = Factory.CurrentUser.pid,
                global_d1 = v.d1,
                global_d2 = v.d2
            }).ToList();

            // Denní souhrny
            v.lisSums = Factory.p31WorksheetBL.GetList_TimelineDays(
                new List<int> { Factory.CurrentUser.pid }, v.d1, v.d2, 0, 0, 0).ToList();

            // Svátky
            v.lisC26 = Factory.c26HolidayBL.GetList(new BO.myQueryC26
            {
                global_d1 = v.d1,
                global_d2 = v.d2
            }).ToList();
        }

        private DateTime? ParseDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            // URL parametry posíláme jako yyyy-MM-dd
            if (DateTime.TryParseExact(s, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var dt))
                return dt;
            // Fallback pro jiné formáty
            try { return BO.Code.Bas.String2Date(s); }
            catch { return null; }
        }
    }
}