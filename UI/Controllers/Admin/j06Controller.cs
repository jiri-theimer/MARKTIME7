using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;

namespace UI.Controllers.Admin
{
    public class j06Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new j06Record() { rec_pid = pid, rec_entity = "j06",d2=DateTime.Today,Volba="j07" };
            v.Rec = new BO.j06UserHistory();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.j06UserHistoryBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                if (v.Rec.j07ID > 0) v.Volba = "j07";
                if (v.Rec.c21ID > 0) v.Volba = "c21";

                v.ComboC21 = v.Rec.c21Name;
                v.ComboJ07 = v.Rec.j07Name;
                v.ComboJ02 = Factory.j02UserBL.Load(v.Rec.j02ID).FullnameDesc;

                v.d1 = v.Rec.ValidFrom;
                v.d2 = v.Rec.ValidUntil;
            }
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(j06Record v)
        {
            

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(j06Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.j06UserHistory c = new BO.j06UserHistory();
                if (v.rec_pid > 0) c = Factory.j06UserHistoryBL.Load(v.rec_pid);
                c.j02ID = v.Rec.j02ID;
                c.j07ID = v.Rec.j07ID;
                c.c21ID = v.Rec.c21ID;
                
                if (v.d2 == null)
                {
                    v.d2 = DateTime.Today;
                }
                if (v.d1 == null)
                {
                    v.d1 = DateTime.Today.AddYears(-1);
                }

                c.pid = Factory.j06UserHistoryBL.Save(c, Convert.ToDateTime(v.d1), Convert.ToDateTime(v.d2));
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
