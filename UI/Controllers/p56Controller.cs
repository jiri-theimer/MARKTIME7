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
        public int RenameToDoList(int p55id, string newname)
        {
            return Factory.p56TaskBL.RenameToDoList(p55id, newname);

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


        public IActionResult TodoList(string wrk_record_prefix, int wrk_record_pid, int p55id)
        {
            var v = new p56TodoListViewModel() { RecordEntity = wrk_record_prefix, RecordPid = wrk_record_pid, Element2Focus = "p55Name" };
            v.ProjectCombo = new ProjectComboViewModel();
            if ((v.RecordEntity == "p41" || v.RecordEntity == "le5") && v.RecordPid > 0)
            {
                v.ProjectCombo.SelectedP41ID = v.RecordPid;
                
            }
            v.ProjectCombo.MyQueryInline = $"j02id_for_p56_o22_entry|int|{this.Factory.CurrentUser.pid}";   //omezit projekty na povolené podle projektové role

            if (p55id > 0)
            {
                v.SelectedP55ID = p55id;
                CloneToDoList(v);
            }

            RefreshStateTodoList(v);

            return View(v);
        }
        private void RefreshStateTodoList(p56TodoListViewModel v)
        {
            v.lisP57 = Factory.p57TaskTypeBL.GetList(new BO.myQuery("p57"));
            if (v.SelectedP57ID == 0 && v.lisP57.Count() > 0)
            {
                v.SelectedP57ID = v.lisP57.First().pid;
            }
            v.lisX67 = Factory.x67EntityRoleBL.GetList(new BO.myQuery("x67")).Where(p => p.x67Entity == "p56");
            if (v.SelectedX67ID == 0 && v.lisX67.Count() > 0)
            {
                v.SelectedX67ID = v.lisX67.First().pid;
            }
            if (v.lisP56 == null)
            {
                v.lisP56 = new List<p56Repeater>();
                //v.lisP56.Add(new p56Repeater() { TempGuid=BO.Code.Bas.GetGuid(), p56PlanUntil = DateTime.Today.AddDays(1) });
                //v.Element2Focus = "lisP56_0__p56Name";
            }


        }

        private void Parse_Todolist_Template(p56TodoListViewModel v)
        {
            v.lisP56.Clear();
            var arr = BO.Code.Bas.ConvertString2List(v.TodoListTemplate, System.Environment.NewLine);
            p56Repeater rec = null;
            var lisJ02 = Factory.j02UserBL.GetList(new BO.myQueryJ02());
            var lisJ11 = Factory.j11TeamBL.GetList(new BO.myQueryJ11());
            var j02ids = new List<int>();
            var j11ids = new List<int>();
            foreach (string s in arr.Where(p => p.Trim().Length > 0))
            {
                if (s.StartsWith("==") && rec != null)
                {
                    var lis = BO.Code.Bas.ConvertString2List(s.Replace("==", "").Replace(";", ","), ",");
                    foreach (var ss in lis)
                    {
                        var qry1 = lisJ02.Where(p => p.FullnameDesc.ToLower().Contains(ss.ToLower()));
                        if (qry1.Count() > 0)
                        {

                            j02ids.Add(qry1.First().pid);
                        }
                        else
                        {
                            var qry2 = lisJ11.Where(p => p.j11Name.ToLower().Contains(ss.ToLower()));
                            if (qry2.Count() > 0)
                            {
                                j11ids.Add(qry2.First().pid);
                            }

                        }
                    }
                }
                else
                {
                    if (s.StartsWith("**"))
                    {
                        v.p55Name = s.Replace("**", "");
                    }
                    else
                    {
                        if (rec != null)
                        {
                            if (j02ids.Count() > 0)
                            {
                                var qry1 = Factory.j02UserBL.GetList(new BO.myQueryJ02() { pids = j02ids });
                                rec.Assign_j02IDs = string.Join(",", qry1.Select(p => p.pid));
                                rec.Assign_Persons = string.Join(",", qry1.Select(p => p.FullnameDesc));
                            }
                            if (j11ids.Count() > 0)
                            {
                                var qry1 = Factory.j11TeamBL.GetList(new BO.myQueryJ11() { pids = j11ids });
                                rec.Assign_j11IDs = string.Join(",", qry1.Select(p => p.pid));
                                rec.Assign_j11Names = string.Join(",", qry1.Select(p => p.j11Name));
                            }
                            v.lisP56.Add(rec);
                        }
                        rec = new p56Repeater() { TempGuid = BO.Code.Bas.GetGuid(), p56Name = s.Trim() };
                        j02ids = new List<int>();
                        j11ids = new List<int>();
                    }

                }

            }

            if (rec != null && (j02ids.Count() > 0 || j11ids.Count() > 0))
            {
                if (j02ids.Count() > 0)
                {
                    var qry1 = Factory.j02UserBL.GetList(new BO.myQueryJ02() { pids = j02ids });
                    rec.Assign_j02IDs = string.Join(",", qry1.Select(p => p.pid));
                    rec.Assign_Persons = string.Join(",", qry1.Select(p => p.FullnameDesc));
                }
                if (j11ids.Count() > 0)
                {
                    var qry1 = Factory.j11TeamBL.GetList(new BO.myQueryJ11() { pids = j11ids });
                    rec.Assign_j11IDs = string.Join(",", qry1.Select(p => p.pid));
                    rec.Assign_j11Names = string.Join(",", qry1.Select(p => p.j11Name));
                }
                v.lisP56.Add(rec);
            }



        }




        [HttpPost]
        public IActionResult TodoList(p56TodoListViewModel v, string guid)
        {
            RefreshStateTodoList(v);
            if (v.IsPostback)
            {
                switch (v.PostbackOper)
                {
                    case "handle_template":
                        Parse_Todolist_Template(v);
                        return View(v);
                    case "add_row":
                        var c = new p56Repeater() { TempGuid = BO.Code.Bas.GetGuid(), p56PlanUntil = DateTime.Today.AddDays(1) };

                        v.lisP56.Add(c);
                        v.Element2Focus = $"lisP56_{v.lisP56.Count() - 1}__p56Name";
                        return View(v);
                    case "clone_row":
                        var row = v.lisP56.Where(p => p.TempGuid == guid).First();
                        var d = new p56Repeater()
                        {
                            TempGuid = BO.Code.Bas.GetGuid(),
                            p56Name = row.p56Name,
                            Assign_j02IDs = row.Assign_j02IDs
                            ,
                            Assign_Persons = row.Assign_Persons,
                            Assign_j11IDs = row.Assign_j11IDs,
                            Assign_j11Names = row.Assign_j11Names
                            ,
                            p56PlanUntil = row.p56PlanUntil
                        };
                        v.lisP56.Add(d);
                        v.Element2Focus = $"lisP56_{v.lisP56.Count() - 1}__p56Name";
                        return View(v);
                    case "delete_row":
                        v.lisP56.First(p => p.TempGuid == guid).IsTempDeleted = true;
                        return View(v);
                    case "clear":
                        v.lisP56.Clear();
                        return View(v);
                    case "month":
                    case "year":
                        foreach (var cc in v.lisP56)
                        {
                            if (cc.p56PlanUntil == null) cc.p56PlanUntil = DateTime.Today;
                            if (v.PostbackOper == "month") cc.p56PlanUntil = cc.p56PlanUntil.Value.AddMonths(1);
                            if (v.PostbackOper == "year") cc.p56PlanUntil = cc.p56PlanUntil.Value.AddYears(1);
                        }
                        return View(v);
                    case "p55id":   //zkopírovat úkoly z uloženého todo-listu
                        if (v.SelectedP55ID > 0)
                        {
                            CloneToDoList(v);


                        }

                        return View(v);

                }

                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (v.SelectedP57ID == 0)
                {
                    this.AddMessage("Chybí Typ úkolu.");
                    return View(v);
                }
                if (string.IsNullOrEmpty(v.p55Name))
                {
                    this.AddMessage("Chybí vyplnit název ToDo-listu.");
                    return View(v);
                }
                int x = 0;
                foreach (var c in v.lisP56.Where(p => p.IsTempDeleted == false))
                {
                    x += 1;
                    if (string.IsNullOrEmpty(c.p56Name))
                    {
                        this.AddMessage("V jednom z řádků chybí vyplnit název úkolu.");
                        return View(v);
                    }
                    if (c.p56PlanUntil == null)
                    {
                        this.AddMessage("V jednom z řádků chybí vyplnit Termín úkolu.");
                        return View(v);
                    }
                }

                var p56ids = new List<int>();
                foreach (var c in v.lisP56.Where(p => p.IsTempDeleted == false))
                {
                    var lisNominee = new List<BO.x69EntityRole_Assign>();
                    foreach (int j02id in BO.Code.Bas.ConvertString2ListInt(c.Assign_j02IDs))
                    {
                        lisNominee.Add(new BO.x69EntityRole_Assign() { j02ID = j02id, x67ID = v.SelectedX67ID });
                    }
                    foreach (int j11id in BO.Code.Bas.ConvertString2ListInt(c.Assign_j11IDs))
                    {
                        lisNominee.Add(new BO.x69EntityRole_Assign() { j11ID = j11id, x67ID = v.SelectedX67ID });
                    }
                    if (lisNominee.Count() == 0)
                    {
                        lisNominee.Add(new BO.x69EntityRole_Assign() { j02ID = Factory.CurrentUser.pid, x67ID = v.SelectedX67ID });
                    }
                    var rec = new BO.p56Task() { p57ID = v.SelectedP57ID, p41ID = v.ProjectCombo.SelectedP41ID, p56Name = c.p56Name, p56PlanUntil = c.p56PlanUntil, p56Ordinary = c.p56Ordinary };

                    int intP56ID = Factory.p56TaskBL.Save(rec, null, lisNominee);
                    if (intP56ID > 0)
                    {
                        p56ids.Add(intP56ID);
                        if (v.RecordEntity != null && v.RecordPid > 0)
                        {
                            var recB05 = new BO.b05Workflow_History() { p56ID = intP56ID, b05IsManualStep = true, b05RecordEntity = v.RecordEntity, b05RecordPid = v.RecordPid };
                            Factory.WorkflowBL.Save2History(recB05);
                        }

                    }


                }

                if (p56ids.Count() > 0 && !string.IsNullOrEmpty(v.p55Name))
                {
                    Factory.p56TaskBL.CreateToDoList(v.p55Name, p56ids);
                    v.SetJavascript_CallOnLoad(v.RecordPid);
                    return View(v);
                }


            }

            this.Notify_RecNotSaved();
            return View(v);

        }


        private void CloneToDoList(p56TodoListViewModel v)
        {
            if (v.lisP56 == null) v.lisP56 = new List<p56Repeater>();

            var lis = Factory.p56TaskBL.GetList(new BO.myQueryP56() { IsRecordValid = null, p55id = v.SelectedP55ID }).OrderBy(p => p.p56Ordinary);
            foreach (var cc in lis)
            {
                var lisX69 = Factory.x67EntityRoleBL.GetList_X69("p56", cc.pid);
                var ri = new p56Repeater()
                {
                    TempGuid = BO.Code.Bas.GetGuid()
                    ,
                    p56Name = cc.p56Name,
                    p56Ordinary = cc.p56Ordinary
                    ,
                    Assign_j02IDs = string.Join(",", lisX69.Where(p => p.j02ID > 0).Select(p => p.j02ID))
                    ,
                    Assign_Persons = string.Join(",", lisX69.Where(p => p.j02ID > 0).Select(p => p.Person))
                    ,
                    Assign_j11IDs = string.Join(",", lisX69.Where(p => p.j11ID > 0).Select(p => p.j11ID))
                    ,
                    Assign_j11Names = string.Join(",", lisX69.Where(p => p.j11ID > 0).Select(p => p.j11Name))
                    ,
                    p56PlanUntil = cc.p56PlanUntil
                };

                v.lisP56.Add(ri);
            }
        }



        public IActionResult Record(int pid, bool isclone, int p41id, string wrk_record_prefix, int wrk_record_pid, string d, int j02id, int p57id, int o43id_source, int p60id)
        {
            if (BO.Code.Entity.GetPrefixDb(wrk_record_prefix) == "p41" && p41id == 0) p41id = wrk_record_pid;

            var v = new p56Record() { rec_pid = pid, rec_entity = "p56", j02id_create_taskfor = j02id, b05RecordEntity = wrk_record_prefix, b05RecordPid = wrk_record_pid, UploadGuid = BO.Code.Bas.GetGuid(), o43id_source = o43id_source, Notepad = new Models.Notepad.EditorViewModel() { Prefix = "p56" } };

            v.Element2Focus = "Rec_p56Name";
            if (v.j02id_create_taskfor == 0) v.j02id_create_taskfor = Factory.CurrentUser.pid;

            v.Rec = new BO.p56Task() { p41ID = p41id, p56PlanUntil = DateTime.Today.AddDays(1).AddHours(12), p57ID = p57id, x04ID = Factory.Lic.x04ID_Default };
            if (v.rec_pid == 0 && v.o43id_source > 0)
            {
                InhaleInboxRecord(v);
            }
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

                v.ComboP55 = v.Rec.p55Name;
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
                c.p55ID = v.Rec.p55ID;
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

                    if (v.o43id_source > 0)
                    {
                        var recO43 = Factory.o43InboxBL.Load(v.o43id_source);
                        recO43.p56ID = c.pid;
                        Factory.o43InboxBL.Save(recO43);
                    }

                    if (v.UploadGuid != null)
                    {
                        Factory.o27AttachmentBL.SaveChangesAndUpload(v.UploadGuid, "p56", c.pid);
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
        private void InhaleInboxRecord(p56Record v)
        {
            var recO43 = Factory.o43InboxBL.Load(v.o43id_source);
            v.Rec.p56Name = recO43.o43Subject;

            if (recO43.o43IsBodyHtml)
            {
                InhaleNotepad(v, Factory.Lic.x04ID_Default, recO43.o43BodyHtml);
            }
            else
            {
                InhaleNotepad(v, Factory.Lic.x04ID_Default, recO43.o43BodyText);
            }
            if (recO43.p41ID > 0)
            {
                v.Rec.p41ID = recO43.p41ID;
            }
            var files = Factory.o43InboxBL.GetInboxFiles(recO43.pid, true).Where(p => p.Key != "eml" && p.Key != "msg");

            foreach (BO.StringPair sp in files)
            {
                System.IO.File.Copy(sp.Value, $"{Factory.TempFolder}\\{sp.Key}", true);
                Factory.o27AttachmentBL.CreateTempInfoxFile(v.UploadGuid, "p56", sp.Key, sp.Key, BO.Code.File.GetContentType(sp.Value));

            }
        }

    }
}
