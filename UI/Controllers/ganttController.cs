using Microsoft.AspNetCore.Mvc;
using UI.Models.Gantt;

namespace UI.Controllers
{
    //Gantt: časový plán projektů (p41PlanFrom/p41PlanUntil) a jejich úkolů (p56PlanFrom/p56PlanUntil)
    public class ganttController : BaseController
    {
        private const int MAX_PROJECTS = 500;
        private const int MAX_TASKS = 2000;

        public IActionResult Index(int p41id)
        {
            if (!Factory.CurrentUser.j04IsModule_p41)
            {
                return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
            }
            var v = new GanttViewModel() { p41id = p41id };
            if (v.p41id > 0)
            {
                v.RecP41 = Factory.p41ProjectBL.Load(v.p41id);
                if (v.RecP41 == null || !Factory.p41ProjectBL.InhaleRecDisposition(v.p41id, v.RecP41).ReadAccess)
                {
                    return this.StopPage(false, "Projekt nebyl nalezen nebo k němu nemáte přístup.");
                }
            }
            v.Zoom = Factory.CBL.LoadUserParam("gantt-zoom", "week");
            v.IsTasksModule = Factory.CurrentUser.j04IsModule_p56;
            v.ShowTasks = v.IsTasksModule && Factory.CBL.LoadUserParamInt("gantt-tasks", 1) == 1;

            return View(v);
        }

        public IActionResult Data(int p41id, bool tasks)
        {
            if (!Factory.CurrentUser.j04IsModule_p41)
            {
                return Json(new { ok = false, message = Factory.tra("Nemáte oprávnění pro tento Modul.") });
            }

            //1. projekty
            var mq = new BO.myQueryP41("p41");
            BO.p41Project recRoot = null;
            if (p41id > 0)
            {
                recRoot = Factory.p41ProjectBL.Load(p41id);
                if (recRoot == null || !Factory.p41ProjectBL.InhaleRecDisposition(p41id, recRoot).ReadAccess)
                {
                    return Json(new { ok = false, message = Factory.tra("Projekt nebyl nalezen nebo k němu nemáte přístup.") });
                }
                mq.IsRecordValid = null;
                if (recRoot.p07Level >= 1 && recRoot.p07Level <= 4)
                {
                    mq.leindex = recRoot.p07Level; mq.lepid = p41id;    //celý podstrom projektu
                }
                else
                {
                    mq.pids = new List<int>() { p41id };
                }
            }
            else
            {
                mq.explicit_sqlwhere = "a.p41PlanFrom IS NOT NULL AND a.p41PlanUntil IS NOT NULL";
                mq.TopRecordsOnly = MAX_PROJECTS + 1;
                mq.explicit_orderby = "a.p41PlanFrom";
            }

            var lisP41 = Factory.p41ProjectBL.GetList(mq).ToList();
            if (recRoot != null && !lisP41.Any(p => p.pid == recRoot.pid))
            {
                lisP41.Insert(0, recRoot);
            }
            bool bolTruncated = false;
            if (lisP41.Count > MAX_PROJECTS)
            {
                bolTruncated = true;
                lisP41 = lisP41.Take(MAX_PROJECTS).ToList();
            }

            //2. úkoly projektů
            var lisP56 = new List<BO.p56Task>();
            if (tasks && Factory.CurrentUser.j04IsModule_p56 && lisP41.Count > 0)
            {
                var mqP56 = new BO.myQueryP56() { TopRecordsOnly = MAX_TASKS + 1 };
                string strIn = string.Join(",", lisP41.Select(p => p.pid));
                if (p41id > 0)
                {
                    mqP56.explicit_sqlwhere = $"a.p41ID IN ({strIn})";  //v kontextu projektu i úkoly bez plánu, aby šel plán nakreslit
                }
                else
                {
                    mqP56.explicit_sqlwhere = $"a.p41ID IN ({strIn}) AND (a.p56PlanFrom IS NOT NULL OR a.p56PlanUntil IS NOT NULL)";
                }
                lisP56 = Factory.p56TaskBL.GetList(mqP56).ToList();
                if (lisP56.Count > MAX_TASKS)
                {
                    bolTruncated = true;
                    lisP56 = lisP56.Take(MAX_TASKS).ToList();
                }
            }

            //3. řádky v pořadí stromu: nadřízený projekt -> jeho úkoly -> podřízené projekty
            bool bolP41Owner = Factory.CurrentUser.IsAdmin || Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_p41_Owner);
            bool bolP56Owner = Factory.CurrentUser.IsAdmin || Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_p56_Owner);
            var ids = lisP41.Select(p => p.pid).ToHashSet();
            var kids = lisP41.Where(p => p.p41ParentID > 0 && ids.Contains(p.p41ParentID) && p.p41ParentID != p.pid).ToLookup(p => p.p41ParentID);
            var taskLookup = lisP56.ToLookup(p => p.p41ID);
            var rows = new List<GanttRow>();
            var done = new HashSet<int>();

