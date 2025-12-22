using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using UI.Models.Tags;
using UI.Models.wrk;

namespace UI.Controllers.wrk
{
    public class workflow_batchController : BaseController
    {
        public IActionResult Index(string prefix, string pids, string guid_pids)
        {
            if (!string.IsNullOrEmpty(guid_pids))
            {
                pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            var v = new WorkflowBatchViewModel() { Record_Prefix = prefix, Record_Pids = pids };            

            var lisPIDs = BO.Code.Bas.ConvertString2ListInt(v.Record_Pids);
            if (lisPIDs.Count() == 0)
            {
                return this.StopPage(true, "Na vstupu chybí výběr záznamů.");
            }
            v.lisRecs = new List<LocalRepeater>();

            switch (v.Record_Prefix)
            {
                case "p56":
                    var lisP56 = Factory.p56TaskBL.GetList(new BO.myQueryP56() { IsRecordValid = null, pids = lisPIDs });
                    foreach (var c in lisP56)
                    {
                        v.lisRecs.Add(new LocalRepeater() { pid = c.pid, b02ID = c.b02ID, b02Name = c.b02Name, Alias = c.FullName,b02Color=c.b02Color });
                    }
                    break;
                case "p28":
                    var lisP28 = Factory.p28ContactBL.GetList(new BO.myQueryP28() { IsRecordValid = null, pids = lisPIDs });
                    foreach (var c in lisP28)
                    {
                        var lr = new LocalRepeater() { pid = c.pid, b02ID = c.b02ID, Alias = c.p28Name };
                        if (c.b02ID > 0)
                        {
                            lr.b02Name = Factory.b02WorkflowStatusBL.Load(c.b02ID).b02Name;
                            lr.b02Color = Factory.b02WorkflowStatusBL.Load(c.b02ID).b02Color;
                        }
                        v.lisRecs.Add(lr);
                    }
                    break;
                case "p91":
                    var lisP91 = Factory.p91InvoiceBL.GetList(new BO.myQueryP91() { IsRecordValid = null, pids = lisPIDs });
                    foreach (var c in lisP91)
                    {
                        v.lisRecs.Add(new LocalRepeater() { pid = c.pid, b02ID = c.b02ID, b02Name = c.b02Name, Alias = c.p91Code});
                    }
                    break;
                case "o23":
                    var lisO23 = Factory.o23DocBL.GetList(new BO.myQueryO23() { IsRecordValid = null, pids = lisPIDs });
                    foreach (var c in lisO23)
                    {
                        v.lisRecs.Add(new LocalRepeater() { pid = c.pid, b02ID = c.b02ID, b02Name = c.b02Name, Alias = c.o23Name });
                    }
                    break;
                case "le4":
                case "le5":
                case "p41":
                    var lisP41 = Factory.p41ProjectBL.GetList(new BO.myQueryP41(v.Record_Prefix) { IsRecordValid = null, pids = lisPIDs });
                    foreach (var c in lisP41)
                    {
                        v.lisRecs.Add(new LocalRepeater() { pid = c.pid, b02ID = c.b02ID, b02Name = c.b02Name, Alias = c.FullName,b02Color=c.b02Color });
                    }
                    break;
            }

            if (v.lisRecs.Count() == 0)
            {
                return this.StopPage(true, "Na vstupu chybí dostupné záznamy.");
            }

            foreach (var row in v.lisRecs)
            {
                row.LocalSteps = new List<LocalStep>();
                
            }

            foreach (var row in v.lisRecs)
            {
                row.LocalSteps.Add(new LocalStep() { b06ID = 0, b06Name = "Nic" });
                row.LocalSteps.Add(new LocalStep() { b06ID = -1, b06Name = "Pouze komentář" });

                if (row.b02ID > 0)
                {
                    var lisB06 = Factory.b06WorkflowStepBL.GetList(new BO.myQuery("b06")).Where(p => p.b02ID == row.b02ID && p.b06AutoRunFlag == BO.b06AutoRunFlagEnum.UzivatelskyKrok && p.b06NomineeFlag == BO.b06NomineeFlagENum._None).ToList();
                    foreach (var c in lisB06)
                    {
                        if (Factory.WorkflowBL.IsStepAvailable4Me(c, row.pid,BO.Code.Entity.GetPrefixDb( v.Record_Prefix)))
                        {

                            row.LocalSteps.Add(new LocalStep() { b06ID = c.pid, b06Name = c.b06Name });
                        }
                    }
                }
                

            }

           
            

            RefreshState(v);

            return View(v);
        }


        private void RefreshState(WorkflowBatchViewModel v)
        {


        }

        [HttpPost]
        public IActionResult Index(WorkflowBatchViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                string s0 = null;int x = 1;int row_pid = 0;
                foreach(var row in v.lisRecs)
                {
                    s0 = "#" + x.ToString();
                    if (row.SelectedB06ID==-1 && (string.IsNullOrEmpty(row.Comment) || row.Comment.Length < 2))
                    {
                        this.AddMessageTranslated(s0+": "+this.Factory.tra("Chybí vyplnit komentář."));
                        return View(v);
                    }
                    x += 1;
                }
                
                foreach (var row in v.lisRecs.Where(p => p.SelectedB06ID !=0))
                {
                    s0 = "#" + x.ToString();
                    int intB05ID = 0;
                    if (row.SelectedB06ID == -1)
                    {
                        //pouze komentář
                        intB05ID=Factory.WorkflowBL.RunWorkflowStep(true,row.pid, BO.Code.Entity.GetPrefixDb(v.Record_Prefix), row.Comment,0,new BO.b06WorkflowStep(),null);
                    }
                    else
                    {
                        var recB06 = Factory.b06WorkflowStepBL.Load(row.SelectedB06ID);
                        intB05ID=Factory.WorkflowBL.RunWorkflowStep(true, row.pid, BO.Code.Entity.GetPrefixDb(v.Record_Prefix), row.Comment, 0, recB06, null);
                    }
                    x += 1;
                    row_pid = row.pid;
                }

                v.SetJavascript_CallOnLoad(row_pid);
                return View(v);
            }


            return View(v);
        }
    }
}
