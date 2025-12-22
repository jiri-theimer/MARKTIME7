using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class o22Controller : BaseController
    {
        public IActionResult Info(int pid)
        {
            return Tab1(pid, "info");
        }

        public IActionResult Tab1(int pid, string caller)
        {
            var v = new o22Tab1() { Factory = this.Factory, prefix = "o22", pid = pid, caller = caller };

            RefreshStateTab1(v);
            return View(v);
        }
        private void RefreshStateTab1(o22Tab1 v)
        {
            v.Rec = Factory.o22MilestoneBL.Load(v.pid);
            if (v.Rec != null)
            {

                v.SetTagging();

                //v.SetFreeFields(0);
            }
        }



        public IActionResult Record(int pid, bool isclone, int p41id, string wrk_record_prefix, int wrk_record_pid,string d,int j02id,int o21id, int o43id_source)
        {            
            var v = new o22Record() { rec_pid = pid, rec_entity = "o22", j02id_create_taskfor = j02id, b05RecordEntity = wrk_record_prefix, b05RecordPid = wrk_record_pid, UploadGuid = BO.Code.Bas.GetGuid(), Notepad = new Models.Notepad.EditorViewModel() { Prefix = "o22" } };
            if (v.j02id_create_taskfor == 0) v.j02id_create_taskfor = Factory.CurrentUser.pid;            

            v.Element2Focus = "Rec_o22Name";
            
            v.Rec = new BO.o22Milestone() {o22DurationCount = 1,o21ID=o21id,p41ID= p41id };
            if (v.rec_pid == 0 && o43id_source > 0)
            {
                var recO43 = Factory.o43InboxBL.Load(o43id_source);
                v.o43id_source = recO43.pid;
                v.Rec.o22Name = recO43.o43Subject;
                v.IsShowMore = true;
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
                    Factory.o27AttachmentBL.CreateTempInfoxFile(v.UploadGuid, "o22", sp.Key, sp.Key, BO.Code.File.GetContentType(sp.Value));

                }
            }
            if (v.Rec.p41ID==0 && v.b05RecordEntity == "p41" && v.b05RecordPid > 0)
            {
                v.Rec.p41ID = v.b05RecordPid;
            }
            if (v.Rec.o21ID > 0)
            {
                v.ComboO21 = Factory.o21MilestoneTypeBL.Load(v.Rec.o21ID).o21Name;
            }
            v.Rec.o22DurationCalcFlag = BO.o22DurationCalcFlagEnum.Pocitat;
            if (d == null)
            {
                v.Rec.o22PlanFrom = DateTime.Today;
                v.Rec.o22PlanUntil = BO.Code.Bas.ConvertDateTo235959(DateTime.Today.AddDays(1));
            }
            else
            {
                v.Rec.o22PlanFrom = BO.Code.Bas.String2Date(d);
                v.Rec.o22PlanUntil = BO.Code.Bas.ConvertDateTo235959(BO.Code.Bas.String2Date(d).AddDays(1));
            }

            if (v.Rec.o21ID == 0)
            {
                var lisO21 = Factory.o21MilestoneTypeBL.GetList(new BO.myQuery("o21"));
                if (lisO21.Count() == 0)
                {
                    return this.StopPage(true, "V administraci chybí naplnit číselník [Typy termínů].");
                }

                v.Rec.o21ID = lisO21.First().pid; v.ComboO21 = lisO21.First().o21Name;

            }

            if (v.rec_pid > 0)
            {
                v.Rec = Factory.o22MilestoneBL.Load(v.rec_pid);
                
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                if (!InhalePermissions(v))
                {
                    return this.StopPage(true, "Nemáte oprávnění k editaci karty záznamu.");
                }
                v.RecO21 = Factory.o21MilestoneTypeBL.Load(v.Rec.o21ID);
                v.b05RecordEntity = v.Rec.b05RecordEntity;
                v.b05RecordPid = v.Rec.b05RecordPid;

                v.SetTagging(Factory.o51TagBL.GetTagging("o22", v.rec_pid));

                v.ComboO21 = v.Rec.o21Name;
                v.ComboOwner = v.Rec.Owner;

                v.Rec.p41ID = v.Rec.p41ID;

                if (!string.IsNullOrEmpty(v.Rec.o22Notepad) || !string.IsNullOrEmpty(v.TagPids))
                {
                    v.IsShowMore = true;
                }

                InhaleNotepad(v, v.Rec.x04ID, v.Rec.o22Notepad);
                

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

        private void RefreshStateRecord(o22Record v)
        {
            if (v.Rec.o21ID > 0 && v.RecO21 == null)
            {
                v.RecO21 = Factory.o21MilestoneTypeBL.Load(v.Rec.o21ID);
                v.ComboO21 = v.RecO21.o21Name;
            }
            if (v.RecO21 != null && v.Rec.o22PlanFrom == null && (v.RecO21.o21TypeFlag == BO.o21TypeFlagEnum.Lhuta || v.RecO21.o21TypeFlag == BO.o21TypeFlagEnum.Udalost))
            {
                v.Rec.o22PlanFrom = DateTime.Today;
            }
            if (v.ProjectCombo == null)
            {
                v.ProjectCombo = new ProjectComboViewModel() { SelectedP41ID = v.Rec.p41ID };
                if (v.ProjectCombo.SelectedP41ID > 0)
                {
                    v.ProjectCombo.SelectedProject = Factory.CBL.GetObjectAlias("p41", v.ProjectCombo.SelectedP41ID);
                }
            }
            v.ProjectCombo.MyQueryInline = $"j02id_for_p56_o22_entry|int|{this.Factory.CurrentUser.pid}";   //omezit projekty na povolené podle projektové role

            if (v.reminder == null)
            {
                v.reminder = new ReminderViewModel() { record_pid = v.rec_pid, record_prefix = "o22" };
            }

            

            InhaleNotepad(v);

            InhaleRoles(v);


            if (v.rec_pid == 0 && v.Rec.j02ID_Owner == 0)
            {
                v.Rec.j02ID_Owner = Factory.CurrentUser.pid;
                v.ComboOwner = Factory.CurrentUser.FullnameDesc;
            }



        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(o22Record v)
        {
            RefreshStateRecord(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "more")
                {
                    v.IsShowMore = !v.IsShowMore;

                }
                if (v.PostbackOper == "o21id")
                {
                    if (v.Rec.o21ID > 0)
                    {
                        v.RecO21 = Factory.o21MilestoneTypeBL.Load(v.Rec.o21ID);
                        InhaleRoles(v);
                        InhaleNotepad(v);
                    }


                }


                return View(v);
            }


            if (ModelState.IsValid)
            {
                BO.o22Milestone c = new BO.o22Milestone();
                if (v.rec_pid > 0) c = Factory.o22MilestoneBL.Load(v.rec_pid);
                c.o21ID = v.Rec.o21ID;

                c.j02ID_Owner = v.Rec.j02ID_Owner;
               
                c.o22Name = v.Rec.o22Name;
                c.o22PlanFrom = v.Rec.o22PlanFrom;
                c.o22PlanUntil = v.Rec.o22PlanUntil;
                c.o22DurationCount = v.Rec.o22DurationCount;
                c.o22DurationUnit = v.Rec.o22DurationUnit;
                c.o22DurationCalcFlag = v.Rec.o22DurationCalcFlag;
                c.o22Location = v.Rec.o22Location;
                
                c.p41ID = v.ProjectCombo.SelectedP41ID;

                if (v.IsShowMore || v.rec_pid == 0)
                {
                    
                    c.o22Notepad = v.Notepad.HtmlContent;
                    c.x04ID = v.Notepad.SelectedX04ID;

                }


                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.o22MilestoneBL.Save(c, v.roles.getList4Save(Factory));
                if (c.pid > 0)
                {
                    if (v.IsShowMore)
                    {
                        Factory.o51TagBL.SaveTagging("o22", c.pid, v.TagPids);
                        Factory.o27AttachmentBL.CommitNotepdChanges(v.Notepad.TempGuid, "o22", c.pid);
                    }
                    if (v.rec_pid == 0 && v.b05RecordEntity != null && v.b05RecordPid > 0)
                    {
                        var recB05 = new BO.b05Workflow_History() { o22ID = c.pid, b05IsManualStep = true, b05RecordEntity = v.b05RecordEntity, b05RecordPid = v.b05RecordPid };
                        Factory.WorkflowBL.Save2History(recB05);
                    }
                    if (v.RecO21.o21TypeFlag == BO.o21TypeFlagEnum.Udalost)
                    {
                        v.reminder.SaveChanges(Factory, c.pid,(c.o22PlanFrom ==null ? c.o22PlanUntil: c.o22PlanFrom));
                    }
                    else
                    {
                        v.reminder.SaveChanges(Factory, c.pid, c.o22PlanUntil);
                    }

                    if (v.o43id_source > 0)
                    {
                        var recO43 = Factory.o43InboxBL.Load(v.o43id_source);
                        recO43.o22ID = c.pid;
                        Factory.o43InboxBL.Save(recO43);
                    }
                    if (v.UploadGuid != null)
                    {
                        Factory.o27AttachmentBL.SaveChangesAndUpload(v.UploadGuid, "o22", c.pid);
                    }
                    


                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }

        private void InhaleNotepad(o22Record v, int x04id = 0, string htmlcontent = null)
        {

            if (v.Notepad == null)
            {
                v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "o22", SelectedX04ID = Factory.Lic.x04ID_Default };
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

        private void InhaleRoles(o22Record v)
        {
            if (v.roles == null)
            {
                v.roles = new RoleAssignViewModel() { RecPid = v.rec_pid, RecPrefix = "o22", RolePrefix = "o22", Header = "." };
                v.roles.Default_j02ID = v.j02id_create_taskfor; v.roles.Default_Person = Factory.j02UserBL.Load(v.j02id_create_taskfor).FullnameDesc;

            }

        }

        private bool InhalePermissions(o22Record v)
        {
            var mydisp = Factory.o22MilestoneBL.InhaleRecDisposition(v.Rec.pid, v.Rec);
            if (!mydisp.OwnerAccess)
            {
                return false;
            }

            return true;
        }

        public DateTime CalcDuration(DateTime? start, int delka, string jednotka) //vrátí datum konce lhůty
        {
            if (start == null) start = DateTime.Today;
            DateTime d = Convert.ToDateTime(start);

            return Factory.o22MilestoneBL.GetKonecLhuty(d, delka, jednotka);

        }
        public string ShowDuration(DateTime d1, DateTime d2)
        {
            return BO.Code.Time.DurationFormatted(d1, d2, true);
        }
    }
}
