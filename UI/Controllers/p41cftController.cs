using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.p41oper;

namespace UI.Controllers
{
    public class p41cftController : BaseController
    {
        public IActionResult Index(int p44id)
        {
            var v = new p41cftViewModel() { p44ID = p44id,TempGuid=BO.Code.Bas.GetGuid() };
            v.RecP44 = Factory.p44ProjectTemplateBL.Load(v.p44ID);
            v.p41ID = v.RecP44.p41ID_Pattern;
            v.RecP41 = Factory.p41ProjectBL.Load(v.p41ID);

            v.p41Name = v.RecP41.p41Name;
            v.p41NameShort = v.RecP41.p41NameShort;

            v.p41BillingFlag = v.RecP41.p41BillingFlag;
            switch (v.p41BillingFlag)
            {
                case BO.p41BillingFlagEnum.CenikPrirazeny:
                    v.SelectedP51ID_Flag2 = v.RecP41.p51ID_Billing;
                    v.SelectedComboP51Name = v.RecP41.p51Name_Billing;
                    break;
                case BO.p41BillingFlagEnum.CenikIndividualni:
                    v.SelectedP51ID_Flag3 = v.RecP41.p51ID_Billing;

                    break;
            }
            if (v.RecP44.p44IsBilling)
            {
                v.p41InvoiceDefaultText2 = v.RecP41.p41InvoiceDefaultText2;
                v.p41InvoiceDefaultText1 = v.RecP41.p41InvoiceDefaultText1;
            }
            if (v.RecP44.p44IsClient)
            {
                v.p28ID_Client = v.RecP41.p28ID_Client;
                v.Klient = v.RecP41.Client;
            }
            if (v.RecP44.p44IsJ18ID)
            {
                v.j18ID = v.RecP41.j18ID;
                v.Stredisko = v.RecP41.j18Name;
            }

            if (v.RecP44.p44IsP40)
            {
                v.lisP40 = new List<p40cft>();
                var lisP40 = Factory.p40WorkSheet_RecurrenceBL.GetList(new BO.myQueryP40() { IsRecordValid = null, p41id = v.p41ID });
                foreach(var c in lisP40)
                {
                    v.lisP40.Add(new p40cft() { pid = c.pid, p40Name = c.p40Name,p40Text=c.p40Text,p40Value=c.p40Value, p40FreeHours = c.p40FreeHours,p40FreeFee=c.p40FreeFee,p40FirstSupplyDate= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) });
                }
                
            }
            if (v.RecP44.p44IsP56)
            {
                v.lisP56 = new List<p56cft>();
                var lisP56 = Factory.p56TaskBL.GetList(new BO.myQueryP56() { IsRecordValid = null, p41id = v.p41ID });
                foreach (var c in lisP56)
                {
                    var cc = new p56cft() { pid = c.pid, p56Name = c.p56Name, p56PlanUntil = c.p56PlanUntil };
                    var lis = Factory.x67EntityRoleBL.GetList_X69_OneTask(c, false);
                    cc.Assign_j02IDs = string.Join(",", lis.Where(p=>p.j02ID>0).Select(p => p.j02ID));
                    cc.Assign_Persons = string.Join(",", lis.Where(p => p.j02ID > 0).Select(p => p.Person));
                    cc.Assign_j11IDs = string.Join(",", lis.Where(p => p.j11ID > 0).Select(p => p.j11ID));
                    cc.Assign_j11Names = string.Join(",", lis.Where(p => p.j11ID > 0).Select(p => p.j11Name));
                    v.lisP56.Add(cc);
                }
            }
            if (v.RecP44.p44IsO22)
            {
                v.lisO22 = new List<o22cft>();
                var lisO22 = Factory.o22MilestoneBL.GetList(new BO.myQueryO22() { IsRecordValid = null, p41id = v.p41ID });
                foreach (var c in lisO22)
                {
                    v.lisO22.Add(new o22cft() { pid = c.pid, o22Name = c.o22Name, o22DurationCount = c.o22DurationCount, o22PlanFrom = c.o22PlanFrom });
                }

            }
            //v.SetTagging(Factory.o51TagBL.GetTagging("p41", v.ProjectCombo.SelectedP41ID));


            RefreshState(v);





            return View(v);
        }

        private void RefreshState(p41cftViewModel v)
        {
            if (v.RecP44 == null)
            {
                v.RecP44 = Factory.p44ProjectTemplateBL.Load(v.p44ID);
            }
            if (v.RecP41 == null)
            {
                v.RecP41 = Factory.p41ProjectBL.Load(v.p41ID);
            }


            if (v.roles == null)
            {
                v.roles = new RoleAssignViewModel() { RecPid = v.p41ID, RecPrefix = "p41", RolePrefix = "p41" };
            }
            if (v.reminder == null)
            {
                v.reminder = new ReminderViewModel() { is_static_date = true, record_pid = v.p41ID, record_prefix = "p41" };
            }
            if (v.hlidac == null)
            {
                v.hlidac = new HlidacViewModel() { rec_entity = "p41", rec_pid = v.p41ID };
                
            }

            if (v.ff1 == null)
            {
                v.ff1 = new FreeFieldsViewModel();
                v.ff1.InhaleFreeFieldsView(Factory, v.p41ID, "p41");
            }
            v.ff1.RefreshInputsVisibility(Factory, v.p41ID, "p41", v.RecP41.p42ID);

        }

