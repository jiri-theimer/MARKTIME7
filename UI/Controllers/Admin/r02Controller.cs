using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;

namespace UI.Controllers.Admin
{
    public class r02Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new r02Record() { rec_pid = pid, rec_entity = "r02" };
            v.Rec = new BO.r02CapacityVersion();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.r02CapacityVersionBL.Load(v.rec_pid);
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

        private void RefreshState(r02Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(r02Record v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                BO.r02CapacityVersion c = new BO.r02CapacityVersion();
                if (v.rec_pid > 0) c = Factory.r02CapacityVersionBL.Load(v.rec_pid);
                c.r02Name = v.Rec.r02Name;
              

                c.r02Ordinary = v.Rec.r02Ordinary;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.r02CapacityVersionBL.Save(c);
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
