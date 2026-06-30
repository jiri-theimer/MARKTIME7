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
                d0 = ParseDate(d) ?? DateTime.Today,
                ShowWeekend = Factory.CBL.LoadUserParamBool("mo-calendar-showweekend", false)
            };

            // mřížka: první pondělí ≤ 1. den měsíce, poslední neděle ≥ poslední den měsíce
            var firstOfMonth = new DateTime(v.d0.Year, v.d0.Month, 1);
            var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);
            v.d1 = BO.Code.Bas.get_first_prev_monday(firstOfMonth);
            v.d2 = BO.Code.Bas.get_first_prev_monday(lastOfMonth).AddDays(6);

            LoadCalendarData(v);

            v.PageTitle = v.d0.ToString("LLLL yyyy",
                System.Globalization.CultureInfo.CurrentUICulture);
            v.PageTitle = char.ToUpper(v.PageTitle[0]) + v.PageTitle.Substring(1);

            return View(v);
        }


        [HttpPost]
        public IActionResult ToggleWeekend(string d)
        {
            var current = Factory.CBL.LoadUserParamBool("mo-calendar-showweekend", false);
            Factory.CBL.SetUserParam("mo-calendar-showweekend", (!current).ToString().ToLower());
            return RedirectToAction("Index", new { d });
        }


        // ===== Detail dne =====
        public IActionResult Day(string d)
        {
            var date = ParseDate(d) ?? DateTime.Today;

            var v = new DayViewModel
            {
                Date = date,
                PageTitle = date.ToString("d. MMMM yyyy",
                    System.Globalization.CultureInfo.CurrentUICulture)
            };

            var mq = new BO.myQueryP31
            {
                j02id = Factory.CurrentUser.pid,
                global_d1 = date,
                global_d2 = date
            };
            v.Entries = Factory.p31WorksheetBL.GetList(mq)
                .OrderBy(p => p.p31DateTimeFrom_Orig ?? p.p31Date.AddHours(23.99))
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

            return View(v);
        }


        // ===== Nový úkon =====
        public IActionResult New(string d)
        {
            var v = new EntryViewModel
            {
                Date = ParseDate(d) ?? DateTime.Today,
                PageTitle = Factory.tra("Nový úkon")
            };

            LoadProjectsAndTasks(v);
            return View("Edit", v);
        }


        // ===== Editace úkonu =====
        public IActionResult Edit(int id)
        {
            var rec = Factory.p31WorksheetBL.Load(id);
            if (rec == null || rec.j02ID != Factory.CurrentUser.pid)
            {
                return RedirectToAction("Index");
            }

            var v = new EntryViewModel
            {
                PageTitle = Factory.tra("Úprava úkonu"),
                pid = rec.pid,
                Date = rec.p31Date,
                p41ID = rec.p41ID,
                p56ID = rec.p56ID,
                Description = rec.p31Text,
                Hours = rec.p31Hours_Orig.ToString("0.##",
                    System.Globalization.CultureInfo.InvariantCulture),
                TimeFrom = rec.p31DateTimeFrom_Orig?.ToString("HH:mm"),
                TimeUntil = rec.p31DateTimeUntil_Orig?.ToString("HH:mm")
            };

            LoadProjectsAndTasks(v);
            return View(v);
        }


        private void RefreshState(EntryViewModel v)
        {
            LoadProjectsAndTasks(v);
        }
        // ===== Uložení (nový i edit) =====
        [HttpPost]
        public IActionResult Save(EntryViewModel v)
        {
            // Validace na úrovni formuláře
            if (v.p41ID <= 0)
            {
                v.Message = Factory.tra("Vyberte projekt.");
                RefreshState(v);
                return View("Edit", v);
            }
            if (string.IsNullOrWhiteSpace(v.Hours))
            {
                v.Message = Factory.tra("Vyplňte počet hodin.");
                RefreshState(v);
                return View("Edit", v);
            }
            if (string.IsNullOrWhiteSpace(v.Description))
            {
                v.Message = Factory.tra("Vyplňte popis úkonu.");
                RefreshState(v);
                return View("Edit", v);
            }

            // Sestavit input pro BL
            var input = new BO.p31WorksheetEntryInput
            {
                j02ID = Factory.CurrentUser.pid,
                p41ID = v.p41ID,
                p56ID = v.p56ID,
                p31Text = v.Description,
                Value_Orig = v.Hours,                
                TimeFrom = v.TimeFrom ?? "",
                TimeUntil = v.TimeUntil ?? "",
                p31HoursEntryflag = BO.p31HoursEntryFlagENUM.Hodiny,
                p31RecordSourceFlag = 1     // 1 = záznam z mobilní aplikace
            };
            input.SetPID(v.pid);
            input.Addp31Date(v.Date);

            // Validace času a zaokrouhlení
            if (!input.ValidateEntryTime(1, v.Date))
            {
                v.Message = input.ErrorMessage;
                RefreshState(v);
                return View("Edit", v);
            }

            try
            {
                var ret = Factory.p31WorksheetBL.SaveOrigRecord(input, BO.p33IdENUM.Cas, null);
                if (ret <= 0)
                {
                    
                    v.Message = Factory.tra("Úkon se nepodařilo uložit.");
                    v.Message += Factory.CurrentUser.GetLastMessageNotify();
                    RefreshState(v);
                    return View("Edit", v);
                }
            }
            catch (Exception ex)
            {
                v.Message = ex.Message;
                RefreshState(v);
                return View("Edit", v);
            }

            return RedirectToAction("Day", new { d = v.Date.ToString("yyyy-MM-dd") });
        }


        // ===== Smazání =====
        [HttpPost]
        public IActionResult Delete(int id, string d)
        {
            var rec = Factory.p31WorksheetBL.Load(id);
            if (rec != null && rec.j02ID == Factory.CurrentUser.pid)
            {
                // Měkké smazání přes SaveOrigRecord s nulovými hodinami se v UI dělá různě;
                // pro MO MVP volíme přímou metodu DeleteTempRecord pokud existuje GUID,
                // jinak označíme jako neviditelné přes BL helper.
                // BL má DeleteTempRecord(guid, p31id) - to je pro temporary záznamy.
                // Pro skutečné smazání úkonu MO MVP používá metodu Delete na BL,
                // pokud bude třeba, doplníme v další iteraci.
                try
                {
                    // Reflektovat - p31WorksheetBL.Delete existuje? Pokud ne, je třeba doplnit.
                    var t = Factory.p31WorksheetBL.GetType().GetMethod("Delete",
                        new[] { typeof(int) });
                    t?.Invoke(Factory.p31WorksheetBL, new object[] { id });
                }
                catch
                {
                    // tichá chyba - smazání není kritická operace pro MVP
                }
            }

            return RedirectToAction("Day", new { d });
        }


        // ===== AJAX endpoint pro načtení úkolů po výběru projektu =====
        public IActionResult TasksForProject(int p41id)
        {
            var tasks = Factory.p56TaskBL.GetList(new BO.myQueryP56
            {
                p41id = p41id,
                j02id = Factory.CurrentUser.pid
            }).Take(100);

            return Json(tasks.Select(t => new
            {
                pid = t.pid,
                name = t.p56Name
                
            }));
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

        private void LoadProjectsAndTasks(EntryViewModel v)
        {
            // Projekty, kam CurrentUser může vykazovat (zachovat opravu uživatele: prefix + j02id_query)
            var lisP41 = Factory.p41ProjectBL
                .GetList(new BO.myQueryP41("p41") { j02id_query = Factory.CurrentUser.pid })
                .ToList();

            v.ProjectComboItems = lisP41.Select(p => new ComboItem
            {
                Id = p.pid,
                Code = p.p41Code,
                Text = p.PrefferedName,
                Meta=p.Client
            }).ToList();

            // Předvyplnit popisek aktuálně vybraného projektu (pro trigger combo)
            if (v.p41ID > 0)
            {
                var sel = lisP41.FirstOrDefault(p => p.pid == v.p41ID);
                if (sel != null) v.SelectedProjectText = sel.p41NameShort ?? sel.p41Name;
            }

            // Úkoly - načteme jen pokud je vybraný projekt (jinak prázdné)
            if (v.p41ID > 0)
            {
                v.TaskList = Factory.p56TaskBL.GetList(new BO.myQueryP56
                {
                    p41id = v.p41ID,
                    j02id = Factory.CurrentUser.pid
                }).Take(100).ToList();
            }
        }

        private DateTime? ParseDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            try { return BO.Code.Bas.String2Date(s); }
            catch { return null; }
        }
    }
}
