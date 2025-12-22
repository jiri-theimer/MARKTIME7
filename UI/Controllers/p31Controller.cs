
using BL;
using BO;
using Google.Apis.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Xml;
using UI.Models;
using UI.Models.p31view;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{

    public class p31Controller : BaseController
    {
        public IActionResult Info(int pid, bool isrecord,bool isdelrecord=false)
        {
            return Tab1(pid, "info", isrecord, isdelrecord);
        }
        public IActionResult Day(string d, int j02id)
        {                        
            var v = new p31DayViewModel() { d = d, j02id = j02id };
            return View(v);
        }

        public IActionResult Tab1(int pid, string caller, bool isrecord, bool isdelrecord = false)
        {
            var v = new p31Tab1() { Factory = this.Factory, prefix = "p31", pid = pid, caller = caller, IsRecord = isrecord };
            v.Rec = Factory.p31WorksheetBL.Load(v.pid, isdelrecord);
           
            if (v.Rec == null)
            {
                return this.StopPageSubform("Záznam nebyl nalezen.");
            }
            if (v.Rec.p91ID > 0)
            {
                v.RecP91 = Factory.p91InvoiceBL.Load(v.Rec.p91ID);
            }
            return View(v);
        }


        public IActionResult Record(int pid, bool isclone, int j02id, int p34id,int p32id, string newrec_prefix, int newrec_pid, string d, string t1, string t2, string approve_guid, int p91id,int p68id,int p49id)
        {
            if (!Factory.CurrentUser.j04IsModule_p31)
            {
                return this.StopPage(true, "Nemáte přístup do modulu [Úkony].");
            }
            var v = new p31Record() { rec_pid = pid, rec_entity = "p31", GuidApprove = approve_guid, Element2Focus = "cmdComboRec_p41ID", UploadGuid = BO.Code.Bas.GetGuid(), p91ID = p91id,p68ID=p68id,p49ID=p49id };
            v.disp = new DispoziceViewModel();
            v.disp.InitItems("p31", Factory);
            v.PravaZaJineho = Factory.CBL.LoadUserParamInt("p31/Record-PravaZaJineho", 1);


            v.IsNavicKusovnik = Factory.CBL.LoadUserParamBool("p31/Record-IsNavicKusovnik", false);

            if (v.IsNavicKusovnik || Factory.p34ActivityGroupBL.GetList(new BO.myQueryP34() { iskusovnik = true,IsRecordValid=true }).Count() > 0)
            {
                v.IsOfferNavicKusovnik = true;
            }



            if (pid == 0 && d != null)
            {
                v.p31Date = BO.Code.Bas.String2Date(d);
            }
            if (pid == 0 && v.GuidApprove != null && string.IsNullOrEmpty(newrec_prefix))    //nový úkon do schvalování: předvyplnit projekt
            {
                newrec_prefix = "p41";
                newrec_pid = Factory.p31WorksheetBL.GetList(new BO.myQueryP31() { tempguid = v.GuidApprove }).First().p41ID;
            }
            if (p32id > 0)
            {                
                v.RecP32 = Factory.p32ActivityBL.Load(p32id);
                p34id = v.RecP32.p34ID;
                if (v.RecP32.p41ID_Absence > 0)
                {
                    newrec_prefix = "p41";
                    newrec_pid = v.RecP32.p41ID_Absence;
                }
                
                

            }
            if (p91id > 0)
            {
                var recP91 = Factory.p91InvoiceBL.Load(p91id);
                newrec_prefix = "p41";
                newrec_pid = recP91.p41ID_First;
            }

            v.Rec = new BO.p31WorksheetEntryInput() { pid = pid, p34ID = p34id, j02ID = j02id, p32ID = p32id };
            
            if (p32id > 0 && v.RecP32 !=null)
            {
                v.Rec.p31Text = InhaleActivityDefaultText(v);
            }
            if (v.p68ID > 0)
            {
                InhaleFromStopWatch(v);  //načíst záznam ze stopek
            }
            if (v.p49ID > 0)
            {
                InhaleFromPlan(v);  //načíst záznam z finančního plánu
            }
            
            switch (newrec_prefix)
            {
                case "p41":
                case "le5":
                case "le4":
                case "le3":
                case "le2":
                case "le1":
                    v.Rec.p41ID = newrec_pid;
                    v.RecP41 = Factory.p41ProjectBL.Load(newrec_pid);
                    v.SelectedLevelIndex = 0;
                    //v.SelectedLevelIndex = v.RecP41.p07Level;
                    InhaleP56Combo(v);

                    v.Element2Focus = "cmdComboRec_p32ID";
                    break;
               
                case "p56":
                    var recP56 = Factory.p56TaskBL.Load(newrec_pid);
                    if (recP56 != null && recP56.p41ID == 0)
                    {
                        this.AddMessage("Tento úkol není svázaný s projektem a proto do něj nelze vykazovat úkony.");
                    }
                    if (recP56 != null && recP56.p41ID > 0)
                    {
                        v.IsShowP56Combo = true;
                        v.Rec.p56ID = recP56.pid;
                        v.RecP41 = Factory.p41ProjectBL.Load(recP56.p41ID);
                        v.Rec.p41ID = v.RecP41.pid;
                        //v.SelectedLevelIndex = v.RecP41.p07Level;
                        v.SelectedComboTask = recP56.FullName;
                    }
                    break;
                case "p32":
                    v.RecP32 = Factory.p32ActivityBL.Load(newrec_pid);
                    v.Rec.p32ID = v.RecP32.pid;
                    v.SelectedComboP32Name = v.RecP32.p32Name;
                    if (v.Rec.p41ID == 0 && v.RecP32.p41ID_Absence > 0)
                    {
                        v.Rec.p41ID = v.RecP32.p41ID_Absence;
                        v.RecP41 = Factory.p41ProjectBL.Load(v.RecP32.p41ID_Absence);
                        //v.SelectedLevelIndex = v.RecP41.p07Level;
                        InhaleP56Combo(v);
                    }
                    
                    v.Element2Focus = "p31Datehelper";
                    break;
                case "p28":
                    var lisP41 = Factory.p41ProjectBL.GetList(new BO.myQueryP41("le5") { p28id = newrec_pid });
                    if (lisP41.Count() == 0)
                    {
                        var lisP30 = Factory.p28ContactBL.GetList_p30_mother(newrec_pid);   //kontaktní osoba klienta projektu?
                        if (lisP30.Count() > 0)
                        {
                            newrec_pid = lisP30.First().p28ID;
                            lisP41 = Factory.p41ProjectBL.GetList(new BO.myQueryP41("le5") { p28id = newrec_pid });
                        }
                    }
                    if (lisP41.Count() == 1)
                    {
                        v.Rec.p41ID = lisP41.First().pid; //v.SelectedLevelIndex = lisP41.First().p07Level;
                    }
                    if (lisP41.Count() > 1)
                    {
                        return RedirectToAction("SelectProject", "p41", new { source_prefix = "p28", source_pid = newrec_pid });
                    }

                    break;
                case "o23":
                    v.Rec.o23ID = newrec_pid;
                    var recO23 = Factory.o23DocBL.Load(v.Rec.o23ID);
                    var recO18 = Factory.o18DocTypeBL.Load(recO23.o18ID);
                    if (recO18.o18TemplateFlag == BO.o18TemplateENUM.Uctenka)
                    {
                        //účtenka                        
                        InhaleFromUctenka(v,recO23,recO18);
                    }
                    else
                    {
                        v.Doc_o23Name = (recO23.o18Name != recO23.o23Name) ? recO23.o18Name + ": " + recO23.o23Name : recO23.o18Name;

                        var lisO19 = Factory.o23DocBL.GetList_o19(v.Rec.o23ID).Where(p => p.o20Entity == "p41");
                        if (lisO19.Count() > 0)
                        {
                            v.Rec.p41ID = lisO19.First().o19RecordPid;
                            v.RecP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID);
                            //v.SelectedLevelIndex = v.RecP41.p07Level;
                        }
                    }
                    
                    break;
            }

            if (pid == 0 && t1 != null)
            {
                v.Rec.TimeFrom = t1;
                if (t2 != null)
                {
                    if (t2 == "24:00") t2 = "23:59";
                    v.Rec.TimeUntil = t2;
                    var xx = Record_RecalcDuration("", "0", t1, t2, "0", "N");
                    if (xx.error == null)
                    {
                        v.Rec.Value_Orig = xx.duration;

                    }
                    else
                    {
                        this.AddMessage(xx.error);
                    }

                }
            }
            Handle_Defaults(v);
            if (v.rec_pid > 0)
            {
                var recP31 = Factory.p31WorksheetBL.Load(v.rec_pid);
                if (recP31 == null)
                {
                    return this.StopPage(true, "Záznam nebyl nalezen.");
                }
                if (recP31.isdeleted)
                {
                    return RedirectToAction("Info", new { pid = pid, isrecord = true });                    
                }
                var disp = InhalePermissions(v, recP31);
                if (!disp.ReadAccess)
                {
                    return this.StopPage(true, "Nemáte oprávnění k záznamu.");
                }
                if (!isclone)
                {
                    if ((!disp.OwnerAccess || disp.RecordState != BO.p31RecordState.Editing) && v.GuidApprove == null)
                    {

                        return RedirectToAction("Info", new { pid = pid, isrecord = true });
                    }
                }


                LoadRecordSetting(v);
                
                v.Rec = Factory.p31WorksheetBL.CovertRec2Input(recP31, v.Setting.TimesheetEntryByMinutes);

                v.p31Date = v.Rec.p31Date.First();
                v.SelectedComboP32Name = recP31.p32Name;
                v.SelectedComboP34Name = recP31.p34Name;
                v.SelectedComboPerson = recP31.Person;
                v.SelectedComboProject = recP31.Project;
                v.SelectedComboTask = recP31.p56Name;
                if (recP31.o23ID > 0)
                {
                    var recO23 = Factory.o23DocBL.Load(v.Rec.o23ID);
                    v.Doc_o23Name = recO23.o18Name + ": " + recO23.o23Name;
                }
                v.SelectedComboJ27Code = recP31.j27Code_Billing_Orig;
                if (v.RecP41 == null) v.RecP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID);
                //v.SelectedLevelIndex = v.RecP41.p07Level;

                if (v.Rec.p28ID_Supplier > 0)
                {
                    try
                    {
                        v.SelectedComboSupplier = Factory.p28ContactBL.Load(v.Rec.p28ID_Supplier).p28Name;
                    }
                    catch
                    {
                        //nic
                    }
                    
                }
                if (v.Rec.p35ID > 0)
                {
                    v.SelectedComboP35Code = Factory.p35UnitBL.Load(v.Rec.p35ID).p35Code;
                }
                if (v.Rec.j19ID > 0)
                {
                    v.SelectedComboJ19Name = Factory.FBL.LoadJ19(v.Rec.j19ID).j19Name;
                }

                InhaleP34(v, v.Rec.p31BitStream);

                if (v.disp.IsTags)
                {
                    v.SetTagging(Factory.o51TagBL.GetTagging("p31", v.rec_pid));
                }

                
                InhaleP56Combo(v);

                if (isclone && recP31.p31Date != DateTime.Today)
                {
                    v.IsShowDateCloneDialog = true;
                    v.Element2Focus = "cmdSetToday";
                }
                if (isclone && !disp.OwnerAccess)       //není oprávnění vlastníka ke kopírovanému záznamu
                {
                    v.Rec.j02ID = Factory.CurrentUser.pid;
                    v.SelectedComboPerson = Factory.CurrentUser.FullnameDesc;
                    this.AddMessageTranslated("Ke kopírovanému záznamu nemáte oprávnění vlastníka.", "info");
                }

                if (v.Setting.HoursFormat == "T" && v.RecP34 != null && v.RecP34.p33ID == BO.p33IdENUM.Cas)
                {
                    v.Rec.Value_Trimmed = BO.Code.Time.ShowAsHHMM(v.Rec.Value_Trimmed);
                }

            }

            v.Toolbar = new MyToolbarViewModel(v.Rec) { AllowArchive = false };
            
            RefreshState_Record(v);

            if (isclone)
            {
                v.MakeClone();
            }

            if (v.Rec.pid > 0 && v.Rec.p40ID_FixPrice==0 && v.Rec.p72ID_AfterTrimming != BO.p72IdENUM._NotSpecified)
            {
                v.IsValueTrimming = true;   //u úkonu je vyplněná korekce -> je třeba zapnout v záznamu korekci
                if ((int)v.Rec.p72ID_AfterTrimming != v.p72ID_DefaultTrimming)
                {
                    v.disp.SetChecked(PosEnum.Trimming, true);

                }
               
            }

            if (v.Rec.pid == 0 && v.lisP40 != null && v.lisP40.Count() > 0 && v.lisP40.Any(p => p.p40FreeHours > p.Cerpano_Hodiny)) //nahodit výchozí paušál
            {
                v.Rec.p40ID_FixPrice = v.lisP40.First(p => p.p40FreeHours > p.Cerpano_Hodiny).pid;  //výchozí vazba na paušální odměnu
            }

            if (v.Rec.pid==0 && v.RecP41 == null)
            {
                v.AutoOpenProjectSearchbox = true;
            }

            return View(v);
        }


        private void InhaleP34(p31Record v, int bitstream)
        {
            if (v.RecP34 == null && v.Rec.p34ID > 0)
            {
                v.RecP34 = Factory.p34ActivityGroupBL.Load(v.Rec.p34ID);
            }
            if (v.RecP34 == null) return;

            v.p34TrimmingFlag = v.RecP34.p34TrimmingFlag;
            v.p34FilesFlag = v.RecP34.p34FilesFlag;
            
            v.p34TagsFlag = v.RecP34.p34TagsFlag;
            v.p34InboxFlag = v.RecP34.p34InboxFlag;

            int intCache = (v.rec_pid == 0 ? Factory.j02UserBL.LoadBitstreamFromUserCache("p31", v.RecP34.pid) : 0);    //pro nový záznam načíst uložená rozšíření z cache
            //int intCache = 0;   //cache nepoužívat
            
            v.p34FilesFlag = v.disp.SetVal(PosEnum.Files, v.p34FilesFlag, bitstream, v.rec_pid, intCache);
            v.p34TrimmingFlag = v.disp.SetVal(PosEnum.Trimming, v.p34TrimmingFlag, bitstream, v.rec_pid, intCache);
            v.p34TagsFlag = v.disp.SetVal(PosEnum.Tags, v.p34TagsFlag, bitstream, v.rec_pid, intCache);
            v.p34InboxFlag = v.disp.SetVal(PosEnum.Inbox, v.p34InboxFlag, bitstream, v.rec_pid, intCache);


            
        }

        private void Handle_Defaults(p31Record v)
        {
            //if (v.SelectedLevelIndex == 0)
            //{
            //    v.SelectedLevelIndex = 5;
            //}
            if (v.p31Date == null)
            {
                v.p31Date = DateTime.Today;
            }
            if (v.rec_pid == 0)
            {
                //vykázat nový úkon
                if (v.Rec.j02ID == 0)
                {
                    v.Rec.j02ID = Factory.CurrentUser.pid;
                }
                if (v.Rec.j02ID == Factory.CurrentUser.pid)
                {
                    v.SelectedComboPerson = Factory.CurrentUser.FullnameDesc;
                }
                else
                {
                    v.SelectedComboPerson = Factory.j02UserBL.Load(v.Rec.j02ID).FullnameDesc;
                }
                if (v.Rec.p34ID == 0)
                {
                    LoadRecordSetting(v);
                    switch (v.Setting.ActivityFlag)
                    {
                        case 0: //první sešit v nabídce sešitů                            
                            var lisP34 = Factory.p34ActivityGroupBL.GetList(new BO.myQueryP34() { p41id = v.Rec.p41ID });
                            if (lisP34.Count() > 0)
                            {
                                v.Rec.p34ID = lisP34.First().pid; v.SelectedComboP34Name = lisP34.First().p34Name;
                            }
                            break;
                        case 1: //sešit z naposledy vykazovaného úkonu
                        case 2: //sešit z naposledy vykazovaného úkonu
                            var recLast = Factory.p31WorksheetBL.LoadMyLastCreated(true);
                            if (recLast != null)
                            {
                                v.Rec.p34ID = recLast.p34ID; v.SelectedComboP34Name = recLast.p34Name;
                                if (v.Setting.ActivityFlag == 2)
                                {
                                    v.Rec.p32ID = recLast.p32ID; v.SelectedComboP32Name = recLast.p32Name;
                                }
                                InhaleP34(v, recLast.p31BitStream);

                            }
                            break;

                    }
                }

                if (v.Rec.p32ID > 0)
                {
                    v.RecP32 = Factory.p32ActivityBL.Load(v.Rec.p32ID);
                    v.Rec.p31MarginHidden = v.RecP32.p32MarginHidden;
                    v.Rec.p31MarginTransparent = v.RecP32.p32MarginTransparent;
                    if (v.Rec.p34ID == 0) v.Rec.p34ID = v.RecP32.p34ID;
                }





            }




        }


        private void RefreshState_Record(p31Record v)
        {
            LoadRecordSetting(v);
            if (v.IsdocLastUpload != null)
            {
                v.disp.SetChecked(PosEnum.Files, true); //nahraný isdoc soubor si vynucuje files box
            }
            if (v.Rec.j02ID == 0)
            {
                v.Rec.j02ID = Factory.CurrentUser.pid; v.SelectedComboPerson = Factory.CurrentUser.FullnameDesc;
            }
            if (v.lisLevelIndex == null)
            {
                v.lisLevelIndex = new List<BO.ListItemValue>();
                if (Factory.p07LevelsCount == 1)
                {
                    v.lisLevelIndex.Add(new BO.ListItemValue() { Text = Factory.getP07Level(5, true), Value = 5 });
                }
                else
                {
                    v.lisLevelIndex.Add(new BO.ListItemValue() { Text = Factory.tra("Všechny úrovně"), Value = 0 });

                    for (int i = 5; i >=1; i--)
                    {
                        if (Factory.getP07Level(i, true) != null)
                        {
                            v.lisLevelIndex.Add(new BO.ListItemValue() { Text = $"{Factory.getP07Level(i, true)} (L{i})", Value = i });
                        }
                    }
                }
            }
            if (v.SelectedLevelIndex == 0)
            {
                v.ProjectEntity = "p41";
            }
            else
            {
                v.ProjectEntity = $"le{v.SelectedLevelIndex}";
            }
            

            if (v.Rec.p41ID > 0)
            {
                if (v.RecP41 == null) v.RecP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID);

                if (string.IsNullOrEmpty(v.SelectedComboProject))
                {
                    v.SelectedComboProject = v.RecP41.FullName;
                }
                v.p61ID = (v.RecP41.p61ID > 0 ? v.RecP41.p61ID : v.RecP41.p61ID_Byp42ID);
                if (v.p61ID > 0)
                {
                    v.p61Name = Factory.p61ActivityClusterBL.Load(v.p61ID).p61Name;
                }
                v.BillingLangIndex = Factory.p41ProjectBL.GetBillingLangIndex(v.RecP41);
                v.BillingLangFlagHtml = Factory.p41ProjectBL.GetBillingLangFlagHtml(v.BillingLangIndex);
                
            }
            else
            {
                v.p61ID = 0;
            }
            if (v.Rec.p34ID > 0 && v.RecP34 == null)
            {
                v.RecP34 = Factory.p34ActivityGroupBL.Load(v.Rec.p34ID);
            }
            
            v.MyQueryInline_Project = $"p07level|int|{v.SelectedLevelIndex}";
        
            if (v.PravaZaJineho == 1)
            {
                v.MyQueryInline_Project = $"j02id_query|int|{v.Rec.j02ID}|p07level|int|{v.SelectedLevelIndex}";
            }
            if (v.RecP34 != null)
            {
                v.MyQueryInline_Project = $"{v.MyQueryInline_Project}|p34id_for_p31_entry|int|{v.RecP34.pid}|p33id_for_p31_entry|int|{(int)v.RecP34.p33ID}|p34incomestatementflag_for_p31_entry|int|{(int)v.RecP34.p34IncomeStatementFlag}";

                if (!v.disp.IsInhaled)
                {
                    InhaleP34(v, 0);
                }

                if (string.IsNullOrEmpty(v.SelectedComboP34Name))
                {
                    v.SelectedComboP34Name = v.RecP34.p34Name;
                }
                if ((v.RecP34.p33ID == BO.p33IdENUM.PenizeBezDPH || v.RecP34.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu) && v.PiecePriceFlag == 0)
                {
                    v.PiecePriceFlag = Factory.CBL.LoadUserParamInt("p31/record-PiecePriceFlag", 1);

                }
                if ((v.RecP34.p33ID == BO.p33IdENUM.PenizeBezDPH || v.RecP34.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu) && v.Rec.j27ID_Billing_Orig == 0 && Factory.Lic.j27ID > 0)
                {
                    v.Rec.j27ID_Billing_Orig = Factory.Lic.j27ID;
                    v.SelectedComboJ27Code = Factory.FBL.LoadCurrencyByID(Factory.Lic.j27ID).j27Code;

                }
                if (v.RecP34.p34ActivityEntryFlag == BO.p34ActivityEntryFlagENUM.AktivitaSeNezadava && v.Rec.p32ID == 0)
                {
                    //výchozí aktivita (skrytá uživateli) - aktivita první v pořadí
                    var lisP32 = Factory.p32ActivityBL.GetList(new BO.myQueryP32() { explicit_orderby = "a.p32Ordinary", p34id = v.RecP34.pid });
                    if (lisP32.Count() > 0)
                    {
                        v.RecP32 = lisP32.First();
                        v.Rec.p32ID = v.RecP32.pid;
                        v.SelectedComboP32Name = v.RecP32.p32Name;

                    }
                }
                if (v.RecP34.p33ID == BO.p33IdENUM.Cas)
                {
                    int timefrom = Factory.CBL.LoadUserParamInt("p31_TimeInputFrom", 8);
                    int timeto = Factory.CBL.LoadUserParamInt("p31_TimeInputTo", 18);
                    var xx = Factory.CBL.LoadUserParamInt("p31_TimeInputInterval", 30);
                    List<string> intervals = new List<string>();

                    for (int i = timefrom * 60; i <= timeto * 60; i += xx)
                    {

                        intervals.Add(BO.Code.Time.GetTimeFromSeconds(i * 60));
                    }
                    v.CasOdDoIntervals = string.Join("|", intervals);


                }
            }

            

            if (v.Rec.p32ID > 0 && v.RecP32 == null)
            {
                v.RecP32 = Factory.p32ActivityBL.Load(v.Rec.p32ID);
            }
            if (v.RecP32 != null && string.IsNullOrEmpty(v.SelectedComboP32Name))
            {
                v.SelectedComboP32Name = v.RecP32.p32Name;
            }

            if (v.ff1 == null)
            {
                v.ff1 = new FreeFieldsViewModel();
                v.ff1.InhaleFreeFieldsView(Factory, v.rec_pid, "p31");
            }
            v.ff1.RefreshInputsVisibility(Factory, v.rec_pid, "p31", v.Rec.p34ID);
            v.ff1.caller = "p31record";

            if (v.reminder == null && v.RecP34 != null && (v.RecP34.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu || v.RecP34.p33ID == BO.p33IdENUM.PenizeBezDPH))
            {
                v.reminder = new ReminderViewModel() { record_pid = v.rec_pid, record_prefix = "p31" };
            }

            if (v.RecP41 != null && v.RecP32 != null && (v.RecP34.p33ID == BO.p33IdENUM.Cas || v.RecP34.p33ID == BO.p33IdENUM.Kusovnik))
            {
                if (v.RecP41.p72ID_BillableHours != BO.p72IdENUM._NotSpecified && v.RecP32.p32IsBillable)
                {
                    v.p72ID_DefaultTrimming = (int)v.RecP41.p72ID_BillableHours;    //výchozí korekce hodin/kusovníku
                }
                if (v.RecP41.p72ID_NonBillable != BO.p72IdENUM._NotSpecified && !v.RecP32.p32IsBillable)
                {
                    v.p72ID_DefaultTrimming = (int)v.RecP41.p72ID_NonBillable;      //výchozí korekce hodin/kusovníku
                }

            }
            if (v.lisP40 == null && v.RecP41 != null && v.RecP34 !=null && v.RecP34.p33ID == BO.p33IdENUM.Cas)
            {
                v.lisP40 = Factory.p40WorkSheet_RecurrenceBL.GetList(new BO.myQueryP40() { p41id = v.RecP41.pid }).Where(p => p.p33ID == 2 || p.p33ID == 5);
                
            }

            if (v.lisP54==null && v.RecP41 != null && v.RecP41.p42IsP54 && v.RecP34 != null && v.RecP34.p33ID == BO.p33IdENUM.Cas)
            {
                v.lisP54 = Factory.p54OvertimeLevelBL.GetList(new BO.myQuery("p54"));   //nabídka přesčasů
            }
            if (v.RecP41 != null && v.RecP41.p28ID_Client>0)
            {
                v.lisP30 = Factory.p28ContactBL.GetList_p30_p31entry(v.RecP41.p28ID_Client);
                
            }


        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p31Record v,string oper, List<IFormFile> files)
        {
            if (oper== "isdoc_upload" && files != null && files.Count()>0)
            {
                Handle_Isdoc_Import(v, files);  //nahrání isdoc souboru
                
                RefreshState_Record(v);
                
                return View(v);

            }
            RefreshState_Record(v);

            if (v.IsPostback)
            {
                Handle_Postback_Record(v);
               
                return View(v);
            }



            if (ModelState.IsValid)
            {
                if (!ValidateBeforeSave(v))
                {
                    return View(v);
                }

                int intP31ID = Handle_Save_Record(v);   //zde save
                if (intP31ID > 0)
                {
                    if (oper== "saveandclone")
                    {
                        return RedirectToAction("Record", new { pid = intP31ID, isclone = true });
                    }
                    if (v.p91ID > 0)
                    {
                        //úkon zakládaný z faktury -> přidat do faktury přes schvalovací UI
                        return RedirectToAction("Index", "p31approveinput", new { prefix = "p31", pids = intP31ID.ToString(), p91id = v.p91ID });
                    }
                    v.SetJavascript_CallOnLoad(intP31ID);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }


        private void Handle_Postback_Record(p31Record v)
        {
            switch (v.PostbackOper)
            {
                case "navic-kusovnik":
                    Factory.CBL.SetUserParam("p31/Record-IsNavicKusovnik", v.IsNavicKusovnik.ToString());
                    if (v.lisKusovnik == null)
                    {
                        v.lisKusovnik = new List<KusovnikInline>();
                    }
                    if (v.RecP41 != null)
                    {
                        var lisP34 = Factory.p34ActivityGroupBL.GetList_WorksheetEntryIn_OneProject(v.RecP41, v.Rec.j02ID).Where(p => p.p33ID == BO.p33IdENUM.Kusovnik);
                        if (lisP34.Count() == 0)
                        {
                            this.AddMessageTranslated("K projektu není k dispozici sešit aktivit pro vykazování kusovníkových úkonů.");
                        }
                        else
                        {
                            v.p34ID_Kusovnik = lisP34.First().pid;
                            v.lisKusovnik.Add(new KusovnikInline());
                            var lisP32 = Factory.p32ActivityBL.GetList(new BO.myQueryP32() { p34id = v.p34ID_Kusovnik });
                        }
                            
                        
                       
                    }
                    

                    break;
                case "kusovnik_add":
                    if (v.lisKusovnik == null)
                    {
                        v.lisKusovnik = new List<KusovnikInline>();
                    }
                    v.lisKusovnik.Add(new KusovnikInline());
                    break;
                case "multidate":
                    if (v.IsMultiDate && !Factory.CurrentUser.IsMobileDisplay())
                    {
                        this.AddMessage("Můžete vykazovat hodiny za více dnů najednou", "info");
                    }                    
                    break;
                case "j02id":
                    v.Element2Focus = "cmdComboRec_p41ID";
                    break;
                case "p34id":

                    InhaleP34(v, v.disp.GetBitStream());
                    

                    if (v.Rec.p34ID > 0)
                    {
                        if (v.Rec.p41ID > 0)
                        {
                            v.Element2Focus = "cmdComboRec_p32ID";
                        }
                        else
                        {
                            v.Element2Focus = "cmdComboRec_p41ID";
                        }

                    }
                    else
                    {
                        v.Element2Focus = "cmdComboRec_p34ID";
                    }

                    v.Rec.p32ID = 0; v.RecP32 = null; v.SelectedComboP32Name = null;    //musí být před RefreshState(v)
                    break;
                case "levelindex":
                    v.Rec.p41ID = 0; v.SelectedComboProject = null; v.Element2Focus = "cmdComboRec_p41ID";
                    break;
                case "p41id":
                    v.Rec.p56ID = 0; v.SelectedComboTask = null; v.IsShowP56Combo = false;
                    if (v.Rec.p41ID > 0)
                    {
                        if (v.RecP34 !=null && v.RecP34.p34ActivityEntryFlag == BO.p34ActivityEntryFlagENUM.AktivitaSeNezadava)
                        {
                            v.Element2Focus = "Rec_Value_Orig";
                        }
                        else
                        {
                            v.Element2Focus = "cmdComboRec_p32ID";
                        }
                        
                        
                        v.RecP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID);
                        v.SelectedComboProject = v.RecP41.FullName;
                        InhaleP56Combo(v);
                        if (v.Rec.p34ID > 0)
                        {
                            var lisP34 = Factory.p34ActivityGroupBL.GetList(new BO.myQueryP34() { p41id = v.Rec.p41ID });
                            if (lisP34.Count() > 0 && !lisP34.Any(p => p.pid == v.Rec.p34ID))
                            {
                                v.RecP34 = lisP34.First();
                                v.Rec.p34ID = v.RecP34.pid; v.SelectedComboP34Name = v.RecP34.p34Name;
                                if (v.Rec.p32ID > 0)
                                {
                                    v.Rec.p32ID = 0; v.SelectedComboP32Name = null;
                                }
                            }
                        }
                    }
                    else
                    {
                        v.Element2Focus = "cmdComboRec_p41ID";
                    }


                    break;
                case "dispozice":
                    v.IsValueTrimming = v.disp.IsTrimming;
                    break;
                case "isvaluetrimming":
                    if (v.Rec.p72ID_AfterTrimming == BO.p72IdENUM.Fakturovat || v.Rec.p72ID_AfterTrimming == BO.p72IdENUM.FakturovatPozdeji)
                    {
                        v.Rec.Value_Trimmed = v.Rec.Value_Orig;
                        
                    }
                    else
                    {
                        v.Rec.Value_Trimmed = "0";
                    }
                    
                    break;
                case "p32id":
                    if (v.Rec.p32ID > 0)
                    {
                        
                        v.Element2Focus = "Rec_Value_Orig";
                        //v.Element2Focus = "Rec_p31Text";
                    }
                    else
                    {
                        v.Element2Focus = "cmdComboRec_p32ID";
                    }
                    RefreshState_Record(v);

                    if (v.rec_pid == 0 && v.RecP32 != null)
                    {
                        
                        v.DefaultText = InhaleActivityDefaultText(v);

                        v.Rec.p31MarginHidden = v.RecP32.p32MarginHidden;
                        v.Rec.p31MarginTransparent = v.RecP32.p32MarginTransparent;
                        if (v.RecP32.x15ID != BO.x15IdEnum.Nic)
                        {
                            string strCountryCode = v.RecP41.j18CountryCode;
                            if (strCountryCode == null && v.Rec.j02ID>0)
                            {
                                strCountryCode = Factory.j02UserBL.Load(v.Rec.j02ID).j02CountryCode;
                            }
                            if (strCountryCode==null)
                            {
                                strCountryCode = Factory.Lic.x01CountryCode;
                            }
                            v.Rec.VatRate_Orig = Factory.p53VatRateBL.NajdiSazbu(v.p31Date.Value, v.RecP32.x15ID, v.Rec.j27ID_Billing_Orig, strCountryCode);
                        }
                        if (v.RecP32.p32Value_Default > 0 && v.p68ID==0 && v.Rec.TimeFrom==null && v.Rec.TimeUntil==null)
                        {
                            if ((v.RecP34.p33ID == BO.p33IdENUM.Cas || v.RecP34.p33ID == BO.p33IdENUM.Kusovnik) && BO.Code.Bas.InDouble(v.Rec.Value_Orig) == 0)
                            {
                                v.Rec.Value_Orig = v.RecP32.p32Value_Default.ToString();
                            }
                            if ((v.RecP34.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu || v.RecP34.p33ID == BO.p33IdENUM.PenizeBezDPH) && v.Rec.Amount_WithoutVat_Orig == 0)
                        {
                                v.Rec.Amount_WithoutVat_Orig = v.RecP32.p32Value_Default;
                                if (v.RecP34.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu)
                                {
                                    v.Rec.Amount_Vat_Orig = v.Rec.Amount_WithoutVat_Orig * v.Rec.VatRate_Orig / 100;
                                    v.Rec.Amount_WithVat_Orig = v.Rec.Amount_Vat_Orig + v.Rec.Amount_WithoutVat_Orig;
                                }
                            }
                        }
                        
                    }
                    break;
                case "today":
                    v.p31Date = DateTime.Today;

                    break;
                case "clear_o23id":
                    v.Rec.o23ID = 0;
                    v.Doc_o23Name = null;
                    this.AddMessage("Změny je třeba potvrdit tlačítkem [Uložit změny]", "info");
                    break;
                case "clear_p49id":
                    v.Rec.p49ID = 0;                    
                    this.AddMessage("Změny je třeba potvrdit tlačítkem [Uložit změny]", "info");
                    break;
                case "pravazajineho":
                    Factory.CBL.SetUserParam("p31/Record-PravaZaJineho", v.PravaZaJineho.ToString());
                    break;
               
            }

            if (v.PostbackOper != "p32id")
            {
                RefreshState_Record(v);
            }
        }

        private int Handle_Save_Record(p31Record v) //vrací uložené p31id
        {
            BO.p31WorksheetEntryInput c = new BO.p31WorksheetEntryInput();
            if (v.rec_pid > 0)
            {
                c.SetPID(v.rec_pid);
            }
            //c.p31Date = Convert.ToDateTime(v.p31Date);
            if (v.IsMultiDate)
            {
                var arr = BO.Code.Bas.ConvertString2List(v.MultiDate, ",");
                foreach(var s in arr)
                {
                    c.Addp31Date(BO.Code.Bas.String2Date(s));
                }
            }
            else
            {
                c.Addp31Date(v.p31Date.Value);
            }
            
            c.j02ID = v.Rec.j02ID;
            c.p41ID = v.Rec.p41ID;
            c.o23ID = v.Rec.o23ID;
            c.p34ID = v.Rec.p34ID;
            c.p32ID = v.Rec.p32ID;
            c.p56ID = v.Rec.p56ID;
            c.p49ID = v.Rec.p49ID;
            
            c.Value_Orig = v.Rec.Value_Orig;

            switch (v.RecP34.p33ID)
            {
                case BO.p33IdENUM.Cas:
                    if (v.Setting.TimesheetEntryByMinutes)
                    {
                        c.p31HoursEntryflag = BO.p31HoursEntryFlagENUM.Minuty;
                    }
                    else
                    {
                        c.p31HoursEntryflag = BO.p31HoursEntryFlagENUM.Hodiny;
                    }
                    c.p54ID = v.Rec.p54ID;  //stupeň přesčasu
                    c.p40ID_FixPrice = v.Rec.p40ID_FixPrice;  //vazba na paušální odměnu

                    break;
                case BO.p33IdENUM.PenizeBezDPH:
                case BO.p33IdENUM.PenizeVcDPHRozpisu:
                    c.j27ID_Billing_Orig = v.Rec.j27ID_Billing_Orig;
                    c.p31Code = v.Rec.p31Code;
                    c.j19ID = v.Rec.j19ID;
                    c.p35ID = v.Rec.p35ID;

                    c.p31Calc_PieceAmount = v.Rec.p31Calc_PieceAmount;
                    c.p31Calc_Pieces = v.Rec.p31Calc_Pieces;
                    

                    c.p31PostCode = v.Rec.p31PostCode;
                    c.p31PostFlag = v.Rec.p31PostFlag;
                    c.p31PostRecipient = v.Rec.p31PostRecipient;

                    break;
                case BO.p33IdENUM.Kusovnik:
                    c.p35ID = v.Rec.p35ID;
                    break;
            }

            if (v.RecP32 !=null)
            {
                if (v.RecP32.p32ManualFeeFlag == 1)
                {
                    c.ManualFee = v.Rec.ManualFee;  //na vstupu je ručně zadaný pevný honorář
                }
                if (v.RecP32.p32IsSupplier)
                {
                    c.p28ID_Supplier = v.Rec.p28ID_Supplier;
                }
            }
            

            c.Amount_WithoutVat_Orig = v.Rec.Amount_WithoutVat_Orig;
            c.VatRate_Orig = v.Rec.VatRate_Orig;
            c.Amount_Vat_Orig = v.Rec.Amount_Vat_Orig;
            c.Amount_WithVat_Orig = v.Rec.Amount_WithVat_Orig;

            c.p31Text = v.Rec.p31Text;
            c.p31TextInternal = v.Rec.p31TextInternal;
            c.TimeFrom = v.Rec.TimeFrom;
            c.TimeUntil = v.Rec.TimeUntil;

            if (v.IsValueTrimming)
            {
                c.p72ID_AfterTrimming = v.Rec.p72ID_AfterTrimming;
                c.Value_Trimmed = v.Rec.Value_Trimmed;

            }
            else
            {
                if (v.p72ID_DefaultTrimming > 0)
                {
                    c.p72ID_AfterTrimming = (BO.p72IdENUM)v.p72ID_DefaultTrimming;  //úkon má korekci automaticky podle výchozí korekce nastavené v projektu
                    c.Value_Trimmed = "0";
                }
            }
            c.p31BitStream = v.disp.GetBitStream();

            

            c.ValidUntil = v.Toolbar.GetValidUntil(c);
            c.ValidFrom = v.Toolbar.GetValidFrom(c);
            c.approve_guid = v.GuidApprove;
            c.p28ID_ContactPerson = v.Rec.p28ID_ContactPerson;

            if (v.IsNavicKusovnik && v.lisKusovnik != null && v.lisKusovnik.Count() > 0)
            {
                //zvalidovat kusovníkové úkony
                foreach (var kus in v.lisKusovnik)
                {
                    var recP32 = Factory.p32ActivityBL.Load(kus.p32ID);
                    if (recP32 == null)
                    {
                        this.AddMessageTranslated("Pro řádek kusovníku chybí aktivita.");
                        return 0;
                    }
                    if (string.IsNullOrEmpty(kus.p31Text) && recP32.p32IsTextRequired)
                    {
                        this.AddMessageTranslated("Kusovník vyžaduje zadat text úkonu.");
                        return 0;
                    }
                    if (kus.Pocet == 0)
                    {
                        this.AddMessageTranslated("Kusovník vyžaduje zadat počet kusů.");
                        return 0;
                    }
                    
                }
            }

            c.pid = Factory.p31WorksheetBL.SaveOrigRecord(c, v.RecP34.p33ID, v.ff1.inputs);
            if (c.pid > 0)
            {
                if (v.disp.IsTags)
                {
                    Factory.o51TagBL.SaveTagging("p31", c.pid, v.TagPids);
                }

               
                if (v.disp.IsFiles)
                {
                    Factory.o27AttachmentBL.SaveChangesAndUpload(v.UploadGuid, "p31", c.pid);
                }
                if (v.p68ID > 0)
                {
                    Factory.CBL.DeleteRecord("p68", v.p68ID);   //odstranit záznam ze stopek
                }
               

                if (v.IsNavicKusovnik && v.lisKusovnik !=null && v.lisKusovnik.Count() > 0)
                {
                    //uložit k časovému úkonu ještě kusovníky (pouze režim nového záznamu
                    foreach(var kus in v.lisKusovnik)
                    {                        
                        var recMaster = Factory.p31WorksheetBL.Load(c.pid); //vzorový úkon
                        var recSlave = new BO.p31WorksheetEntryInput() {p31MasterID=c.pid, p31Text=kus.p31Text,p34ID=v.p34ID_Kusovnik,p32ID=kus.p32ID,Value_Orig=kus.Pocet.ToString(), p41ID=recMaster.p41ID,p56ID=recMaster.p56ID, j02ID = recMaster.j02ID };
                        recSlave.p31Date = new List<DateTime>(); recSlave.p31Date.Add(recMaster.p31Date);
                        recSlave.p40ID_FixPrice = recMaster.p40ID_FixPrice; recSlave.p54ID = recMaster.p54ID;                      
                        Factory.p31WorksheetBL.SaveOrigRecord(recSlave, BO.p33IdENUM.Kusovnik, null);
                    }
                }

                if (v.GuidApprove != null)
                {
                    //úprava úkonu, který se právě schvaluje
                    var mq = new BO.myQueryP31();
                    mq.SetPids(c.pid.ToString());
                    var lisP31 = this.Factory.p31WorksheetBL.GetList(mq);

                    BO.p72IdENUM p72id = BO.p72IdENUM.Fakturovat;
                    var recTemp = Factory.p31WorksheetBL.LoadTempRecord(c.pid, v.GuidApprove);
                    if (recTemp != null) p72id = recTemp.p72ID_AfterApprove;

                    Factory.p31WorksheetBL.DeleteTempRecord(v.GuidApprove, c.pid);
                    bool bolSetDefaultApproveState = Factory.CurrentUser.IsHes(131072); //zda nahazovat automaticky fakturační status u schvalovaného úkonu
                    
                    BL.Code.p31Support.SetupTempApproving(this.Factory, lisP31, v.GuidApprove, 0, bolSetDefaultApproveState, p72id);
                }

                if (v.reminder != null)
                {
                    v.reminder.SaveChanges(Factory, c.pid, c.p31Date.First());
                }


                return c.pid;
            }
            else
            {
                if (string.IsNullOrEmpty(v.Rec.p31Text)) v.Element2Focus = "Rec_p31Text";
                if (v.Rec.p41ID == 0) v.Element2Focus = "cmdComboRec_p41ID";
                if (v.Rec.p32ID == 0) v.Element2Focus = "cmdComboRec_p32ID";

            }

            return 0;
        }

        private void LoadRecordSetting(p31Record v)
        {
            if (v.Setting == null)
            {
                v.Setting = new Models.p31oper.hesViewModel() { HoursFormat = Factory.CurrentUser.j02DefaultHoursFormat, HesBitStream = Factory.CurrentUser.j02HesBitStream };
                v.Setting.InhaleSetting();

            }
        }

        private bool ValidateBeforeSave(p31Record v)
        {
            if (v.p31Date == null)
            {
                this.AddMessage("Chybí vyplnit datum."); return false;
            }
            if (v.Rec.p34ID == 0)
            {
                this.AddMessage("Chybí vyplnit sešit."); v.Element2Focus = "cmdComboRec_p34ID"; return false;
            }
            return true;
        }

        private BO.p31RecDisposition InhalePermissions(p31Record v, BO.p31Worksheet recP31)
        {
            return Factory.p31WorksheetBL.InhaleRecDisposition(recP31);


        }


        private string Record_RecalcDuration_getInfo(int seconds, string p41id, string hours_orig)
        {
            if (seconds == 0) return null;

            int round2minutes = Factory.Lic.x01Round2Minutes;
            int projectid = BO.Code.Bas.InInt(p41id);
            if (projectid > 0)
            {
                var recP41 = Factory.p41ProjectBL.Load(projectid);
                if (recP41.p28ID_Client > 0)
                {
                    var recP28 = Factory.p28ContactBL.Load(recP41.p28ID_Client);
                    if (recP28.p28Round2Minutes > 0) round2minutes = recP28.p28Round2Minutes;
                }
                if (recP41.p41Round2Minutes > 0)
                {
                    round2minutes = recP41.p41Round2Minutes;    //zaokrouhlování na míru na projektu
                }
            }
            string s = null;
            
            if (hours_orig != null && hours_orig.Contains(":"))
            {
                s = $"{BO.Code.Time.GetTimeFromSeconds(seconds)} (HH:mm) -> {Convert.ToDouble(seconds) / 60 / 60} (h)";
            }
            else
            {
                s = $"{Convert.ToDouble(seconds) / 60 / 60} (h) -> {BO.Code.Time.GetTimeFromSeconds(seconds)} (HH:mm)";
            }
            if (round2minutes > 0)
            {
                int secs1 = seconds;
                int secs2 = BO.Code.Time.RoundSeconds(seconds, round2minutes * 60);

                if (secs1 != secs2)
                {
                    s = $"{s}, po {round2minutes}ti minutovém zaokrouhlení systém uloží jako {BO.Code.Time.GetTimeFromSeconds((double)secs2)} ({Convert.ToDouble(secs2) / 60 / 60})";
                }
            }
            return s;



        }
        public Models.p31oper.p31CasOdDo Record_RecalcDuration(string caller, string hours, string timefrom, string timeuntil, string p41id, string hoursformat)
        {
            //caller může být: hours/timefrom/timeuntil
            var ret = new UI.Models.p31oper.p31CasOdDo();

            if (!string.IsNullOrEmpty(hours) && caller=="hours")
            {
                hours = hours.Trim();
                if (hours.Length >= 4 && !hours.Contains(":") && !hours.Contains(","))
                {
                    hours = $"{hours.Substring(0, 2)}:{hours.Substring(2, 2)}";
                    ret.update_orig_hours = "true";
                }
                            
            }
            if (!string.IsNullOrEmpty(timefrom))
            {
                timefrom = timefrom.Replace(",", ":").Replace(".", ":");
                if (timefrom.Length>=4 && BO.Code.Bas.InInt(timefrom)>0)
                {
                    timefrom = $"{timefrom.Substring(0, 2)}:{timefrom.Substring(2, 2)}";
                }
            }
            if (!string.IsNullOrEmpty(timeuntil))
            {
                timeuntil = timeuntil.Replace(",", ":").Replace(".", ":");
                if (timeuntil.Length >= 4 && BO.Code.Bas.InInt(timeuntil) > 0)
                {
                    timeuntil = $"{timeuntil.Substring(0, 2)}:{timeuntil.Substring(2, 2)}";
                }
            }

            if (string.IsNullOrEmpty(hoursformat)) hoursformat = "N";

            

            int hrs = BO.Code.Time.ConvertTimeToSeconds(hours);
            int t1 = BO.Code.Time.ConvertTimeToSeconds(timefrom);
            int t2 = BO.Code.Time.ConvertTimeToSeconds(timeuntil);


            if (hrs != 0 && t1 <= 0 && t2 <= 0)    //bez zadání času od-do
            {
                ret.t1 = "00:00";
                ret.t2 = "00:00";
                ret.duration = BO.Code.Time.GetTimeFromSeconds(hrs);
                ret.info = Record_RecalcDuration_getInfo(hrs, p41id, hours);
                return ret;
            }

            if (caller == "hours" && hrs > 0)
            {
                if (t1 > 0)
                {
                    t2 = t1 + hrs;
                }
                else
                {
                    if (t2 > 0)
                    {
                        t1 = t2 - hrs;
                    }
                }

            }
            if (caller == "timefrom" && hrs > 0)
            {
                t2 = t1 + hrs;
            }
            //if (t2 > 26 * 60 * 60)
            //{
            //    ret.error = Factory.tra("[Čas do] musí být menší než 24:00.");
            //}
            if (t1 > 0 && t2 == 0)
            {
                t2 = t1 + 60 * 60;
            }
            if (t2 > 24 * 60 * 60)  //čas-do vykázaný přes půlnoc
            {
                if (t2 > 36 * 60 * 60)
                {
                    t2 = 36 * 60 * 60;
                }
                t2 = t2-24*60*60;
            }
            //if (t1 > t2 && hrs>8*60*60) //pokud vykazuji přes půlnoc, tak hodiny musí být pod 8 hodin
            //{
            //    ret.error = Factory.tra("[Čas do] musí být větší než [Čas od].");
               
            //}
            if (t1 < 0)
            {
                t1 = BO.Code.Time.ConvertTimeToSeconds("01:00");
                ret.error = Factory.tra("[Čas do] musí být větší než [Čas od].");
            }
            ret.t1 = BO.Code.Time.GetTimeFromSeconds(t1);
            ret.t2 = BO.Code.Time.GetTimeFromSeconds(t2);
            if (t2 > t1)
            {
                ret.duration = BO.Code.Time.GetTimeFromSeconds(t2 - t1);
                ret.info = Record_RecalcDuration_getInfo(t2 - t1, p41id, hours);
            }
            if (t1>0 && t2>0 && t1>t2)
            {
                //čas od-do přes půlnoc
                ret.duration = BO.Code.Time.GetTimeFromSeconds(t2 + (24 * 60 * 60) - t1);
                ret.info = Record_RecalcDuration_getInfo(t2 + (24 * 60 * 60) - t1, p41id, hours);
            }


            if (ret.update_orig_hours==null && (caller != "hours" || hoursformat=="T" || (t1>0 && t2>0)))
            {
                ret.update_orig_hours = "true";
            }
            return ret;
        }


       

        private void InhaleP56Combo(p31Record v)
        {
            v.IsShowP56Combo = false;
            if (v.Rec.p41ID == 0 || !Factory.CurrentUser.j04IsModule_p56) return;
            if (Factory.p56TaskBL.GetList(new BO.myQueryP56() { p41id = v.Rec.p41ID, j02id = v.Rec.j02ID, IsRecordValid = true }).Count() > 0)
            {
                v.IsShowP56Combo = true;
            }

        }

        private void InhaleFromStopWatch(p31Record v)
        {
            var c = Factory.p68StopWatchBL.Load(v.p68ID);

            v.p31Date = new DateTime(c.DateInsert.Value.Year, c.DateInsert.Value.Month, c.DateInsert.Value.Day);
            if (c.p41ID > 0)
            {
                v.Rec.p41ID = c.p41ID;
                v.RecP41 = Factory.p41ProjectBL.Load(c.p41ID);
                //v.SelectedLevelIndex = v.RecP41.p07Level;
                InhaleP56Combo(v);
            }
            if (c.p32ID > 0)
            {
                var recP32 = Factory.p32ActivityBL.Load(c.p32ID);
                v.Rec.p32ID = recP32.pid;
                v.Rec.p34ID = recP32.p34ID;
                if (v.RecP34 !=null && v.Rec.p34ID != v.RecP34.pid)
                {
                    v.RecP34 = Factory.p34ActivityGroupBL.Load(v.Rec.p34ID);
                }
                v.SelectedComboP32Name = recP32.p32Name;
                v.SelectedComboP34Name = recP32.p34Name;
            }
            else
            {
                if (v.RecP34 != null && v.RecP34.p33ID !=BO.p33IdENUM.Cas)
                {
                    //předvyplnit časový sešit
                    v.RecP34 = null;
                    v.RecP32 = null;
                }
            }
            v.Rec.p31Text = c.p68Text;
            v.Rec.Value_Orig = BO.Code.Time.GetTimeFromSeconds((double)c.p68Duration);
            

            v.Element2Focus = "cmdComboRec_p32ID";
        }
        private void InhaleFromUctenka(p31Record v,BO.o23Doc recO23,BO.o18DocType recO18)
        {            
            v.p31Date = recO23.o23FreeDate01;
            v.Rec.p34ID = recO23.p34ID_Expense;
            if (recO23.p32ID_Expense==0 && recO18.p32ID_Uctenka > 0)
            {
                recO23.p32ID_Expense = recO18.p32ID_Uctenka;
            }
            if (recO23.p32ID_Expense > 0)
            {
                v.Rec.p32ID = recO23.p32ID_Expense;
                v.SelectedComboP32Name = Factory.p32ActivityBL.Load(v.Rec.p32ID).p32Name;
            }
          
            if (recO23.j27ID_Expense > 0)
            {
                v.Rec.j27ID_Billing_Orig = recO23.j27ID_Expense;
                v.SelectedComboJ27Code = Factory.FBL.LoadCurrencyByID(recO23.j27ID_Expense).j27Code;
            }
            v.SelectedComboP34Name = Factory.p34ActivityGroupBL.Load(v.Rec.p34ID).p34Name;
            v.Rec.p31Text = recO23.o23Name ;
            if (recO23.p41ID_Expense > 0)
            {
                v.Rec.p41ID = recO23.p41ID_Expense;
                v.SelectedComboProject = Factory.p41ProjectBL.Load(recO23.p41ID_Expense).FullName;
            }
            v.Rec.j02ID = recO23.j02ID_Owner;
            if (recO23.j02ID_Owner == Factory.CurrentUser.pid)
            {
                v.SelectedComboPerson = Factory.CurrentUser.FullnameDesc;
            }
            else
            {
                v.SelectedComboPerson = Factory.j02UserBL.Load(recO23.j02ID_Owner).FullnameDesc;
            }
            
            v.Rec.p31Code = recO23.o23Code;
            v.Rec.p31Text = recO23.o23Name;
            
            v.Rec.Amount_WithVat_Orig = recO23.o23FreeNumber01;
            v.Rec.VatRate_Orig = recO23.o23FreeNumber02;
            v.Rec.Amount_WithoutVat_Orig = recO23.o23FreeNumber03;            
            v.Rec.Amount_Vat_Orig = recO23.o23FreeNumber04;
        }
        private void InhaleFromPlan(p31Record v)
        {
            var c = Factory.p49FinancialPlanBL.Load(v.p49ID);
            v.Rec.p49ID = c.pid;
            v.Rec.p41ID = c.p41ID;
            v.p31Date = c.p49Date;
            v.RecP41 = Factory.p41ProjectBL.Load(c.p41ID);
            //v.SelectedLevelIndex = v.RecP41.p07Level;
            InhaleP56Combo(v);

            var recP32 = Factory.p32ActivityBL.Load(c.p32ID);
            v.Rec.p32ID = recP32.pid;
            v.Rec.p34ID = recP32.p34ID;
            v.SelectedComboP32Name = recP32.p32Name;
            v.SelectedComboP34Name = recP32.p34Name;
            v.Rec.p56ID = c.p56ID;
            v.SelectedComboTask = c.p56Name;
            v.Rec.p31Calc_Pieces = c.p49Pieces;
            v.Rec.p31Calc_PieceAmount = c.p49PieceAmount;

            v.Rec.p31Text = c.p49Text;
            v.Rec.Amount_WithoutVat_Orig = c.p49Amount;


        }

        public string GetText(int p31id)
        {
            return this.Factory.p31WorksheetBL.Load(p31id).p31Text;
        }

        private string InhaleActivityDefaultText(p31Record v)
        {
            switch (v.BillingLangIndex)
            {
                case 1:
                    return v.RecP32.p32DefaultWorksheetText_Lang1;
                    
                case 2:
                    return v.RecP32.p32DefaultWorksheetText_Lang2;
                    
                case 3:
                    return v.RecP32.p32DefaultWorksheetText_Lang3;
                    
                case 4:
                    return v.RecP32.p32DefaultWorksheetText_Lang4;
                    
            }
            return v.RecP32.p32DefaultWorksheetText;
            

        }

        private void Handle_Isdoc_Import(p31Record v, List<IFormFile> files)
        {
            var formFile = files.First();
            var strPath = $"{Factory.TempFolder}\\{v.UploadGuid}_{formFile.FileName}";
            using (var stream = new FileStream(strPath, FileMode.Create))
            {
                formFile.CopyTo(stream);
            }
            if (formFile.FileName.ToLower().Contains(".pdf"))
            {
                var pdfsup = new UI.PdfSupport();
                strPath=pdfsup.SaveIsdocAttachmentToTemp(strPath, $"{Factory.TempFolder}\\{BO.Code.Bas.GetGuid()}.xml");
                if (strPath == null)
                {
                    this.AddMessageTranslated("PDF soubor neobsahuje ISDOC soubor.");
                    return;
                }

            }
            var xmldoc = new XmlDocument();
            xmldoc.Load(strPath);
            var xx = BL.Code.p31Support.ImportIsDoc(xmldoc.InnerXml, null);
            v.IsdocLastUpload = formFile.FileName;
            v.IsdocLastDokladText = xx.doklad_text;            
            v.Rec.p31Code = xx.doklad_id;
            v.Rec.Amount_WithoutVat_Orig = xx.castka_bezdph;
            v.Rec.Amount_WithVat_Orig = xx.castka_vcdphh;
            v.Rec.Amount_Vat_Orig = xx.castka_dph;
            v.Rec.VatRate_Orig = xx.sazba_dph;            
            v.Rec.p31Text = xx.doklad_text;
            if (xx.mena != null)
            {
                var recJ27 = Factory.FBL.LoadCurrencyByCode(xx.mena);
                if (recJ27 != null)
                {
                    v.SelectedComboJ27Code = recJ27.j27Code;
                    v.Rec.j27ID_Billing_Orig = recJ27.pid;
                }
            }
            if (xx.dodavatel_ico != null)    //doplnit projekt nebo dodavatele
            {
                var recP28 = Factory.p28ContactBL.LoadByICO(xx.dodavatel_ico, 0);
                if (recP28 != null)
                {
                    if (v.Rec.p41ID == 0)
                    {
                        var lis = Factory.p41ProjectBL.GetList(new BO.myQueryP41("le5") { p28id = recP28.pid });
                        if (lis.Count() > 0)
                        {
                            v.SelectedComboProject = lis.First().FullName;
                            v.Rec.p41ID = lis.First().pid;
                        }
                    }
                    if (v.Rec.p28ID_Supplier == 0)
                    {
                        v.Rec.p28ID_Supplier = recP28.pid;
                        v.SelectedComboSupplier = recP28.p28Name;
                    }
                }
                else
                {
                    this.AddMessageTranslated($"Subjekt {xx.dodavatel_nazev} (IČO: {xx.dodavatel_ico}) není zaveden v kontaktech.", "info");
                }
            }
            
            
            v.p31Date = xx.datum_duzp;

            Factory.o27AttachmentBL.CreateTempInfoxFile(v.UploadGuid, "p31", $"{v.UploadGuid}_{formFile.FileName}", formFile.FileName, formFile.ContentType);
        }
    }
}
