using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class p36Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p36Record() { rec_pid = pid, rec_entity = "p36" };
            v.Rec = new BO.p36LockPeriod();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p36LockPeriodBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.ComboPerson = v.Rec.Person;
                v.ComboTeam = v.Rec.j11Name;
                
                v.SelectedP34IDs = Factory.p34ActivityGroupBL.GetList(new BO.myQueryP34() { p36id = v.rec_pid }).Select(p => p.pid).ToList();

                if (v.Rec.j02ID==0 && v.Rec.j11ID == 0)
                {
                    v.ScopePrefix = "all";
                }
                else
                {
                    if (v.Rec.j02ID > 0)
                    {
                        v.ScopePrefix = "j02";
                    }
                    if (v.Rec.j11ID > 0)
                    {
                        v.ScopePrefix = "j11";
                    }
                }


            }
            else
            {
                v.ScopePrefix = "all";
            }
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec) { AllowArchive = false };
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(p36Record v)
        {
            v.lisAllP34 = Factory.p34ActivityGroupBL.GetList(new BO.myQueryP34());

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p36Record v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                BO.p36LockPeriod c = new BO.p36LockPeriod();
                if (v.rec_pid > 0) c = Factory.p36LockPeriodBL.Load(v.rec_pid);
                switch (v.ScopePrefix)
                {
                    case "j02":
                        c.j02ID = v.Rec.j02ID; c.p36IsAllPersons = false;c.j11ID = 0;
                        break;
                    case "j11":
                        c.j11ID = v.Rec.j11ID; c.p36IsAllPersons = false;c.j02ID = 0;
                        break;
                    case "all":
                        c.p36IsAllPersons = true;c.j02ID = 0;c.j11ID = 0;
                        break;
                }
                                
                c.p36IsAllSheets = v.Rec.p36IsAllSheets;
                c.p36DateFrom = v.Rec.p36DateFrom;
                c.p36DateUntil = v.Rec.p36DateUntil;
                
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
               
                if (v.SelectedP34IDs != null)
                {
                    c.pid = Factory.p36LockPeriodBL.Save(c, v.SelectedP34IDs.Where(p => p > 0).ToList());
                }
                else
                {
                    c.pid = Factory.p36LockPeriodBL.Save(c, new List<int>());
                }
                
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
