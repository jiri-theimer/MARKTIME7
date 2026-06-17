using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;

namespace UI.Controllers.Admin
{
    public class x52Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone, string viewurl, int parentid)
        {
            var v = new x52Record() { rec_pid = pid, rec_entity = "x52" };
            v.Rec = new BO.x52Blog() { x52Date = DateTime.Today };
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.x52BlogBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
               

            }
           

            RefreshState(v);
            v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "x52", SelectedX04ID = Factory.Lic.x04ID_Default, HtmlContent = v.Rec.x52Html };

            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(x52Record v)
        {


        }
        [HttpPost]
        public IActionResult Record(x52Record v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                BO.x52Blog c = new BO.x52Blog();
                if (v.rec_pid > 0) c = Factory.x52BlogBL.Load(v.rec_pid);
                c.x52Name = v.Rec.x52Name;
                c.x52Date = v.Rec.x52Date;
               
                c.x52Html = v.Notepad.HtmlContent;
                c.x52Editorial = v.Rec.x52Editorial;
                c.x52Symbol = v.Rec.x52Symbol;
                c.x52Ordinary = v.Rec.x52Ordinary;
               
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.x52BlogBL.Save(c);
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
