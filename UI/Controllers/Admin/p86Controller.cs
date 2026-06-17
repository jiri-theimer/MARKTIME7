using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class p86Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p86Record() { rec_pid = pid, rec_entity = "p86" };
            v.Rec = new BO.p86BankAccount();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p86BankAccountBL.Load(v.rec_pid);
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

        private void RefreshState(p86Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p86Record v)
        {
            RefreshState(v);

            if (ModelState.IsValid)
            {
                BO.p86BankAccount c = new BO.p86BankAccount();
                if (v.rec_pid > 0) c = Factory.p86BankAccountBL.Load(v.rec_pid);
                c.p86Name = v.Rec.p86Name;
                c.p86BankName = v.Rec.p86BankName;
                c.p86Account = v.Rec.p86Account;
                c.p86Code = v.Rec.p86Code;
                c.p86IBAN = v.Rec.p86IBAN;
                c.p86SWIFT = v.Rec.p86SWIFT;
                c.p86BankAddress = v.Rec.p86BankAddress;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p86BankAccountBL.Save(c);
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
