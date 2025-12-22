using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class p84Controller : BaseController
    {
        public IActionResult Info(int pid)
        {
            return Tab1(pid, "info");
        }
        public IActionResult Tab1(int pid, string caller)
        {
            var v = new p84Tab1() { Factory = this.Factory, pid = pid, caller = caller };
            v.Rec = Factory.p84UpominkaBL.Load(v.pid);
            if (v.Rec != null)
            {
                v.SetTagging();
                v.RecP91 = Factory.p91InvoiceBL.Load(v.Rec.p91ID);
                v.RecP83 = Factory.p83UpominkaTypeBL.Load(v.Rec.p83ID);
                v.SetFreeFields(0);
            }
            return View(v);
        }
        public IActionResult Create(string p91ids, int p83id,string guid_pids)
        {
            var v = new p84CreateViewModel() { p91ids=p91ids };
            if (!string.IsNullOrEmpty(guid_pids))
            {
                p91ids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            RefreshState_Create(v);
            
            return View(v);
        }

        private void RefreshState_Create(p84CreateViewModel v)
        {
            var pids = BO.Code.Bas.ConvertString2ListInt(v.p91ids);
            v.lisP91 = Factory.p91InvoiceBL.GetList(new BO.myQueryP91() { pids = pids });
            v.lisP91 = v.lisP91.Where(p => p.p91IsDraft == false && p.p91Amount_Debt > 1);
                
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(p84CreateViewModel v)
        {
            RefreshState_Create(v);

            if (ModelState.IsValid)
            {
                if (v.IsPostback)
                {
                    return View(v);
                }
                v.RetErrors = new List<BO.StringPair>();
                var oks = new List<string>();
                foreach (var c in v.lisP91)
                {
                    if (Factory.CurrentUser.Messages4Notify != null)
                    {
                        Factory.CurrentUser.Messages4Notify.Clear();
                    }
                    
                    int intRet = Factory.p84UpominkaBL.TryCreate(c.pid);   
                    if (intRet == 0)
                    {
                        v.RetErrors.Add(new BO.StringPair() { Key = c.p91Code, Value = Factory.CurrentUser.GetLastMessageNotify() });
                    }
                    else
                    {
                        oks.Add(c.p91Code);
                    }
                    
                }
                if (v.RetErrors.Count() == 0)
                {
                    v.SetJavascript_CallOnLoad(v.lisP91.First().pid);
                    return View(v);
                }
                else
                {
                    Factory.CurrentUser.Messages4Notify.Clear();
                   
                    foreach (var c in oks)
                    {
                        this.AddMessageTranslated($"{c}: Upomínka vytvořena.","info");
                    }
                    RefreshState_Create(v);
                }
                
                
            }


            this.Notify_RecNotSaved();
            return View(v);
        }

        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p84Record() { rec_pid = pid, rec_entity = "p84" };
            v.Rec = new BO.p84Upominka();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p84UpominkaBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                if (v.Rec.j02ID_Owner > 0)
                {
                    v.ComboOwner = Factory.j02UserBL.Load(v.Rec.j02ID_Owner).FullnameDesc;
                    
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

        private void RefreshState_Record(p84Record v)
        {
            v.RecP91 = Factory.p91InvoiceBL.Load(v.Rec.p91ID);

            if (v.reminder == null)
            {
                v.reminder = new ReminderViewModel() { is_static_date = true, record_pid = v.rec_pid, record_prefix = "p84" };
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p84Record v)
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

                BO.p84Upominka c = new BO.p84Upominka();
                if (v.rec_pid > 0) c = Factory.p84UpominkaBL.Load(v.rec_pid);
                c.p84Name = v.Rec.p84Name;
                c.p83ID = v.Rec.p83ID;
                c.p84Code = v.Rec.p84Code;
                c.p84TextA = v.Rec.p84TextA;
                c.p84TextB = v.Rec.p84TextB;
                c.p84Date = v.Rec.p84Date;
                c.p84Index = v.Rec.p84Index;
                c.j02ID_Owner = v.Rec.j02ID_Owner;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p84UpominkaBL.Save(c);
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
