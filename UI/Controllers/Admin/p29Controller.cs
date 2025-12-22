using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class p29Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p29Record() { rec_pid = pid, rec_entity = "p29" };
            v.Rec = new BO.p29ContactType();
            v.roles = new RoleAssignViewModel() { RecPid = v.rec_pid, RecPrefix = "p29",RolePrefix="p28",Header="Automatické obsazení rolí v záznamech kontaktů tohoto typu"};
            v.creates = new CreateAssignViewModel() { j08RecordEntity = "p29", j08RecordPid = v.rec_pid };

            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p29ContactTypeBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.ComboB01Name = v.Rec.b01Name;
                
                if (v.Rec.x38ID > 0)
                {
                    v.ComboX38Name = Factory.x38CodeLogicBL.Load(v.Rec.x38ID).x38Name;
                }

                v.creates.SetInitValues(Factory, "p29", v.rec_pid);

            }
            RefreshState(v);
            

            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(p29Record v)
        {
            

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p29Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                BO.p29ContactType c = new BO.p29ContactType();
                if (v.rec_pid > 0) c = Factory.p29ContactTypeBL.Load(v.rec_pid);
                c.p29Name = v.Rec.p29Name;
                
                c.b01ID = v.Rec.b01ID;
                c.x38ID = v.Rec.x38ID;
                c.p29ScopeFlag = v.Rec.p29ScopeFlag;
                c.p29Ordinary = v.Rec.p29Ordinary;
              
                c.p29FilesTab = v.Rec.p29FilesTab;
                c.p29RolesTab = v.Rec.p29RolesTab;
                c.p29BillingTab = v.Rec.p29BillingTab;
                c.p29ContactPersonsTab = v.Rec.p29ContactPersonsTab;
                c.p29ContactMediaTab = v.Rec.p29ContactMediaTab;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                
                c.pid = Factory.p29ContactTypeBL.Save(c, v.roles.getList4Save(Factory), v.creates.getList4Save(Factory));
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
