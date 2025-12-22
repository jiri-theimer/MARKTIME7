using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;
using UI.Models.Tab1;
using System.Security.Cryptography.Xml;

namespace UI.Controllers
{
    public class p40Controller : BaseController
    {
        private readonly BL.Singleton.ThePeriodProvider _pp;
        public p40Controller(BL.Singleton.ThePeriodProvider pp)
        {
            _pp = pp;
        }

        public int GenerateClear(int p39id)
        {
            var rec = Factory.p40WorkSheet_RecurrenceBL.LoadP39(p39id);
            return Factory.p40WorkSheet_RecurrenceBL.Generate_Clear(rec);
        }
        public int GenerateManually(int p39id)
        {
            var rec = Factory.p40WorkSheet_RecurrenceBL.LoadP39(p39id);
            var recP40 = Factory.p40WorkSheet_RecurrenceBL.Load(rec.p40ID);
            if (!TestNeededPermissions(recP40.p41ID))
            {
                return -1;
            }
            return Factory.p40WorkSheet_RecurrenceBL.Generate_Recurrence_Instance(rec);
        }
        public IActionResult Info(int pid)
        {
            return Tab1(pid, "info");
        }
        public IActionResult Tab1(int pid, string caller)
        {
            var v = new p40Tab1() { Factory = this.Factory, pid = pid, caller = caller };
            v.PlusMinusp39Mesice = Factory.CBL.LoadUserParamInt("p40-PlusMinusp39Mesice", 6);
            v.Rec = Factory.p40WorkSheet_RecurrenceBL.Load(v.pid);
            if (v.Rec != null)
            {
               
                v.lisP39 = Factory.p40WorkSheet_RecurrenceBL.GetList_p39(v.pid,v.PlusMinusp39Mesice);
                v.RecP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID);

            }

            return View(v);
        }

