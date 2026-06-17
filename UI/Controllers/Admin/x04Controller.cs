using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;


namespace UI.Controllers.Admin
{
    public class x04Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new x04Record() { rec_pid = pid, rec_entity = "x04" };
            
            v.Rec = new BO.x04NotepadConfig() { x04FileAllowedTypes = "pdf,docx,doc,xlsx,xls,txt,csv", x04FileMaxSize = 2097152, x04ImageMaxSize = 1048576,x04ImageAllowedTypes="png,jpg,jpeg,gif,bmp",x04PlaceHolder= Factory.tra("Takhle vypadá nakonfigurovaný Notepad editor...") };
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.x04NotepadConfigBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }


            }

            v.Notepad = new Models.Notepad.EditorViewModel();

            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(x04Record v)
        {

            v.Notepad.ExternalConfig = new BO.x04NotepadConfig() { x04ToolbarButtons = v.Rec.x04ToolbarButtons, x04IsToolbarInline = v.Rec.x04IsToolbarInline, x04IsToolbarSticky = v.Rec.x04IsToolbarSticky,x04IsTrackChanges=v.Rec.x04IsTrackChanges,x04PlaceHolder=v.Rec.x04PlaceHolder };

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(x04Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {

                
                BO.x04NotepadConfig c = new BO.x04NotepadConfig();
                if (v.rec_pid > 0) c = Factory.x04NotepadConfigBL.Load(v.rec_pid);
                c.x04Name = v.Rec.x04Name;
                c.x04Ordinary = v.Rec.x04Ordinary;
                c.x04ToolbarButtons = v.Rec.x04ToolbarButtons;
                c.x04ToolbarButtonsXS = v.Rec.x04ToolbarButtonsXS;
                c.x04IsToolbarInline = v.Rec.x04IsToolbarInline;
                c.x04IsToolbarSticky = v.Rec.x04IsToolbarSticky;
                c.x04IsTrackChanges = v.Rec.x04IsTrackChanges;
                c.x04ImageMaxSize = v.Rec.x04ImageMaxSize;
                c.x04FileMaxSize = v.Rec.x04FileMaxSize;
                c.x04FileAllowedTypes = v.Rec.x04FileAllowedTypes;
                c.x04ImageAllowedTypes = v.Rec.x04ImageAllowedTypes;
                c.x04PlaceHolder = v.Rec.x04PlaceHolder;
                c.x04InlineClasses = v.Rec.x04InlineClasses;
                c.x04PlaceHolder = v.Rec.x04PlaceHolder;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.x04NotepadConfigBL.Save(c);
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
