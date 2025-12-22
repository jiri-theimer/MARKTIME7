using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class c26Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new c26Record() { rec_pid = pid, rec_entity = "c26" };
            v.Rec = new BO.c26Holiday() { c26CountryCode = Factory.Lic.x01CountryCode };
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.c26HolidayBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
               //v.ComboJ17Name = v.Rec.j17Name;

            }
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(c26Record v)
        {
            

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(c26Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                BO.c26Holiday c = new BO.c26Holiday();
                if (v.rec_pid > 0) c = Factory.c26HolidayBL.Load(v.rec_pid);
                c.c26Name = v.Rec.c26Name;
                c.c26Date = v.Rec.c26Date;
                c.c26CountryCode = v.Rec.c26CountryCode;          

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.c26HolidayBL.Save(c);
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
