using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.p41oper;

namespace UI.Controllers
{
    //Průvodce založením projektu ze šablony (p44ProjectTemplate).
    //Šablona = vzorový projekt + volby, které části vzoru průvodce nabídne a převezme.
    public class p41cftController : BaseController
    {
        public IActionResult Index(int p44id)
        {
            var v = new p41cftViewModel() { p44ID = p44id, TempGuid = BO.Code.Bas.GetGuid() };
            string strErr = LoadTemplate(v);
            if (strErr != null)
            {
                return this.StopPage(true, strErr);
            }

            v.p41ID = v.RecP44.p41ID_Pattern;

            v.p41Name = v.RecP41.p41Name;
            v.p41NameShort = v.RecP41.p41NameShort;

            v.p41BillingFlag = BO.p41BillingFlagEnum.CenikDedit;
            if (v.RecP44.p44IsBilling)
            {
                if (v.RecP41.p41BillingFlag != BO.p41BillingFlagEnum._NotSpecified)
                {
                    v.p41BillingFlag = v.RecP41.p41BillingFlag;
                }
                switch (v.p41BillingFlag)
                {
                    case BO.p41BillingFlagEnum.CenikPrirazeny:
                        v.SelectedP51ID_Flag2 = v.RecP41.p51ID_Billing;
                        v.SelectedComboP51Name = v.RecP41.p51Name_Billing;
                        break;
                    case BO.p41BillingFlagEnum.CenikIndividualni:
                        v.SelectedP51ID_Flag3 = v.RecP41.p51ID_Billing;   //ceník vzoru slouží jen jako předloha, nový projekt dostane vlastní kopii
                        break;
                }
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
                foreach (var c in lisP40)
                {
                    v.lisP40.Add(new p40cft() { pid = c.pid, p40Name = c.p40Name, p40Text = c.p40Text, p40Value = c.p40Value, p40FreeHours = c.p40FreeHours, p40FreeFee = c.p40FreeFee, p40FirstSupplyDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) });
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
                    cc.Assign_j02IDs = string.Join(",", lis.Where(p => p.j02ID > 0).Select(p => p.j02ID).Distinct());
                    cc.Assign_Persons = string.Join(",", lis.Where(p => p.j02ID > 0).Select(p => p.Person).Distinct());
                    cc.Assign_j11IDs = string.Join(",", lis.Where(p => p.j11ID > 0).Select(p => p.j11ID).Distinct());
                    cc.Assign_j11Names = string.Join(",", lis.Where(p => p.j11ID > 0).Select(p => p.j11Name).Distinct());
                    v.lisP56.Add(cc);
                }
            }
            if (v.RecP44.p44IsO22)
            {
                v.lisO22 = new List<o22cft>();
                var lisO22 = Factory.o22MilestoneBL.GetList(new BO.myQueryO22() { IsRecordValid = null, p41id = v.p41ID });
                foreach (var c in lisO22)
                {
                    v.lisO22.Add(new o22cft() { pid = c.pid, o22Name = c.o22Name, o22DurationCount = c.o22DurationCount, o22PlanFrom = c.o22PlanFrom, o22PlanUntil = c.o22PlanUntil });
                }

            }
            if (v.RecP44.p44IsTags)
            {
                var tg = Factory.o51TagBL.GetTagging("p41", v.p41ID);
                if (tg != null)
                {
                    v.TagPids = tg.TagPids; v.TagNames = tg.TagNames; v.TagHtml = tg.TagHtml;
                }
            }

            RefreshState(v);

            return View(v);
        }

        //načte šablonu a vzorový projekt, vrací text chyby nebo null
        private string LoadTemplate(p41cftViewModel v)
        {
            v.RecP44 = Factory.p44ProjectTemplateBL.Load(v.p44ID);
            if (v.RecP44 == null)
            {
                return Factory.tra("Šablona projektu neexistuje.");
            }
            if (!Factory.p44ProjectTemplateBL.GetList_ProjectCreate().Any(p => p.pid == v.p44ID))
            {
                return Factory.tra("Šablona není dostupná: je uzavřená, nebo nemáte oprávnění zakládat projekty tohoto typu.");
            }
            v.RecP41 = Factory.p41ProjectBL.Load(v.RecP44.p41ID_Pattern);
            if (v.RecP41 == null)
            {
                return Factory.tra("Vzorový projekt šablony už neexistuje. Šablonu je třeba opravit.");
            }
            return null;
        }

