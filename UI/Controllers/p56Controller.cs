using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class p56Controller : BaseController
    {
        public IActionResult NewBianco()
        {
            return View(new BaseViewModel());
        }
        public IActionResult Info(int pid)
        {
            return Tab1(pid, "Info");
        }
        public IActionResult Dashboard(int pid)
        {
            return Tab1(pid, "Dashboard");
        }

        public IActionResult Tab1(int pid, string caller)
        {
            var v = new p56Tab1() { Factory = this.Factory, prefix = "p56", pid = pid, caller = caller };

            RefreshStateTab1(v);
            return View(v);
        }

        private void RefreshStateTab1(p56Tab1 v)
        {
            v.Rec = Factory.p56TaskBL.Load(v.pid);
            if (v.Rec != null)
            {
                v.RecSum = Factory.p56TaskBL.LoadSumRow(v.Rec.pid);

                v.SetTagging();

                v.SetFreeFields(0);
            }
        }


        public IActionResult Record(int pid, bool isclone, int p41id, string wrk_record_prefix, int wrk_record_pid, string d, int j02id, int p57id, int p60id)
        {
            if (BO.Code.Entity.GetPrefixDb(wrk_record_prefix) == "p41" && p41id == 0) p41id = wrk_record_pid;

            var v = new p56Record() { rec_pid = pid, rec_entity = "p56", j02id_create_taskfor = j02id, b05RecordEntity = wrk_record_prefix, b05RecordPid = wrk_record_pid, UploadGuid = BO.Code.Bas.GetGuid(), Notepad = new Models.Notepad.EditorViewModel() { Prefix = "p56" } };

            v.Element2Focus = "Rec_p56Name";
            if (v.j02id_create_taskfor == 0) v.j02id_create_taskfor = Factory.CurrentUser.pid;

            v.Rec = new BO.p56Task() { p41ID = p41id, p56PlanUntil = DateTime.Today.AddDays(1).AddHours(12), p57ID = p57id, x04ID = Factory.Lic.x04ID_Default };
            
            if (p60id > 0)
            {
                InhaleTemplateRecord(v, p60id);
            }
            if (v.Rec.p57ID > 0)
            {
                v.ComboP57 = Factory.p57TaskTypeBL.Load(v.Rec.p57ID).p57Name;
            }
            if (!string.IsNullOrEmpty(d))
            {

                v.Rec.p56PlanUntil = BO.Code.Bas.ConvertDateTo235959(BO.Code.Bas.String2Date(d));
            }
            var lisP57 = Factory.p57TaskTypeBL.GetList(new BO.myQuery("p57"));
            if (lisP57.Count() > 1)
            {
                v.IsShowP57Combo = true;
            }
            if (v.Rec.p57ID == 0)
            {
                if (lisP57.Count() == 0)
                {
                    return this.StopPage(true, "V administraci chybí naplnit číselník [Typy úkolů].");
                }

                v.Rec.p57ID = lisP57.First().pid; v.ComboP57 = lisP57.First().p57Name;

            }


            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p56TaskBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                if (!InhalePermissions(v))
                {
                    return this.StopPage(true, "Nemáte oprávnění k editaci karty záznamu.");
                }
                v.RecP57 = Factory.p57TaskTypeBL.Load(v.Rec.p57ID);
                v.b05RecordEntity = v.Rec.b05RecordEntity;
                v.b05RecordPid = v.Rec.b05RecordPid;
                

                v.SetTagging(Factory.o51TagBL.GetTagging("p56", v.rec_pid));

                v.ComboP57 = v.Rec.p57Name;
                v.ComboOwner = v.Rec.Owner;

                if (v.Rec.p15ID > 0)
                {
                    v.ComboP15 = Factory.p15LocationBL.Load(v.Rec.p15ID).p15Name;
                }

                InhaleNotepad(v, v.Rec.x04ID, v.Rec.p56Notepad);

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
                if (pid > 0 && Factory.p65ChecklistBL.IsAvailable())
                {
                    //kopie úkolu převezme položky check-listu, neodškrtnuté
                    v.ChecklistPending = System.Text.Json.JsonSerializer.Serialize(Factory.p65ChecklistBL.GetList(pid).Select(p => p.p65Name).ToList());
                }

            }

            return View(v);
        }

        private void RefreshStateRecord(p56Record v)
        {
            if (v.Rec.p57ID > 0 && v.RecP57 == null)
            {
                v.RecP57 = Factory.p57TaskTypeBL.Load(v.Rec.p57ID);
                v.ComboP57 = v.RecP57.p57Name;
            }

            if (v.reminder == null)
            {
                v.reminder = new ReminderViewModel() { record_pid = v.rec_pid, record_prefix = "p56" };
            }

            if (v.ProjectCombo == null)
            {
                v.ProjectCombo = new ProjectComboViewModel() { SelectedP41ID = v.Rec.p41ID };
            }
            v.ProjectCombo.MyQueryInline = $"j02id_for_p56_o22_entry|int|{this.Factory.CurrentUser.pid}";   //omezit projekty na povolené podle projektové role

            InhaleNotepad(v, v.Rec.x04ID);

            InhaleRoles(v);


            if (v.rec_pid == 0 && v.Rec.j02ID_Owner == 0)
            {
                v.Rec.j02ID_Owner = Factory.CurrentUser.pid;
                v.ComboOwner = Factory.CurrentUser.FullnameDesc;
            }

            if (v.ff1 == null)
            {
                v.ff1 = new FreeFieldsViewModel();
                v.ff1.InhaleFreeFieldsView(Factory, v.rec_pid, "p56");
            }
            v.ff1.RefreshInputsVisibility(Factory, v.rec_pid, "p56", v.Rec.p57ID);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p56Record v)
        {
            RefreshStateRecord(v);

            if (v.IsPostback)
            {

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
                BO.p56Task c = new BO.p56Task();
                if (v.rec_pid > 0) c = Factory.p56TaskBL.Load(v.rec_pid);
                c.p57ID = v.Rec.p57ID;

                c.p56Name = v.Rec.p56Name;
                c.p56PlanUntil = v.Rec.p56PlanUntil;
                if (v.RecP57 != null && v.RecP57.p57PlanScope == BO.p57PlanScopeEnum.NenabizetPlanAniTermin)
                {
                    c.p56PlanUntil = null;  //úkol bez termínu
                }
                

                if (v.RecP57 != null && v.RecP57.p57ProjectFlag != BO.p57ProjectFlagEnum.ProjectHidden)
                {
                    c.p41ID = v.ProjectCombo.SelectedP41ID;
                }

                c.p56PlanFrom = v.Rec.p56PlanFrom;
                c.p56PlanFlag = v.Rec.p56PlanFlag;
                c.p56Plan_Hours = v.Rec.p56Plan_Hours;
                c.p56Plan_Expenses = v.Rec.p56Plan_Expenses;
                c.p56Plan_Revenue = v.Rec.p56Plan_Revenue;
                c.p56Plan_Internal_Fee = v.Rec.p56Plan_Internal_Fee;
                c.p56Notepad = v.Notepad.HtmlContent;
                c.x04ID = v.Notepad.SelectedX04ID;
                c.p56IsStopNotify = v.Rec.p56IsStopNotify;
                c.p15ID = v.Rec.p15ID;
                c.p56Ordinary = v.Rec.p56Ordinary;
                c.p60ID = v.Rec.p60ID;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                v.roles.RecPid = c.pid;
                v.roles.RecPrefix = "p56";
                c.pid = Factory.p56TaskBL.Save(c, v.ff1.inputs, v.roles.getList4Save(Factory));
                if (c.pid > 0)
                {
                    Factory.o51TagBL.SaveTagging("p56", c.pid, v.TagPids);
                    Factory.o27AttachmentBL.CommitNotepdChanges(v.Notepad.TempGuid, "p56", c.pid);

                    if (v.rec_pid == 0 && v.b05RecordEntity != null && v.b05RecordPid > 0)
                    {
                        var recB05 = new BO.b05Workflow_History() { p56ID = c.pid, b05IsManualStep = true, b05RecordEntity = v.b05RecordEntity, b05RecordPid = v.b05RecordPid };
                        Factory.WorkflowBL.Save2History(recB05);
                    }
                    v.reminder.SaveChanges(Factory, c.pid, c.p56PlanUntil);

                   
                    if (v.UploadGuid != null)
                    {
                        Factory.o27AttachmentBL.SaveDropzoneFromTemp(v.UploadGuid, "p56", c.pid);
                    }

                    if (v.rec_pid == 0 && !string.IsNullOrEmpty(v.ChecklistPending) && Factory.p65ChecklistBL.IsAvailable())
                    {
                        //check-list zadaný u nového úkolu (u uloženého úkolu se položky ukládají průběžně samy)
                        try
                        {
                            var names = System.Text.Json.JsonSerializer.Deserialize<List<string>>(v.ChecklistPending);
                            if (names != null && names.Count > 0) Factory.p65ChecklistBL.Add(c.pid, names);
                        }
                        catch (System.Text.Json.JsonException)
                        {
                            //nečitelný vstup: úkol je uložený, check-list zůstane prázdný
                        }
                    }

                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }

        private void InhaleNotepad(p56Record v, int x04id = 0, string htmlcontent = null)
        {
            if (v.Notepad == null)
            {
                v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "p56", SelectedX04ID = Factory.Lic.x04ID_Default };
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

        private void InhaleRoles(p56Record v)
        {
            if (v.roles == null)
            {
                v.roles = new RoleAssignViewModel() { RecPid = v.rec_pid, RecPrefix = "p56", RolePrefix = "p56", Header = "." };
                v.roles.Default_j02ID = v.j02id_create_taskfor; v.roles.Default_Person = Factory.j02UserBL.Load(v.j02id_create_taskfor).FullnameDesc;

            }

        }

        private bool InhalePermissions(p56Record v)
        {
            var perm = Factory.p56TaskBL.InhaleRecDisposition(v.Rec.pid, v.Rec);
            if (v.Rec.x38ID > 0)
            {
                v.CanEditRecordCode = Factory.x38CodeLogicBL.CanEditRecordCode(v.Rec.x38ID, perm);
            }
            else
            {
                v.CanEditRecordCode = perm.OwnerAccess;
            }
            if (!perm.OwnerAccess)
            {
                return false;
            }

            return true;
        }
        private void InhaleTemplateRecord(p56Record v, int p60id)
        {
            v.Rec.p60ID = p60id;
            var recP60 = Factory.p60TaskTemplateBL.Load(p60id);
            v.Rec.p56Name = recP60.p60Name;
            v.Rec.p57ID = recP60.p57ID; v.Rec.p56Plan_Hours = recP60.p60Plan_Hours; v.Rec.p56Plan_Expenses = recP60.p60Plan_Expenses;v.Rec.p56Plan_Revenue = recP60.p60Plan_Revenue;
            v.Notepad.HtmlContent = recP60.p60Notepad;
            if (recP60.p60PlanFrom_Unit != null)
            {
                v.Rec.p56PlanFrom = recP60.p60PlanFrom_UC == 0 ? DateTime.Today : Factory.o22MilestoneBL.GetKonecLhuty(DateTime.Today, recP60.p60PlanFrom_UC, recP60.p60PlanFrom_Unit);
                                
            }
            if (recP60.p60PlanUntil_Unit != null)
            {
                v.Rec.p56PlanUntil = recP60.p60PlanUntil_UC == 0 ? DateTime.Today : Factory.o22MilestoneBL.GetKonecLhuty(DateTime.Today, recP60.p60PlanUntil_UC, recP60.p60PlanUntil_Unit);
            }
            if (recP60.p41ID > 0)
            {
                v.ProjectCombo = new ProjectComboViewModel() { SelectedP41ID = recP60.p41ID, SelectedProject = recP60.ProjectWithClient };                          
            }

            v.roles = new RoleAssignViewModel() { RecPid =p60id, RecPrefix = "p60", RolePrefix = "p56", Header = "." };
           
            //v.roles.Default_j02ID = v.j02id_create_taskfor; v.roles.Default_Person = Factory.j02UserBL.Load(v.j02id_create_taskfor).FullnameDesc;
        }
        

    }
}
