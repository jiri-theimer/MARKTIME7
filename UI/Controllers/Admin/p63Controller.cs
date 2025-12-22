using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers.Admin
{
    public class p63Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p63Record() { rec_pid = pid, rec_entity = "p63" };
            v.Rec = new BO.p63Overhead();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p63OverheadBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.ComboP32 = v.Rec.p32Name;

            }
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(p63Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p63Record v)
        {
            RefreshState(v);

            if (ModelState.IsValid)
            {
                BO.p63Overhead c = new BO.p63Overhead();
                if (v.rec_pid > 0) c = Factory.p63OverheadBL.Load(v.rec_pid);
                c.p63Name = v.Rec.p63Name;
                c.p32ID = v.Rec.p32ID;
                c.p63PercentRate = v.Rec.p63PercentRate;
                c.p63IsIncludeExpense = v.Rec.p63IsIncludeExpense;
                c.p63IsIncludeFees = v.Rec.p63IsIncludeFees;
                c.p63IsIncludeTime = v.Rec.p63IsIncludeTime;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p63OverheadBL.Save(c);
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
