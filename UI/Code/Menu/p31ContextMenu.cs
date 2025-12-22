

using BL;
using ceTe.DynamicPDF;

namespace UI.Menu
{
    public class p31ContextMenu: BaseContextMenu
    {
        public p31ContextMenu(BL.Factory f, int pid, string source,string master_entity,string device) : base(f, pid)
        {
            this.device = device;
            var rec = f.p31WorksheetBL.Load(pid);            
            if (rec.isdeleted)
            {
                AMI("Detail záznamu v archivu", $"javascript: _window_open('/p31/Info?pid={pid}&isrecord=true&isdelrecord=true',3)", "edit_note");
                return;
            }
            var disp = f.p31WorksheetBL.InhaleRecDisposition(rec);
            
            if (master_entity == "p91Invoice")
            {
                AMI("Upravit položku vyúčtování", $"javascript: _window_open('/p91oper/p31edit?p31id={pid}',3)", "edit_note");
                AMI("Vyjmout položku z vyúčtování", $"javascript: _window_open('/p91oper/p31remove?p31id={pid}')", "content_cut");
                AMI("Přesunout do jiného vyúčtování", $"javascript: _window_open('/p91oper/p31move2invoice?p31id={pid}')", "switch_right");
                
            }
            else
            {
                if (disp.OwnerAccess && disp.RecordState==BO.p31RecordState.Editing)
                {
                    AMI("Upravit", $"javascript:window.parent._edit('p31',{pid})", "edit_note");                    
                }
                else
                {
                    AMI("Detail", $"javascript:_edit('p31',{pid})", "edit_note");
                }
            }

            AMI_Clone("p31", pid);


            if (disp.OwnerAccess && disp.RecordState == BO.p31RecordState.Editing)
            {
                AMI("Odstranit", $"javascript:_delete('p31',{pid})", "delete_forever");
            }
                
            
            if (rec.p41BillingFlag != BO.p41BillingFlagEnum.NuloveSazbyBezWip)
            {
                bool bolDiv = false;
                if (rec.p91ID == 0 && (disp.CanApprove || disp.CanApproveAndEdit))
                {
                    DIV(); bolDiv = true;
                    AMI(BO.Code.Bas.IIFS(rec.p71ID == BO.p71IdENUM.Nic, "Schválit/Vyúčtovat", "Pře-schválit/Vyúčtovat"), $"javascript: _window_open('/p31approveinput/Index?prefix=p31&pids={pid}', 2)", "approval");
                    bool b = f.CurrentUser.TestPermission(BO.PermValEnum.GR_P91_Creator, BO.PermValEnum.GR_P91_Draft_Creator);
                  
                    if (!b)
                    {
                        var dispP41 = f.p41ProjectBL.InhaleRecDisposition(rec.p41ID);   //oprávnění uživatele k projektu úkonu
                        b = dispP41.p91_Create || dispP41.p91_DraftCreate;
                        
                    }
                    
                    if (b)
                    {
                        AMI("Přidat do vybraného vyúčtování", $"javascript: _window_open('/p31invoice/Append2Invoice?pids={pid}',2)", "receipt_long");

                    }
                }
                if (rec.p33ID == BO.p33IdENUM.Cas || rec.p33ID == BO.p33IdENUM.Kusovnik || rec.p31ExcludeBillingFlag > 0)
                {
                    if (!bolDiv) DIV();
                    if (disp.OwnerAccess && disp.RecordState == BO.p31RecordState.Editing)
                    {
                        AMI("Přesunout do [Vyloučené z vyúčtování]", $"javascript: _window_open('/p31excludebilling/Index?pids={pid}&flag=1&prefix=p31',2)", "delete_sweep");

                    }
                    else
                    {
                        if (disp.OwnerAccess && rec.p31ExcludeBillingFlag == 1)
                        {
                            if (!bolDiv) DIV();
                            AMI("Přesunout z [Vyloučené z vyúčtování]", $"javascript: _window_open('/p31excludebilling/Index?pids={pid}&flag=0&prefix=p31',2)", "delete_sweep");

                        }
                    }
                }
            }
            
            
            
            

            DIV();
            AMI("Další", null, null, null, "more");
            AMI_Workflow("p31", pid, 0, 0, "more");
            if (rec.p33ID == BO.p33IdENUM.Cas && rec.p71ID==BO.p71IdENUM.Nic)
            {
                AMI("Rozdělit úkon na 2 kusy", $"javascript: _window_open('/p31split/Index?pid={pid}',2)", "arrow_split", "more");
            }
            
            
            AMI_ChangeLog("p31", pid, "more");

            AMI_SendMail("p31", pid, "more");
            
            if (rec.p33ID == BO.p33IdENUM.Cas)
            {
                AMI("Exportovat do Kalendáře (iCalendar)", "/iCalendar/p31?pids=" + rec.pid.ToString(), "event", "more");
            }
            AMI("Zobrazit v Tabulce", $"/TheGrid/FlatView?prefix=p31&myqueryinline=pids|list_int|{rec.pid}", "grid_view","more",null,"_top");


            AMI_RowColor("p31", rec.pid);


            if (_f.CurrentUser.j04IsModule_p41)
            {
                AMI("Vazby", null, null, null, "rel");
                var recP41 = _f.p41ProjectBL.Load(rec.p41ID);
                AMI_RecPage($"{recP41.p42Name}: {recP41.FullName}", "p41", recP41.pid, "rel");
                if (_f.CurrentUser.j04IsModule_p28)
                {
                    if (recP41.p28ID_Client > 0)
                    {
                        var recP28 = _f.p28ContactBL.Load(recP41.p28ID_Client);
                        if (recP28 != null)
                        {
                            AMI_RecPage(_f.tra("Klient") + ": " + recP28.p28Name, "p28", recP41.p28ID_Client, "rel");
                        }                        
                    }
                    if (rec.p28ID_ContactPerson > 0)
                    {
                        var recP28CP = f.p28ContactBL.Load(rec.p28ID_ContactPerson);
                        if (recP28CP != null)
                        {
                            AMI_RecPage(_f.tra("Kontaktní osoba") + ": " + recP28CP.p28Name, "p28", rec.p28ID_ContactPerson, "rel");
                        }
                    }
                    if (rec.p28ID_Supplier > 0)
                    {
                        var recP28 = _f.p28ContactBL.Load(rec.p28ID_Supplier);
                        if (recP28 != null)
                        {
                            AMI_RecPage(_f.tra("Dodavatel") + ": " + recP28.p28Name, "p28", rec.p28ID_Supplier, "rel");
                        }

                    }
                }
                
                
                if (rec.p56ID > 0)
                {
                    AMI_RecPage(rec.p56Name, "p56", rec.p56ID, "rel");
                }
                if (rec.p91ID>0 && _f.CurrentUser.j04IsModule_p91)
                {
                    var recP91 = _f.p91InvoiceBL.Load(rec.p91ID);
                    AMI_RecPage($"{recP91.p91Code}: {recP91.p92Name}", "p91", rec.p91ID, "rel");
                }
                if (rec.o23ID>0 && _f.CurrentUser.j04IsModule_o23)
                {
                    var recO23 = _f.o23DocBL.Load(rec.o23ID);
                    AMI_RecPage(recO23.NameWithCode, "o23", rec.o23ID, "rel");
                }
                if (_f.CurrentUser.j04IsModule_j02)
                {
                    var recJ02 = _f.j02UserBL.Load(rec.j02ID);
                    AMI_RecPage(recJ02.FullNameAsc, "j02", rec.j02ID, "rel");
                }
                if (rec.p40ID_Source > 0 && _f.CurrentUser.j04IsModule_p91)
                {
                    var recP40 = _f.p40WorkSheet_RecurrenceBL.Load(rec.p40ID_Source);
                    AMI_RecPage(recP40.p40Name, "p40", rec.p40ID_Source, "rel", "repeat_on");
                }


                
            }
            
            if (rec.p51ID_BillingRate>0 && _f.CurrentUser.TestPermission(BO.PermValEnum.GR_p51_Admin))
            {
                var recP51 = _f.p51PriceListBL.Load(rec.p51ID_BillingRate);
                if (recP51 != null)
                {
                    AMI_RecPage($"{BO.Code.Bas.Number2String(rec.p31Rate_Billing_Orig)} {rec.j27Code_Billing_Orig}: {recP51.p51Name}", "p51", rec.p51ID_BillingRate, "rel", "price_change");
                }
                
            }
            if (rec.p51ID_CostRate > 0 && _f.CurrentUser.TestPermission(BO.PermValEnum.GR_p51_Admin))
            {
                var recP51 = _f.p51PriceListBL.Load(rec.p51ID_CostRate);
                if (recP51 != null)
                {
                    AMI_RecPage($"{BO.Code.Bas.Number2String(rec.p31Rate_Internal_Orig)}: {recP51.p51Name}", "p51", rec.p51ID_CostRate, "rel", "price_change");
                }
                
            }

        }
    }
}
