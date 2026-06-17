using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class p35Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p35Record() { rec_pid = pid, rec_entity = "p35" };
            v.Rec = new BO.p35Unit();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p35UnitBL.Load(v.rec_pid);
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
        [ValidateAntiForgeryToken]
        public IActionResult Record(Models.Record.p35Record v)
        {

            if (ModelState.IsValid)
            {
                BO.p35Unit c = new BO.p35Unit();
                if (v.rec_pid > 0) c = Factory.p35UnitBL.Load(v.rec_pid);
                c.p35Name = v.Rec.p35Name;
                c.p35Code = v.Rec.p35Code;
               
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p35UnitBL.Save(c);
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
