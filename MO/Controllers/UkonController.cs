using Microsoft.AspNetCore.Mvc;
using MO.Models;

namespace MO.Controllers
{
    public class UkonController : BaseController
    {
        // ===== Nový úkon - výchozí hodinový formulář =====
        public IActionResult New(string d)
        {
            var v = new EntryHoursViewModel
            {
                Date = ParseDate(d) ?? DateTime.Today,
                PageTitle = Factory.tra("Nový úkon")
            };

            LoadSesitList(v);
            LoadProjects(v);
            return View("EditHours", v);
        }


        // ===== Editace existujícího úkonu =====
        public IActionResult Edit(int id)
        {
            var rec = Factory.p31WorksheetBL.Load(id);
            if (rec == null || rec.j02ID != Factory.CurrentUser.pid)
            {
                return RedirectToAction("Index", "Calendar");
            }

            // Rozcestník podle typu sešitu - zatím umíme jen hodiny
            var sesit = Factory.p34ActivityGroupBL.Load(rec.p34ID);
            if (sesit != null && sesit.p33ID != BO.p33IdENUM.Cas)
            {
                // Ne-hodinový úkon - zatím připravujeme
                return View("NotYet", new BaseViewModel
                {
                    PageTitle = Factory.tra("Připravujeme"),
                    PageTitleAfter = sesit.p33Name
                });
            }

            var v = new EntryHoursViewModel
            {
                PageTitle = Factory.tra("Úprava úkonu"),
                pid = rec.pid,
                Date = rec.p31Date,
                p34ID = rec.p34ID,
                p41ID = rec.p41ID,
                p32ID = rec.p32ID,
                p56ID = rec.p56ID,
                Description = rec.p31Text,
                Hours = rec.p31Hours_Orig.ToString("0.##",
                    System.Globalization.CultureInfo.InvariantCulture),
                TimeFrom = rec.p31DateTimeFrom_Orig?.ToString("HH:mm"),
                TimeUntil = rec.p31DateTimeUntil_Orig?.ToString("HH:mm")
            };

            LoadSesitList(v);
            LoadProjects(v);
            LoadActivities(v);
            return View("EditHours", v);
        }


        // ===== Postback / uložení hodinového úkonu =====
        [HttpPost]
        public IActionResult SaveHours(EntryHoursViewModel v, string oper)
        {
            // --- Postback: změna sešitu ---
            if (oper == "p34change")
            {
                var sesit = Factory.p34ActivityGroupBL.Load(v.p34ID);

                // Jiný typ než hodiny -> redirect na příslušné view (zatím připravujeme)
                if (sesit != null && sesit.p33ID != BO.p33IdENUM.Cas)
                {
                    return RedirectToAction("NotYetByType", new { p34id = v.p34ID, d = v.Date.ToString("yyyy-MM-dd") });
                }

                // Stejný typ (hodiny) -> přenačti aktivity, vyčisti vybranou aktivitu
                v.p32ID = 0;
                v.SelectedActivityText = null;
                LoadSesitList(v);
                LoadProjects(v);
                LoadActivities(v);
                return View("EditHours", v);
            }

            // --- Postback: změna projektu (přenačtení aktivit + úkolů) ---
            if (oper == "p41change")
            {
                v.p32ID = 0;
                v.SelectedActivityText = null;
                LoadSesitList(v);
                LoadProjects(v);
                LoadActivities(v);
                return View("EditHours", v);
            }

            // --- Skutečné uložení ---
            if (v.p34ID <= 0)
            {
                v.Message = Factory.tra("Vyberte sešit.");
                ReloadAll(v);
                return View("EditHours", v);
            }
            if (v.p41ID <= 0)
            {
                v.Message = Factory.tra("Vyberte projekt.");
                ReloadAll(v);
                return View("EditHours", v);
            }

            // Aktivita povinná dle sešitu?
            var recSesit = Factory.p34ActivityGroupBL.Load(v.p34ID);
            if (recSesit != null
                && recSesit.p34ActivityEntryFlag == BO.p34ActivityEntryFlagENUM.AktivitaJePovinna
                && v.p32ID <= 0)
            {
                v.Message = Factory.tra("Vyberte aktivitu.");
                ReloadAll(v);
                return View("EditHours", v);
            }

            if (string.IsNullOrWhiteSpace(v.Hours))
            {
                v.Message = Factory.tra("Vyplňte počet hodin.");
                ReloadAll(v);
                return View("EditHours", v);
            }
            if (string.IsNullOrWhiteSpace(v.Description))
            {
                v.Message = Factory.tra("Vyplňte popis úkonu.");
                ReloadAll(v);
                return View("EditHours", v);
            }

            var input = new BO.p31WorksheetEntryInput
            {
                j02ID = Factory.CurrentUser.pid,
                p34ID = v.p34ID,
                p41ID = v.p41ID,
                p32ID = v.p32ID,
                p56ID = v.p56ID,
                p31Text = v.Description,
                Value_Orig = v.Hours,
                TimeFrom = v.TimeFrom ?? "",
                TimeUntil = v.TimeUntil ?? "",
                p31HoursEntryflag = BO.p31HoursEntryFlagENUM.Hodiny,
                p31RecordSourceFlag = 1     // 1 = mobilní aplikace
            };
            input.SetPID(v.pid);
            input.Addp31Date(v.Date);

            if (!input.ValidateEntryTime(1, v.Date))
            {
                v.Message = input.ErrorMessage;
                ReloadAll(v);
                return View("EditHours", v);
            }

            try
            {
                var ret = Factory.p31WorksheetBL.SaveOrigRecord(input, BO.p33IdENUM.Cas, null);
                if (ret <= 0)
                {
                    v.Message = Factory.tra("Úkon se nepodařilo uložit.");
                    ReloadAll(v);
                    return View("EditHours", v);
                }
            }
            catch (Exception ex)
            {
                v.Message = ex.Message;
                ReloadAll(v);
                return View("EditHours", v);
            }

            return RedirectToAction("Day", "Calendar", new { d = v.Date.ToString("yyyy-MM-dd") });
        }


