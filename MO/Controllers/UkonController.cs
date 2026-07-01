using Microsoft.AspNetCore.Mvc;
using MO.Models;

namespace MO.Controllers
{
    public class UkonController : BaseController
    {
        // ===== Kopírování úkonu =====
        public IActionResult Copy(int id, string d)
        {
            var rec = Factory.p31WorksheetBL.Load(id);
            if (rec == null || rec.j02ID != Factory.CurrentUser.pid)
                return RedirectToAction("Index", "Calendar");

            var sesit = Factory.p34ActivityGroupBL.Load(rec.p34ID);
            if (sesit != null && sesit.p33ID != BO.p33IdENUM.Cas)
            {
                return View("NotYet", new BaseViewModel
                {
                    PageTitle = Factory.tra("Připravujeme"),
                    PageTitleAfter = sesit.p33Name
                });
            }

            var v = new EntryHoursViewModel
            {
                PageTitle = Factory.tra("Kopie úkonu"),
                pid = 0,                          // nový záznam
                Date = ParseDate(d) ?? rec.p31Date,
                p34ID = rec.p34ID,
                p41ID = rec.p41ID,
                p32ID = rec.p32ID,
                p56ID = rec.p56ID,
                Description = rec.p31Text,
                Hours = rec.p31Hours_Orig.ToString("0.##",
                    System.Globalization.CultureInfo.InvariantCulture),
                // Čas od/do záměrně nekopírujeme — bývá specifický pro konkrétní den
            };

            LoadSesitList(v);
            LoadProjects(v);
            LoadActivities(v);
            return View("EditHours", v);
        }


        // ===== Nový úkon - sešit již vybrán v Day view (nebo auto-vybrán, když přicházíme z odkazu projektu) =====
        public IActionResult New(string d, int p34id, int p41id = 0)
        {
            // Rozcestník podle typu sešitu hned na vstupu
            if (p34id > 0)
            {
                var sesit = Factory.p34ActivityGroupBL.Load(p34id);
                if (sesit != null && sesit.p33ID != BO.p33IdENUM.Cas)
                {
                    return View("NotYet", new BaseViewModel
                    {
                        PageTitle = Factory.tra("Připravujeme"),
                        PageTitleAfter = sesit.p33Name
                    });
                }
            }
            else
            {
                // Žádný sešit nezvolen (např. odkaz z hitparády projektů) - auto-vybrat první hodinový sešit
                var lisP34 = Factory.p34ActivityGroupBL
                    .GetList_WorksheetEntry_InAllProjects(Factory.CurrentUser.pid);
                var hodinovySesit = lisP34.FirstOrDefault(s => s.p33ID == BO.p33IdENUM.Cas);
                if (hodinovySesit != null)
                {
                    p34id = hodinovySesit.pid;
                }
                else
                {
                    // Uživatel nemá žádný hodinový sešit - nasměrovat na Den, ať si vybere sám
                    return RedirectToAction("Day", "Calendar", new { d });
                }
            }

            var v = new EntryHoursViewModel
            {
                Date = ParseDate(d) ?? DateTime.Today,
                PageTitle = Factory.tra("Nový úkon"),
                p34ID = p34id,
                p41ID = p41id
            };

            LoadSesitList(v);
            LoadProjects(v);
            if (v.p41ID > 0)
            {
                LoadActivities(v);
            }
            LoadFreeFields(v, 0);
            return View("EditHours", v);
        }


        // ===== Editace existujícího úkonu =====
        public IActionResult Edit(int id, string ret = null, string retd = null)
        {
            var rec = Factory.p31WorksheetBL.Load(id);
            if (rec == null || rec.j02ID != Factory.CurrentUser.pid)
                return RedirectToAction("Index", "Calendar");

            // Ověření oprávnění
            var disp = Factory.p31WorksheetBL.InhaleRecDisposition(rec);

            if (!disp.ReadAccess)
            {
                return View("EditHours", new EntryHoursViewModel
                {
                    PageTitle = Factory.tra("Úprava úkonu"),
                    Date = rec.p31Date,
                    Message = Factory.tra("Nemáte oprávnění k záznamu.")
                });
            }

            // Rozcestník podle typu sešitu - zatím umíme jen hodiny
            var sesit = Factory.p34ActivityGroupBL.Load(rec.p34ID);
            if (sesit != null && sesit.p33ID != BO.p33IdENUM.Cas)
            {
                return View("NotYet", new BaseViewModel
                {
                    PageTitle = Factory.tra("Připravujeme"),
                    PageTitleAfter = sesit.p33Name
                });
            }

            var isReadOnly = !disp.OwnerAccess || disp.RecordState != BO.p31RecordState.Editing;

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
                TimeUntil = rec.p31DateTimeUntil_Orig?.ToString("HH:mm"),
                IsReadOnly = isReadOnly,
                RecordStateLabel = isReadOnly ? (disp.LockedReasonMessage ?? Factory.tra("Záznam nelze upravovat.")) : null,
                Ret = ret,
                RetD = retd
            };

