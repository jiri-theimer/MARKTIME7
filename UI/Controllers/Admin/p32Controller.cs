using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class p32Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p32Record() { rec_pid = pid, rec_entity = "p32" };
            v.Rec = new BO.p32Activity() { p32IsBillable = true, p32IsTextRequired = true };
            v.ProjectCombo = new ProjectComboViewModel();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p32ActivityBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.ComboP34Name = v.Rec.p34Name;
                v.ComboP38Name = v.Rec.p38Name;
                v.ComboP95Name = v.Rec.p95Name;
                v.ProjectCombo.SelectedP41ID = v.Rec.p41ID_Absence;
                
                
            }
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(p32Record v)
        {
            

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p32Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.p32Activity c = new BO.p32Activity();
                if (v.rec_pid > 0) c = Factory.p32ActivityBL.Load(v.rec_pid);
                c.p34ID = v.Rec.p34ID;
                c.p95ID = v.Rec.p95ID;
                c.p38ID = v.Rec.p38ID;
                c.x15ID = v.Rec.x15ID;

                c.p32IsBillable = v.Rec.p32IsBillable;
                c.p32IsTextRequired = v.Rec.p32IsTextRequired;
                c.p32IsSupplier = v.Rec.p32IsSupplier;

                c.p32Name = v.Rec.p32Name;
                c.p32Code = v.Rec.p32Code;
                c.p32Ordinary = v.Rec.p32Ordinary;
                c.p32Name_BillingLang1 = v.Rec.p32Name_BillingLang1;
                c.p32Name_BillingLang2 = v.Rec.p32Name_BillingLang2;
                c.p32Name_BillingLang3 = v.Rec.p32Name_BillingLang3;
                c.p32Name_BillingLang4 = v.Rec.p32Name_BillingLang4;

                c.p32DefaultWorksheetText = v.Rec.p32DefaultWorksheetText;
                c.p32DefaultWorksheetText_Lang1 = v.Rec.p32DefaultWorksheetText_Lang1;
                c.p32DefaultWorksheetText_Lang2 = v.Rec.p32DefaultWorksheetText_Lang2;
                c.p32DefaultWorksheetText_Lang3 = v.Rec.p32DefaultWorksheetText_Lang3;
                c.p32DefaultWorksheetText_Lang4 = v.Rec.p32DefaultWorksheetText_Lang4;

                c.p32Value_Default = v.Rec.p32Value_Default;
                c.p32Value_Minimum = v.Rec.p32Value_Minimum;
                c.p32Value_Maximum = v.Rec.p32Value_Maximum;

                c.p32DefaultWorksheetText = v.Rec.p32DefaultWorksheetText;
                //c.p32HelpText = v.Rec.p32HelpText;

                c.p32ExternalCode = v.Rec.p32ExternalCode;
                c.p32ManualFeeFlag = v.Rec.p32ManualFeeFlag;
                c.p32ManualFeeDefAmount = v.Rec.p32ManualFeeDefAmount;

                //c.p32MarginHidden = v.Rec.p32MarginHidden;
                //c.p32MarginTransparent = v.Rec.p32MarginTransparent;
                //c.p32AccountingIDS = v.Rec.p32AccountingIDS;
                //c.p32ActivityIDS = v.Rec.p32ActivityIDS;
                c.p32IsCP = v.Rec.p32IsCP;
                c.p32HelpText = v.Rec.p32HelpText;
                
                c.p41ID_Absence = v.ProjectCombo.SelectedP41ID;
                c.p32Color = v.Rec.p32Color;
                c.p32AbsenceFlag = v.Rec.p32AbsenceFlag;
                c.p32AbsenceBreakFlag = v.Rec.p32AbsenceBreakFlag;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p32ActivityBL.Save(c);
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
