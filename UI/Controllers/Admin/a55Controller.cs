using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class a55Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {

            var v = new a55Record() { rec_pid = pid, rec_entity = "a55" };

            v.Rec = new BO.a55RecPage() {};
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.a55RecPageBL.Load(v.rec_pid);
                
            }
            v.Toolbar = new MyToolbarViewModel(v.Rec);


            if (isclone)
            {
                v.MakeClone();

            }
            RefreshState(v);
            return ViewTup(v,BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(a55Record v)
        {
            
        }

        [HttpPost]
        public IActionResult Record(a55Record v)
        {
            RefreshState(v);

            if (ModelState.IsValid)
            {
                if (v.IsPostback)
                {
                    return View(v);
                }
                BO.a55RecPage c = new BO.a55RecPage();
                if (v.rec_pid > 0) c = Factory.a55RecPageBL.Load(v.rec_pid);

               
                c.a55Name = v.Rec.a55Name;                
                c.a55Entity = v.Rec.a55Entity;
                c.a55CssFile = v.Rec.a55CssFile;
               
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.a55RecPageBL.Save(c);
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
