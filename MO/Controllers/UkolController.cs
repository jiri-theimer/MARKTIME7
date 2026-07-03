using Microsoft.AspNetCore.Mvc;
using MO.Models;

namespace MO.Controllers
{
    public class UkolController : BaseController
    {
        public IActionResult Edit(int id, string ret = null, string retd = null)
        {
            var rec = Factory.p56TaskBL.Load(id);
            if (rec == null)
                return RedirectToAction("Tasks", "Home");

            var disp = Factory.p56TaskBL.InhaleRecDisposition(id, rec);
            if (!disp.ReadAccess)
            {
                return View(new UkolViewModel
                {
                    PageTitle = Factory.tra("Úkol"),
                    HideHeaderTitle = true,
                    Message = Factory.tra("Nemáte oprávnění k tomuto úkolu."),
                    Ret = ret,
                    RetD = retd
                });
            }

            var v = BuildViewModel(rec, disp, ret, retd);
            return View(v);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UkolViewModel v, string oper)
        {
            var rec = Factory.p56TaskBL.Load(v.pid);
            if (rec == null)
                return RedirectToAction("Tasks", "Home");

            var disp = Factory.p56TaskBL.InhaleRecDisposition(v.pid, rec);
            if (!disp.ReadAccess || !disp.OwnerAccess)
            {
                var vRo = BuildViewModel(rec, disp, v.Ret, v.RetD);
                vRo.Message = Factory.tra("Nemáte oprávnění tento úkol upravovat.");
                return View(vRo);
            }

            // Uložit jen to, co ve zjednodušeném mobilním formuláři skutečně editujeme -
            // zbytek záznamu (typ, projekt, vlastník, kód...) zůstává z Load() beze změny.
            if (string.IsNullOrWhiteSpace(v.Name))
            {
                var vErrName = BuildViewModel(rec, disp, v.Ret, v.RetD);
                vErrName.Message = Factory.tra("Vyplňte název úkolu.");
                return View(vErrName);
            }

            var statusChanged = v.TargetB02ID > 0 && v.TargetB02ID != rec.b02ID;

            rec.p56Name = v.Name;
            rec.p56PlanFrom = ParseDate(v.PlanFrom);
            rec.p56PlanUntil = ParseDate(v.PlanUntil);
            rec.p56Notepad = v.Notepad;

            try
            {
                var ret = Factory.p56TaskBL.Save(rec, null, null);
                if (ret <= 0)
                {
                    var vErr = BuildViewModel(rec, disp, v.Ret, v.RetD);
                    vErr.Message = Factory.tra("Úkol se nepodařilo uložit.");
                    return View(vErr);
                }

                // Stav úkolu se neukládá přes p56TaskBL.Save, ale přes workflow engine
                if (statusChanged)
                {
                    var retWf = Factory.WorkflowBL.RunWorkflowStatus("p56", rec.pid, v.TargetB02ID, null, 0);
                    if (retWf <= 0)
                    {
                        var vErr = BuildViewModel(Factory.p56TaskBL.Load(rec.pid), disp, v.Ret, v.RetD);
                        vErr.Message = Factory.tra("Stav úkolu se nepodařilo změnit.");
                        return View(vErr);
                    }
                }
            }
            catch (Exception ex)
            {
                var vErr = BuildViewModel(rec, disp, v.Ret, v.RetD);
                vErr.Message = ex.Message;
                return View(vErr);
            }

            if (v.Ret == "ukoly" && !string.IsNullOrEmpty(v.RetD))
            {
                return Redirect(v.RetD + "#task-" + v.pid);
            }
            if (v.Ret == "week" && !string.IsNullOrEmpty(v.RetD))
            {
                return RedirectToAction("Week", "Calendar", new { d = v.RetD });
            }
            if (v.Ret == "day" && !string.IsNullOrEmpty(v.RetD))
            {
                return RedirectToAction("Day", "Calendar", new { d = v.RetD });
            }
            return Redirect(Url.Action("Tasks", "Home") + "#task-" + v.pid);
        }


        // ===== Pomocné metody =====

        private UkolViewModel BuildViewModel(BO.p56Task rec, BO.p56RecDisposition disp, string ret, string retd)
        {
            var isReadOnly = !disp.OwnerAccess;

            var statusOptions = Factory.b02WorkflowStatusBL
                .GetList(new BO.myQuery("b02") { IsRecordValid = true })
                .Where(s => s.b01ID == rec.b01ID)
                .OrderBy(s => s.b02Ordinary)
                .Select(s => new ComboItem { Id = s.pid, Text = s.b02Name })
                .ToList();

            return new UkolViewModel
            {
                PageTitle = Factory.tra("Úkol"),
                HideHeaderTitle = true,
                pid = rec.pid,
                Name = rec.p56Name,
                TaskType = rec.p57Name,
                ProjectDisplay = rec.p41ID > 0 ? rec.ProjectWithClient : null,
                p41ID = rec.p41ID,
                Owner = rec.Owner,
                WorkflowStatusName = rec.b02Name,
                WorkflowStatusColor = rec.b02Color,
                WorkflowStatusForeColor = rec.ForeColor,
                TargetB02ID = rec.b02ID,
                StatusOptions = statusOptions,
                PlanFrom = rec.p56PlanFrom?.ToString("yyyy-MM-dd"),
                PlanUntil = rec.p56PlanUntil?.ToString("yyyy-MM-dd"),
                Notepad = rec.p56Notepad,
                IsReadOnly = isReadOnly,
                Ret = ret,
                RetD = retd
            };
        }

        private DateTime? ParseDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParseExact(s, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var dt))
                return dt;
            return null;
        }
    }
}