            void AddProject(BO.p41Project c, string strParentKey, int intDepth)
            {
                if (!done.Add(c.pid) || intDepth > 10) return;    //pojistka proti cyklu v p41ParentID
                var row = new GanttRow()
                {
                    key = "p41-" + c.pid.ToString(), prefix = "p41", pid = c.pid, parent = strParentKey, depth = intDepth,
                    name = string.IsNullOrEmpty(c.p41NameShort) ? c.p41Name : c.p41NameShort, code = c.p41Code, sub = c.Client,
                    hours = c.p41Plan_Hours, closed = c.isclosed,
                    edit = (bolP41Owner || c.j02ID_Owner == Factory.CurrentUser.pid) ? 1 : 2
                };
                if (c.p41PlanFrom != null && c.p41PlanUntil != null)
                {
                    row.d1 = Convert.ToDateTime(c.p41PlanFrom).ToString("yyyy-MM-dd");
                    row.d2 = Convert.ToDateTime(c.p41PlanUntil).ToString("yyyy-MM-dd");
                }
                rows.Add(row);

                foreach (var t in taskLookup[c.pid].OrderBy(p => p.p56PlanFrom ?? p.p56PlanUntil ?? DateTime.MaxValue).ThenBy(p => p.p56Name))
                {
                    var rt = new GanttRow()
                    {
                        key = "p56-" + t.pid.ToString(), prefix = "p56", pid = t.pid, parent = row.key, depth = intDepth + 1,
                        name = t.p56Name, code = t.p56Code, sub = t.b02Name, color = t.b02Color,
                        hours = t.p56Plan_Hours, closed = t.isclosed,
                        edit = (bolP56Owner || t.j02ID_Owner == Factory.CurrentUser.pid) ? 1 : 2
                    };
                    var d1 = t.p56PlanFrom ?? t.p56PlanUntil;
                    var d2 = t.p56PlanUntil ?? t.p56PlanFrom;
                    if (d1 != null && d2 != null)
                    {
                        if (d2 < d1) d2 = d1;
                        rt.d1 = Convert.ToDateTime(d1).ToString("yyyy-MM-dd");
                        rt.d2 = Convert.ToDateTime(d2).ToString("yyyy-MM-dd");
                        rt.onlyend = (t.p56PlanFrom == null);
                    }
                    rows.Add(rt);
                }

                foreach (var k in kids[c.pid].OrderBy(p => p.p41Name))
                {
                    AddProject(k, row.key, intDepth + 1);
                }
            }

            foreach (var c in lisP41.Where(p => p.p41ParentID == 0 || !ids.Contains(p.p41ParentID) || p.p41ParentID == p.pid).OrderBy(p => p.p41Name))
            {
                AddProject(c, null, 0);
            }
            foreach (var c in lisP41.Where(p => !done.Contains(p.pid)).ToList())    //zbytek (cyklus)
            {
                AddProject(c, null, 0);
            }

