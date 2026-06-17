using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class p38Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p38Record() { rec_pid = pid, rec_entity = "p38" };
            v.Rec = new BO.p38ActivityTag();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p38ActivityTagBL.Load(v.rec_pid);
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

        private void RefreshState(p38Record v)
        {
            

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p38Record v)
        {
            RefreshState(v);

            if (ModelState.IsValid)
            {
                BO.p38ActivityTag c = new BO.p38ActivityTag();
                if (v.rec_pid > 0) c = Factory.p38ActivityTagBL.Load(v.rec_pid);
                c.p38Name = v.Rec.p38Name;                
                
                c.p38Ordinary = v.Rec.p38Ordinary;
                
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p38ActivityTagBL.Save(c);
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
