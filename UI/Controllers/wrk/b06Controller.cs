using BL;
using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;
using System.Linq;

namespace UI.Controllers.wrk
{
    public class b06Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone, int b02id)
        {
            var v = new b06Record() { rec_pid = pid, rec_entity = "b06" };
            v.Rec = new BO.b06WorkflowStep() { b02ID = b02id,b06AutoRunFlag=BO.b06AutoRunFlagEnum.UzivatelskyKrok };

            if (v.rec_pid > 0)
            {
                v.Rec = Factory.b06WorkflowStepBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }

                var lisB08 = Factory.b06WorkflowStepBL.GetListB08(v.rec_pid);
                v.Receiver_x67IDs = lisB08.Where(p => p.x67ID > 0).Select(p => p.x67ID).ToList();
                v.Receiver_j11IDs = string.Join(",", lisB08.Where(p => p.j11ID > 0).Select(p => p.j11ID));
                v.Receiver_j11Names = string.Join(",", lisB08.Where(p => p.j11ID > 0).Select(p => p.j11Name));
                v.Receiver_j04IDs = string.Join(",", lisB08.Where(p => p.j04ID > 0).Select(p => p.j04ID));
                v.Receiver_j04Names = string.Join(",", lisB08.Where(p => p.j04ID > 0).Select(p => p.j04Name));
                if (lisB08.Where(p => p.b08IsRecordOwner).Count() > 0)
                {
                    v.Receiver_IsRecordOwner = true;
                }
                if (lisB08.Where(p => p.b08IsRecordCreator).Count() > 0)
                {
                    v.Receiver_IsRecordCreator = true;
                }

                var lisB11 = Factory.b06WorkflowStepBL.GetListB11(v.rec_pid);
                foreach (var c in lisB11)
                {
                    var cc = new b11RepeaterItem() { j04ID = c.j04ID,j04Name=c.j04Name, j11ID = c.j11ID,j11Name=c.j11Name, x67ID = c.x67ID,x67Name=c.x67Name, j61ID = c.j61ID,j61Name=c.j61Name,b11Subject=c.b11Subject };
                    if (c.b11IsRecordOwner) cc.RecordUserRole = 1;
                    if (c.b11IsRecordCreator) cc.RecordUserRole = 2;
                    if (c.b11IsRecordCreatorByEmail) cc.RecordUserRole = 3;
                    if (v.lisB11 == null) v.lisB11 = new List<b11RepeaterItem>();
                    v.lisB11.Add(cc);
                }
                if (v.Rec.j11ID_Direct > 0)
                {
                    v.Nominee_j04Name = Factory.j11TeamBL.Load(v.Rec.j11ID_Direct).j11Name;
                }
            }
            
            RefreshState(v);
            if (v.RecB02 == null)
            {
                return this.StopPage(true, "Na vstupu chybí Workflow stav.");
            }
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(b06Record v)
        {            
            if (v.Rec.b02ID > 0)
            {
                v.RecB02 = Factory.b02WorkflowStatusBL.Load(v.Rec.b02ID);
                if (v.RecB02.b02AutoRunFlag==BO.b02AutoRunFlagEnum.Technicky && v.Rec.b06AutoRunFlag == BO.b06AutoRunFlagEnum.UzivatelskyKrok)
                {
                    v.Rec.b06AutoRunFlag = BO.b06AutoRunFlagEnum.NeUzivatelskyKrok;
                }
                v.lisTargetB02 = Factory.b02WorkflowStatusBL.GetList(new BO.myQuery("b02") { IsRecordValid=null}).Where(p => p.b01ID == v.RecB02.b01ID && p.pid !=v.Rec.b02ID && p.b02AutoRunFlag !=BO.b02AutoRunFlagEnum.Technicky);

                var lisX67 = Factory.x67EntityRoleBL.GetList(new BO.myQuery("x67"));
                switch (v.RecB02.b01Entity)
                {
                    case "p56":
                        lisX67 = lisX67.Where(p => p.x67Entity == "p56" || p.x67Entity == "p41");
                        break;
                    case "o23":
                        lisX67 = lisX67.Where(p => p.x67Entity == "o23" || p.x67Entity == "p28" || p.x67Entity == "p41");
                        break;
                    case "p91":
                        lisX67 = lisX67.Where(p => p.x67Entity == "p91" || p.x67Entity == "p41");
                        break;
                    default:                    
                        lisX67 = lisX67.Where(p => p.x67Entity == v.RecB02.b01Entity);
                        break;

                }
                v.Receiver_AllX67 = lisX67.ToList();
                v.lisAllX67_Nominee = lisX67.Where(p => p.x67Entity == v.RecB02.b01Entity).ToList();
                
                if (v.lisB11 == null)
                {
                    v.lisB11 = new List<b11RepeaterItem>();
                }


                v.lisP60 = Factory.p60TaskTemplateBL.GetList(new BO.myQuery("p60"));

                if (v.RecB02.b01Entity == "p91")
                {
                    v.lisP83 = Factory.p83UpominkaTypeBL.GetList(new BO.myQuery("p83"));
                }
            }
            

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(b06Record v,string guid)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper == "add_b11")
                {                  
                    v.lisB11.Add(new b11RepeaterItem());
                    return View(v);
                }
                if (v.PostbackOper == "delete_b11")
                {
                    v.lisB11.First(p => p.TempGuid == guid).IsTempDeleted = true;
                    return View(v);
                }
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.b06WorkflowStep c = new BO.b06WorkflowStep();
                if (v.rec_pid > 0) c = Factory.b06WorkflowStepBL.Load(v.rec_pid);

                c.b02ID = v.Rec.b02ID;
                c.b02ID_Target = v.Rec.b02ID_Target;
                c.p60ID = v.Rec.p60ID;
                c.b06AutoTaskFlag = v.Rec.b06AutoTaskFlag;
                c.b06Name = v.Rec.b06Name;
                c.b06Ordinary = v.Rec.b06Ordinary;
                c.b06AutoRunFlag = v.Rec.b06AutoRunFlag;
                c.b06NotepadFlag = v.Rec.b06NotepadFlag;
                c.b06GeoFlag = v.Rec.b06GeoFlag;
                c.b06NextRunAfterHours = v.Rec.b06NextRunAfterHours;
                c.b06CacheValueSql = v.Rec.b06CacheValueSql;

                c.x67ID_Nominee = v.Rec.x67ID_Nominee;
                c.b06NomineeFlag = v.Rec.b06NomineeFlag;
                
                c.x67ID_Direct = v.Rec.x67ID_Direct;
                c.j11ID_Direct = v.Rec.j11ID_Direct;
                c.b02ID_LastReceiver_ReturnTo = v.Rec.b02ID_LastReceiver_ReturnTo;

                c.b06RunSQL = v.Rec.b06RunSQL;
                c.b06ValidateBeforeRunSQL = v.Rec.b06ValidateBeforeRunSQL;
                c.b06ValidateBeforeErrorMessage = v.Rec.b06ValidateBeforeErrorMessage;

                c.b06ValidateAutoMoveSQL = v.Rec.b06ValidateAutoMoveSQL;
                c.b06FrameworkSql = v.Rec.b06FrameworkSql;
                c.p83ID = v.Rec.p83ID;
                c.b06JobFlag = v.Rec.b06JobFlag;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
                //c.b06IsNotifyMerge = v.Rec.b06IsNotifyMerge;
                //c.b06NotifyMergeTime = v.Rec.b06NotifyMergeTime;


                c.pid = Factory.b06WorkflowStepBL.Save(c, Prepare2Save_b08(v), Prepare2Save_b11(v));
                if (c.pid > 0)
                {

                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }

        private List<BO.b11WorkflowMessageToStep> Prepare2Save_b11(b06Record v)
        {
            var ret = new List<BO.b11WorkflowMessageToStep>();
            foreach (var c in v.lisB11.Where(p => p.IsTempDeleted==false))
            {
                var cc = new BO.b11WorkflowMessageToStep() { j04ID = c.j04ID, x67ID = c.x67ID, j11ID = c.j11ID,j61ID=c.j61ID,b11Subject=c.b11Subject };
                if (c.RecordUserRole == 1) cc.b11IsRecordOwner = true;
                if (c.RecordUserRole == 2) cc.b11IsRecordCreator = true;
                if (c.RecordUserRole == 3) cc.b11IsRecordCreatorByEmail = true;
                ret.Add(cc);
            }

            return ret;
        }
        private List<BO.b08WorkflowReceiverToStep> Prepare2Save_b08(b06Record v)
        {
            var lisB08 = new List<BO.b08WorkflowReceiverToStep>();
            if (v.Receiver_j04IDs != null)
            {
                foreach (int j04id in BO.Code.Bas.ConvertString2ListInt(v.Receiver_j04IDs))
                {
                    lisB08.Add(new BO.b08WorkflowReceiverToStep() { j04ID = j04id });
                }
            }
            if (v.Receiver_j11IDs != null)
            {
                foreach (int j11id in BO.Code.Bas.ConvertString2ListInt(v.Receiver_j11IDs))
                {
                    lisB08.Add(new BO.b08WorkflowReceiverToStep() { j11ID = j11id });
                }
            }
            if (v.Receiver_x67IDs != null && v.Receiver_x67IDs.Count()>0)
            {
                foreach (int x67id in v.Receiver_x67IDs.Where(p=>p>0))
                {
                   
                    lisB08.Add(new BO.b08WorkflowReceiverToStep() { x67ID = x67id });
                }
            }

            if (v.Receiver_IsRecordOwner)
            {
                lisB08.Add(new BO.b08WorkflowReceiverToStep() { b08IsRecordOwner = true });
            }
            if (v.Receiver_IsRecordCreator)
            {
                lisB08.Add(new BO.b08WorkflowReceiverToStep() { b08IsRecordCreator = true });
            }

            return lisB08;
        }
    }
}
