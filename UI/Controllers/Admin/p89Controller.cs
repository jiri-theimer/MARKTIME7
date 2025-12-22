using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers.Admin
{
    public class p89Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p89Record() { rec_pid = pid, rec_entity = "p89" };
            v.creates = new CreateAssignViewModel() { j08RecordEntity = "p89", j08RecordPid = v.rec_pid };
            v.Rec = new BO.p89ProformaType();
            
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p89ProformaTypeBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.ComboP93Name = v.Rec.p93Name;
                if (v.Rec.x31ID > 0)
                {
                    try
                    {
                        v.ComboX31Name = Factory.x31ReportBL.Load(v.Rec.x31ID).x31Name;
                    }
                    catch
                    {
                        //nic
                    }
                    
                }
                if (v.Rec.x31ID_Payment > 0)
                {
                    try
                    {
                        v.ComboX31Name_Payment = Factory.x31ReportBL.Load(v.Rec.x31ID_Payment).x31Name;
                    }
                    catch
                    {
                        //nic
                    }
                }                                
                if (v.Rec.x38ID > 0)
                {
                    v.ComboX38Name = Factory.x38CodeLogicBL.Load(v.Rec.x38ID).x38Name;
                }
                if (v.Rec.x38ID_Payment > 0)
                {
                    v.ComboX38Name_Payment = Factory.x38CodeLogicBL.Load(v.Rec.x38ID_Payment).x38Name;
                }
                if (v.Rec.j61ID > 0)
                {
                    v.ComboJ61Name = Factory.j61TextTemplateBL.Load(v.Rec.j61ID).j61Name;
                }
                v.creates.SetInitValues(Factory, "p89", v.rec_pid);
            }
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(p89Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p89Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.p89ProformaType c = new BO.p89ProformaType();
                if (v.rec_pid > 0) c = Factory.p89ProformaTypeBL.Load(v.rec_pid);
                c.p89Name = v.Rec.p89Name;
                c.x38ID = v.Rec.x38ID;
                c.x38ID_Payment = v.Rec.x38ID_Payment;
                c.p93ID = v.Rec.p93ID;
                c.x31ID = v.Rec.x31ID;
                c.x31ID_Payment = v.Rec.x31ID_Payment;
                c.j61ID = v.Rec.j61ID;
                c.p89DefaultText1 = v.Rec.p89DefaultText1;
                c.p89DefaultText2 = v.Rec.p89DefaultText2;

                c.p89RolesTab = v.Rec.p89RolesTab;
                
                c.p89FilesTab = v.Rec.p89FilesTab;


                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p89ProformaTypeBL.Save(c,v.creates.getList4Save(Factory));
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
