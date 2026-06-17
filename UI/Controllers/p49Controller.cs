using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class p49Controller : BaseController
    {

        public IActionResult Info(int pid)
        {
            return Tab1(pid, "info");
        }
        public IActionResult Tab1(int pid, string caller)
        {
            var v = new p49Tab1() { Factory = this.Factory, pid = pid, caller = caller };
           
            v.Rec = Factory.p49FinancialPlanBL.Load(v.pid);
            if (v.Rec != null)
            {
                
                v.RecP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID);

            }

            return View(v);
        }

        public IActionResult Record(int pid, bool isclone, int p41id,int p56id)
        {            
            var v = new p49Record() { rec_pid = pid, rec_entity = "p49", ComboJ02 = Factory.CurrentUser.FullnameDesc };
            if (p56id > 0 && pid == 0)
            {
                var recP56=Factory.p56TaskBL.Load(p56id);
                p41id = recP56.p41ID;v.ComboP56 = recP56.p56Name;
            }
            v.ProjectCombo = new ProjectComboViewModel() { SelectedP41ID = p41id };
            v.Rec = new BO.p49FinancialPlan() { j02ID = Factory.CurrentUser.pid,p56ID=p56id,p41ID=p41id, p49Date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) };

            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p49FinancialPlanBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.ComboJ02 = v.Rec.j02Name;
                v.ComboP32 = v.Rec.p32Name;
                v.ComboP34 = v.Rec.p34Name;
                v.ComboJ27Code = v.Rec.j27Code;
                BO.p41Project recP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID);
                v.ProjectCombo.SelectedP41ID = v.Rec.p41ID;
                if (v.Rec.p28ID_Supplier > 0)
                {
                    v.ComboSupplier = Factory.p28ContactBL.Load(v.Rec.p28ID_Supplier).p28Name;
                }
                if (v.Rec.p56ID > 0)
                {                    
                    v.ComboP56 = v.Rec.p56Name;
                }
                
            }
            else
            {
                //nový záznam                
               
                if (v.rec_pid == 0 && !isclone) //výchozí hodnoty nového kontaktu natáhnout z naposledy mnou vytvořeného
                {                   
                    var qry = Factory.p49FinancialPlanBL.GetList(new BO.myQueryP49() { j02id = Factory.CurrentUser.pid, TopRecordsOnly = 1, explicit_orderby = "a.p49ID DESC" });
                    if (qry.Count() > 0)
                    {
                        v.Rec.p34ID = qry.First().p34ID;
                        v.ComboP34 = qry.First().p34Name;
                        v.Rec.p32ID = qry.First().p32ID;
                        v.ComboP32 = qry.First().p32Name;
                        v.Rec.j27ID = qry.First().j27ID;
                        v.ComboJ27Code = qry.First().j27Code;
                        
                        v.Rec.p49Text = qry.First().p49Text;
                    }

                }

                
               
                if (v.Rec.j27ID == 0)
                {
                    v.Rec.j27ID = Factory.Lic.j27ID;
                    v.ComboJ27Code = Factory.FBL.LoadCurrencyByID(v.Rec.j27ID).j27Code;
                }

            }

            if (!TestNeededPermissions(v.Rec.p41ID))
            {
                return this.StopPage(true, "Nedisponujete vlastnickým oprávněním k projektu.");
            }


            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return View(v);
        }

        private bool TestNeededPermissions(int p41id)
        {
            if (p41id == 0) return true;
            var mydisp = Factory.p41ProjectBL.InhaleRecDisposition(p41id);
            return mydisp.OwnerAccess;
        }



        private void RefreshState(p49Record v)
        {

            v.ProjectCombo.CssClassDiv = "col-sm-10 col-md-9";
            if (v.Rec.p34ID > 0)
            {
                v.RecP34 = Factory.p34ActivityGroupBL.Load(v.Rec.p34ID);
            }
            if (v.ProjectCombo.SelectedP41ID > 0)
            {
                v.lisP56 = Factory.p56TaskBL.GetList(new BO.myQueryP56() {p41id=v.ProjectCombo.SelectedP41ID, IsRecordValid = null });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p49Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper == "p34id")
                {
                    v.Rec.p32ID = 0;
                    v.ComboP32 = null;
                }
                if (v.PostbackOper == "m+1")
                {
                    v.Rec.p49Date=v.Rec.p49Date.Value.AddMonths(1);
                }
                if (v.PostbackOper == "m-1")
                {
                    v.Rec.p49Date=v.Rec.p49Date.Value.AddMonths(-1);
                }
                if (v.PostbackOper == "y+1")
                {
                    v.Rec.p49Date = v.Rec.p49Date.Value.AddYears(1);
                }
                if (v.PostbackOper == "y-1")
                {
                    v.Rec.p49Date = v.Rec.p49Date.Value.AddYears(-1);
                }

                return View(v);
            }

            if (ModelState.IsValid)
            {

                BO.p49FinancialPlan c = new BO.p49FinancialPlan();
                if (v.rec_pid > 0) c = Factory.p49FinancialPlanBL.Load(v.rec_pid);
                
                c.p41ID = v.ProjectCombo.SelectedP41ID;
                c.j02ID = v.Rec.j02ID;
                c.p56ID = v.Rec.p56ID;
                c.p34ID = v.Rec.p34ID;
                c.p32ID = v.Rec.p32ID;
                c.j27ID = v.Rec.j27ID;
                c.p49Text = v.Rec.p49Text;
                c.p49Amount = v.Rec.p49Amount;
                c.p49Date = v.Rec.p49Date;
               
                c.p28ID_Supplier = v.Rec.p28ID_Supplier;
                c.p49PieceAmount = v.Rec.p49PieceAmount;
                c.p49Pieces = v.Rec.p49Pieces;
                c.p49MarginHidden = v.Rec.p49MarginHidden;
                c.p49MarginTransparent = v.Rec.p49MarginTransparent;
                c.p49StatusFlag = v.Rec.p49StatusFlag;


                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);


                if (!TestNeededPermissions(c.p41ID))
                {
                    return this.StopPage(true, "Nedisponujete vlastnickým oprávněním k projektu.");
                }


                c.pid = Factory.p49FinancialPlanBL.Save(c);
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
