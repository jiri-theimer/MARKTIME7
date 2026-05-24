using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers.Admin
{
    public class o58Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new o58Record() { rec_pid = pid, rec_entity = "o58" };
            v.Rec = new BO.o58GlobalParam();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.o58GlobalParamBL.Load(v.rec_pid);
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
        public IActionResult Record(Models.Record.o58Record v)
        {

            if (ModelState.IsValid)
            {
                BO.o58GlobalParam c = new BO.o58GlobalParam();
                if (v.rec_pid > 0) c = Factory.o58GlobalParamBL.Load(v.rec_pid);
                c.o58Name = v.Rec.o58Name;
                c.x24ID = v.Rec.x24ID;
                c.o58Key = v.Rec.o58Key;
                c.o58Entity = v.Rec.o58Entity;
                c.o58IsPerUser = v.Rec.o58IsPerUser;
                c.o58Ordinary = v.Rec.o58Ordinary;
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.o58GlobalParamBL.Save(c);
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
