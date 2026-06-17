using BL;
using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;

namespace UI.Controllers.Admin
{
    public class o21Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new o21Record() { rec_pid = pid, rec_entity = "o21" };
            v.Rec = new BO.o21MilestoneType();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.o21MilestoneTypeBL.Load(v.rec_pid);
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

        private void RefreshState(o21Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(o21Record v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                BO.o21MilestoneType c = new BO.o21MilestoneType();
                if (v.rec_pid > 0) c = Factory.o21MilestoneTypeBL.Load(v.rec_pid);
                c.o21Name = v.Rec.o21Name;
                c.o21TypeFlag = v.Rec.o21TypeFlag;                
                c.o21Ordinary = v.Rec.o21Ordinary;
                c.o21Color = v.Rec.o21Color;
                c.o21IsP41Compulsory = v.Rec.o21IsP41Compulsory;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.o21MilestoneTypeBL.Save(c);
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
