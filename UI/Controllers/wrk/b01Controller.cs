using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;

namespace UI.Controllers.wrk
{
    public class b01Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone, string prefix)
        {
            var v = new b01Record() { rec_pid = pid, rec_entity = "b01" };
            v.Rec = new BO.b01WorkflowTemplate() { b01Entity = prefix };

            if (v.rec_pid > 0)
            {
                v.Rec = Factory.b01WorkflowTemplateBL.Load(v.rec_pid);
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

        private void RefreshState(b01Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(b01Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.b01WorkflowTemplate c = new BO.b01WorkflowTemplate();
                if (v.rec_pid > 0) c = Factory.b01WorkflowTemplateBL.Load(v.rec_pid);                
                c.b01Name = v.Rec.b01Name;                
                c.b01Entity = v.Rec.b01Entity;
                c.b01PrincipleFlag = v.Rec.b01PrincipleFlag;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);


                c.pid = Factory.b01WorkflowTemplateBL.Save(c);
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
