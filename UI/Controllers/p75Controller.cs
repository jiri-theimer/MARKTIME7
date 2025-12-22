using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class p75Controller : BaseController
    {
        public IActionResult Info(int pid)
        {
            return View(new BaseTab1ViewModel() { pid = pid, prefix = "p75" });
        }
        public IActionResult Tab1(int pid, string caller)
        {
            var v = new p75Tab1() { Factory = this.Factory, prefix = "p75", pid = pid, caller = caller };

            RefreshStateTab1(v);
            return View(v);
        }
        private void RefreshStateTab1(p75Tab1 v)
        {
            v.Rec = Factory.p75InvoiceRecurrenceBL.Load(v.pid);
            if (v.Rec != null)
            {
                v.lisP76 = Factory.p75InvoiceRecurrenceBL.GetList_p76(v.Rec.pid);

                if (v.Rec.p41ID > 0)
                {
                    v.RecP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID);
                }
                if (v.Rec.p28ID > 0)
                {
                    v.RecP28 = Factory.p28ContactBL.Load(v.Rec.p28ID);
                }
            }


        }

        public BO.Result TryGenerate_Recurrence_Instance(int p75id, int p76id)
        {
            var ret = new BO.Result(false);

            int intP91ID = Factory.p75InvoiceRecurrenceBL.Generate_Recurrence_Instance(p75id, p76id);
            if (intP91ID == 0)
            {
                return new BO.Result(true, Factory.GetFirstNotifyMessage());
            }
            return new BO.Result(false);
        }


        public IActionResult Record(int pid, bool isclone,int p41id,int p28id)
        {

            var v = new p75Record() { rec_pid = pid, rec_entity = "p75"};
            DateTime d0 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
            v.Rec = new BO.p75InvoiceRecurrence() {p75Generate_DaysToBase_D=-1, p75DateMaturityDaysAfter=10, p41ID = p41id,p28ID=p28id, p75BaseDateStart = d0, p75RecurrenceType = BO.Code.RecurrenceTypeENUM.Month,p75BaseDateEnd=d0.AddYears(2) };
            v.Rec.p75InvoiceText = "Měsíční vyúčtování [YYYY]/[MM]";
            

            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p75InvoiceRecurrenceBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                
                
                if (v.Rec.p28ID > 0)
                {
                    v.ComboP28 = Factory.p28ContactBL.Load(v.Rec.p28ID).p28Name;
                }
                
                v.ComboOwner = v.Rec.Owner;
                
                if (v.Rec.p41ID > 0)
                {
                    v.RecP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID);
                    
                }
                


            }
            else
            {
                v.ComboOwner = Factory.CurrentUser.FullnameDesc;

            }

            RefreshStateRecord(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();

            }

            return ViewTup(v,BO.PermValEnum.GR_P91_Owner);
        }

        private void RefreshStateRecord(p75Record v)
        {
            if (v.Rec.p41ID > 0 && v.RecP41 == null)
            {
                v.RecP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID);
                
            }
            if (v.Rec.p28ID > 0 && v.RecP28 == null)
            {
                v.RecP28 = Factory.p28ContactBL.Load(v.Rec.p28ID);
                v.ComboP28 = v.RecP28.p28Name;
            }
            if (v.ProjectCombo == null)
            {
                v.ProjectCombo = new ProjectComboViewModel() { SelectedP41ID = v.Rec.p41ID };
                if (v.RecP41 !=null)
                {
                    v.ProjectCombo.SelectedProject = v.RecP41.FullName;
                }
            }



            if (v.rec_pid == 0 && v.Rec.j02ID_Owner == 0)
            {
                v.Rec.j02ID_Owner = Factory.CurrentUser.pid;
                v.ComboOwner = Factory.CurrentUser.FullnameDesc;
            }

            

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p75Record v)
        {
            RefreshStateRecord(v);

            if (v.IsPostback)
            {
                
               


                return View(v);
            }


            if (ModelState.IsValid)
            {
                BO.p75InvoiceRecurrence c = new BO.p75InvoiceRecurrence();
                if (v.rec_pid > 0) c = Factory.p75InvoiceRecurrenceBL.Load(v.rec_pid);
                
                c.p41ID = v.ProjectCombo.SelectedP41ID;
                c.p28ID = v.Rec.p28ID;

                c.p75Name = v.Rec.p75Name;
                c.p75RecurrenceType = v.Rec.p75RecurrenceType;
                c.p75BaseDateStart = v.Rec.p75BaseDateStart;
                c.p75BaseDateEnd = v.Rec.p75BaseDateEnd;
                c.p75Generate_DaysToBase_D = v.Rec.p75Generate_DaysToBase_D;
                
                c.p75DateSupplyFlag = v.Rec.p75DateSupplyFlag;
                c.p75IsDraft = v.Rec.p75IsDraft;

                c.p75DateMaturityDaysAfter = v.Rec.p75DateMaturityDaysAfter;
                c.p75InvoiceText = v.Rec.p75InvoiceText;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.p75PeriodFlag = v.Rec.p75PeriodFlag;

                c.pid = Factory.p75InvoiceRecurrenceBL.Save(c);
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