        private void RefreshState(p41cftViewModel v)
        {
            //co šablona nepřenáší, začíná v průvodci prázdné (pid=0 -> sdílené prvky nic nenačtou)
            if (v.roles == null)
            {
                v.roles = new RoleAssignViewModel() { RecPid = (v.RecP44.p44IsRoles ? v.p41ID : 0), RecPrefix = "p41", RolePrefix = "p41" };
            }
            if (v.reminder == null)
            {
                v.reminder = new ReminderViewModel() { is_static_date = true, record_pid = (v.RecP44.p44IsO24 ? v.p41ID : 0), record_prefix = "p41" };
            }
            if (v.hlidac == null)
            {
                v.hlidac = new HlidacViewModel() { rec_entity = "p41", rec_pid = (v.RecP44.p44IsB20 ? v.p41ID : 0) };
            }

            if (v.ff1 == null)
            {
                v.ff1 = new FreeFieldsViewModel();
                v.ff1.InhaleFreeFieldsView(Factory, v.p41ID, "p41");
            }
            v.ff1.RefreshInputsVisibility(Factory, v.p41ID, "p41", v.RecP41.p42ID);

        }

        //vlastní kopie ceníku sazeb na míru: vzor a nový projekt nesmí sdílet jeden ceník
        private BO.p51PriceList GetOrCreateCustomPriceList(p41cftViewModel v)
        {
            var recP51 = Factory.p51PriceListBL.LoadByTempGuid(v.TempGuid);
            if (recP51 != null || v.SelectedP51ID_Flag3 == 0)
            {
                return recP51;
            }
            var recSource = Factory.p51PriceListBL.Load(v.SelectedP51ID_Flag3);
            if (recSource == null)
            {
                return null;
            }
            var lisP52 = Factory.p51PriceListBL.GetList_p52(recSource.pid).ToList();
            foreach (var c in lisP52)
            {
                c.pid = 0;
            }
            recSource.pid = 0;
            recSource.p51IsCustomTailor = true;
            recSource.p51IsMasterPriceList = false;
            recSource.p51Name = v.TempGuid;   //podle názvu se ceník dohledá, konečný název doplní uložení projektu
            int intP51ID = Factory.p51PriceListBL.Save(recSource, lisP52, new DateTime(DateTime.Today.Year, 1, 1), new DateTime(3000, 1, 1));
            if (intP51ID == 0)
            {
                return null;
            }
            return Factory.p51PriceListBL.Load(intP51ID);
        }

        //obsazení rolí nového úkolu: beze změny řešitelů se převezme obsazení vzorového úkolu
        private List<BO.x69EntityRole_Assign> GetTaskRoles(p56cft cc)
        {
            var lisPattern = Factory.x67EntityRoleBL.GetList_X69("p56", cc.pid).ToList();
            var j02ids = BO.Code.Bas.ConvertString2ListInt(cc.Assign_j02IDs).Where(p => p > 0).Distinct().ToList();
            var j11ids = BO.Code.Bas.ConvertString2ListInt(cc.Assign_j11IDs).Where(p => p > 0).Distinct().ToList();

            var j02pattern = lisPattern.Where(p => p.j02ID > 0).Select(p => p.j02ID).Distinct().ToList();
            var j11pattern = lisPattern.Where(p => p.j11ID > 0).Select(p => p.j11ID).Distinct().ToList();
            if (j02ids.Count == j02pattern.Count && !j02ids.Except(j02pattern).Any() && j11ids.Count == j11pattern.Count && !j11ids.Except(j11pattern).Any())
            {
                return lisPattern;
            }

            //řešitelé se změnili: obsadit jimi roli, kterou úkol používal ve vzoru, jinak první roli úkolu
            int intX67ID = lisPattern.Where(p => p.j02ID > 0 || p.j11ID > 0).Select(p => p.x67ID).FirstOrDefault();
            if (intX67ID == 0)
            {
                var recX67 = Factory.x67EntityRoleBL.GetList(new BO.myQuery("x67") { explicit_orderby = "a.x67Entity,a.x67Ordinary" }).FirstOrDefault(p => p.x67Entity == "p56");
                if (recX67 != null) intX67ID = recX67.pid;
            }
            var lis = lisPattern.Where(p => p.x69IsAllUsers).ToList();
            if (intX67ID > 0)
            {
                foreach (int j02id in j02ids)
                {
                    lis.Add(new BO.x69EntityRole_Assign() { x67ID = intX67ID, j02ID = j02id, x69RecordEntity = "p56" });
                }
                foreach (int j11id in j11ids)
                {
                    lis.Add(new BO.x69EntityRole_Assign() { x67ID = intX67ID, j11ID = j11id, x69RecordEntity = "p56" });
                }
            }
            return lis;
        }

