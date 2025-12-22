using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;

namespace UI.Controllers.Admin
{
    public class x07Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new x07Record() { rec_pid = pid, rec_entity = "x07" };
            v.Rec = new BO.x07Integration() { x07Flag = BO.x07FlagEnum.Ecomail };
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.x07IntegrationBL.Load(v.rec_pid);
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

            RefreshSate(v);
            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshSate(x07Record v)
        {
            if (v.Rec.x07Flag == BO.x07FlagEnum.Ecomail)
            {
                v.IsShowToken = true;
            }
            if (v.Rec.x07Flag == BO.x07FlagEnum.AbraFlexi)
            {
                v.IsShowLoginPassword = true;
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(Models.Record.x07Record v)
        {
            RefreshSate(v);

            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.x07Integration c = new BO.x07Integration();
                if (v.rec_pid > 0) c = Factory.x07IntegrationBL.Load(v.rec_pid);
                c.x07Name = v.Rec.x07Name;
                c.x07Login = v.Rec.x07Login;
                c.x07Password = v.Rec.x07Password;
                c.x07Token = v.Rec.x07Token;
                c.x07Ordinary = v.Rec.x07Ordinary;
                c.x07Flag = v.Rec.x07Flag;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.x07IntegrationBL.Save(c);
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
