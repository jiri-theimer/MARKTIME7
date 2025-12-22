using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;

namespace UI.Controllers.Admin
{
    public class o17Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new o17Record() { rec_pid = pid, rec_entity = "o17" };
            v.roles = new RoleAssignViewModel() { RecPid = v.rec_pid, RecPrefix = "o17", RolePrefix = "o17", Header = "Přístup uživatelů k Menu" };
            v.Rec = new BO.o17DocMenu();
            var lisX67 = Factory.x67EntityRoleBL.GetList(new BO.myQuery("x67")).Where(p => p.x67Entity == "o17");
            if (lisX67.Count() == 0)
            {
                //vytvořit roli pro menu                
                Factory.x67EntityRoleBL.Save(new BO.x67EntityRole() { x67Entity = "o17", x67Name = "Přístup" }, null);
            }
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.o17DocMenuBL.Load(v.rec_pid);
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

        private void RefreshState(o17Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(o17Record v)
        {
            RefreshState(v);

            if (ModelState.IsValid)
            {
                BO.o17DocMenu c = new BO.o17DocMenu();
                if (v.rec_pid > 0) c = Factory.o17DocMenuBL.Load(v.rec_pid);
                c.o17Name = v.Rec.o17Name;                
                c.o17Ordinary = v.Rec.o17Ordinary;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.o17DocMenuBL.Save(c, v.roles.getList4Save(Factory));
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

