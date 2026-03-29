using Microsoft.AspNetCore.Mvc;
using UI.Models;

using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class o27Controller : BaseController
    {
        public IActionResult Info(int pid)
        {
            return Tab1(pid, "info");
        }

        public IActionResult Tab1(int pid, string caller)
        {
            var v = new o27Tab1() { Factory = this.Factory, prefix = "o27", pid = pid, caller = caller };

            RefreshStateTab1(v);
            return View(v);
        }
        private void RefreshStateTab1(o27Tab1 v)
        {
            v.Rec = Factory.o27AttachmentBL.Load(v.pid);
            if (v.Rec != null)
            {

                v.SetTagging();


            }
        }

        public IActionResult Record(int pid, bool isclone)
        {
            var v = new o27Record() { rec_pid = pid, rec_entity = "o27" };
            //v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "o27" };

            v.Rec = new BO.o27Attachment();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.o27AttachmentBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                if (v.Notepad != null)
                {
                    v.Notepad.HtmlContent = v.Rec.o27FullText;
                }
                
            }
            if (v.Rec.j02ID_Owner !=Factory.CurrentUser.pid && !Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_o27_Owner))
            {
                return this.StopPage(true, "Nemáte editační oprávnění k tomuto FILEBOX záznamu.");
            }
            if (v.Rec.o27Entity == "x31")
            {
                return this.StopPage(true, "Šablona pevné tiskové sestavy");
            }
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            v.Toolbar.AllowClone = false;

            if (isclone)
            {
                v.MakeClone();
            }
            return View(v);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(Models.Record.o27Record v)
        {

            if (ModelState.IsValid)
            {
                
                BO.o27Attachment c = Factory.o27AttachmentBL.Load(v.rec_pid);
                c.o27Name = v.Rec.o27Name;
                if (v.Notepad != null)
                {
                    c.o27FullText = v.Notepad.HtmlContent;
                }
                

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);


                if (v.PostbackOper == "vycistit-vazbu")
                {
                    c.o27Entity = null;
                    c.o27RecordPid = 0;
                }

                c.pid = Factory.o27AttachmentBL.Save(c);
                if (c.pid > 0)
                {

                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }

        public IActionResult MailBodyHtml(int pid)
        {
            var v = new BaseViewModel();
            var rec = Factory.o27AttachmentBL.Load(pid);
            ViewData["o27MailBodyHtml"] = rec.o27MailBodyHtml;
            return View(v);
        }

        public IActionResult Fronta(string prefix,int pid)
        {
            var v = new DropzoneFrontaViewModel() { prefix = prefix, pid = pid };

            v.lisO27 = Factory.o27AttachmentBL.GetList(new BO.myQueryO27() { mavazbu=false });
            return View(v);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Fronta(UI.Models.DropzoneFrontaViewModel v,int o27id)
        {

            if (ModelState.IsValid)
            {

                var rec = Factory.o27AttachmentBL.Load(o27id);
                rec.o27RecordPid = v.pid;
                rec.o27Entity = v.prefix;
                
                Factory.o27AttachmentBL.Save(rec);
                v.SetJavascript_CallOnLoad(0);
                return View(v);

            }


            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
