using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;

namespace UI.Controllers.Admin
{
    public class p54Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p54Record() { rec_pid = pid, rec_entity = "p54" };
            v.Rec = new BO.p54OvertimeLevel() { p54BillingRate = 1, p54InternalRate = 1 };
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p54OvertimeLevelBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }

            }
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }
            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }
        [HttpPost]        
        public IActionResult Record(p54Record v)
        {

            if (ModelState.IsValid)
            {
                BO.p54OvertimeLevel c = new BO.p54OvertimeLevel();
                if (v.rec_pid > 0) c = Factory.p54OvertimeLevelBL.Load(v.rec_pid);
                c.p54Name = v.Rec.p54Name;
                c.p54Ordinary = v.Rec.p54Ordinary;
                c.p54BillingRate = v.Rec.p54BillingRate;
                c.p54InternalRate = v.Rec.p54InternalRate;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p54OvertimeLevelBL.Save(c);
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
