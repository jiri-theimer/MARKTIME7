using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;
using UI.Models.Admin;

namespace UI.Controllers
{
    public class p07Controller : BaseController
    {
        public IActionResult ProjectLevels()
        {
            var v = new p07ProjectLevelsViewModel();
            v.RecL1 = Factory.p07ProjectLevelBL.LoadByLevel(1);
            if (v.RecL1 == null) v.RecL1 = new BO.p07ProjectLevel() { p07Level = 1 };
            v.UseL1 = !v.RecL1.isclosed;

            v.RecL2 = Factory.p07ProjectLevelBL.LoadByLevel(2);
            if (v.RecL2 == null) v.RecL2 = new BO.p07ProjectLevel() { p07Level = 2 };
            v.UseL2 = !v.RecL2.isclosed;

            v.RecL3 = Factory.p07ProjectLevelBL.LoadByLevel(3);
            if (v.RecL3 == null) v.RecL3= new BO.p07ProjectLevel() { p07Level = 3 };
            v.UseL3 = !v.RecL3.isclosed;

            v.RecL4 = Factory.p07ProjectLevelBL.LoadByLevel(4);
            if (v.RecL4 == null) v.RecL4 = new BO.p07ProjectLevel() { p07Level = 4 };
            v.UseL4 = !v.RecL4.isclosed;

            v.RecL5 = Factory.p07ProjectLevelBL.LoadByLevel(5);
            if (v.RecL5 == null) v.RecL5 = new BO.p07ProjectLevel() { p07Level = 5 };
            v.UseL5 = true; //nejnižší úroveň je povinná


            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProjectLevels(p07ProjectLevelsViewModel v,string oper)
        {
           if (v.IsPostback)
            {
                
                return View(v);
            }
            if (!string.IsNullOrEmpty(oper))
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {
                bool b = true;
                
                if (!Handle_SaveLevel(1, v.UseL1, v.RecL1))
                {
                    b = false;
                }
                if (!Handle_SaveLevel(2, v.UseL2, v.RecL2))
                {
                    b = false;
                }
                if (!Handle_SaveLevel(3, v.UseL3, v.RecL3))
                {
                    b = false;
                }
                if (!Handle_SaveLevel(4, v.UseL4, v.RecL4))
                {
                    b = false;
                }
                if (!Handle_SaveLevel(5, true, v.RecL5))    //pátá úroveň je povinná
                {
                    b = false;
                }
                

                if (b)
                {
                    Factory.j02UserBL.ClearAllUsersCache();

                    v.SetJavascript_CallOnLoad(0);
                    return View(v);
                }
                

            }


            this.Notify_RecNotSaved();
            return View(v);
        }

        private bool Handle_SaveLevel(int levelindex,bool isuse,BO.p07ProjectLevel recsource)
        {
            var rec2save = new BO.p07ProjectLevel();
            if (recsource.pid > 0) rec2save = Factory.p07ProjectLevelBL.Load(recsource.pid);
            rec2save.p07Level = levelindex;

            if (isuse)    //uložení úrovně levelindex
            {
                rec2save.p07Name = recsource.p07Name;
                rec2save.p07NamePlural = recsource.p07NamePlural;
                rec2save.p07NameInflection = recsource.p07NameInflection;
                rec2save.ValidUntil = new DateTime(3000, 1, 1);
            }
            else
            {
                rec2save.ValidUntil = DateTime.Now.AddMinutes(-1);
            }

            rec2save.pid = Factory.p07ProjectLevelBL.Save(rec2save);
            if (rec2save.pid == 0)
            {                
                this.Notify_RecNotSaved();
                return false;
            }
            return true;
        }
       
    }
}
