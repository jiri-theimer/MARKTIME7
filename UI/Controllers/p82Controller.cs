using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class p82Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone,int p90id)
        {
            var v = new p82Record() { rec_pid = pid, rec_entity = "p82",p90ID=p90id,CanEditRecordCode=true };
            
            v.Rec = new BO.p82Proforma_Payment() { p82Date = DateTime.Today };
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p82Proforma_PaymentBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.p90ID = v.Rec.p90ID;

            }
            if (v.p90ID == 0)
            {
                return this.StopPage(true, "Na vstupu chybí záloha.");
            }
            
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }
            RefreshState(v);
            if (v.rec_pid == 0)
            {
                if (v.RecP90.p90Amount_Debt == v.RecP90.p90Amount)
                {
                    v.Rec.p82Amount = v.RecP90.p90Amount;
                    v.Rec.p82Amount_WithoutVat = v.RecP90.p90Amount_WithoutVat;
                    v.Rec.p82Amount_Vat = v.RecP90.p90Amount_Vat;
                }
                else
                {
                    v.Rec.p82Amount = v.RecP90.p90Amount_Debt;
                    
                }
                
            }
            return View(v);
        }

        private void RefreshState(p82Record v)
        {
            v.Toolbar.AllowArchive = false;
            if (v.RecP90 == null)
            {
                v.RecP90 = Factory.p90ProformaBL.Load(v.p90ID);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p82Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.p82Proforma_Payment c = new BO.p82Proforma_Payment() { p90ID = v.p90ID };
                if (v.rec_pid > 0) c = Factory.p82Proforma_PaymentBL.Load(v.rec_pid);
                c.p82Date = v.Rec.p82Date;
                c.p82DateIssue = v.Rec.p82DateIssue;
                c.p82Amount = v.Rec.p82Amount;
                c.p82Text = v.Rec.p82Text;
                c.p82Amount_WithoutVat = v.Rec.p82Amount_WithoutVat;
                c.p82Amount_Vat = v.Rec.p82Amount_Vat;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p82Proforma_PaymentBL.Save(c);
                if (c.pid > 0)
                {

                    v.SetJavascript_CallOnLoad(v.p90ID);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