        [HttpPost]
        public IActionResult Index(p41cftViewModel v)
        {
            string strErr = LoadTemplate(v);
            if (strErr != null)
            {
                return this.StopPage(true, strErr);
            }
            v.p41ID = v.RecP44.p41ID_Pattern;   //vzor vždy podle šablony, ne podle odeslaného formuláře
            RefreshState(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "p51custom")
                {
                    //před otevřením okna se sazbami připravit vlastní kopii ceníku vzoru
                    GetOrCreateCustomPriceList(v);
                    v.IsOpenCustomPriceList = true;
                }
                return View(v);
            }
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(v.p41Name))
                {
                    this.AddMessage("Chybí vyplnit název projektu."); return View(v);
                }
                if (v.p41BillingFlag == BO.p41BillingFlagEnum.CenikPrirazeny && v.SelectedP51ID_Flag2 == 0)
                {
                    this.AddMessage("Chybí vybrat ceník fakturačních hodinových sazeb."); return View(v);
                }
                if (v.lisP40 != null && v.lisP40.Any(p => p.IsChecked && p.p40Value <= 0))
                {
                    this.AddMessage("V předpisech opakovaných odměn/paušálů je nulová částka."); return View(v);
                }
                if (v.lisP40 != null && v.lisP40.Any(p => p.IsChecked && (p.p40Text == null || p.p40Name == null)))
                {
                    this.AddMessage("V předpisech opakovaných odměn/paušálů je nevyplněná maska textu nebo nevyplněný název předpisu."); return View(v);
                }
                if (v.lisP40 != null && v.lisP40.Any(p => p.IsChecked && p.p40FirstSupplyDate == null))
                {
                    this.AddMessage("V předpisech opakovaných odměn/paušálů chybí datum [Od]."); return View(v);
                }
                if (v.lisP56 != null && v.lisP56.Any(p => p.IsChecked && (p.p56PlanUntil == null || p.p56Name == null)))
                {
                    this.AddMessage("V úkolech je nevyplněný název nebo datum."); return View(v);
                }
                if (v.lisO22 != null && v.lisO22.Any(p => p.IsChecked && ((p.o22PlanFrom == null && p.o22PlanUntil == null) || p.o22Name == null)))
                {
                    this.AddMessage("V termínech/lhůtách je nevyplněný název nebo datum."); return View(v);
                }
                if (v.reminder != null && v.reminder.lisReminder != null && v.reminder.lisReminder.Any(p => p.IsTempDeleted == false && p.o24StaticDate == null))
                {
                    this.AddMessage("V upozornění chybí vyplnit datum+čas."); return View(v);
                }

                int intP41ID = 0;
                try
                {
                    //projekt i jeho vazby se založí celé, nebo vůbec
                    using (var sc = new System.Transactions.TransactionScope())
                    {
                        intP41ID = SaveProject(v);
                        if (intP41ID > 0)
                        {
                            sc.Complete();
                        }
                    }
                }
                catch (System.Transactions.TransactionException)
                {
                    intP41ID = 0;   //některé z dílčích uložení transakci zrušilo, důvod je ve zprávách
                }

                if (intP41ID > 0)
                {
                    v.SetJavascript_CallOnLoad(intP41ID);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }

        //vrací ID nového projektu, 0 = nepodařilo se (volající transakci nepotvrdí)
        private int SaveProject(p41cftViewModel v)
        {
            BO.p41Project c = Factory.p41ProjectBL.Load(v.p41ID);
            c.pid = 0;

            c.p41Name = v.p41Name;
            c.p41NameShort = v.p41NameShort;

            var recP42 = Factory.p42ProjectTypeBL.Load(c.p42ID);
            c.p41Code = Factory.x38CodeLogicBL.x38_get_freecode(recP42.x38ID, "p41", 0);

            c.ValidFrom = DateTime.Now;
            c.ValidUntil = new DateTime(3000, 1, 1);
            c.j02ID_Owner = Factory.CurrentUser.pid;

            //ze vzoru se nikdy nepřenáší: externí kód, nadřízený projekt a stav workflow (nový projekt začíná výchozím stavem)
            c.p41ExternalCode = null;
            c.p41ParentID = 0;
            c.b02ID = 0;

            c.p28ID_Client = v.p28ID_Client;
            if (!v.RecP44.p44IsClient)
            {
                c.p28ID_Billing = 0;   //odběratel faktury patří ke klientovi vzoru
            }
            c.j18ID = v.j18ID;

            if (!v.RecP44.p44IsBilling)
            {
                //fakturační nastavení vzoru se nepřebírá
                c.p92ID = 0;
                c.p41InvoiceMaturityDays = 0;
                c.p41BillingLangIndex = 0;
                c.p41BillingMemo200 = null;
            }
            c.p41BillingFlag = v.p41BillingFlag;
            c.p51ID_Billing = 0;
            if (v.p41BillingFlag == BO.p41BillingFlagEnum.CenikPrirazeny)
            {
                c.p51ID_Billing = v.SelectedP51ID_Flag2;
            }
            if (v.p41BillingFlag == BO.p41BillingFlagEnum.CenikIndividualni)
            {
                var recP51 = GetOrCreateCustomPriceList(v);
                if (recP51 == null)
                {
                    this.AddMessage("Chybí ceník hodinových sazeb na míru."); return 0;
                }
                c.p51ID_Billing = recP51.pid;
            }
            c.p41InvoiceDefaultText1 = v.p41InvoiceDefaultText1;
            c.p41InvoiceDefaultText2 = v.p41InvoiceDefaultText2;

            c.pid = Factory.p41ProjectBL.Save(c, v.ff1.inputs, v.roles.getList4Save(Factory), null);
            if (c.pid == 0)
            {
                return 0;
            }

            Factory.o51TagBL.SaveTagging("p41", c.pid, v.TagPids);

            if (v.lisP40 != null)
            {
                foreach (var cc in v.lisP40.Where(p => p.IsChecked))
                {
                    var rec = Factory.p40WorkSheet_RecurrenceBL.Load(cc.pid);
                    if (rec == null) continue;
                    DateTime? datFirstOrig = rec.p40FirstSupplyDate; DateTime? datLastOrig = rec.p40LastSupplyDate;
                    rec.pid = 0;
                    rec.p41ID = c.pid;
                    rec.p40Name = cc.p40Name;
                    rec.p40Text = cc.p40Text;
                    rec.p40FreeFee = cc.p40FreeFee;
                    rec.p40FreeHours = cc.p40FreeHours;
                    rec.p40Value = cc.p40Value;
                    rec.p40FirstSupplyDate = cc.p40FirstSupplyDate;
                    if (datFirstOrig != null && datLastOrig != null && datLastOrig.Value > datFirstOrig.Value)
                    {
                        rec.p40LastSupplyDate = cc.p40FirstSupplyDate.Value.Add(datLastOrig.Value - datFirstOrig.Value);   //stejně dlouhé období jako ve vzoru
                    }
                    else
                    {
                        rec.p40LastSupplyDate = cc.p40FirstSupplyDate.Value.AddYears(5);
                    }
                    rec.ValidFrom = DateTime.Today;
                    rec.ValidUntil = new DateTime(3000, 1, 1);
                    if (Factory.p40WorkSheet_RecurrenceBL.Save(rec, null) == 0)
                    {
                        return 0;
                    }
                }
            }
            if (v.lisP56 != null)
            {
                foreach (var cc in v.lisP56.Where(p => p.IsChecked))
                {
                    var rec = Factory.p56TaskBL.Load(cc.pid);
                    if (rec == null) continue;
                    var lisX69 = GetTaskRoles(cc);
                    rec.pid = 0;
                    rec.p41ID = c.pid;
                    rec.p56Name = cc.p56Name;
                    rec.p56PlanUntil = cc.p56PlanUntil;
                    if (rec.p56PlanFrom != null && rec.p56PlanUntil != null && rec.p56PlanFrom.Value > rec.p56PlanUntil.Value)
                    {
                        rec.p56PlanFrom = null;   //zahájení ze vzoru by bylo až po novém termínu
                    }
                    rec.ValidFrom = DateTime.Today;
                    rec.ValidUntil = new DateTime(3000, 1, 1);

                    if (Factory.p56TaskBL.Save(rec, null, lisX69) == 0)
                    {
                        return 0;
                    }
                }
            }
            if (v.lisO22 != null)
            {
                foreach (var cc in v.lisO22.Where(p => p.IsChecked))
                {
                    var rec = Factory.o22MilestoneBL.Load(cc.pid);
                    if (rec == null) continue;
                    var lisX69 = Factory.x67EntityRoleBL.GetList_X69("o22", cc.pid).ToList();
                    rec.pid = 0;
                    rec.p41ID = c.pid;
                    rec.o22Name = cc.o22Name;
                    rec.o22PlanFrom = cc.o22PlanFrom;
                    rec.o22DurationCount = cc.o22DurationCount;
                    rec.o22PlanUntil = cc.o22PlanUntil;

                    rec.ValidFrom = DateTime.Today;
                    rec.ValidUntil = new DateTime(3000, 1, 1);

                    if (Factory.o22MilestoneBL.Save(rec, lisX69) == 0)
                    {
                        return 0;
                    }
                }
            }

            if (v.hlidac != null && v.hlidac.lisItems != null)
            {
                foreach (var cc in v.hlidac.lisItems)
                {
                    cc.b21ID = 0;
                }
                v.hlidac.SaveChanges(Factory, c.pid);

            }
            if (v.reminder != null && v.reminder.lisReminder != null)
            {
                foreach (var cc in v.reminder.lisReminder)
                {
                    cc.o24ID = 0;
                }
                v.reminder.SaveChanges(Factory, c.pid);

            }

            return c.pid;
        }
    }
}
