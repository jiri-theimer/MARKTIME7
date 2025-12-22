using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class o15Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new o15Record() { rec_pid = pid, rec_entity = "o15" };
            v.Rec = new BO.o15AutoComplete();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.o15AutoCompleteBL.Load(v.rec_pid);
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
        public IActionResult Record(Models.Record.o15Record v)
        {

            if (ModelState.IsValid)
            {
                BO.o15AutoComplete c = new BO.o15AutoComplete();
                if (v.rec_pid > 0) c = Factory.o15AutoCompleteBL.Load(v.rec_pid);
                c.o15Value = v.Rec.o15Value;
                c.o15Flag = v.Rec.o15Flag;               
                c.o15Ordinary = v.Rec.o15Ordinary;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.o15AutoCompleteBL.Save(c);
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