        public IActionResult PostGenerate(string p40ids, string oper,int j72id)
        {
            if (!Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_p41_Owner) && !Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Owner))
            {
                return this.StopPage(true, "Nedisponujete oprávněním pro tuto funkci.");
            }
            var v = new p40PostGenerateViewModel() { p40ids = p40ids, oper = oper ,j72id=j72id};
            if (string.IsNullOrEmpty(v.p40ids))
            {
                return this.StopPage(true, "pids missing");
            }

            
            

            RefreshState_PostGenerate(v);
            return View(v);
        }
        private void RefreshState_PostGenerate(p40PostGenerateViewModel v)
        {
            v.periodinput = new Views.Shared.Components.myPeriod.myPeriodViewModel() { prefix = "p40", UserParamKey = "p40-postgenerate" };
            v.periodinput.LoadUserSetting(_pp, Factory);    //načtení d1/d2/period_field pro query


            v.gridinput = new Views.Shared.Components.myGrid.myGridInput() {j72id=v.j72id, is_enable_selecting = false, entity = "p40WorkSheet_Recurrence", master_entity = "inform", myqueryinline = $"pids|list_int|{v.p40ids}", oncmclick = "", ondblclick = "" };

            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load("p40", null, 0, $"pids|list_int|{v.p40ids}");
            

            v.lisP40 = Factory.p40WorkSheet_RecurrenceBL.GetList(new BO.myQueryP40() { pids = BO.Code.Bas.ConvertString2ListInt(v.p40ids) });
            if (v.periodinput.d1 == null)
            {
                v.periodinput.d1 = new DateTime(2000, 1, 1);
                v.periodinput.d2 = new DateTime(2100, 1, 1);
            }
            v.lisP39 = Factory.p40WorkSheet_RecurrenceBL.GetList_p39_waiting_on_generate(v.periodinput.d1.Value, v.periodinput.d2.Value,v.gridinput.query.pids);
        }
        [HttpPost]
        public IActionResult PostGenerate(p40PostGenerateViewModel v)
        {
            RefreshState_PostGenerate(v);
            if (v.IsPostback)
            {


                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (v.periodinput.d1==null || v.periodinput.d2==null)
                {
                    this.AddMessage("Chybí zadat období.");
                    return View(v);
                }
                if (v.lisP39.Count() == 0)
                {
                    this.AddMessage("Na vstupu chybí odměny k vygenerování.");
                    return View(v);
                }

                bool bolOK = false;
                foreach (var c in v.lisP39)
                {
                    if (Factory.p40WorkSheet_RecurrenceBL.Generate_Recurrence_Instance(c) > 0)
                    {
                        bolOK = true;
                    }

                }

                if (bolOK)
                {
                    v.SetJavascript_CallOnLoad(v.lisP40.First().pid);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }

        public IActionResult Record(int pid, bool isclone, int p41id,int p28id)
        {            
            if (p28id>0 && p41id == 0)
            {
                var lis = Factory.p41ProjectBL.GetList(new BO.myQueryP41("le5") { p28id = p28id });
                if (lis.Count()>0) p41id = lis.First().pid;
            }
            var v = new p40Record() { rec_pid = pid, rec_entity = "p40",ComboJ02=Factory.CurrentUser.FullnameDesc};
            v.ProjectCombo = new ProjectComboViewModel() { SelectedP41ID = p41id};
            v.Rec = new BO.p40WorkSheet_Recurrence() { j02ID=Factory.CurrentUser.pid,p40FirstSupplyDate=new DateTime(DateTime.Now.Year,DateTime.Now.Month,1).AddMonths(1).AddDays(-1), p40LastSupplyDate = new DateTime(DateTime.Now.Year+5, 12, 31),p40Name="Měsíční paušální odměna",p40Text="Měsíční paušál [YYYY]/[MM].",p40RecurrenceType=BO.Code.RecurrenceTypeENUM.Month };
           
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p40WorkSheet_RecurrenceBL.Load(v.rec_pid);
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
            }
            else
            {
                //nový záznam
                if (p41id > 0)
                {
                    v.Rec.p41ID = p41id;
                    
                }
                

                if (v.rec_pid == 0 && !isclone) //výchozí hodnoty nového kontaktu natáhnout z naposledy mnou vytvořeného
                {
                    var qry = Factory.p40WorkSheet_RecurrenceBL.GetList(new BO.myQueryP40() { j02id = Factory.CurrentUser.pid, TopRecordsOnly = 1, explicit_orderby = "a.p40ID DESC" });
                    if (qry.Count() > 0)
                    {
                        v.Rec.p34ID = qry.First().p34ID;
                        v.ComboP34 = qry.First().p34Name;
                        v.Rec.p32ID = qry.First().p32ID;
                        v.ComboP32 = qry.First().p32Name;
                        v.Rec.j27ID = qry.First().j27ID;
                        v.ComboJ27Code = qry.First().j27Code;
                        v.Rec.x15ID = qry.First().x15ID;
                        v.Rec.p40RecurrenceType = qry.First().p40RecurrenceType;
                        v.Rec.p40GenerateDayAfterSupply = qry.First().p40GenerateDayAfterSupply;
                        v.Rec.p40Text = qry.First().p40Text;
                    }

                }
                
                if (v.Rec.p34ID == 0)
                {
                    var lisP34 = Factory.p34ActivityGroupBL.GetList(new BO.myQueryP34() { ismoneyinput = true }).Where(p => p.p34IncomeStatementFlag == BO.p34IncomeStatementFlagENUM.Prijem);                    
                   
                    if (lisP34.Count() > 0)
                    {                        
                        v.Rec.p34ID = lisP34.First().pid;
                        v.ComboP34 = lisP34.First().p34Name;

                        var lisP32 = Factory.p32ActivityBL.GetList(new BO.myQueryP32() { p34id = v.Rec.p34ID });
                        if (lisP32.Any(p=>p.p32Name=="Opakovaná odměna"))
                        {
                            var recP32 = lisP32.Where(p => p.p32Name == "Opakovaná odměna").First();
                            v.Rec.p32ID = recP32.pid;
                            v.ComboP32 = recP32.p32Name;
                        }
                    }
                    
                }
                if (v.Rec.x15ID == BO.x15IdEnum.Nic)
                {
                    v.Rec.x15ID = Factory.Lic.x15ID;
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
                v.Rec.p40FirstSupplyDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                v.Rec.p40LastSupplyDate = new DateTime(DateTime.Now.Year + 5, 12, 31);
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

        

        private void RefreshState(p40Record v)
        {

            v.ProjectCombo.CssClassDiv = "col-sm-10 col-md-9";
            if (v.Rec.p34ID > 0)
            {
                var recP34 = Factory.p34ActivityGroupBL.Load(v.Rec.p34ID);
                switch (recP34.p33ID)
                {
                    case BO.p33IdENUM.Cas:
                        v.ValueLabel = Factory.tra("Hodiny");break;
                    case BO.p33IdENUM.Kusovnik:
                        v.ValueLabel = Factory.tra("Počet"); break;
                    default:
                        v.ValueLabel = Factory.tra("Částka bez DPH"); break;
                }
            }
            else
            {
                v.ValueLabel = Factory.tra("Hodnota úkonu");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p40Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
               

                return View(v);
            }

            if (ModelState.IsValid)
            {

                BO.p40WorkSheet_Recurrence c = new BO.p40WorkSheet_Recurrence();
                if (v.rec_pid > 0) c = Factory.p40WorkSheet_RecurrenceBL.Load(v.rec_pid);
                c.p40Name = v.Rec.p40Name;
                c.p41ID = v.ProjectCombo.SelectedP41ID;
                c.j02ID = v.Rec.j02ID;
                c.p34ID = v.Rec.p34ID;
                c.p32ID = v.Rec.p32ID;
                c.j27ID = v.Rec.j27ID;
                c.p40Text = v.Rec.p40Text;
                c.p40Value = v.Rec.p40Value;
                c.p40RecurrenceType = v.Rec.p40RecurrenceType;
                c.p40LastSupplyDate = v.Rec.p40LastSupplyDate;
                c.p40FirstSupplyDate = v.Rec.p40FirstSupplyDate;
                c.p40GenerateDayAfterSupply = v.Rec.p40GenerateDayAfterSupply;
                c.x15ID = v.Rec.x15ID;
                c.p40FreeFee = v.Rec.p40FreeFee;
                c.p40FreeHours = v.Rec.p40FreeHours;
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
                c.p40TextInternal = v.Rec.p40TextInternal;

                if (!TestNeededPermissions(c.p41ID))
                {
                    return this.StopPage(true, "Nedisponujete vlastnickým oprávněním k projektu.");
                }


                c.pid = Factory.p40WorkSheet_RecurrenceBL.Save(c);
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