        // ===== Rozcestník pro ne-hodinové typy (zatím připravujeme) =====
        public IActionResult NotYetByType(int p34id, string d)
        {
            var sesit = Factory.p34ActivityGroupBL.Load(p34id);
            return View("NotYet", new BaseViewModel
            {
                PageTitle = Factory.tra("Připravujeme"),
                PageTitleAfter = sesit?.p33Name
            });
        }


        // ===== Smazání =====
        [HttpPost]
        public IActionResult Delete(int id, string d)
        {
            var rec = Factory.p31WorksheetBL.Load(id);
            if (rec != null && rec.j02ID == Factory.CurrentUser.pid)
            {
                try
                {
                    var t = Factory.p31WorksheetBL.GetType().GetMethod("Delete", new[] { typeof(int) });
                    t?.Invoke(Factory.p31WorksheetBL, new object[] { id });
                }
                catch { /* tichá chyba pro MVP */ }
            }

            return RedirectToAction("Day", "Calendar", new { d });
        }


        // ===== AJAX: úkoly pro projekt =====
        public IActionResult TasksForProject(int p41id)
        {
            var tasks = Factory.p56TaskBL.GetList(new BO.myQueryP56
            {
                p41id = p41id,
                j02id = Factory.CurrentUser.pid
            }).Take(100);

            return Json(tasks.Select(t => new { pid = t.pid, name = t.p56Name }));
        }


        // ===== Pomocné metody =====
        private void LoadSesitList(EntryHoursViewModel v)
        {
            var lisP34 = Factory.p34ActivityGroupBL
                .GetList_WorksheetEntry_InAllProjects(Factory.CurrentUser.pid)
                .ToList();

            v.SesitComboItems = lisP34.Select(s => new ComboItem
            {
                Id = s.pid,
                Code = s.p34Code,
                Text = s.p34Name,
                Meta = s.p33Name      // typ úkonu (Čas / Peníze / Kusovník)
            }).ToList();

            if (v.p34ID > 0)
            {
                var sel = lisP34.FirstOrDefault(s => s.pid == v.p34ID);
                if (sel != null)
                {
                    v.SelectedSesitText = sel.p34Name;
                    v.ActivityEntryFlag = (int)sel.p34ActivityEntryFlag;
                }
            }
        }

        private void LoadProjects(EntryHoursViewModel v)
        {
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

            if (v.p41ID > 0)
            {
                var sel = lisP41.FirstOrDefault(p => p.pid == v.p41ID);
                if (sel != null) v.SelectedProjectText = sel.p41NameShort ?? sel.p41Name;
            }

            // úkoly k projektu
            if (v.p41ID > 0)
            {
                v.TaskList = Factory.p56TaskBL.GetList(new BO.myQueryP56
                {
                    p41id = v.p41ID,
                    j02id = Factory.CurrentUser.pid
                }).Take(100).ToList();
            }
        }

        private void LoadActivities(EntryHoursViewModel v)
        {
            // Aktivity se zadávají jen pokud to sešit dovoluje a je vybrán projekt
            if (v.p34ID <= 0 || v.p41ID <= 0) return;
            if (v.ActivityEntryFlag == (int)BO.p34ActivityEntryFlagENUM.AktivitaSeNezadava) return;

            var lisP32 = Factory.p32ActivityBL.GetList(new BO.myQueryP32
            {
                p34id = v.p34ID,
                p41id = v.p41ID
            }).ToList();

            v.ActivityComboItems = lisP32.Select(a => new ComboItem
            {
                Id = a.pid,
                Code = a.p32Code,
                Text = a.p32Name
            }).ToList();

            if (v.p32ID > 0)
            {
                var sel = lisP32.FirstOrDefault(a => a.pid == v.p32ID);
                if (sel != null) v.SelectedActivityText = sel.p32Name;
            }
        }

        private void ReloadAll(EntryHoursViewModel v)
        {
            LoadSesitList(v);
            LoadProjects(v);
            LoadActivities(v);
        }

        private DateTime? ParseDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            try { return BO.Code.Bas.String2Date(s); }
            catch { return null; }
        }
    }
}
