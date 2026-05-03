using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class p27Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p27Record() { rec_pid = pid, rec_entity = "p27" };
            v.Rec = new BO.p27Pctype();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p27PctypeBL.Load(v.rec_pid);
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
        public IActionResult Record(Models.Record.p27Record v)
        {

            if (ModelState.IsValid)
            {
                BO.p27Pctype c = new BO.p27Pctype();
                if (v.rec_pid > 0) c = Factory.p27PctypeBL.Load(v.rec_pid);
                c.p27Name = v.Rec.p27Name;
                
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p27PctypeBL.Save(c);
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
