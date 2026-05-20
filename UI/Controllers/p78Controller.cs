using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class p78Controller : BaseController
    {
        public IActionResult Info(int pid)
        {
            return Tab1(pid, "info");
        }
        public IActionResult Tab1(int pid, string caller)
        {
            var v = new p78Tab1() { Factory = this.Factory, pid = pid, caller = caller };
            v.Rec = Factory.p78UpominkaSdruzenaBL.Load(v.pid);
            if (v.Rec != null)
            {
                v.SetTagging();
                
                v.SetFreeFields(0);
            }
            return View(v);
        }
     

        

      
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p78Record() { rec_pid = pid, rec_entity = "p78" };
            v.Rec = new BO.p78UpominkaSdruzena();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p78UpominkaSdruzenaBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                if (v.Rec.j02ID_Owner > 0)
                {
                    v.ComboOwner = Factory.j02UserBL.Load(v.Rec.j02ID_Owner).FullnameDesc;

                }
                if (v.Rec.p28ID > 0)
                {
                    v.ComboP28Name = v.Rec.p28Name;
                }
                if (v.Rec.j27ID > 0)
                {
                    v.ComboJ27Code = Factory.FBL.LoadCurrencyByID(v.Rec.j27ID).j27Code;
                }

                var lisP84 = Factory.p84UpominkaBL.GetList(new BO.myQueryP84() { p78id = v.rec_pid });
                if (lisP84.Count() > 0)
                {
                    v.SelectedP84IDs = lisP84.Select(p => p.pid).ToList();
                }
            }
            else
            {
                v.ComboOwner = Factory.CurrentUser.FullnameDesc;
            }
            RefreshState_Record(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec) { AllowClone = false, AllowArchive = true };
            if (isclone)
            {
                v.MakeClone();
            }

            return View(v);
        }

        private void RefreshState_Record(p78Record v)
        {
           
            if (v.reminder == null)
            {
                v.reminder = new ReminderViewModel() { is_static_date = true, record_pid = v.rec_pid, record_prefix = "p78" };
            }
            if (v.Rec.p28ID > 0)
            {
                v.lisAllP84 = Factory.p84UpominkaBL.GetList(new BO.myQueryP84() { p28id = v.Rec.p28ID });
            }
            else
            {
                v.lisAllP84 = Factory.p84UpominkaBL.GetList(new BO.myQueryP84() { p28id = 999988888 }); //prázdný seznam upomínek k zaškrtnutí
            }
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p78Record v)
        {
            RefreshState_Record(v);

            if (v.IsPostback)
            {
               
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (v.reminder.lisReminder != null && v.reminder.lisReminder.Where(p => p.IsTempDeleted == false && p.o24StaticDate == null).Count() > 0)
                {
                    this.AddMessage("V upozornění chybí vyplnit datum+čas."); return View(v);
                }

                BO.p78UpominkaSdruzena c = new BO.p78UpominkaSdruzena();
                if (v.rec_pid > 0) c = Factory.p78UpominkaSdruzenaBL.Load(v.rec_pid);
                c.p78Name = v.Rec.p78Name;
                c.j27ID = v.Rec.j27ID;
                
                c.p78Code = v.Rec.p78Code;
                c.p78TextA = v.Rec.p78TextA;
                c.p78TextB = v.Rec.p78TextB;
                c.p78Date = v.Rec.p78Date;
                
                c.j02ID_Owner = v.Rec.j02ID_Owner;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.p78Client = v.Rec.p78Client;
                c.p78Client_ICDPH_SK = v.Rec.p78Client_ICDPH_SK;
                c.p78Client_RegID = v.Rec.p78Client_RegID;
                c.p78Client_VatID = v.Rec.p78Client_VatID;
                c.p78ClientAddress1_City = v.Rec.p78ClientAddress1_City;
                c.p78ClientAddress1_Country = v.Rec.p78ClientAddress1_Country;
                c.p78ClientAddress1_Before = v.Rec.p78ClientAddress1_Before;
                c.p78ClientAddress1_Street = v.Rec.p78ClientAddress1_Street;

                List<int> p84ids = null;
                if (v.SelectedP84IDs != null)
                {
                    p84ids=v.SelectedP84IDs.Where(p => p > 0).ToList();
                }

                c.pid = Factory.p78UpominkaSdruzenaBL.Save(c,p84ids);
                if (c.pid > 0)
                {

                    if (v.reminder != null)
                    {
                        v.reminder.SaveChanges(Factory, c.pid);
                    }

                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
