using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers.Admin
{
    public class x01Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new x01Record() { rec_pid = pid, rec_entity = "x01" };
            v.Rec = new BO.x01License();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.x01LicenseBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                if (v.Rec.x04ID_Default > 0)
                {
                    v.ComboX04 = Factory.x04NotepadConfigBL.GetList(new BO.myQuery("x04")).First(p => p.pid == v.Rec.x04ID_Default).x04Name;

                }

               

            }
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            v.Toolbar.AllowDelete = false;
            v.Toolbar.AllowClone = false;
            v.Toolbar.AllowArchive = false;
            
           

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(x01Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(x01Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper == "ping")
                {
                    var cping = new BL.Code.PingSupport();
                    var ret=cping.SendPing(Factory).Result;
                    this.AddMessageTranslated($"Ping vrátil: {ret}", "info");
                }
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.x01License c = new BO.x01License();
                if (v.rec_pid > 0) c = Factory.x01LicenseBL.Load(v.rec_pid);
                c.x01AppName = v.Rec.x01AppName;
                c.x01LangIndex = v.Rec.x01LangIndex;
                c.x01BillingLang1 = v.Rec.x01BillingLang1;
                c.x01BillingLang2 = v.Rec.x01BillingLang2;
                c.x01BillingLang3 = v.Rec.x01BillingLang3;
                c.x01BillingLang4 = v.Rec.x01BillingLang4;
                c.x01BillingFlag1 = v.Rec.x01BillingFlag1;
                c.x01BillingFlag2 = v.Rec.x01BillingFlag2;
                c.x01BillingFlag3 = v.Rec.x01BillingFlag3;
                c.x01BillingFlag4 = v.Rec.x01BillingFlag4;
                c.x01Round2Minutes = v.Rec.x01Round2Minutes;
                c.x01IsAllowPasswordRecovery = v.Rec.x01IsAllowPasswordRecovery;
                c.j27ID = v.Rec.j27ID;
                c.x01CountryCode = v.Rec.x01CountryCode;
                c.x01AppHost = v.Rec.x01AppHost;
                c.x01RobotLogin = v.Rec.x01RobotLogin;
                c.x01PasswordRecovery_Answer = v.Rec.x01PasswordRecovery_Answer;
                c.x01PasswordRecovery_Question = v.Rec.x01PasswordRecovery_Question;
              
                c.x01IsAllowDuplicity_RegID = v.Rec.x01IsAllowDuplicity_RegID;
                c.x01IsAllowDuplicity_VatID = v.Rec.x01IsAllowDuplicity_VatID;
                c.x01IsAllowDuplicity_p86 = v.Rec.x01IsAllowDuplicity_p86;

                c.x04ID_Default = v.Rec.x04ID_Default;
                c.x15ID = v.Rec.x15ID;
                c.x01InvoiceMaturityDays = v.Rec.x01InvoiceMaturityDays;
                c.x01IsCapacityFaNefa = v.Rec.x01IsCapacityFaNefa;
                c.x01LockFlag = v.Rec.x01LockFlag;
               

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.x01LicenseBL.Save(c);
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
