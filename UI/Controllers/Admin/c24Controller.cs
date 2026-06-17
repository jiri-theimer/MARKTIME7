using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;

namespace UI.Controllers.Admin
{
    public class c24Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new c24Record() { rec_pid = pid, rec_entity = "c24" };
            v.Rec = new BO.c24DayColor();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.c24DayColorBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                

            }
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(c24Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(c24Record v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                BO.c24DayColor c = new BO.c24DayColor();
                if (v.rec_pid > 0) c = Factory.c24DayColorBL.Load(v.rec_pid);
                c.c24Name = v.Rec.c24Name;
                c.c24Color = v.Rec.c24Color;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.c24DayColorBL.Save(c);
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
