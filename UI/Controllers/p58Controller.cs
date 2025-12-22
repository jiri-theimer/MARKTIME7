using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class p58Controller : BaseController
    {
        public IActionResult Info(int pid)
        {
            return View(new BaseTab1ViewModel() { pid = pid, prefix = "p58" });
        }
        public IActionResult Tab1(int pid, string caller)
        {
            var v = new p58Tab1() { Factory = this.Factory, prefix = "p58", pid = pid, caller = caller };

            RefreshStateTab1(v);
            return View(v);
        }
        private void RefreshStateTab1(p58Tab1 v)
        {
            v.Rec = Factory.p58TaskRecurrenceBL.Load(v.pid);
            if (v.Rec != null)
            {
                v.lisP59 = Factory.p58TaskRecurrenceBL.GetList_p59(v.Rec.pid);

                if (v.Rec.p41ID > 0)
                {
                    v.RecP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID);
                }
            }

            
        }

        public BO.Result TryGenerate_Recurrence_Instance(int p58id,int p59id)
        {
            var ret = new BO.Result(false);

            int intP56ID=Factory.p58TaskRecurrenceBL.Generate_Recurrence_Instance(p58id, p59id);
            if (intP56ID == 0)
            {                
                return new BO.Result(true, Factory.GetFirstNotifyMessage());
            }
            return new BO.Result(false);
        }


        public IActionResult Record(int pid, bool isclone, int p41id, string wrk_record_prefix, int wrk_record_pid)
        {
            
            var v = new p58Record() { rec_pid = pid, rec_entity = "p58", b05RecordEntity = wrk_record_prefix, b05RecordPid = wrk_record_pid, Notepad = new Models.Notepad.EditorViewModel() { Prefix = "p58" } };
            v.Element2Focus = "Rec_p58Name";
            DateTime d0 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
            v.Rec = new BO.p58TaskRecurrence() { p58Name = (p41id > 0 ? "Měsíční přiznání DPH [YYYY]/[MM]" : "Opakovaný úkol [YYYY]/[MM]"),p41ID = p41id, p58BaseDateStart =d0, p58RecurrenceType=BO.Code.RecurrenceTypeENUM.Month };
            
            v.Rec.p58BaseDateEnd = new DateTime(d0.Year+1,12,31);v.Rec.p58IsPlanUntil = true;
            v.Rec.p58Generate_DaysToBase_D = -1;
            var lisP57 = Factory.p57TaskTypeBL.GetList(new BO.myQuery("p57"));
            if (lisP57.Count() == 0)
            {
                return this.StopPage(true, "V administraci chybí naplnit číselník [Typy úkolů].");
            }
            
            v.Rec.p57ID = lisP57.First().pid; v.ComboP57 = lisP57.First().p57Name;

            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p58TaskRecurrenceBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                if (!InhalePermissions(v))
                {
                    return this.StopPage(true, "Nemáte oprávnění k editaci karty záznamu.");
                }
                v.RecP57 = Factory.p57TaskTypeBL.Load(v.Rec.p57ID);

               
                v.ComboP57 = v.Rec.p57Name;
                v.ComboOwner = v.Rec.Owner;
                v.b05RecordEntity = v.Rec.b05RecordEntity;
                v.b05RecordPid = v.Rec.b05RecordPid;

                if (v.Rec.p41ID > 0)
                {
                    v.RecP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID);
                    
                }
                if (!string.IsNullOrEmpty(v.Rec.p58Notepad) || v.Rec.p58Plan_Hours > 0 || v.Rec.p58Plan_Expenses > 0)
                {
                    v.IsShowMore = true;
                }

                InhaleNotepad(v, v.Rec.x04ID, v.Rec.p58Notepad);


            }
            else
            {
                v.ComboOwner = Factory.CurrentUser.FullnameDesc;
                
            }

            RefreshStateRecord(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();

            }

            return View(v);
        }

        private void RefreshStateRecord(p58Record v)
        {
            if (v.Rec.p57ID > 0 && v.RecP57 == null)
            {
                v.RecP57 = Factory.p57TaskTypeBL.Load(v.Rec.p57ID);
                v.ComboP57 = v.RecP57.p57Name;
            }

            if (v.ProjectCombo == null)
            {
                v.ProjectCombo = new ProjectComboViewModel() { SelectedP41ID = v.Rec.p41ID };
                
            }

            if (v.Rec.p41ID > 0 && v.RecP41==null)
            {
                v.RecP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID);
            }

            InhaleNotepad(v);

            InhaleRoles(v);


            if (v.rec_pid == 0 && v.Rec.j02ID_Owner == 0)
            {
                v.Rec.j02ID_Owner = Factory.CurrentUser.pid;
                v.ComboOwner = Factory.CurrentUser.FullnameDesc;
            }

            if (v.reminder == null)
            {
                v.reminder = new ReminderViewModel() { record_pid = v.rec_pid, record_prefix = "p58" };
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p58Record v)
        {
            RefreshStateRecord(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "more")
                {
                    v.IsShowMore = !v.IsShowMore;

                }
                if (v.PostbackOper == "p57id")
                {
                    if (v.Rec.p57ID > 0)
                    {
                        v.RecP57 = Factory.p57TaskTypeBL.Load(v.Rec.p57ID);
                        InhaleRoles(v);
                        InhaleNotepad(v);
                    }


                }


                return View(v);
            }


            if (ModelState.IsValid)
            {
                BO.p58TaskRecurrence c = new BO.p58TaskRecurrence();
                if (v.rec_pid > 0) c = Factory.p58TaskRecurrenceBL.Load(v.rec_pid);
                c.p57ID = v.Rec.p57ID;

                c.p58Name = v.Rec.p58Name;
                c.p58RecurrenceType = v.Rec.p58RecurrenceType;
                c.p58BaseDateStart = v.Rec.p58BaseDateStart;
                c.p58BaseDateEnd = v.Rec.p58BaseDateEnd;
                c.p58Generate_DaysToBase_D = v.Rec.p58Generate_DaysToBase_D;
                c.p58Generate_DaysToBase_M = v.Rec.p58Generate_DaysToBase_M;

                c.p58IsPlanFrom = v.Rec.p58IsPlanFrom;
                c.p58PlanUntil_D2Base = v.Rec.p58PlanUntil_D2Base;
                c.p58PlanUntil_M2Base = v.Rec.p58PlanUntil_M2Base;

                c.p58IsPlanUntil = v.Rec.p58IsPlanUntil;
                c.p58PlanFrom_D2Base = v.Rec.p58PlanFrom_D2Base;
                c.p58PlanFrom_M2Base = v.Rec.p58PlanFrom_M2Base;


                c.p41ID = v.ProjectCombo.SelectedP41ID;

                if (v.IsShowMore || v.rec_pid == 0)
                {                                        
                    c.p58PlanFlag = v.Rec.p58PlanFlag;
                    c.p58Plan_Hours = v.Rec.p58Plan_Hours;
                    c.p58Plan_Expenses = v.Rec.p58Plan_Expenses;
                    c.p58Plan_Revenue = v.Rec.p58Plan_Revenue;
                    c.p58Notepad = v.Notepad.HtmlContent;
                    c.x04ID = v.Notepad.SelectedX04ID;
                    c.p58IsStopNotify = v.Rec.p58IsStopNotify;
                }


                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p58TaskRecurrenceBL.Save(c,v.roles.getList4Save(Factory));
                if (c.pid > 0)
                {
                    if (v.IsShowMore)
                    {
                        
                        Factory.o27AttachmentBL.CommitNotepdChanges(v.Notepad.TempGuid, "p56", c.pid);
                    }

                    if (v.rec_pid == 0 && v.b05RecordEntity != null && v.b05RecordPid > 0)
                    {
                        var recB05 = new BO.b05Workflow_History() { p58ID = c.pid, b05IsManualStep = true, b05RecordEntity = v.b05RecordEntity, b05RecordPid = v.b05RecordPid };
                        Factory.WorkflowBL.Save2History(recB05);
                    }

                    v.reminder.SaveChanges(Factory, c.pid, c.p58BaseDateStart);



                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }

        private void InhaleNotepad(p58Record v, int x04id = 0, string htmlcontent = null)
        {

            if (v.Notepad == null)
            {
                v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "p58", SelectedX04ID = Factory.Lic.x04ID_Default };
            }
            if (!string.IsNullOrEmpty(htmlcontent))
            {
                v.Notepad.HtmlContent = htmlcontent;
            }
            if (x04id > 0)
            {
                v.Notepad.SelectedX04ID = x04id;
            }


        }

        private void InhaleRoles(p58Record v)
        {
            if (v.roles == null)
            {
                v.roles = new RoleAssignViewModel() { RecPid = v.rec_pid, RecPrefix = "p58", RolePrefix = "p56", Header = "." };
                v.roles.Default_j02ID = Factory.CurrentUser.pid; v.roles.Default_Person = Factory.CurrentUser.FullnameDesc;
                
            }

        }

        private bool InhalePermissions(p58Record v)
        {            
            if (Factory.CurrentUser.IsAdmin || Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_p41_Creator))
            {
                return true;
            }

            if (v.rec_pid>0 && v.Rec != null)
            {
                if (v.Rec.j02ID_Owner == Factory.CurrentUser.pid) return true;
            }
            
           
            return false;
        }
    }
}