        [HttpPost]
        public IActionResult Index(p41cftViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {

                BO.p41Project c = Factory.p41ProjectBL.Load(v.p41ID);
                c.pid = 0;
                c.p41Name = v.p41Name;
                c.p41NameShort = v.p41NameShort;
                
                c.p28ID_Client = v.p28ID_Client;
                c.ValidFrom = DateTime.Now;
                c.ValidUntil = new DateTime(3000, 1, 1);
                c.j02ID_Owner = Factory.CurrentUser.pid;
                c.p41ExternalCode = null;
                

                if (v.p41BillingFlag == BO.p41BillingFlagEnum.CenikPrirazeny)
                {
                    c.p51ID_Billing = v.SelectedP51ID_Flag2;
                    if (c.p51ID_Billing == 0)
                    {
                        this.AddMessage("Chybí vybrat ceník fakturačních hodinových sazeb."); return View(v);
                    }
                }
                if (v.p41BillingFlag == BO.p41BillingFlagEnum.CenikIndividualni)
                {
                    if (v.SelectedP51ID_Flag3 == 0)
                    {
                        var recP51 = Factory.p51PriceListBL.LoadByTempGuid(v.TempGuid);
                        if (recP51 == null)
                        {
                            this.AddMessage("Chybí ceník hodinových sazeb na míru."); return View(v);
                        }
                        c.p51ID_Billing = recP51.pid;
                    }
                    else
                    {
                        c.p51ID_Billing = v.SelectedP51ID_Flag3;
                    }
                }
                if (v.lisP40 !=null && v.lisP40.Any(p =>p.IsChecked && p.p40Value <= 0))
                {
                    this.AddMessage("V předpisech opakovaných odměn/paušálů je nulová částka."); return View(v);
                }
                if (v.lisP40 != null && v.lisP40.Any(p =>p.IsChecked && ( p.p40Text==null || p.p40Name == null)))
                {
                    this.AddMessage("V předpisech opakovaných odměn/paušálů je nevyplněná maska textu nebo nevyplněný název předpisu."); return View(v);
                }
                if (v.lisP56 != null && v.lisP56.Any(p => p.IsChecked && (p.p56PlanUntil == null || p.p56Name == null)))
                {
                    this.AddMessage("V úkolech je nevyplněný název nebo datum."); return View(v);
                }
                if (v.lisO22 !=null && v.lisO22.Any(p => p.IsChecked && ( (p.o22PlanFrom == null && p.o22PlanUntil==null) || p.o22Name==null)))
                {
                    this.AddMessage("V termínech/lhůtách je nevyplněný název nebo datum."); return View(v);
                }
                c.p41InvoiceDefaultText1 = v.p41InvoiceDefaultText1;
                c.p41InvoiceDefaultText2 = v.p41InvoiceDefaultText2;


                c.pid = Factory.p41ProjectBL.Save(c, null, v.roles.getList4Save(Factory), null);
                if (c.pid > 0)
                {
                    if (v.lisP40 != null)
                    {
                        foreach (var cc in v.lisP40.Where(p=>p.IsChecked))
                        {
                            var rec = Factory.p40WorkSheet_RecurrenceBL.Load(cc.pid);
                            rec.pid = 0;
                            rec.p41ID = c.pid;
                            rec.p40Name = cc.p40Name;
                            rec.p40FreeFee = cc.p40FreeFee;
                            rec.p40FreeHours = cc.p40FreeHours;
                            rec.p40Value = cc.p40Value;
                            rec.p40FirstSupplyDate = cc.p40FirstSupplyDate;
                            rec.p40LastSupplyDate = rec.p40FirstSupplyDate.Value.AddYears(5);
                            rec.ValidFrom = DateTime.Today;
                            rec.ValidUntil = new DateTime(3000, 1, 1);
                            Factory.p40WorkSheet_RecurrenceBL.Save(rec);
                        }
                    }
                    if (v.lisP56 != null)
                    {
                        foreach(var cc in v.lisP56.Where(p => p.IsChecked))
                        {
                            var rec = Factory.p56TaskBL.Load(cc.pid);
                            rec.pid = 0;
                            rec.p41ID = c.pid;
                            rec.p56Name = cc.p56Name;
                            rec.p56Notepad = cc.p56Notepad;
                            rec.p56PlanUntil = cc.p56PlanUntil;
                            rec.p56PlanFrom = cc.p56PlanFrom;
                            rec.ValidFrom = DateTime.Today;
                            rec.ValidUntil = new DateTime(3000, 1, 1);

                            Factory.p56TaskBL.Save(rec, null, null);
                        }
                    }
                    if (v.lisO22 != null)
                    {
                        foreach (var cc in v.lisO22.Where(p => p.IsChecked))
                        {
                            var rec = Factory.o22MilestoneBL.Load(cc.pid);
                            rec.pid = 0;
                            rec.p41ID = c.pid;
                            rec.o22Name = cc.o22Name;
                            rec.o22PlanFrom = cc.o22PlanFrom;
                            rec.o22DurationCount = cc.o22DurationCount;
                            rec.o22PlanUntil = cc.o22PlanUntil = rec.o22PlanUntil;

                            rec.ValidFrom = DateTime.Today;
                            rec.ValidUntil = new DateTime(3000, 1, 1);
                            
                            Factory.o22MilestoneBL.Save(rec,null);
                        }
                    }

                    if (v.hlidac != null)
                    {
                        foreach(var cc in v.hlidac.lisItems)
                        {
                            cc.b21ID = 0;
                           
                        }
                        v.hlidac.SaveChanges(Factory, c.pid);
                        
                    }
                    if (v.reminder != null)
                    {
                        foreach (var cc in v.reminder.lisReminder)
                        {
                            cc.o24ID = 0;                            
                        }
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
