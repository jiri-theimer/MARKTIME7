using BL;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class p42Controller : BaseController
    {
        public IActionResult Info(int pid)
        {
            var v = new BaseTab1ViewModel() { prefix = "p42", pid = pid };
            return View(v);
        }
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p42Record() { rec_pid = pid, rec_entity = "p42" };
            v.Rec = new BO.p42ProjectType() {p42RolesTab=3,p42ClientTab=3,p42BillingTab=3, p42BudgetTab=1 };
                  
            v.creates = new CreateAssignViewModel() { j08RecordEntity = "p42", j08RecordPid = v.rec_pid };
            v.lisP43 = new List<p43Repeater>();
            var lisP34 = Factory.p34ActivityGroupBL.GetList(new BO.myQueryP34() { IsRecordValid = true });
            foreach(var c in lisP34)
            {
                v.lisP43.Add(new p43Repeater() { p34ID = c.pid, p34Name = c.p34Name });
            }
            if (v.rec_pid==0)
            {
                var lisX38 = Factory.x38CodeLogicBL.GetList(new BO.myQuery("x38")).Where(p => p.x38Entity == "p41");
                if (lisX38.Count() > 0)
                {
                    v.ComboX38Name = lisX38.First().x38Name;
                    v.Rec.x38ID = lisX38.First().pid;
                }
                if (lisP34.Count() <= 6)
                {
                    foreach (var c in v.lisP43.Where(p=>!p.p34Name.Contains("Interní")))
                    {
                        c.IsSelected = true;
                    }
                }
                
            }

            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p42ProjectTypeBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.ComboB01Name = v.Rec.b01Name;
                
                v.ComboX38Name = v.Rec.x38Name;
                if (v.Rec.p61ID > 0)
                {
                    v.ComboP61Name = Factory.p61ActivityClusterBL.Load(v.Rec.p61ID).p61Name;
                }
                
                v.creates.SetInitValues(Factory, "p42", v.rec_pid);

                var saved = Factory.p42ProjectTypeBL.GetList_p43(v.rec_pid);
                foreach(var c in saved)
                {
                    if (v.lisP43.Any(p => p.p34ID == c.p34ID))
                    {
                        var rec = v.lisP43.First(p => p.p34ID == c.p34ID);
                        rec.IsSelected = true;
                        
                    }
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

        private void RefreshState(p42Record v)
        {
            
            v.lisAllP34 = Factory.p34ActivityGroupBL.GetList(new BO.myQueryP34());

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p42Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                BO.p42ProjectType c = new BO.p42ProjectType();
                if (v.rec_pid > 0) c = Factory.p42ProjectTypeBL.Load(v.rec_pid);
                c.b01ID = v.Rec.b01ID;
                c.x38ID = v.Rec.x38ID;
                
                c.p61ID = v.Rec.p61ID;

                c.p42Ordinary = v.Rec.p42Ordinary;
                c.p42Name = v.Rec.p42Name;
                c.p42Code = v.Rec.p42Code;
                c.p42ArchiveFlag = v.Rec.p42ArchiveFlag;
                c.p42ArchiveFlagP31 = v.Rec.p42ArchiveFlagP31;

               
                c.p42FilesTab = v.Rec.p42FilesTab;
                c.p42BillingTab = v.Rec.p42BillingTab;
                c.p42RolesTab = v.Rec.p42RolesTab;
                c.p42ClientTab = v.Rec.p42ClientTab;
                c.p42BudgetTab = v.Rec.p42BudgetTab;
                c.p42ContactsTab = v.Rec.p42ContactsTab;
                c.p42IsP54 = v.Rec.p42IsP54;
                c.p42BillingFlag = v.Rec.p42BillingFlag;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                var lisP43 = new List<BO.p43ProjectType_Workload>();
                foreach(var rec in v.lisP43.Where(p => p.IsSelected == true))
                {
                    lisP43.Add(new BO.p43ProjectType_Workload() { p34ID = rec.p34ID });
                }

                c.pid = Factory.p42ProjectTypeBL.Save(c, lisP43, v.creates.getList4Save(Factory));
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
