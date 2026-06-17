using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers.Admin
{
    public class p80Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p80Record() { rec_pid = pid, rec_entity = "p80" };
            v.Rec = new BO.p80InvoiceAmountStructure();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p80InvoiceAmountStructureBL.Load(v.rec_pid);
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

        private void RefreshState(p80Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p80Record v)
        {
            RefreshState(v);

            if (ModelState.IsValid)
            {
                if (v.IsPostback)
                {
                    return View(v);
                }
                BO.p80InvoiceAmountStructure c = new BO.p80InvoiceAmountStructure();
                if (v.rec_pid > 0) c = Factory.p80InvoiceAmountStructureBL.Load(v.rec_pid);
                c.p80Name = v.Rec.p80Name;
                c.p80IsExpenseSeparate = v.Rec.p80IsExpenseSeparate;
                c.p80IsFeeSeparate = v.Rec.p80IsFeeSeparate;
                c.p80IsTimeSeparate = v.Rec.p80IsTimeSeparate;
                c.p80GroupByPrefix = v.Rec.p80GroupByPrefix;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p80InvoiceAmountStructureBL.Save(c);
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
