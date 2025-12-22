using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers.Admin
{
    public class p53Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p53Record() { rec_pid = pid, rec_entity = "p53" };
            v.Rec = new BO.p53VatRate();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p53VatRateBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.ComboJ27Code = v.Rec.j27Code;
                
                v.d1 = v.Rec.ValidFrom;
                v.d2 = v.Rec.ValidUntil;
            }
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            v.Toolbar.AllowArchive = false;
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(p53Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p53Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.p53VatRate c = new BO.p53VatRate();
                if (v.rec_pid > 0) c = Factory.p53VatRateBL.Load(v.rec_pid);
                c.j27ID = v.Rec.j27ID;
                c.p53CountryCode = v.Rec.p53CountryCode;
                c.x15ID = v.Rec.x15ID;
                c.p53Value = v.Rec.p53Value;
                if (v.d2 == null)
                {
                    v.d2 = new DateTime(3000, 1, 1);
                }
                if (v.d1 == null)
                {
                    v.d1 = new DateTime(DateTime.Today.Year, 1, 1);
                }

                c.pid = Factory.p53VatRateBL.Save(c,Convert.ToDateTime(v.d1),Convert.ToDateTime(v.d2));
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
