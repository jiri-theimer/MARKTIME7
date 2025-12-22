using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using UI.Models;
using UI.Models.capacity;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class p41Controller : BaseController
    {
        public IActionResult Info(int pid)
        {
            return Tab1(pid, "info", "p41");
        }

        public IActionResult Tab1(int pid, string caller, string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) prefix = "p41";

            var v = new p41Tab1() { Factory = this.Factory, prefix = prefix, pid = pid, caller = caller };

            v.Rec = Factory.p41ProjectBL.Load(v.pid);
            if (v.Rec != null)
            {

                v.RecSum = Factory.p41ProjectBL.LoadSumRow(v.Rec.pid);

                v.SetTagging();

                v.SetFreeFields(0);

                v.lisP26 = Factory.p41ProjectBL.GetList_p26(v.pid);
            }

            return View(v);
        }
        public IActionResult Dashboard(int pid, string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) prefix = "p41";

            var v = new p41Tab1() { Factory = this.Factory, prefix = prefix, pid = pid, caller = "portal" };

            v.Rec = Factory.p41ProjectBL.Load(v.pid);
            if (v.Rec != null)
            {
                v.RecSum = Factory.p41ProjectBL.LoadSumRow(v.Rec.pid);

                v.SetTagging();

                v.SetFreeFields(0);

            }

            return View(v);
        }

        public IActionResult TabR01(string master_entity, int master_pid, string caller)
        {
            if (master_pid == 0) return this.StopPageSubform("master_pid missing");

            var v = new p41TabR01ViewModel() { p41id = master_pid, timeline = new CapacityTimelineJ02ViewModel() };
            v.timeline.p41ID = v.p41id;
            v.timeline.UserKeyBase = "TabR01";


            v.HasOwnerPermissions = Factory.p41ProjectBL.InhaleRecDisposition(v.p41id).OwnerAccess;
            v.timeline.IsReadOnly = !v.HasOwnerPermissions;

            v.lisR04 = Factory.p41ProjectBL.GetList_r04(v.p41id, 0);

            if (caller == "grid")
            {
                Factory.CBL.SaveLastCallingRecPid(master_entity, master_pid, "grid", true, false, null);
            }

            v.RecP41 = Factory.p41ProjectBL.Load(v.p41id);
            v.timeline.RecP41 = v.RecP41;


            return View(v);
        }
        public IActionResult TabP40(string master_entity, int master_pid, string caller)
        {
            if (master_pid == 0) return this.StopPageSubform("master_pid missing");

            var v = new p41TabP40ViewModel() { p41id = master_pid };
            v.HasOwnerPermissions = Factory.p41ProjectBL.InhaleRecDisposition(v.p41id).OwnerAccess;
            v.lisP40 = Factory.p40WorkSheet_RecurrenceBL.GetList(new BO.myQueryP40() { p41id = v.p41id });

            if (caller == "grid")
            {
                Factory.CBL.SaveLastCallingRecPid(master_entity, master_pid, "grid", true, false, null);
            }


            return View(v);
        }


        public IActionResult Record(int pid, bool isclone, string levelprefix, int p42id, int p28id)
        {
            var v = new p41Record() { rec_pid = pid, rec_entity = "p41", TempGuid = BO.Code.Bas.GetGuid(), UploadGuid = BO.Code.Bas.GetGuid(), SelectedComboOwner = Factory.CurrentUser.FullnameDesc, Element2Focus = "Rec_p41Name" };
            v.BillingMemo = new PellEditorViewModel();
            v.Rec = new BO.p41Project() { p42ID = p42id, p41BillingFlag = BO.p41BillingFlagEnum.CenikDedit };
            
            v.Rec.p07ID = Factory.lisP07.Where(p => !p.isclosed).OrderByDescending(p => p.p07Level).First().pid;
            v.disp = new DispoziceViewModel();
            v.disp.InitItems("p41", Factory);
            v.lisP26 = new List<p26Repeater>();

            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p41ProjectBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.RecP42 = Factory.p42ProjectTypeBL.Load(v.Rec.p42ID);
                v.SelectedP07ID = v.Rec.p07ID;
                v.SetTagging(Factory.o51TagBL.GetTagging("p41", v.rec_pid));

                v.SelectedComboP92Name = v.Rec.p92Name;
                v.SelectedComboP42Name = v.Rec.p42Name;
                v.SelectedComboJ18Name = v.Rec.j18Name;
                
                if (v.Rec.p41ParentID > 0)
                {
                    v.RecParent = Factory.p41ProjectBL.Load(v.Rec.p41ParentID);
                    v.SelectedComboParent = v.RecParent.FullName;
                    v.SelectedParentLevelIndex = v.RecParent.p07Level;
                }
                if (v.Rec.p28ID_Client > 0)
                {
                    v.SelectedComboClient = v.Rec.Client;
                }
                if (v.Rec.p28ID_Billing > 0)
                {
                    v.SelectedComboOdberatel = Factory.p28ContactBL.Load(v.Rec.p28ID_Billing).p28Name;
                }
                if (v.Rec.p61ID > 0)
                {
                    v.SelectedComboP61Name = Factory.p61ActivityClusterBL.Load(v.Rec.p61ID).p61Name;
                }
                if (v.Rec.p15ID > 0)
                {
                    v.SelectedComboP15Name = Factory.p15LocationBL.Load(v.Rec.p15ID).p15Name;
                }
                if (v.Rec.p41BillingMemo200 != null && v.BillingMemo !=null)
                {
                    v.BillingMemo.HtmlValue = Factory.p41ProjectBL.LoadBillingMemo(v.rec_pid);
                }
                


                    var perm = Factory.p41ProjectBL.InhaleRecDisposition(v.rec_pid, v.Rec);
                if (!perm.OwnerAccess)
                {
                    return this.StopPage(true, "Nemáte oprávnění k editaci karty záznamu.");
                }

                InhaleDisp(v, v.Rec.p41BitStream);

                if (v.disp.IsProjectContacts)
                {
                    var lisP26 = Factory.p41ProjectBL.GetList_p26(v.rec_pid).ToList();
                    foreach (var c in lisP26)
                    {
                        v.lisP26.Add(new p26Repeater()
                        {
                            TempGuid = BO.Code.Bas.GetGuid(),
                            pid = c.pid,
                            p26Name = c.p26Name,
                            p28ID = c.p28ID,
                            ComboP28 = c.p28Name,
                            p27ID = c.p27ID
                        });
                    }
                }

                v.SelectedComboOwner = v.Rec.Owner;

                if (!InhalePermissions(v))
                {
                    return this.StopPage(true, "Nemáte oprávnění k editaci karty záznamu.");
                }

                if (v.Rec.p41BillingFlag == BO.p41BillingFlagEnum._NotSpecified) v.Rec.p41BillingFlag = BO.p41BillingFlagEnum.CenikDedit;
                if (v.Rec.p51ID_Billing > 0)
                {
                    var recP51 = Factory.p51PriceListBL.Load(v.Rec.p51ID_Billing);
                    v.SelectedComboP51Name = recP51.p51Name;
                    if (recP51.p51IsCustomTailor)
                    {
                        v.Rec.p41BillingFlag = BO.p41BillingFlagEnum.CenikIndividualni;  //ceník na míru
                        v.SelectedP51ID_Flag3 = v.Rec.p51ID_Billing;
                    }
                    else
                    {
                        v.Rec.p41BillingFlag = BO.p41BillingFlagEnum.CenikPrirazeny;
                        v.SelectedP51ID_Flag2 = v.Rec.p51ID_Billing;
                    }
                }
                if (v.Rec.p51ID_Internal > 0)
                {
                    v.SelectedComboP51Name_Internal = Factory.p51PriceListBL.Load(v.Rec.p51ID_Internal).p51Name;
                }

            }
            else
            {
                //nový projekt
                if (!Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_p41_Creator))
                {
                    if (Factory.p42ProjectTypeBL.GetList_ProjectCreate().Count() == 0)
                    {
                        return this.StopPage(true, "Nemáte oprávnění zakládat nové projekty.");
                    }
                }

                int levelindex = 5;
                if (!string.IsNullOrEmpty(levelprefix))
                {
                    levelindex = Convert.ToInt32(levelprefix.Substring(2, 1));
                }
                if (v.Rec.p42ID > 0)    //volání po založení projektu konkrétního typu
                {
                    v.RecP42 = Factory.p42ProjectTypeBL.Load(v.Rec.p42ID);
                    v.SelectedComboP42Name = v.RecP42.p42Name;
                }
                else
                {
                    var qry = Factory.p41ProjectBL.GetList(new BO.myQueryP41("p41") { j02id_owner = Factory.CurrentUser.pid, TopRecordsOnly = 1, p07level = levelindex, explicit_orderby = "a.p41ID DESC" });
                    if (qry.Count() > 1)
                    {
                        var recLast = qry.First();
                        v.Rec.p42ID = recLast.p42ID;
                        v.RecP42 = Factory.p42ProjectTypeBL.Load(v.Rec.p42ID);
                        v.SelectedComboP42Name = recLast.p42Name;

                        if (recLast.p41ParentID > 0)
                        {
                            var recParent = Factory.p41ProjectBL.Load(recLast.p41ParentID);
                            v.SelectedParentLevelIndex = recParent.p07Level;
                        }

                    }
                }

                if (v.Rec.p42ID == 0)
                {
                    var lisP42 = Factory.p42ProjectTypeBL.GetList(new BO.myQueryP42());
                    if (lisP42.Count() > 0)
                    {
                        v.Rec.p42ID = lisP42.First().pid;
                        v.SelectedComboP42Name = lisP42.First().p42Name;
                        v.RecP42 = lisP42.First();
                    }
                }
                if (p28id > 0)
                {
                    //založit projekt pro konkrétního klienta
                    var recP28 = Factory.p28ContactBL.Load(p28id);
                    v.Rec.p28ID_Client = recP28.pid;
                    v.SelectedComboClient = recP28.p28Name;
                }

            }

            RefreshStateRecord(v,isclone);

            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                var lisP56 = Factory.p56TaskBL.GetList(new BO.myQueryP56() { p41id = v.rec_pid, IsRecordValid = null });
                if (lisP56.Count() > 0)
                {
                    v.lisP56Clone = new List<p56Clone>();
                    foreach (var c in lisP56)
                    {
                        string ss = string.Join(", ", Factory.x67EntityRoleBL.GetRolesAssignedToRecord(c.pid, "p56").Select(p => p.Receivers));
                        v.lisP56Clone.Add(new p56Clone() { p56Notepad = ss, Importovat = false, p57ID = c.p57ID, p56Name = c.p56Name, pid = c.pid, p56PlanFrom = c.p56PlanFrom, p56PlanUntil = c.p56PlanUntil, p56Plan_Hours = c.p56Plan_Hours });
                    }
                }

                var lisP40 = Factory.p40WorkSheet_RecurrenceBL.GetList(new BO.myQueryP40() { p41id = v.rec_pid, IsRecordValid = null });
                if (lisP40.Count() > 0)
                {
                    v.lisP40Clone = new List<p40Clone>();
                }
                foreach (var c in lisP40)
                {
                    var d0 = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddDays(-1);
                    v.lisP40Clone.Add(new p40Clone() { Importovat = false, p40Name = c.p40Name, p40Value = c.p40Value, pid = c.pid, p40FirstSupplyDate = d0, p40LastSupplyDate = new DateTime(d0.AddYears(5).Year, 12, 31), p40Text = c.p40Text,p40TextInternal=c.p40TextInternal });
                }

                v.MakeClone();
                if (v.lisP26 != null)
                {
                    foreach (var c in v.lisP26)
                    {
                        c.pid = 0;
                    }
                }

            }

            return View(v);
        }

        private void RefreshStateRecord(p41Record v,bool isclone)
        {
            if (v.RecParent == null && v.Rec.p41ParentID > 0)
            {
                v.RecParent = Factory.p41ProjectBL.Load(v.Rec.p41ParentID);
            }
            if (v.Rec.p42ID > 0)
            {
                if (v.RecP42 == null)
                {
                    v.RecP42 = Factory.p42ProjectTypeBL.Load(v.Rec.p42ID);
                }
                
            }
            v.TagEntity = "p41Project";
            if (v.SelectedP07ID > 0)
            {
                v.RecP07 = Factory.lisP07.Where(p => p.pid == v.SelectedP07ID).First();
                v.lisParentLevels = Factory.lisP07.Where(p => !p.isclosed && p.p07Level < v.RecP07.p07Level);
            }
            else
            {
                v.RecP07= Factory.lisP07.Where(p => p.p07Level == 5).First(); ;
            }



            v.TagEntity = $"le{v.RecP07.p07Level}";

            if (!v.disp.IsInhaled)
            {
                InhaleDisp(v, 0);
            }

            InhaleRoles(v);
            if (v.disp.IsProjectContacts && v.lisP26 == null)
            {
                v.lisP26 = new List<p26Repeater>();
            }
            if (v.reminder == null)
            {
                v.reminder = new ReminderViewModel() { is_static_date = true, record_pid = v.rec_pid, record_prefix = "p41" };
            }
            if (v.hlidac == null)
            {
                v.hlidac = new HlidacViewModel() { rec_entity = "p41", rec_pid = v.rec_pid };
            }
            if (v.ff1 == null)
            {
                v.ff1 = new FreeFieldsViewModel();
                v.ff1.InhaleFreeFieldsView(Factory, v.rec_pid, "p41");
            }
            v.ff1.RefreshInputsVisibility(Factory, v.rec_pid, "p41", v.Rec.p42ID);

            if (v.Rec.j02ID_Owner == 0)
            {
                v.Rec.j02ID_Owner = Factory.CurrentUser.pid;
                v.SelectedComboOwner = Factory.CurrentUser.FullnameDesc;
            }
            if (v.barcodes == null)
            {
                v.barcodes = new BarcodesViewModel() { record_pid = v.rec_pid, record_prefix = "p41", TempGuid = v.UploadGuid };
            }


            



        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p41Record v, string guid)
        {
            RefreshStateRecord(v,false);
            if (v.IsPostback)
            {
                switch (v.PostbackOper)
                {
                    case "parentlevel":
                        v.Element2Focus = "cmdComboRec_p41ParentID";
                        v.SelectedComboParent = null;
                        v.Rec.p41ParentID = 0;
                        break;
                    case "p07id":
                        if (v.rec_pid == 0 && v.RecP07 != null)
                        {
                            v.SelectedParentLevelIndex = v.RecP07.p07Level - 1;
                            RefreshStateRecord(v, false);
                        }
                        
                        break;
                    case "p42id":                        
                        v.Element2Focus = "cmdComboRec_p28ID_Client";
                        
                        if (v.Rec.p42ID > 0)
                        {
                            if (v.RecP42 == null)
                            {
                                v.RecP42 = Factory.p42ProjectTypeBL.Load(v.Rec.p42ID);
                            }
                            InhaleDisp(v, v.disp.GetBitStream());
                            if (v.rec_pid == 0) v.disp.RecoveryDefaultCheckedStates();
                            InhaleRoles(v);
                            
                        }
                        break;
                    case "add_p26":
                        var c = new p26Repeater() { TempGuid = BO.Code.Bas.GetGuid() };
                        v.lisP26.Add(c);
                        break;
                    case "delete_p26":
                        v.lisP26.First(p => p.TempGuid == guid).IsTempDeleted = true;
                        break;
                }
                return View(v);
            }


            if (ModelState.IsValid)
            {
                BO.p41Project c = new BO.p41Project();
                if (v.rec_pid > 0) c = Factory.p41ProjectBL.Load(v.rec_pid);
                c.p41Name = v.Rec.p41Name;
                c.p41NameShort = v.Rec.p41NameShort;
                c.p41ParentID = v.Rec.p41ParentID;
                c.p28ID_Client = v.Rec.p28ID_Client;
                c.p28ID_Billing = v.Rec.p28ID_Billing;
                c.p41PlanFrom = v.Rec.p41PlanFrom;
                c.p41PlanUntil = v.Rec.p41PlanUntil;
                c.p41Plan_Hours_Billable = v.Rec.p41Plan_Hours_Billable;
                c.p41Plan_Hours_Nonbillable = v.Rec.p41Plan_Hours_Nonbillable;
                c.p41Plan_Hours = v.Rec.p41Plan_Hours_Billable + v.Rec.p41Plan_Hours_Nonbillable;
                c.p41Plan_Internal_Fee = v.Rec.p41Plan_Internal_Fee;

                c.p41Plan_Expenses = v.Rec.p41Plan_Expenses;
                c.p41Plan_Revenue = v.Rec.p41Plan_Revenue;
                c.p41Plan_Internal_Rate = v.Rec.p41Plan_Internal_Rate;
                c.p42ID = v.Rec.p42ID;
                c.p07ID = v.SelectedP07ID;
                                
                c.p41BillingLangIndex = v.Rec.p41BillingLangIndex;
                c.j18ID = v.Rec.j18ID;
                c.p41InvoiceMaturityDays = v.Rec.p41InvoiceMaturityDays;
                c.p61ID = v.Rec.p61ID;
                c.p92ID = v.Rec.p92ID;
                c.p51ID_Internal = v.Rec.p51ID_Internal;

                c.p72ID_NonBillable = v.Rec.p72ID_NonBillable;
                c.p72ID_BillableHours = v.Rec.p72ID_BillableHours;
                
                c.p51ID_Billing = 0;
                if (v.Rec.p41BillingFlag == BO.p41BillingFlagEnum.CenikPrirazeny)
                {
                    c.p51ID_Billing = v.SelectedP51ID_Flag2;
                    if (c.p51ID_Billing == 0)
                    {
                        this.AddMessage("Chybí vybrat ceník fakturačních hodinových sazeb."); return View(v);
                    }
                }
                if (v.Rec.p41BillingFlag == BO.p41BillingFlagEnum.CenikIndividualni)
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
                if (v.reminder.lisReminder != null && v.reminder.lisReminder.Where(p => p.IsTempDeleted == false && p.o24StaticDate == null).Count() > 0)
                {
                    this.AddMessage("V upozornění chybí vyplnit datum+čas."); return View(v);
                }

                c.p41BitStream = v.disp.GetBitStream();


                c.p41InvoiceDefaultText1 = v.Rec.p41InvoiceDefaultText1;
                c.p41InvoiceDefaultText2 = v.Rec.p41InvoiceDefaultText2;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
                c.j02ID_Owner = v.Rec.j02ID_Owner;
                c.p41ExternalCode = v.Rec.p41ExternalCode;
                c.p15ID = v.Rec.p15ID;
                c.p41BillingFlag = v.Rec.p41BillingFlag;
                c.p41WorksheetOperFlag = v.Rec.p41WorksheetOperFlag;
                c.p41Round2Minutes = v.Rec.p41Round2Minutes;
                c.p41AccountingIds = v.Rec.p41AccountingIds;
                if (v.BillingMemo != null)
                {
                    c.p41BillingMemo200 = v.BillingMemo.GetText200();
                }
                
                

                List<BO.p26ProjectContact> lisP26 = null;
                if (v.disp.IsProjectContacts)
                {
                    lisP26 = new List<BO.p26ProjectContact>();
                    foreach (var cc in v.lisP26)
                    {
                        lisP26.Add(new BO.p26ProjectContact()
                        {
                            IsSetAsDeleted = cc.IsTempDeleted,
                            pid = cc.pid,
                            p26Name = cc.p26Name,
                            p28ID = cc.p28ID,
                            p27ID = cc.p27ID
                        });
                    }
                }

                c.pid = Factory.p41ProjectBL.Save(c, v.ff1.inputs, v.disp.IsRoles ? v.roles.getList4Save(Factory) : null, lisP26,(v.BillingMemo !=null ? v.BillingMemo.HtmlValue : null));
                if (c.pid > 0)
                {
                    Factory.o51TagBL.SaveTagging("p41", c.pid, v.TagPids);

                    //if (v.rec_pid==0 && v.Notepad !=null && !string.IsNullOrWhiteSpace(v.Notepad.HtmlContent))  //zda uložit fakturační poznámku
                    //{
                    //    var recB05 = new BO.b05Workflow_History() { b05RecordEntity = "p41",b05IsCommentOnly=true,b05IsManualStep=true,x04ID=v.Notepad.SelectedX04ID, b05RecordPid = c.pid, b05Notepad = v.Notepad.HtmlContent,b05Tab1Flag=6 };
                    //    Factory.WorkflowBL.Save2History(recB05);

                    //}

                    

                    if (v.disp.IsFiles)
                    {
                        Factory.o27AttachmentBL.SaveChangesAndUpload(v.UploadGuid, "p41", c.pid);

                    }
                    if (v.reminder != null)
                    {
                        v.reminder.SaveChanges(Factory, c.pid);
                    }
                    if (v.hlidac != null)
                    {
                        v.hlidac.SaveChanges(Factory, c.pid);
                    }
                    if (v.lisP56Clone != null)  //uložit kopírované úkoly
                    {
                        foreach (var cc in v.lisP56Clone.Where(p => p.Importovat))
                        {
                            var recP56 = Factory.p56TaskBL.Load(cc.pid);
                            recP56.p56Name = cc.p56Name; recP56.p56PlanFrom = cc.p56PlanFrom; recP56.p56PlanUntil = cc.p56PlanUntil; recP56.pid = 0; recP56.ValidFrom = DateTime.Now; recP56.ValidUntil = new DateTime(3000, 1, 1); recP56.p41ID = c.pid; recP56.p56Plan_Hours = cc.p56Plan_Hours;
                            Factory.p56TaskBL.Save(recP56, null, Factory.x67EntityRoleBL.GetList_X69("p56", cc.pid).ToList());
                        }
                    }
                    if (v.lisP40Clone != null)  //uložit kopírované opakované odměny
                    {
                        foreach (var cc in v.lisP40Clone.Where(p => p.Importovat))
                        {
                            var recP40 = Factory.p40WorkSheet_RecurrenceBL.Load(cc.pid);
                            recP40.p40Name = cc.p40Name; recP40.p40Value = cc.p40Value; recP40.p40Text = cc.p40Text;recP40.p40TextInternal = cc.p40TextInternal; recP40.pid = 0; recP40.ValidFrom = DateTime.Now; recP40.ValidUntil = new DateTime(3000, 1, 1); recP40.p41ID = c.pid; recP40.p40FirstSupplyDate = cc.p40FirstSupplyDate; recP40.p40LastSupplyDate = cc.p40LastSupplyDate;
                            Factory.p40WorkSheet_RecurrenceBL.Save(recP40);
                        }
                    }
                    
                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }

        private bool InhalePermissions(p41Record v)
        {
            var mydisp = Factory.p41ProjectBL.InhaleRecDisposition(v.Rec.pid, v.Rec);
            if (!mydisp.OwnerAccess)
            {
                return false;
            }
            if (v.Rec.x38ID > 0)
            {
                v.CanEditRecordCode = Factory.x38CodeLogicBL.CanEditRecordCode(v.Rec.x38ID, mydisp);
            }
            else
            {
                v.CanEditRecordCode = mydisp.OwnerAccess;
            }
            return true;
        }



        private void InhaleDisp(p41Record v, int bitstream)
        {
            if (v.RecP42 == null) return;

            //int intCache = (v.rec_pid == 0 ? Factory.j02UserBL.LoadBitstreamFromUserCache("p41", v.RecP42.pid) : 0);    //pro nový záznam načíst uložená rozšíření z cache
            int intCache = 0;   //cache nepoužívat


            v.disp.SetVal(PosEnum.Files, v.RecP42.p42FilesTab, bitstream, v.rec_pid, intCache);
            v.disp.SetVal(PosEnum.BillingTab, v.RecP42.p42BillingTab, bitstream, v.rec_pid, intCache);
            v.disp.SetVal(PosEnum.ProjectClient, v.RecP42.p42ClientTab, bitstream, v.rec_pid, intCache);
            v.disp.SetVal(PosEnum.Roles, v.RecP42.p42RolesTab, bitstream, v.rec_pid, intCache);
            v.disp.SetVal(PosEnum.ProjectBudget, v.RecP42.p42BudgetTab, bitstream, v.rec_pid, intCache);
            v.disp.SetVal(PosEnum.ProjectContacts, v.RecP42.p42ContactsTab, bitstream, v.rec_pid, intCache);
        }
        private void InhaleRoles(p41Record v)
        {
            if (v.disp.IsRoles && v.roles == null)
            {
                v.roles = new RoleAssignViewModel() { RecPid = v.rec_pid, RecPrefix = "p41", RolePrefix = "p41" };
            }

        }



        public IActionResult SelectProject(string source_prefix, int source_pid, int leindex)
        {

            if (leindex == 0) leindex = 5;
            var v = new UI.Models.p41oper.SelectProjectViewModel() { source_prefix = source_prefix, source_pid = source_pid, leindex = 5 };

            var mq = new BO.myQueryP41("le" + v.leindex.ToString());
            switch (v.source_prefix)
            {
                case "p28":
                    mq.p28id = v.source_pid; break;
            }
            v.lisP41 = Factory.p41ProjectBL.GetList(mq).OrderBy(p => p.FullName);
            return View(v);
        }



    }
}