            return Json(new { ok = true, rows = rows, today = DateTime.Today.ToString("yyyy-MM-dd"), truncated = bolTruncated });
        }

        private string TakeNotifyMessage()   //chybová hláška z BL jde do odpovědi, ne do notifikace při příštím načtení stránky
        {
            string s = Factory.GetFirstNotifyMessage();
            Factory.CurrentUser.Messages4Notify = null;
            return string.IsNullOrEmpty(s) ? Factory.tra("Záznam se nepodařilo uložit.") : s;
        }

        [HttpPost]
        public IActionResult SaveDates(string prefix, int pid, string d1, string d2)
        {
            DateTime dat1; DateTime dat2;
            if (pid <= 0
                || !DateTime.TryParseExact(d1, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dat1)
                || !DateTime.TryParseExact(d2, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dat2)
                || dat2 < dat1)
            {
                return Json(new { ok = false, message = Factory.tra("Neplatné období.") });
            }

            Factory.CurrentUser.Messages4Notify = null;     //aby GetFirstNotifyMessage vracel chybu právě tohoto uložení
            switch (prefix)
            {
                case "p41":
                    var recP41 = Factory.p41ProjectBL.Load(pid);
                    if (recP41 == null || !Factory.p41ProjectBL.InhaleRecDisposition(pid, recP41).OwnerAccess)
                    {
                        return Json(new { ok = false, message = Factory.tra("Nemáte oprávnění upravovat tento projekt.") });
                    }
                    //kapacitní plán projektu (r01) musí zůstat uvnitř plánovaného období projektu
                    var recR01 = Factory.db.Load<BO.GetInteger>("select count(*) as Value FROM r01Capacity WHERE p41ID=@pid AND (r01Start<@d1 OR r01End>@d2)", new { pid = pid, d1 = dat1, d2 = dat2.AddDays(1).AddSeconds(-1) });
                    if (recR01 != null && recR01.Value > 0)
                    {
                        return Json(new { ok = false, message = Factory.tra("Projekt má kapacitní plán, který by zůstal mimo nové období. Období posuňte v kapacitním plánu projektu.") });
                    }
                    recP41.p41PlanFrom = dat1;
                    recP41.p41PlanUntil = dat2;     //konec dne doplní p41ProjectBL.Save
                    if (Factory.p41ProjectBL.Save(recP41, null, null, null) == 0)
                    {
                        return Json(new { ok = false, message = TakeNotifyMessage() });
                    }
                    break;
                case "p56":
                    var recP56 = Factory.p56TaskBL.Load(pid);
                    if (recP56 == null || !Factory.p56TaskBL.InhaleRecDisposition(pid, recP56).OwnerAccess)
                    {
                        return Json(new { ok = false, message = Factory.tra("Nemáte oprávnění upravovat tento úkol.") });
                    }
                    //čas v rámci dne se zachovává, mění se jen datum
                    var tsUntil = recP56.p56PlanUntil != null ? Convert.ToDateTime(recP56.p56PlanUntil).TimeOfDay : TimeSpan.Zero;  //00:00 -> p56TaskBL.Save doplní konec dne
                    var tsFrom = recP56.p56PlanFrom != null ? Convert.ToDateTime(recP56.p56PlanFrom).TimeOfDay : TimeSpan.Zero;
                    if (recP56.p56PlanFrom == null && dat1 == dat2)
                    {
                        recP56.p56PlanUntil = dat2.Add(tsUntil);    //úkol jen s termínem: posouvá se pouze termín
                    }
                    else
                    {
                        recP56.p56PlanFrom = dat1.Add(tsFrom);
                        recP56.p56PlanUntil = dat2.Add(tsUntil);
                        if (tsUntil != TimeSpan.Zero && recP56.p56PlanUntil < recP56.p56PlanFrom)
                        {
                            recP56.p56PlanFrom = dat1;
                        }
                    }
                    if (Factory.p56TaskBL.Save(recP56, null, null) == 0)
                    {
                        return Json(new { ok = false, message = TakeNotifyMessage() });
                    }
                    break;
                default:
                    return BadRequest();
            }

            return Json(new { ok = true });
        }
    }
}
