using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class x38Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new x38Record() { rec_pid = pid, rec_entity = "x38" };
            v.Rec = new BO.x38CodeLogic();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.x38CodeLogicBL.Load(v.rec_pid);
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

        private void RefreshState(x38Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(x38Record v,string oper)
        {
            RefreshState(v);
            if (oper == "postback")
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.x38CodeLogic c = new BO.x38CodeLogic();
                if (v.rec_pid > 0) c = Factory.x38CodeLogicBL.Load(v.rec_pid);
                c.x38Name = v.Rec.x38Name;
                c.x38Entity = v.Rec.x38Entity;
                c.x38Scale = v.Rec.x38Scale;
                c.x38EditModeFlag = v.Rec.x38EditModeFlag;
                c.x38ConstantBeforeValue = v.Rec.x38ConstantBeforeValue;
                c.x38ConstantAfterValue = v.Rec.x38ConstantAfterValue;
                c.x38SqlMaskSyntax = v.Rec.x38SqlMaskSyntax;
                c.x38ExplicitIncrementStart = v.Rec.x38ExplicitIncrementStart;
                c.x38IsUseDbPID = v.Rec.x38IsUseDbPID;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.x38CodeLogicBL.Save(c);
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