            LoadSesitList(v);
            LoadProjects(v);
            LoadActivities(v);
            LoadFreeFields(v, rec.pid);
            return View(isReadOnly ? "ViewHours" : "EditHours", v);
        }


        // ===== Postback / uložení hodinového úkonu =====
        [HttpPost]
        public IActionResult SaveHours(EntryHoursViewModel v, string oper)
        {
            // --- Postback: změna projektu (přenačtení aktivit + úkolů) ---
            if (oper == "p41change")
            {
                LoadSesitList(v);
                LoadProjects(v);
                LoadActivities(v);

                // Zachovat vybranou aktivitu, pokud existuje v nabídce pro nový projekt
                if (v.p32ID > 0 && !v.ActivityComboItems.Any(a => a.Id == v.p32ID))
                {
                    v.p32ID = 0;
                    v.SelectedActivityText = null;
                }

                return View("EditHours", v);
            }

            // --- Skutečné uložení ---
            // Ověřit oprávnění i při POSTu (obrana proti manipulaci s formulářem)
            if (v.pid > 0)
            {
                var rec2 = Factory.p31WorksheetBL.Load(v.pid);
                if (rec2 != null)
                {
                    var disp2 = Factory.p31WorksheetBL.InhaleRecDisposition(rec2);
                    if (!disp2.OwnerAccess || disp2.RecordState != BO.p31RecordState.Editing)
                    {
                        v.Message = Factory.tra("Záznam nelze upravovat.");
                        v.IsReadOnly = true;
                        ReloadAll(v);
                        return View("EditHours", v);
                    }
                }
            }

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

            // Načíst definice freefields + posbírat hodnoty z formuláře
            LoadFreeFields(v, v.pid);
            CollectFreeFieldsFromForm(v);

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
                var ret = Factory.p31WorksheetBL.SaveOrigRecord(input, BO.p33IdENUM.Cas, v.ff1?.inputs);
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

            if (v.Ret == "week" && !string.IsNullOrEmpty(v.RetD))
            {
                return Redirect(Url.Action("Week", "Calendar", new { d = v.RetD }) + "#entry-" + v.pid);
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
        public IActionResult Delete(int id, string d, string ret = null, string retd = null)
        {
            var rec = Factory.p31WorksheetBL.Load(id);
            if (rec != null && rec.j02ID == Factory.CurrentUser.pid)
            {
                var err = Factory.CBL.DeleteRecord("p31", id);
                if (!string.IsNullOrEmpty(err) && err != "1")
                {
                    SetMessage(err);
                }
            }

            if (ret == "week" && !string.IsNullOrEmpty(retd))
            {
                return RedirectToAction("Week", "Calendar", new { d = retd });
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
                .GetList(new BO.myQueryP41("p41") { j02id_query = Factory.CurrentUser.pid, p34id_for_p31_entry = v.p34ID })
                .ToList();

            v.ProjectComboItems = lisP41.Select(p => new ComboItem
            {
                Id = p.pid,
                Code = p.p41Code,
                Text = p.PrefferedName,
                Meta = p.Client
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

                if (v.p56ID > 0)
                {
                    var selTask = v.TaskList.FirstOrDefault(t => t.pid == v.p56ID);
                    if (selTask != null) v.SelectedTaskText = selTask.p56Name;
                }
            }
        }

        private void LoadFreeFields(EntryHoursViewModel v, int recPid)
        {
            v.ff1 = new FreeFieldsViewModel();
            v.ff1.InhaleFreeFieldsView(Factory, recPid, "p31");
            // Viditelnost dle sešitu (p34ID určuje typ záznamu)
            if (v.p34ID > 0)
            {
                v.ff1.RefreshInputsVisibility(Factory, recPid, "p31", v.p34ID);
            }
        }

        private void CollectFreeFieldsFromForm(EntryHoursViewModel v)
        {
            if (v.ff1?.inputs == null) return;

            foreach (var ff in v.ff1.inputs)
            {
                var key = "ff_" + ff.x28Field;
                if (!Request.Form.ContainsKey(key))
                {
                    // checkbox neposílá nic když není zaškrtnutý
                    if (ff.TypeName == "boolean") ff.CheckInput = false;
                    continue;
                }

                var raw = Request.Form[key].ToString();
                switch (ff.TypeName)
                {
                    case "boolean":
                        ff.CheckInput = raw == "true" || raw == "on";
                        break;
                    case "date":
                    case "datetime":
                        if (DateTime.TryParse(raw, out var dt)) ff.DateInput = dt;
                        else ff.DateInput = null;
                        break;
                    case "decimal":
                        if (double.TryParse(raw.Replace(',', '.'),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out var num))
                            ff.NumInput = num;
                        break;
                    case "integer":
                        if (int.TryParse(raw, out var iv)) ff.IntInput = iv;
                        break;
                    default:
                        ff.StringInput = raw;
                        break;
                }
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
                Text = a.p32Name,
                GroupBy = a.p38Name
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
            // Freefields: definice + viditelnost, pak posbírat hodnoty z POSTu zpět
            LoadFreeFields(v, v.pid);
            CollectFreeFieldsFromForm(v);
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
    }
}