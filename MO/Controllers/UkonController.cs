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
            if (sesit != null && sesit.p33ID == BO.p33IdENUM.Kusovnik)
            {
                return View("NotYet", new BaseViewModel
                {
                    PageTitle = Factory.tra("Připravujeme"),
                    PageTitleAfter = sesit.p33Name
                });
            }

            if (sesit != null && IsMoneyType(sesit.p33ID))
            {
                var vm = new EntryMoneyViewModel
                {
                    PageTitle = Factory.tra("Kopie úkonu"),
                    pid = 0,                          // nový záznam
                    Date = ParseDate(d) ?? rec.p31Date,
                    p34ID = rec.p34ID,
                    p41ID = rec.p41ID,
                    p32ID = rec.p32ID,
                    p56ID = rec.p56ID,
                    Description = rec.p31Text,
                    AmountWithoutVat = rec.p31Amount_WithoutVat_Orig.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                    VatRatePercent = rec.p31VatRate_Orig.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                    AmountWithVat = rec.p31Amount_WithVat_Orig.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                    AmountVat = rec.p31Amount_Vat_Orig.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                    j27ID = rec.j27ID_Billing_Orig,
                    DocumentCode = null,   // kód dokladu záměrně nekopírujeme
                    j19ID = rec.j19ID,
                };

                ReloadAllMoney(vm);
                return View("EditMoney", vm);
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
        // p68id: pokud přicházíme ze Stopek, předvyplní se projekt/aktivita/text/čas z daného záznamu stopek
        public IActionResult New(string d, int p34id, int p41id = 0, int p68id = 0)
        {
            BO.p68StopWatch stopwatch = null;
            if (p68id > 0)
            {
                stopwatch = Factory.p68StopWatchBL.Load(p68id);
                if (stopwatch == null || stopwatch.j02ID != Factory.CurrentUser.pid || stopwatch.p68IsRunning)
                {
                    // cizí, neexistující nebo dosud běžící záznam stopek - ignorovat
                    stopwatch = null;
                }
                else
                {
                    if (p41id <= 0) p41id = stopwatch.p41ID;
                    if (p34id <= 0 && stopwatch.p32ID > 0)
                    {
                        var recP32Stopky = Factory.p32ActivityBL.Load(stopwatch.p32ID);
                        if (recP32Stopky != null) p34id = recP32Stopky.p34ID;
                    }
                }
            }

            // Rozcestník podle typu sešitu hned na vstupu
            if (p34id > 0)
            {
                var sesit = Factory.p34ActivityGroupBL.Load(p34id);
                if (sesit != null && sesit.p33ID == BO.p33IdENUM.Kusovnik)
                {
                    return View("NotYet", new BaseViewModel
                    {
                        PageTitle = Factory.tra("Připravujeme"),
                        PageTitleAfter = sesit.p33Name
                    });
                }
                if (sesit != null && IsMoneyType(sesit.p33ID))
                {
                    return NewMoney(d, p34id, p41id, sesit);
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

            if (stopwatch != null)
            {
                v.p68ID = stopwatch.pid;
                v.p32ID = stopwatch.p32ID;
                v.Description = stopwatch.p68Text;
                v.Hours = BO.Code.Time.GetTimeFromSeconds((double)stopwatch.p68Duration);
                if (stopwatch.DateInsert.HasValue)
                {
                    v.Date = stopwatch.DateInsert.Value.Date;
                }
            }

            LoadSesitList(v);
            LoadProjects(v);
            if (v.p41ID > 0)
            {
                LoadActivities(v);
            }
            LoadFreeFields(v, 0);
            LoadKusovnikOffer(v);
            if (v.p41ID > 0 && v.IsNavicKusovnik)
            {
                LoadKusovnikForProject(v);
            }
            return View("EditHours", v);
        }


        // ===== Nový peněžní úkon =====
        private IActionResult NewMoney(string d, int p34id, int p41id, BO.p34ActivityGroup sesit)
        {
            var v = new EntryMoneyViewModel
            {
                Date = ParseDate(d) ?? DateTime.Today,
                PageTitle = Factory.tra("Nový úkon"),
                p34ID = p34id,
                p41ID = p41id
            };

            // výchozí měna - domácí měna licence
            if (Factory.Lic.j27ID > 0)
            {
                v.j27ID = Factory.Lic.j27ID;
            }

            ReloadAllMoney(v, sesit);
            return View("EditMoney", v);
        }


        // ===== Editace existujícího úkonu =====
        public IActionResult Edit(int id, string ret = null, string retd = null)
        {
            var rec = Factory.p31WorksheetBL.Load(id);
            if (rec == null || rec.j02ID != Factory.CurrentUser.pid)
                return RedirectToAction("Index", "Calendar");

            // Rozcestník podle typu sešitu - kusovník zatím nepodporujeme
            var sesit = Factory.p34ActivityGroupBL.Load(rec.p34ID);
            if (sesit != null && sesit.p33ID == BO.p33IdENUM.Kusovnik)
            {
                return View("NotYet", new BaseViewModel
                {
                    PageTitle = Factory.tra("Připravujeme"),
                    PageTitleAfter = sesit.p33Name
                });
            }

            // Ověření oprávnění
            var disp = Factory.p31WorksheetBL.InhaleRecDisposition(rec);

            if (sesit != null && IsMoneyType(sesit.p33ID))
            {
                return EditMoney(rec, sesit, disp, ret, retd);
            }

            if (!disp.ReadAccess)
            {
                return View("EditHours", new EntryHoursViewModel
                {
                    PageTitle = Factory.tra("Úprava úkonu"),
                    Date = rec.p31Date,
                    Message = Factory.tra("Nemáte oprávnění k záznamu.")
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
            if (!isReadOnly)
            {
                LoadKusovnikOffer(v);
                LoadExistingKusovnikEntries(v);
            }
            return View(isReadOnly ? "ViewHours" : "EditHours", v);
        }


        // ===== Editace existujícího peněžního úkonu =====
        private IActionResult EditMoney(BO.p31Worksheet rec, BO.p34ActivityGroup sesit, BO.p31RecDisposition disp, string ret, string retd)
        {
            if (!disp.ReadAccess)
            {
                return View("EditMoney", new EntryMoneyViewModel
                {
                    PageTitle = Factory.tra("Úprava úkonu"),
                    Date = rec.p31Date,
                    Message = Factory.tra("Nemáte oprávnění k záznamu.")
                });
            }

            var isReadOnly = !disp.OwnerAccess || disp.RecordState != BO.p31RecordState.Editing;

            var v = new EntryMoneyViewModel
            {
                PageTitle = Factory.tra("Úprava úkonu"),
                pid = rec.pid,
                Date = rec.p31Date,
                p34ID = rec.p34ID,
                p41ID = rec.p41ID,
                p32ID = rec.p32ID,
                p56ID = rec.p56ID,
                Description = rec.p31Text,
                AmountWithoutVat = rec.p31Amount_WithoutVat_Orig.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                VatRatePercent = rec.p31VatRate_Orig.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                AmountWithVat = rec.p31Amount_WithVat_Orig.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                AmountVat = rec.p31Amount_Vat_Orig.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                j27ID = rec.j27ID_Billing_Orig,
                DocumentCode = rec.p31Code,
                j19ID = rec.j19ID,
                IsReadOnly = isReadOnly,
                RecordStateLabel = isReadOnly ? (disp.LockedReasonMessage ?? Factory.tra("Záznam nelze upravovat.")) : null,
                Ret = ret,
                RetD = retd
            };

            ReloadAllMoney(v, sesit);
            return View(isReadOnly ? "ViewMoney" : "EditMoney", v);
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

                LoadKusovnikOffer(v);
                if (v.IsNavicKusovnik)
                {
                    // Projekt se změnil - nabídka kusovníkových sešitů/aktivit je pro jiný projekt jiná
                    v.p34ID_Kusovnik = 0;
                    LoadKusovnikForProject(v);
                }

                return View("EditHours", v);
            }

            // --- Postback: zapnutí/vypnutí "K hodinám vykázat i kusovníkové úkony" ---
            if (oper == "kusovnik_toggle")
            {
                ReloadAll(v);
                if (v.IsNavicKusovnik)
                {
                    LoadKusovnikForProject(v);
                    if (v.KusovnikRows.Count == 0)
                    {
                        v.KusovnikRows.Add(new KusovnikRowViewModel());
                    }
                }
                return View("EditHours", v);
            }

            // --- Postback: změna kusovníkového sešitu (přenačtení aktivit pro řádky) ---
            if (oper == "kusovnik_p34id")
            {
                ReloadAll(v);
                LoadKusovnikForProject(v);
                return View("EditHours", v);
            }

            // --- Postback: přidat prázdný řádek kusovníku ---
            if (oper == "kusovnik_add")
            {
                ReloadAll(v);
                LoadKusovnikForProject(v);
                v.KusovnikRows.Add(new KusovnikRowViewModel());
                return View("EditHours", v);
            }

            // --- Postback: odebrat řádek kusovníku ---
            if (!string.IsNullOrEmpty(oper) && oper.StartsWith("kusovnik_remove_"))
            {
                ReloadAll(v);
                LoadKusovnikForProject(v);
                if (int.TryParse(oper.Substring("kusovnik_remove_".Length), out var removeIdx)
                    && removeIdx >= 0 && removeIdx < v.KusovnikRows.Count)
                {
                    v.KusovnikRows.RemoveAt(removeIdx);
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

            // Validace kusovníkových řádků (jen ty vyplněné - prázdné řádky se ignorují)
            string kusovnikError = null;
            if (v.IsNavicKusovnik)
            {
                kusovnikError = ValidateKusovnikRows(v);
                if (kusovnikError != null)
                {
                    v.Message = kusovnikError;
                    ReloadAll(v);
                    return View("EditHours", v);
                }
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

                // Uložit i navazující kusovníkové úkony (přidávají se, existující se nepřepisují)
                if (v.IsNavicKusovnik && v.KusovnikRows != null && v.KusovnikRows.Count > 0)
                {
                    SaveKusovnikRows(ret, v);
                }

                // Úkon vznikl ze stopek - záznam stopek už není potřeba
                if (v.p68ID > 0)
                {
                    Factory.CBL.DeleteRecord("p68", v.p68ID);
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
            if (v.Ret == "ukony" && !string.IsNullOrEmpty(v.RetD))
            {
                return Redirect(v.RetD + "#entry-" + v.pid);
            }
            return RedirectToAction("Day", "Calendar", new { d = v.Date.ToString("yyyy-MM-dd") });
        }


        // ===== Postback / uložení peněžního úkonu =====
        [HttpPost]
        public IActionResult SaveMoney(EntryMoneyViewModel v, string oper)
        {
            // Sešit určuje konkrétní typ (PenizeBezDPH / PenizeVcDPHRozpisu) a zda jde o výdaj či odměnu
            var recSesit = v.p34ID > 0 ? Factory.p34ActivityGroupBL.Load(v.p34ID) : null;

            // --- Postback: změna projektu (přenačtení aktivit + úkolů) ---
            if (oper == "p41change")
            {
                ReloadAllMoney(v, recSesit);

                // Zachovat vybranou aktivitu, pokud existuje v nabídce pro nový projekt
                if (v.p32ID > 0 && !v.ActivityComboItems.Any(a => a.Id == v.p32ID))
                {
                    v.p32ID = 0;
                    v.SelectedActivityText = null;
                }

                return View("EditMoney", v);
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
                        ReloadAllMoney(v, recSesit);
                        return View("EditMoney", v);
                    }
                }
            }

            if (v.p34ID <= 0 || recSesit == null)
            {
                v.Message = Factory.tra("Vyberte sešit.");
                ReloadAllMoney(v, recSesit);
                return View("EditMoney", v);
            }
            if (v.p41ID <= 0)
            {
                v.Message = Factory.tra("Vyberte projekt.");
                ReloadAllMoney(v, recSesit);
                return View("EditMoney", v);
            }

            // Aktivita povinná dle sešitu?
            if (recSesit.p34ActivityEntryFlag == BO.p34ActivityEntryFlagENUM.AktivitaJePovinna
                && v.p32ID <= 0)
            {
                v.Message = Factory.tra("Vyberte aktivitu.");
                ReloadAllMoney(v, recSesit);
                return View("EditMoney", v);
            }

            if (v.j27ID <= 0)
            {
                v.Message = Factory.tra("Vyberte měnu.");
                ReloadAllMoney(v, recSesit);
                return View("EditMoney", v);
            }

            double dblAmountWithoutVat = ParseDecimal(v.AmountWithoutVat);
            double dblAmountWithVat = ParseDecimal(v.AmountWithVat);

            if (recSesit.p33ID == BO.p33IdENUM.PenizeBezDPH && dblAmountWithoutVat == 0)
            {
                v.Message = Factory.tra("Vyplňte částku.");
                ReloadAllMoney(v, recSesit);
                return View("EditMoney", v);
            }
            if (recSesit.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu && dblAmountWithoutVat == 0 && dblAmountWithVat == 0)
            {
                v.Message = Factory.tra("Vyplňte částku.");
                ReloadAllMoney(v, recSesit);
                return View("EditMoney", v);
            }

            if (string.IsNullOrWhiteSpace(v.Description))
            {
                v.Message = Factory.tra("Vyplňte popis úkonu.");
                ReloadAllMoney(v, recSesit);
                return View("EditMoney", v);
            }

            // Načíst definice freefields + posbírat hodnoty z formuláře
            v.ff1 = LoadFreeFieldsFor(v.p34ID, v.pid);
            CollectFreeFieldsFromFormInto(v.ff1);

            var input = new BO.p31WorksheetEntryInput
            {
                j02ID = Factory.CurrentUser.pid,
                p34ID = v.p34ID,
                p41ID = v.p41ID,
                p32ID = v.p32ID,
                p56ID = v.p56ID,
                p31Text = v.Description,
                j27ID_Billing_Orig = v.j27ID,
                Amount_WithoutVat_Orig = dblAmountWithoutVat,
                p31RecordSourceFlag = 1     // 1 = mobilní aplikace
            };
            input.SetPID(v.pid);
            input.Addp31Date(v.Date);

            if (recSesit.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu)
            {
                input.VatRate_Orig = ParseDecimal(v.VatRatePercent);
                input.Amount_WithVat_Orig = dblAmountWithVat;
                input.Amount_Vat_Orig = ParseDecimal(v.AmountVat);
            }

            if (recSesit.p34IncomeStatementFlag == BO.p34IncomeStatementFlagENUM.Vydaj)
            {
                input.p31Code = v.DocumentCode;
                input.j19ID = v.j19ID;
            }

            try
            {
                var ret = Factory.p31WorksheetBL.SaveOrigRecord(input, recSesit.p33ID, v.ff1?.inputs);
                if (ret <= 0)
                {
                    v.Message = Factory.tra("Úkon se nepodařilo uložit.");
                    ReloadAllMoney(v, recSesit);
                    return View("EditMoney", v);
                }
            }
            catch (Exception ex)
            {
                v.Message = ex.Message;
                ReloadAllMoney(v, recSesit);
                return View("EditMoney", v);
            }

            if (v.Ret == "week" && !string.IsNullOrEmpty(v.RetD))
            {
                return Redirect(Url.Action("Week", "Calendar", new { d = v.RetD }) + "#entry-" + v.pid);
            }
            if (v.Ret == "ukony" && !string.IsNullOrEmpty(v.RetD))
            {
                return Redirect(v.RetD + "#entry-" + v.pid);
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
            if (ret == "ukony" && !string.IsNullOrEmpty(retd))
            {
                return Redirect(retd);
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
            v.ff1 = LoadFreeFieldsFor(v.p34ID, recPid);
        }

        private FreeFieldsViewModel LoadFreeFieldsFor(int p34ID, int recPid)
        {
            var ff1 = new FreeFieldsViewModel();
            ff1.InhaleFreeFieldsView(Factory, recPid, "p31");
            // Viditelnost dle sešitu (p34ID určuje typ záznamu)
            if (p34ID > 0)
            {
                ff1.RefreshInputsVisibility(Factory, recPid, "p31", p34ID);
            }
            return ff1;
        }

        private void CollectFreeFieldsFromForm(EntryHoursViewModel v)
        {
            CollectFreeFieldsFromFormInto(v.ff1);
        }

        private void CollectFreeFieldsFromFormInto(FreeFieldsViewModel ff1)
        {
            if (ff1?.inputs == null) return;
            // Request.Form lze číst jen u POSTu s form Content-Type (např. GET na New/Edit žádné tělo nemá)
            if (!Request.HasFormContentType) return;

            foreach (var ff in ff1.inputs)
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

            LoadKusovnikOffer(v);
            if (v.IsNavicKusovnik && v.p41ID > 0)
            {
                LoadKusovnikForProject(v);
            }
            LoadExistingKusovnikEntries(v);
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


        // ===== Pomocné metody - kusovníkové úkony "navíc" k hodinám =====

        // Zda vůbec nabízet checkbox "K hodinám vykázat i kusovníkové úkony" - existuje-li pro uživatele
        // aspoň jeden sešit typu Kusovník (v libovolném jeho projektu).
        private void LoadKusovnikOffer(EntryHoursViewModel v)
        {
            v.IsOfferNavicKusovnik = Factory.p34ActivityGroupBL
                .GetList_WorksheetEntry_InAllProjects(Factory.CurrentUser.pid)
                .Any(s => s.p33ID == BO.p33IdENUM.Kusovnik);
        }

        // Již uložené kusovníkové úkony navázané na tento hodinový úkon (p31MasterID) - jen pro zobrazení.
        private void LoadExistingKusovnikEntries(EntryHoursViewModel v)
        {
            if (v.pid <= 0)
            {
                v.ExistingKusovnikEntries = new List<BO.p31Worksheet>();
                return;
            }
            v.ExistingKusovnikEntries = Factory.p31WorksheetBL.GetList(new BO.myQueryP31 { p31masterid = v.pid }).ToList();
        }

        // Nabídka kusovníkových sešitů a aktivit pro konkrétní vybraný projekt (v.p41ID).
        private void LoadKusovnikForProject(EntryHoursViewModel v)
        {
            if (v.p41ID <= 0) return;

            var recP41 = Factory.p41ProjectBL.Load(v.p41ID);
            if (recP41 == null) return;

            var lisP34 = Factory.p34ActivityGroupBL
                .GetList_WorksheetEntryIn_OneProject(recP41, Factory.CurrentUser.pid)
                .Where(s => s.p33ID == BO.p33IdENUM.Kusovnik)
                .ToList();

            v.KusovnikSesitComboItems = lisP34.Select(s => new ComboItem
            {
                Id = s.pid,
                Text = s.p34Name
            }).ToList();

            if (lisP34.Count == 0)
            {
                v.p34ID_Kusovnik = 0;
                v.KusovnikActivityComboItems = new List<ComboItem>();
                return;
            }

            if (v.p34ID_Kusovnik <= 0 || !lisP34.Any(s => s.pid == v.p34ID_Kusovnik))
            {
                v.p34ID_Kusovnik = lisP34.First().pid;
            }

            var lisP32 = Factory.p32ActivityBL.GetList(new BO.myQueryP32
            {
                p34id = v.p34ID_Kusovnik
            }).ToList();

            v.KusovnikActivityComboItems = lisP32.Select(a => new ComboItem
            {
                Id = a.pid,
                Text = a.p32Name
            }).ToList();
        }

        // Zvaliduje vyplněné řádky kusovníku (prázdné řádky se přeskočí). Vrátí chybovou zprávu, nebo null.
        private string ValidateKusovnikRows(EntryHoursViewModel v)
        {
            if (v.KusovnikRows == null) return null;

            foreach (var row in v.KusovnikRows)
            {
                bool isUsed = row.p32ID > 0 || !string.IsNullOrWhiteSpace(row.Pocet) || !string.IsNullOrWhiteSpace(row.Text);
                if (!isUsed) continue;

                if (row.p32ID <= 0)
                {
                    return Factory.tra("U kusovníkového řádku vyberte aktivitu.");
                }
                var recP32 = Factory.p32ActivityBL.Load(row.p32ID);
                if (recP32 == null)
                {
                    return Factory.tra("U kusovníkového řádku vyberte aktivitu.");
                }
                if (ParseDecimal(row.Pocet) == 0)
                {
                    return Factory.tra("U kusovníkového řádku zadejte počet.");
                }
                if (recP32.p32IsTextRequired && string.IsNullOrWhiteSpace(row.Text))
                {
                    return Factory.tra("U kusovníkového řádku vyplňte text.");
                }
            }
            return null;
        }

        // Uloží vyplněné kusovníkové řádky jako samostatné úkony navázané na hlavní hodinový úkon (p31MasterID).
        // Prázdné řádky se přeskočí. Vždy se přidávají nové - existující se touto cestou needitují.
        private void SaveKusovnikRows(int masterPid, EntryHoursViewModel v)
        {
            var recMaster = Factory.p31WorksheetBL.Load(masterPid);
            if (recMaster == null) return;

            foreach (var row in v.KusovnikRows)
            {
                bool isUsed = row.p32ID > 0 || !string.IsNullOrWhiteSpace(row.Pocet) || !string.IsNullOrWhiteSpace(row.Text);
                if (!isUsed) continue;
                if (row.p32ID <= 0 || ParseDecimal(row.Pocet) == 0) continue;

                var recSlave = new BO.p31WorksheetEntryInput
                {
                    p31MasterID = masterPid,
                    p31Text = row.Text,
                    p34ID = v.p34ID_Kusovnik,
                    p32ID = row.p32ID,
                    Value_Orig = row.Pocet,
                    p41ID = recMaster.p41ID,
                    p56ID = recMaster.p56ID,
                    j02ID = recMaster.j02ID
                };
                recSlave.Addp31Date(recMaster.p31Date);

                Factory.p31WorksheetBL.SaveOrigRecord(recSlave, BO.p33IdENUM.Kusovnik, null);
            }
        }


        // ===== Pomocné metody - peněžní úkon =====

        private static bool IsMoneyType(BO.p33IdENUM p33id)
        {
            return p33id == BO.p33IdENUM.PenizeBezDPH || p33id == BO.p33IdENUM.PenizeVcDPHRozpisu;
        }

        private double ParseDecimal(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            double.TryParse(s.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var num);
            return num;
        }

        private void LoadSesitListMoney(EntryMoneyViewModel v)
        {
            var lisP34 = Factory.p34ActivityGroupBL
                .GetList_WorksheetEntry_InAllProjects(Factory.CurrentUser.pid)
                .Where(s => IsMoneyType(s.p33ID))
                .ToList();

            v.SesitComboItems = lisP34.Select(s => new ComboItem
            {
                Id = s.pid,
                Code = s.p34Code,
                Text = s.p34Name,
                Meta = s.p33Name
            }).ToList();

            if (v.p34ID > 0)
            {
                var sel = lisP34.FirstOrDefault(s => s.pid == v.p34ID);
                if (sel != null)
                {
                    v.SelectedSesitText = sel.p34Name;
                    v.ActivityEntryFlag = (int)sel.p34ActivityEntryFlag;
                    v.p33ID = sel.p33ID;
                    v.IncomeStatementFlag = (int)sel.p34IncomeStatementFlag;
                }
            }
        }

        private void LoadProjectsMoney(EntryMoneyViewModel v)
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

        private void LoadActivitiesMoney(EntryMoneyViewModel v)
        {
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

        private void LoadCurrencyList(EntryMoneyViewModel v)
        {
            var lis = Factory.FBL.GetListCurrency().ToList();

            v.CurrencyComboItems = lis.Select(c => new ComboItem
            {
                Id = c.pid,
                Code = c.j27Code,
                Text = c.j27Name
            }).ToList();

            if (v.j27ID > 0)
            {
                var sel = lis.FirstOrDefault(c => c.pid == v.j27ID);
                if (sel == null)
                {
                    // Vybraná měna nemusí být v aktuálně platném seznamu (např. u staršího záznamu) - dohledat samostatně
                    sel = Factory.FBL.LoadCurrencyByID(v.j27ID);
                }
                if (sel != null) v.SelectedCurrencyText = sel.j27Code;
            }
        }

        private void LoadPaymentTypeList(EntryMoneyViewModel v)
        {
            var lis = Factory.FBL.GetListJ19().ToList();

            v.PaymentTypeComboItems = lis.Select(p => new ComboItem
            {
                Id = p.pid,
                Text = p.j19Name
            }).ToList();

            if (v.j19ID > 0)
            {
                var sel = lis.FirstOrDefault(p => p.pid == v.j19ID);
                if (sel != null) v.SelectedPaymentTypeText = sel.j19Name;
            }
        }

        // Přenačte veškeré nabídky pro peněžní úkon. Sešit lze předat rovnou (např. z New/Edit),
        // jinak se dohledá dle v.p34ID.
        private void ReloadAllMoney(EntryMoneyViewModel v, BO.p34ActivityGroup sesit = null)
        {
            LoadSesitListMoney(v);
            LoadProjectsMoney(v);
            LoadActivitiesMoney(v);
            LoadCurrencyList(v);

            if (sesit == null && v.p34ID > 0)
            {
                sesit = Factory.p34ActivityGroupBL.Load(v.p34ID);
            }
            if (sesit != null)
            {
                v.p33ID = sesit.p33ID;
                v.IncomeStatementFlag = (int)sesit.p34IncomeStatementFlag;
            }

            if (v.IncomeStatementFlag == (int)BO.p34IncomeStatementFlagENUM.Vydaj)
            {
                LoadPaymentTypeList(v);
            }

            // Freefields: definice + viditelnost, pak posbírat hodnoty z POSTu zpět
            v.ff1 = LoadFreeFieldsFor(v.p34ID, v.pid);
            CollectFreeFieldsFromFormInto(v.ff1);
        }
    }
}