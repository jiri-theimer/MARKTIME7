using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;

namespace UI.Controllers
{
    public class j79Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new j79Record() { rec_pid = pid, rec_entity = "j79" };
            v.Rec = Factory.j79TotalsTemplateBL.Load(v.rec_pid);
            if (v.Rec == null)
            {
                return RecNotFound(v);
            }

            if (v.Rec.j02ID != Factory.CurrentUser.pid && !Factory.CurrentUser.IsAdmin)
            {
                return this.StopPage(true, "Upravit hlavičku šablony může pouze vlastník záznamu nebo admin.");
            }
            

            if (v.Rec.j02ID > 0)
            {
                v.ComboPerson = Factory.j02UserBL.Load(v.Rec.j02ID).FullnameDesc;
            }

            if (!v.Rec.j79IsSystem && v.Rec.pid>0)
            {
                var lis = Factory.j04UserRoleBL.GetList(new BO.myQueryJ04() { j79id = v.Rec.pid });
                v.j04IDs = string.Join(",", lis.Select(p => p.pid));
                v.j04Names = string.Join(",", lis.Select(p => p.j04Name));
            }
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            v.Toolbar.AllowArchive = false;
            v.Toolbar.AllowClone = false;

            if (isclone)
            {
                v.MakeClone();
            }
            return View(v);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(j79Record v)
        {
            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {

                var c = Factory.j79TotalsTemplateBL.Load(v.rec_pid);
                c.j02ID = v.Rec.j02ID;
                c.j79IsPublic = v.Rec.j79IsPublic;
                c.j79Name = v.Rec.j79Name;
                c.j79Ordinary = v.Rec.j79Ordinary;

                List<int> j04ids = BO.Code.Bas.ConvertString2ListInt(v.j04IDs);
                List<int> j11ids = BO.Code.Bas.ConvertString2ListInt(v.j11IDs);
                
                c.pid = Factory.j79TotalsTemplateBL.Save(c, j04ids, j11ids);
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
