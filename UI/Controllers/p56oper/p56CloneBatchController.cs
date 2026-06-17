using Microsoft.AspNetCore.Mvc;
using UI.Models.p31oper;
using UI.Models;
using UI.Models.p56oper;
using UI.Models.Record;

namespace UI.Controllers.p56oper
{
    public class p56CloneBatchController : BaseController
    {
        public IActionResult Index(string pids,string guid_pids)
        {
            if (!string.IsNullOrEmpty(guid_pids))
            {
                pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            var v = new p56CloneBatchViewModel() { pids = pids };
            v.ProjectCombo = new ProjectComboViewModel();

            RefreshState(v);
            if (v.lisSourceP56.Count() == 0)
            {
                return StopPage(true, "Na vstupu chybí záznamy úkolů.");
            }
            v.lisDest = new List<p56Clone>();
            foreach (var rec in v.lisSourceP56)
            {
                string ss = string.Join(", ", Factory.x67EntityRoleBL.GetRolesAssignedToRecord(rec.pid, "p56").Select(p => p.Receivers));
                v.lisDest.Add(new p56Clone() {Importovat=true, p56Name = rec.p56Name, pid = rec.pid, p56Notepad = ss, p56PlanUntil = rec.p56PlanUntil, p56PlanFrom = rec.p56PlanFrom, p41ID = rec.p41ID,Project=rec.p41Name });

            }
           


            return View(v);
        }

        private void RefreshState(p56CloneBatchViewModel v)
        {
            
            var mq = new BO.myQueryP56() { pids = BO.Code.Bas.ConvertString2ListInt(v.pids),IsRecordValid=null };
            v.lisSourceP56 = Factory.p56TaskBL.GetList(mq);

            if (v.ProjectCombo.SelectedP41ID > 0)
            {
                v.RecP41 = Factory.p41ProjectBL.Load(v.ProjectCombo.SelectedP41ID);
            }

        }

        [HttpPost]
        public IActionResult Index(p56CloneBatchViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "p41id" && v.ProjectCombo.SelectedP41ID > 0)
                {
                    foreach (var c in v.lisDest)
                    {
                        c.p41ID = v.ProjectCombo.SelectedP41ID; c.Project = v.RecP41.p41Name;
                        
                    }
                }



                return View(v);
            }

            if (ModelState.IsValid)
            {
                int intErrs = 0; int intOks = 0;
                foreach (var c in v.lisDest.Where(p=>p.Importovat))
                {
                    var rec = v.lisSourceP56.First(p => p.pid == c.pid);
                    rec.pid = 0;

                    rec.p56Name = c.p56Name; rec.p56PlanFrom = c.p56PlanFrom; rec.p56PlanUntil = c.p56PlanUntil; rec.pid = 0; rec.ValidFrom = DateTime.Now; rec.ValidUntil = new DateTime(3000, 1, 1); rec.p41ID = v.ProjectCombo.SelectedP41ID; rec.p56Plan_Hours = c.p56Plan_Hours;
                    rec.p55ID = 0;
                    int intNewPID=Factory.p56TaskBL.Save(rec, null, Factory.x67EntityRoleBL.GetList_X69("p56", c.pid).ToList());

                    if (intNewPID == 0)
                    {
                        intErrs += 1;
                    }
                    else
                    {
                        intOks += 1;
                    }

                }
                if (intErrs > 0)
                {

                    this.AddMessageTranslated($"Počet uložených úkolů: {intOks}, počet chyb: {intErrs}.", "info");
                }
                else
                {
                    v.SetJavascript_CallOnLoad(0);
                    return View(v);
                }
            }

            return View(v);
        }
    }
}
