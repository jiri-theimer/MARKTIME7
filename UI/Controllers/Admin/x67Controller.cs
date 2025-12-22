using Microsoft.AspNetCore.Mvc;

using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class x67Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone,string prefix)
        {
            var v = new x67Record() { rec_pid = pid, rec_entity = "x67",Entity=prefix };            
            v.Rec = new BO.x67EntityRole();
            
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.x67EntityRoleBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.Entity = v.Rec.x67Entity;
               
                v.SelectedPermNumbers = Factory.x67EntityRoleBL.GetList_BoundPerms(v.rec_pid).Select(p => p.ValueInt).ToList();
                
                if (v.SelectedPermNumbers.Count()==0 && v.Rec.x67RoleValue.Contains("1"))
                {
                    //rekonstrukce tabulky x68EntityRole_Permission
                    Factory.x67EntityRoleBL.Recovery_x68(v.Rec);                  
                    v.SelectedPermNumbers = Factory.x67EntityRoleBL.GetList_BoundPerms(v.rec_pid).Select(p => p.ValueInt).ToList();
                }



            }
            if (v.Entity == null)
            {
                return this.StopPage(true, "prefix missing");
            }
            
            RefreshState(v);
            if (v.Entity == "p41")
            {
                SetupO28List(v);
            }
            

            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(x67Record v)
        {
            
            v.lisAllPermissions = BO.Code.Entity.GetAllPermissions(v.Entity);
           
            
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(x67Record v)
        {
            RefreshState(v);

            if (ModelState.IsValid)
            {
                BO.x67EntityRole c = new BO.x67EntityRole();
                if (v.rec_pid > 0) c = Factory.x67EntityRoleBL.Load(v.rec_pid);
                c.x67Entity = v.Entity;
                c.x67Name = v.Rec.x67Name;
                c.x67Ordinary = v.Rec.x67Ordinary;
                c.a55ID = v.Rec.a55ID;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
                c.x67IsRequired = v.Rec.x67IsRequired;

                c.pid = Factory.x67EntityRoleBL.Save(c,v.SelectedPermNumbers);
                if (c.pid > 0)
                {
                    if (c.x67Entity == "p41")
                    {
                        Factory.x67EntityRoleBL.SaveO28(c.pid, v.lisO28);
                    }
                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }
                
            }


            this.Notify_RecNotSaved();
            return View(v);
        }


        private void SetupO28List(x67Record v)
        {
            v.lisO28 = new List<BO.o28ProjectRole_Workload>();
            var lisP34 = Factory.p34ActivityGroupBL.GetList(new BO.myQueryP34());

            foreach (var recP34 in lisP34)
            {
                var c = new BO.o28ProjectRole_Workload() { p34ID = recP34.pid, p34Name = recP34.p34Name };

                v.lisO28.Add(c);
            }

            if (v.rec_pid > 0)
            {
                var lisO28 = Factory.x67EntityRoleBL.GetListO28(v.rec_pid);
                foreach (var c in lisO28)
                {
                    if (v.lisO28.Where(p => p.p34ID == c.p34ID).Any())
                    {
                        var rec = v.lisO28.Where(p => p.p34ID == c.p34ID).First();

                        rec.o28EntryFlag = c.o28EntryFlag;
                        rec.o28PermFlag = c.o28PermFlag;
                    }
                }
            }

        }

    }
}
