using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;

namespace UI.Controllers
{
    public class p60Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {

            var v = new p60Record() { rec_pid = pid, rec_entity = "p60", Notepad = new Models.Notepad.EditorViewModel() { Prefix = "p60",SelectedX04ID = Factory.Lic.x04ID_Default } };
            v.Element2Focus = "Rec_p60Name";
            v.Rec = new BO.p60TaskTemplate() { x04ID = Factory.Lic.x04ID_Default };

            var lisP57 = Factory.p57TaskTypeBL.GetList(new BO.myQuery("p57"));
            if (lisP57.Count() == 0)
            {
                return this.StopPage(true, "V administraci chybí naplnit číselník [Typy úkolů].");
            }

            v.Rec.p57ID = lisP57.First().pid; v.ComboP57 = lisP57.First().p57Name;

            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p60TaskTemplateBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                
                v.RecP57 = Factory.p57TaskTypeBL.Load(v.Rec.p57ID);

                v.ComboOwner = v.Rec.Owner;
                v.ComboP57 = v.Rec.p57Name;
               
                if (v.Rec.p41ID > 0)
                {
                    v.RecP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID);

                }
              

                InhaleNotepad(v, v.Rec.x04ID, v.Rec.p60Notepad);


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




        private void RefreshStateRecord(p60Record v)
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

            if (v.Rec.p41ID > 0 && v.RecP41 == null)
            {
                v.RecP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID);
            }
            if (v.rec_pid == 0 && v.Rec.j02ID_Owner == 0)
            {
                v.Rec.j02ID_Owner = Factory.CurrentUser.pid;
                v.ComboOwner = Factory.CurrentUser.FullnameDesc;
            }

            InhaleNotepad(v);

            InhaleRoles(v);





        }

        private void InhaleRoles(p60Record v)
        {
            if (v.roles == null)
            {
                v.roles = new RoleAssignViewModel() { RecPid = v.rec_pid, RecPrefix = "p60", RolePrefix = "p56", Header = "." };
                v.roles.Default_j02ID = Factory.CurrentUser.pid; v.roles.Default_Person = Factory.CurrentUser.FullnameDesc;

            }

        }
        private void InhaleNotepad(p60Record v, int x04id = 0, string htmlcontent = null)
        {

            if (v.Notepad == null)
            {
                v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "p60", SelectedX04ID = Factory.Lic.x04ID_Default };
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p60Record v)
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
                BO.p60TaskTemplate c = new BO.p60TaskTemplate();
                if (v.rec_pid > 0) c = Factory.p60TaskTemplateBL.Load(v.rec_pid);

                c.p57ID = v.Rec.p57ID;
                c.p60Name = v.Rec.p60Name;
                c.p60PlanFrom_UC = v.Rec.p60PlanFrom_UC;
                c.p60PlanFrom_Unit = v.Rec.p60PlanFrom_Unit;
                c.p60PlanUntil_UC = v.Rec.p60PlanUntil_UC;
                c.p60PlanUntil_Unit = v.Rec.p60PlanUntil_Unit;
                c.j02ID_Owner = v.Rec.j02ID_Owner;
                c.p60IsPublic = v.Rec.p60IsPublic;
                c.p41ID = v.ProjectCombo.SelectedP41ID;                

                c.p60PlanFlag = v.Rec.p60PlanFlag;
                c.p60Plan_Hours = v.Rec.p60Plan_Hours;
                c.p60Plan_Expenses = v.Rec.p60Plan_Expenses;
                c.p60Plan_Revenue = v.Rec.p60Plan_Revenue;
                c.p60Plan_Internal_Fee = v.Rec.p60Plan_Internal_Fee;
                c.p60Notepad = v.Notepad.HtmlContent;
                c.x04ID = v.Notepad.SelectedX04ID;
                c.p60IsStopNotify = v.Rec.p60IsStopNotify;
                c.p55ID = v.Rec.p55ID;
                c.p15ID = v.Rec.p15ID;
                c.p60Ordinary = v.Rec.p60Ordinary;


                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p60TaskTemplateBL.Save(c, v.roles.getList4Save(Factory));
                if (c.pid > 0)
                {
                    Factory.o27AttachmentBL.CommitNotepdChanges(v.Notepad.TempGuid, "p60", c.pid);

                   
                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
