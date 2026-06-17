using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class HelpdeskController : BaseController
    {
        public IActionResult Index(int pid, int p41id,int p28id, int p57id)
        {
            var v = new HelpdeskViewModel() { rec_pid = pid,p41id=p41id,p28id=p28id,p57ID=p57id, Notepad = new Models.Notepad.EditorViewModel() { Prefix = "p56" } };
            v.Notepad.SelectedX04ID = Factory.Lic.x04ID_Default;
                        
            if (v.rec_pid > 0)
            {
                var rec = Factory.p56TaskBL.Load(v.rec_pid);
                if (rec == null)
                {
                    return RecNotFound(v);
                }
                var mydisp = Factory.p56TaskBL.InhaleRecDisposition(rec.pid, rec);
                if (!mydisp.OwnerAccess && rec.j02ID_Owner !=Factory.CurrentUser.pid)
                {
                    return this.StopPage(true, "Nemáte oprávnění k editaci karty požadavku.");
                }
                
                v.x04ID = rec.x04ID;
                v.p56Notepad = rec.p56Notepad;

                v.p57ID = rec.p57ID;
                v.RecP57 = Factory.p57TaskTypeBL.Load(rec.p57ID);
                v.ComboP57 = v.RecP57.p57Name;
                
                InhaleNotepad(v, v.x04ID, v.p56Notepad);

            }
            else
            {
                var lisP57 = Factory.p57TaskTypeBL.GetList(new BO.myQuery("p57")).Where(p => p.p57HelpdeskFlag == BO.p57HelpdeskFlagEnum.Helpdesk);
                if (lisP57.Count() == 0)
                {
                    return this.StopPage(true, "V administraci chybí typ úkolu [Helpdesk].");
                }
                if (v.p57ID == 0)
                {
                    v.p57ID = lisP57.First().pid;
                }
                v.RecP57 = Factory.p57TaskTypeBL.Load(v.p57ID);
                v.ComboP57 = v.RecP57.p57Name;
                v.IsShowP57Combo = true;
                if (lisP57.Count() == 1)
                {
                    v.IsShowP57Combo = false;
                    
                }
            }
            

            RefreshState(v);


          

            return View(v);
        }

        private void RefreshState(HelpdeskViewModel v)
        {
            if (v.p57ID > 0 && v.RecP57 == null)
            {
                v.RecP57 = Factory.p57TaskTypeBL.Load(v.p57ID);
                v.ComboP57 = v.RecP57.p57Name;
            }

            if (v.p41id>0 && v.SelectedComboProject == null)
            {
                var recP41 = Factory.p41ProjectBL.Load(v.p41id);
                v.SelectedComboProject = recP41.p41Name;
            }

            InhaleNotepad(v);

            
            if (v.ff1 == null)
            {
                v.ff1 = new FreeFieldsViewModel();
                v.ff1.InhaleFreeFieldsView(Factory, v.rec_pid, "p56");
            }
            v.ff1.RefreshInputsVisibility(Factory, v.rec_pid, "p56", v.p57ID);

        }

        private void InhaleNotepad(HelpdeskViewModel v, int x04id = 0, string htmlcontent = null)
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


        [HttpPost]
        public IActionResult Index(HelpdeskViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "p57id")
                {
                    if (v.p57ID > 0)
                    {
                        v.RecP57 = Factory.p57TaskTypeBL.Load(v.p57ID);
                    }
                }

                return View(v);
            }

            if (ModelState.IsValid)
            {                
                BO.p56Task c = new BO.p56Task();
                if (v.rec_pid > 0) c = Factory.p56TaskBL.Load(v.rec_pid);
                c.p57ID = v.p57ID;
                c.p56Name = v.p56Name;
                c.p56Notepad = v.Notepad.HtmlContent;
                c.x04ID = v.Notepad.SelectedX04ID;
                c.p41ID = v.p41id;
                if (c.pid == 0)
                {
                    c.j02ID_Owner = Factory.CurrentUser.pid;
                }
               
                if(string.IsNullOrEmpty(v.Notepad.HtmlContent) || string.IsNullOrEmpty(v.p56Name))
                {
                    this.AddMessage("Název a podrobný popis je povinné vyplnit.");
                    return View(v);
                }

                c.pid = Factory.p56TaskBL.Save(c, v.ff1.inputs, null);
                if (c.pid > 0)
                {

                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }

            this.Notify_RecNotSaved();
            return View(v);
        }


        
    }
}
