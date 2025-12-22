using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class p95Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p95Record() { rec_pid = pid, rec_entity = "p95" };
            v.Rec = new BO.p95InvoiceRow();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p95InvoiceRowBL.Load(v.rec_pid);
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

        private void RefreshState(p95Record v)
        {
            

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p95Record v)
        {
            RefreshState(v);

            if (ModelState.IsValid)
            {
                BO.p95InvoiceRow c = new BO.p95InvoiceRow();
                if (v.rec_pid > 0) c = Factory.p95InvoiceRowBL.Load(v.rec_pid);
                c.p95Name = v.Rec.p95Name;
                //c.p95AccountingIDS = v.Rec.p95AccountingIDS;
                c.p95Code = v.Rec.p95Code;
                c.p95Ordinary = v.Rec.p95Ordinary;
                c.p95Name_BillingLang1 = v.Rec.p95Name_BillingLang1;
                c.p95Name_BillingLang2 = v.Rec.p95Name_BillingLang2;
                c.p95Name_BillingLang3 = v.Rec.p95Name_BillingLang3;
                c.p95Name_BillingLang4 = v.Rec.p95Name_BillingLang4;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p95InvoiceRowBL.Save(c);
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
