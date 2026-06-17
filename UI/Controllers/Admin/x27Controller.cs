using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class x27Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new x27Record() { rec_pid = pid, rec_entity = "x27" };
            v.Rec = new BO.x27EntityFieldGroup();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.x27EntityFieldGroupBL.Load(v.rec_pid);
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

        private void RefreshState(x27Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(x27Record v)
        {
            RefreshState(v);

            if (ModelState.IsValid)
            {
                BO.x27EntityFieldGroup c = new BO.x27EntityFieldGroup();
                if (v.rec_pid > 0) c = Factory.x27EntityFieldGroupBL.Load(v.rec_pid);
                c.x27Name = v.Rec.x27Name;
                c.x27Ordinary = v.Rec.x27Ordinary;
                
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.x27EntityFieldGroupBL.Save(c);
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
