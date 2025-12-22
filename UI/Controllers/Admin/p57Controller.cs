using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;
using System.Net.WebSockets;

namespace UI.Controllers.Admin
{
    public class p57Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p57Record() { rec_pid = pid, rec_entity = "p57" };
            v.Rec = new BO.p57TaskType();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p57TaskTypeBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                if (v.Rec.x38ID > 0)
                {
                    v.ComboX38 = Factory.x38CodeLogicBL.Load(v.Rec.x38ID).x38Name;
                }
                v.ComboB01 = v.Rec.b01Name;

            }
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(p57Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p57Record v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                BO.p57TaskType c = new BO.p57TaskType();
                if (v.rec_pid > 0) c = Factory.p57TaskTypeBL.Load(v.rec_pid);
                c.p57Name = v.Rec.p57Name;
                c.x38ID = v.Rec.x38ID;
                c.b01ID = v.Rec.b01ID;
                c.p57PlanScope = v.Rec.p57PlanScope;
                c.p57Ordinary = v.Rec.p57Ordinary;
                
                c.p57ProjectFlag = v.Rec.p57ProjectFlag;
                c.p57HelpdeskFlag = v.Rec.p57HelpdeskFlag;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p57TaskTypeBL.Save(c);
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
