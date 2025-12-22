using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers.Admin
{
    public class j25Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new j25Record() { rec_pid = pid, rec_entity = "j25" };
            v.Rec = new BO.j25ReportCategory();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.j25ReportCategoryBL.Load(v.rec_pid);
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

        private void RefreshState(j25Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(j25Record v)
        {
            RefreshState(v);

            if (ModelState.IsValid)
            {
                BO.j25ReportCategory c = new BO.j25ReportCategory();
                if (v.rec_pid > 0) c = Factory.j25ReportCategoryBL.Load(v.rec_pid);
                c.j25Name = v.Rec.j25Name;
                c.j25Code = v.Rec.j25Code;
                c.j25Ordinary = v.Rec.j25Ordinary;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.j25ReportCategoryBL.Save(c);
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
