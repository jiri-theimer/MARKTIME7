using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;
using UI.Models.Tab1;

namespace UI.Controllers.wrk
{
    public class b02Controller : BaseController
    {
        public IActionResult Info(string prefix,int pid)
        {
            var v = new BaseTab1ViewModel() { prefix = prefix, pid = pid };
            return View(v);
        }
        public IActionResult Record(int pid, bool isclone,int b01id)
        {
            var v = new b02Record() { rec_pid = pid, rec_entity = "b02" };
            v.Rec = new BO.b02WorkflowStatus() { b01ID = b01id };

            if (v.rec_pid > 0)
            {
                v.Rec = Factory.b02WorkflowStatusBL.Load(v.rec_pid);
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

            return ViewTup(v,BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(b02Record v)
        {
            if (v.Rec.b01ID > 0)
            {
                v.RecB01 = Factory.b01WorkflowTemplateBL.Load(v.Rec.b01ID);
                if (v.ComboB01 == null) v.ComboB01 = v.RecB01.b01Name;
            }
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(b02Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.b02WorkflowStatus c = new BO.b02WorkflowStatus();
                if (v.rec_pid > 0) c = Factory.b02WorkflowStatusBL.Load(v.rec_pid);
                
                c.b01ID = v.Rec.b01ID;
                c.b02Name = v.Rec.b02Name;
                c.b02Ordinary = v.Rec.b02Ordinary;
                c.b02AutoRunFlag = v.Rec.b02AutoRunFlag;
                c.b02RecordFlag = v.Rec.b02RecordFlag;
                c.b02Color = v.Rec.b02Color;
                c.b02IsRecordReadOnly4Owner = v.Rec.b02IsRecordReadOnly4Owner;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);


                c.pid = Factory.b02WorkflowStatusBL.Save(c);
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
