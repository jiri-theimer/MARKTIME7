
using BL;
using UI.Models;

namespace UI.Code
{
    public class NavTabsSupport
    {
        private BL.Factory _f { get; set; }
        private List<NavTab> _tabs { get; set; }
        public NavTabsSupport(BL.Factory f)
        {
            _f = f;            
        }

        private void Handle_DocsTabs(IEnumerable<BO.o17DocMenu> lisO17,string strBadge)
        {
            if (!_f.CurrentUser.j04IsModule_o23) return;
            _tabs.Add(AddTab("Dokumenty", "o23Doc", "/TheGrid/SlaveView?prefix=o23", true, strBadge));

            //if (lisO17 == null || lisO17.Count()<=1)
            //{
            //    _tabs.Add(AddTab("Dokumenty", "o23Doc", "/TheGrid/SlaveView?prefix=o23",true, strBadge));
            //}
            //else
            //{
            //    foreach(var c in lisO17)
            //    {
            //        _tabs.Add(AddTab(c.o17Name, "o23Doc"+c.pid.ToString(), $"/TheGrid/SlaveView?prefix=o23&rez={c.pid}",false,null));
            //    }
            //}
            
        }
        public List<NavTab> getOverGridTabs(string prefix,string deftab,bool flatview,string rez)
        {
            _tabs = new List<NavTab>();
            string strUrl = "MasterView";
            if (flatview) strUrl = "FlatView";

            switch (prefix)
            {
                case "p31":
                    _tabs.Add(AddTab("ÚKONY", "zero", "/TheGrid/FlatView?prefix=p31&tab=tab1", false, Badge1Flat(0, "TheGridRows")));                    
                    break;
                case "approve":
                    _tabs.Add(AddTab("<span class='material-icons-outlined-nosize'>functions</span>", "zero", "javascript:changetab('tab1')", false, Badge1Flat(0, "TheGridRows")));
                    _tabs.Add(AddTab("Hodiny", "time", "javascript:changetab('time')", true, Badge1Flat(0, "TheGridRowsTime", "flat_tab_sum")));
                    _tabs.Add(AddTab("Výdaje", "expense", "javascript:changetab('expense')", true, Badge1Flat(0, "TheGridRowsExpense", "flat_tab_sum")));
                    _tabs.Add(AddTab("Odměny", "fee", "javascript:changetab('fee')", true, Badge1Flat(0, "TheGridRowsFee", "flat_tab_sum")));
                    _tabs.Add(AddTab("Ks", "kusovnik", "javascript:changetab('kusovnik')", false, Badge1Flat(0, "TheGridRowsKusovnik", "flat_tab_sum")));
                    break;
                
                case "p41":                   
                case "le5":
                    _tabs.Add(AddTab(_f.getP07Level(5, false), "zero", $"/TheGrid/{strUrl}?prefix={prefix}", false, Badge1Flat(0, "TheGridRows")));
                    break;
                case "le4":
                case "le3":
                case "le2":
                case "le1":
                    int intLevelIndex = Convert.ToInt32(prefix.Substring(2, 1));
                    _tabs.Add(AddTab(_f.getP07Level(intLevelIndex, false), "zero", $"/TheGrid/{strUrl}?prefix={prefix}", false, Badge1Flat(0, "TheGridRows")));
                    break;
                case "o23":
                    if (string.IsNullOrEmpty(rez))
                    {
                        _tabs.Add(AddTab(_f.EProvider.ByPrefix(prefix).AliasPlural, "zero", $"/TheGrid/{strUrl}?prefix={prefix}&rez={rez}", false, Badge1Flat(0, "TheGridRows")));
                    }
                    else
                    {
                        _tabs.Add(AddTab(_f.GetListO17().First(p=>p.pid==Convert.ToInt32(rez)).o17Name, "zero", $"/TheGrid/{strUrl}?prefix={prefix}&rez={rez}", false, Badge1Flat(0, "TheGridRows")));
                    }
                    break;
                default:
                    _tabs.Add(AddTab(_f.EProvider.ByPrefix(prefix).AliasPlural, "zero", $"/TheGrid/{strUrl}?prefix={prefix}&rez={rez}", false, Badge1Flat(0, "TheGridRows")));

                    break;
            }
            if (deftab == null || deftab=="tab1")
            {
                _tabs[0].CssClass += " active";
            }
            else
            {
                if (_tabs.Any(p => p.Entity == deftab))
                {
                    _tabs.First(p => p.Entity == deftab).CssClass += " active";
                }
            }
            return _tabs;
        }
        public string Badge1Flat(int num1, string clientid,string cssclass= "badge bg-primary")
        {
            
            return $"<span id='{clientid}' class='{cssclass}' style='margin-left:2px;'>{num1}</span>";
        }

        public List<NavTab> getMasterTabs(string prefix,int pid,bool loadsums)
        {
            _tabs = new List<NavTab>();
            BO.p41ProjectSum cp41 = null;

            IEnumerable<BO.o17DocMenu> lisO17 = null;
            //string strO17IDs = "," + _f.CurrentUser.o17IDs + ",";
            //if (_f.CurrentUser.j04IsModule_o23)
            //{
            //    lisO17= _f.GetListO17().Where(p => p.x01ID == _f.CurrentUser.x01ID);
            //    if (lisO17.Count() > 0 && !_f.CurrentUser.IsAdmin)
            //    {
            //        lisO17 = lisO17.Where(p => strO17IDs.Contains("," + p.pid.ToString() + ","));
            //    }
                    
            //}
            
            
            switch (prefix)
            {
                case "j02":
                    var cpj02 = _f.j02UserBL.LoadSumRow(pid);
                    
                    _tabs.Add(AddTab(cpj02.tabname, "tab1", "/j02/Tab1?pid=" + AppendPid2Url(pid)));
                    
                    if (_f.CurrentUser.j04IsModule_p31)
                    {
                        _tabs.Add(AddTab("Úkony", "p31Worksheet", "/TheGrid/SlaveView?prefix=p31", true, Badge3(cpj02.p31_Wip_Time_Count, cpj02.p31_Wip_Expense_Count, cpj02.p31_Wip_Fee_Count, "badge bg-warning text-dark")));
                        _tabs.Add(AddTab("Hodiny", "p31time", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|time", true, Badge2(cpj02.p31_Wip_Time_Count, cpj02.p31_Approved_Time_Count)));
                        _tabs.Add(AddTab("Výdaje", "p31expense", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|expense", true, Badge2(cpj02.p31_Wip_Expense_Count, cpj02.p31_Approved_Expense_Count)));
                        _tabs.Add(AddTab("Odměny", "p31fee", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|fee", true, Badge2(cpj02.p31_Wip_Fee_Count, cpj02.p31_Approved_Fee_Count)));
                        if (_f.CurrentUser.j04IsModule_p91)
                        {
                            _tabs.Add(AddTab("Vyúčtování", "p91Invoice", "/TheGrid/SlaveView?prefix=p91", true, Badge1(cpj02.p91_Count)));
                        }
                        
                        //_tabs.Add(AddTab("∑", "p31totals", "/p31totals/Index?prefix=j02", false));
                    }
                    if (_f.CurrentUser.j04IsModule_r01)
                    {
                        _tabs.Add(AddTab("Kapacity", "capacity", "/j02/TabR01?prefix=j02"));
                    }
                    
                    Handle_DocsTabs(lisO17, null);


                    _tabs.Add(AddTab("Outbox", "x40MailQueue", "/TheGrid/SlaveView?prefix=x40"));
                    if (_f.CurrentUser.j04IsModule_o43)
                    {
                        _tabs.Add(AddTab("Inbox", "o43Inbox", "/TheGrid/SlaveView?prefix=o43", true, Badge1(cpj02.o43_Count)));
                    }
                    
                    _tabs.Add(AddTab("PING Log", "j92PingLog", "/TheGrid/SlaveView?prefix=j92"));
                    _tabs.Add(AddTab("LOGIN Log", "j90LoginAccessLog", "/TheGrid/SlaveView?prefix=j90"));

                    break;

                case "p28":                                        
                    var cp28 = new BO.p28ContactSum();
                    if (loadsums)
                    {
                        cp28 = _f.p28ContactBL.LoadSumRow(pid);
                    }
                    _tabs.Add(AddTab(cp28.tabname, "tab1", "/p28/Tab1?pid=" + AppendPid2Url(pid), false));
                    if (_f.CurrentUser.j04IsModule_p31)
                    {
                        _tabs.Add(AddTab("Úkony", "p31Worksheet", "/TheGrid/SlaveView?prefix=p31", true, Badge3(cp28.p31_Wip_Time_Count, cp28.p31_Wip_Expense_Count, cp28.p31_Wip_Fee_Count, "badge bg-warning text-dark")));
                        _tabs.Add(AddTab("Hodiny", "p31time", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|time", true, Badge2(cp28.p31_Wip_Time_Count, cp28.p31_Approved_Time_Count)));
                        _tabs.Add(AddTab("Výdaje", "p31expense", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|expense", true, Badge2(cp28.p31_Wip_Expense_Count, cp28.p31_Approved_Expense_Count)));
                        _tabs.Add(AddTab("Odměny", "p31fee", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|fee", true, Badge2(cp28.p31_Wip_Fee_Count, cp28.p31_Approved_Fee_Count)));
                        if (_f.CurrentUser.j04IsModule_p41)
                        {
                            _tabs.Add(AddTab("Projekty", "p28Contact", "/TheGrid/SlaveView?prefix=p41", false, Badge2(cp28.p41_Actual_Count, cp28.p41_Closed_Count, "badge bg-primary", "badge bg-primary")));                            
                            if (_f.CurrentUser.IsRatesAccess)
                            {
                                _tabs.Add(AddTab("Opakované odměny", "p40WorkSheet_Recurrence", "/TheGrid/SlaveView?prefix=p40", true, Badge1(cp28.p40_Count), "Předpisy opakovaných odměn"));
                            }
                            
                        }
                        if (_f.CurrentUser.j04IsModule_p91)
                        {
                            _tabs.Add(AddTab("Vyúčtování", "p91Invoice", "/TheGrid/SlaveView?prefix=p91", true, Badge1(cp28.p91_Count)));
                        }
                        
                        //if (_f.CurrentUser.IsVysledovkyAccess)
                        //{
                        //    _tabs.Add(AddTab("<span class='material-icons-outlined-btn'>speed</span>", "vysledovka", "/Vysledovka/Index?prefix=p28", false, null, "Výsledovky"));
                        //}
                        
                    }
                    
                    if (_f.CurrentUser.j04IsModule_p90)
                    {
                        _tabs.Add(AddTab("Zálohy", "p90Proforma", "/TheGrid/SlaveView?prefix=p90", true, Badge1(cp28.p90_Count)));
                    }
                    Handle_DocsTabs(lisO17, Badge1(cp28.o23_Count));
                    
                    if (_f.CurrentUser.j04IsModule_o43)
                    {
                        _tabs.Add(AddTab("Inbox", "o43Inbox", "/TheGrid/SlaveView?prefix=o43", true, Badge1(cp28.o43_Count)));
                    }
                        
                    _tabs.Add(AddTab("<span class='material-icons-outlined-btn'>speaker_notes</span>", "b05", "/Record/TabB05?prefix=p28",false, Badge1(cp28.b05_Count),"Notepad, workflow historie"));
                    break;
                case "p41":
                case "le1":
                case "le2":
                case "le3":
                case "le4":
                case "le5":
                    cp41 = _f.p41ProjectBL.LoadSumRow(pid);
                    
                    _tabs.Add(AddTab(cp41.tabname, "tab1", $"/p41/Tab1?pid={AppendPid2Url(pid)}&prefix={prefix}", false));
                    if (_f.CurrentUser.j04IsModule_p31)
                    {
                        _tabs.Add(AddTab("Úkony", "p31Worksheet", "/TheGrid/SlaveView?prefix=p31", true, Badge3(cp41.p31_Wip_Time_Count, cp41.p31_Wip_Expense_Count, cp41.p31_Wip_Fee_Count, "badge bg-warning text-dark")));
                        _tabs.Add(AddTab("Hodiny", "p31time", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|time", true, Badge2(cp41.p31_Wip_Time_Count, cp41.p31_Approved_Time_Count)));
                        _tabs.Add(AddTab("Výdaje", "p31expense", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|expense", true, Badge2(cp41.p31_Wip_Expense_Count, cp41.p31_Approved_Expense_Count)));
                        
                        if (_f.CurrentUser.IsRatesAccess)
                        {
                            _tabs.Add(AddTab("Odměny", "p31fee", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|fee", true, Badge2(cp41.p31_Wip_Fee_Count, cp41.p31_Approved_Fee_Count)));
                            _tabs.Add(AddTab("Opakované odměny", "p40", "/p41/TabP40?prefix=p41", true, Badge1(cp41.p40_Count)));
                        }
                            
                    }
                    if (_f.CurrentUser.j04IsModule_p91)
                    {
                        _tabs.Add(AddTab("Vyúčtování", "p91Invoice", "/TheGrid/SlaveView?prefix=p91", true, Badge1(cp41.p91_Count)));
                    }
                    //if (_f.CurrentUser.j04IsModule_p31 && _f.CurrentUser.IsVysledovkyAccess)
                    //{
                    //    //_tabs.Add(AddTab("∑", "p31totals", "/p31totals/Index?prefix=" + prefix, false));
                    //    _tabs.Add(AddTab("<span class='material-icons-outlined-btn'>speed</span>", "vysledovka", $"/Vysledovka/Index?prefix={prefix}", false,null,"Výsledovky"));
                    //}
                    
                    if (_f.CurrentUser.j04IsModule_p56)
                    {
                        _tabs.Add(AddTab("Úkoly", "p56Task", "/TheGrid/SlaveView?prefix=p56", true, Badge1(cp41.p56_Actual_Count)));
                    }
                    if (_f.CurrentUser.j04IsModule_r01)
                    {
                        _tabs.Add(AddTab("Kapacity", "capacity", $"/p41/TabR01?prefix={prefix}", true));
                    }
                    if (_f.CurrentUser.j04IsModule_p49)
                    {
                        _tabs.Add(AddTab("FP", "p49FinancialPlan", "/TheGrid/SlaveView?prefix=p49", true, Badge1(cp41.p49_Count),"Finanční plány"));
                    }
                    Handle_DocsTabs(lisO17, Badge1(cp41.o23_Count));
                   
                    if (_f.CurrentUser.j04IsModule_o43)
                    {
                        _tabs.Add(AddTab("Inbox", "o43Inbox", "/TheGrid/SlaveView?prefix=o43", true, Badge1(cp41.o43_Count)));
                    }
                    
                    _tabs.Add(AddTab("<span class='material-icons-outlined-btn'>speaker_notes</span>", "b05", "/Record/TabB05?prefix=p41", false, Badge1(cp41.b05_Count),"Poznámky, workflow historie"));
                    break;
                case "o23":
                    var cpo23 = _f.o23DocBL.LoadSumRow(pid);
                  
                    _tabs.Add(AddTab(cpo23.tabname, "tab1", "/o23/Tab1?pid=" + AppendPid2Url(pid), false));
                    if (_f.CurrentUser.j04IsModule_p31)
                    {
                        _tabs.Add(AddTab("Úkony", "p31Worksheet", "/TheGrid/SlaveView?prefix=p31", true, Badge3(cpo23.p31_Wip_Time_Count, cpo23.p31_Wip_Expense_Count, cpo23.p31_Wip_Fee_Count, "badge bg-warning text-dark")));
                        _tabs.Add(AddTab("Hodiny", "p31time", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|time", true, Badge2(cpo23.p31_Wip_Time_Count, cpo23.p31_Approved_Time_Count)));
                        _tabs.Add(AddTab("Výdaje", "p31expense", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|expense", true, Badge2(cpo23.p31_Wip_Expense_Count, cpo23.p31_Approved_Expense_Count)));
                        _tabs.Add(AddTab("Odměny", "p31fee", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|fee", true, Badge2(cpo23.p31_Wip_Fee_Count, cpo23.p31_Approved_Fee_Count)));
                    }
                    if (_f.CurrentUser.j04IsModule_p91)
                    {
                        _tabs.Add(AddTab("Vyúčtování", "p91Invoice", "/TheGrid/SlaveView?prefix=p91", true, Badge1(cpo23.p91_Count)));
                    }
                    
                    if (_f.CurrentUser.j04IsModule_o43)
                    {
                        _tabs.Add(AddTab("Inbox", "o43Inbox", "/TheGrid/SlaveView?prefix=o43", true, Badge1(cpo23.o43_Count)));
                    }
                    
                    _tabs.Add(AddTab("<span class='material-icons-outlined-btn'>speaker_notes</span>", "b05", "/Record/TabB05?prefix=o23", false,null,"Notepad, workflow historie"));
                    
                    break;
                case "p51":
                    _tabs.Add(AddTab("Ceník", "tab1", "/p51/Tab1?pid=" + AppendPid2Url(pid), false));                    
                    var cp51 = _f.p51PriceListBL.LoadSumRow(pid);
                    
                    //if (cp51.p51TypeFlag == (int) BO.p51TypeFlagENUM.BillingRates)
                    //{
                        
                        
                    //}
                    _tabs.Add(AddTab("Klienti projektů s vazbou na ceník", "p28Contact", "/TheGrid/SlaveView?prefix=p28", true, Badge1(cp51.p28_Count)));
                    _tabs.Add(AddTab("Projekty s přímou vazbou na ceník", "p41Direct", "/TheGrid/SlaveView?prefix=p41", true, Badge1(cp51.p41_Count_Direct)));
                    _tabs.Add(AddTab("Hodiny", "p31time", $"/TheGrid/SlaveView?prefix=p31", true, Badge1(cp51.Hodiny_Count)));


                    break;
                case "o43":
                    _tabs.Add(AddTab("Inbox", "tab1", "/o43/Tab1?pid=" + AppendPid2Url(pid), false));
                    break;
                case "p56":
                    _tabs.Add(AddTab("Úkol", "tab1", "/p56/Tab1?pid=" + AppendPid2Url(pid), false));
                    var cp56 = new BO.p56TaskSum();
                    if (loadsums)
                    {
                        cp56 = _f.p56TaskBL.LoadSumRow(pid);
                    }
                    if (_f.CurrentUser.j04IsModule_p31)
                    {
                        _tabs.Add(AddTab("Úkony", "p31Worksheet", "/TheGrid/SlaveView?prefix=p31", true, Badge3(cp56.p31_Wip_Time_Count, cp56.p31_Wip_Expense_Count, cp56.p31_Wip_Fee_Count, "badge bg-warning text-dark")));
                        _tabs.Add(AddTab("Hodiny", "p31time", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|time", true, Badge2(cp56.p31_Wip_Time_Count, cp56.p31_Approved_Time_Count)));
                        _tabs.Add(AddTab("Výdaje", "p31expense", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|expense", true, Badge2(cp56.p31_Wip_Expense_Count, cp56.p31_Approved_Expense_Count)));
                        _tabs.Add(AddTab("Odměny", "p31fee", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|fee", true, Badge2(cp56.p31_Wip_Fee_Count, cp56.p31_Approved_Fee_Count)));
                        _tabs.Add(AddTab("Vyúčtování", "p91Invoice", "/TheGrid/SlaveView?prefix=p91", true, Badge1(cp56.p91_Count)));
                        //_tabs.Add(AddTab("∑", "p31totals", "/p31totals/Index?prefix=p56", false));
                    }
                    if (_f.CurrentUser.j04IsModule_p49)
                    {
                        _tabs.Add(AddTab("FP", "p49FinancialPlan", "/TheGrid/SlaveView?prefix=p49", true, Badge1(cp56.p49_Count),"Finanční plány"));
                    }
                    if (_f.CurrentUser.j04IsModule_o43)
                    {
                        _tabs.Add(AddTab("Inbox", "o43Inbox", "/TheGrid/SlaveView?prefix=o43", true, Badge1(cp56.o43_Count)));
                    }
                    
                    _tabs.Add(AddTab("<span class='material-icons-outlined-btn'>speaker_notes</span>", "b05", "/Record/TabB05?prefix=p56", false, Badge1(cp56.b05_Count),"Notepad, workflow historie"));
                    break;
                case "p55":
                    _tabs.Add(AddTab("Todo-list", "tab1", "/p55/Tab1?pid=" + AppendPid2Url(pid), false));
                    _tabs.Add(AddTab("Úkoly", "p56Task", "/TheGrid/SlaveView?prefix=p56", true));
                    break;
                case "p58":
                    var cpp58 = _f.p58TaskRecurrenceBL.LoadSumRow(pid);
                    _tabs.Add(AddTab("Opakovaný úkol", "tab1", "/p58/Tab1?pid=" + AppendPid2Url(pid), false));
                    _tabs.Add(AddTab("Vygenerované úkoly", "p56Task", "/TheGrid/SlaveView?prefix=p56", true, Badge2(cpp58.p56_Count_Pending,cpp58.p56_Count_Closed, "badge bg-primary", "badge bg-primary")));
                    break;
                case "p75":
                    var cpp75 = _f.p75InvoiceRecurrenceBL.LoadSumRow(pid);
                    _tabs.Add(AddTab("Opakované vyúčtování", "tab1", "/p75/Tab1?pid=" + AppendPid2Url(pid), false));
                    _tabs.Add(AddTab("Vygenerované faktury", "p91Invoice", "/TheGrid/SlaveView?prefix=p91", true, Badge2(cpp75.p91_Count, 0, "badge bg-primary", "badge bg-primary")));
                    break;
                case "b05":
                    _tabs.Add(AddTab("Poznámky, workflow historie", "tab1", "/b05/Tab1?pid=" + AppendPid2Url(pid), false));
                    break;
                case "o22":
                    var co22 = new BO.o22MilestoneSum();
                    if (loadsums)
                    {
                        co22 = _f.o22MilestoneBL.LoadSumRow(pid);
                    }
                    _tabs.Add(AddTab("Termín", "tab1", "/o22/Tab1?pid=" + AppendPid2Url(pid), false));
                    _tabs.Add(AddTab("<span class='material-icons-outlined-btn'>speaker_notes</span>", "b05", "/Record/TabB05?prefix=o22", false, Badge1(co22.b05_Count),"Poznámky, workflow historie"));
                    break;
                case "p40":
                    _tabs.Add(AddTab("Předpis opakování", "tab1", "/p40/Tab1?pid=" + AppendPid2Url(pid), false));
                    _tabs.Add(AddTab("Vygenerované úkony", "p31", "/TheGrid/SlaveView?prefix=p31", true));
                    _tabs.Add(AddTab("Hodiny v odměně", "hodinyvpausalu", "/TheGrid/SlaveView?prefix=p31&myqueryinline=hodiny_v_p40id|bool|1", true));
                    //_tabs.Add(AddTab("Plán generování úkonů", "p39WorkSheet_Recurrence_Plan", "/TheGrid/SlaveView?prefix=p39", true));

                    break;
                case "p15":
                    _tabs.Add(AddTab("Lokalita", "tab1", "/p15/Tab1?pid=" + AppendPid2Url(pid), false));
                    if (_f.CurrentUser.j04IsModule_p41)
                    {
                        _tabs.Add(AddTab("Projekty", "p14", "/TheGrid/SlaveView?prefix=p41", true));
                    }
                    _tabs.Add(AddTab("Historie počasí", "j95GeoWeatherLog", "/TheGrid/SlaveView?prefix=j95"));
                    break;
                case "o51":
                    _tabs.Add(AddTab("Štítek", "tab1", "/o51/Tab1?pid=" + AppendPid2Url(pid), false));
                    var cpo51 = new BO.o51TagSum();
                    if (loadsums)
                    {
                        cpo51 = _f.o51TagBL.LoadSumRow(pid);
                    }
                    if (_f.CurrentUser.j04IsModule_p28)
                    {
                        _tabs.Add(AddTab("Kontakty", "p28Contact", "/TheGrid/SlaveView?prefix=p28", true, Badge1(cpo51.p28_Count)));
                    }
                    Handle_DocsTabs(lisO17, Badge1(cpo51.o23_Count));
                    
                    if (_f.CurrentUser.j04IsModule_p91)
                    {
                        _tabs.Add(AddTab("Vyúčtování", "p91Invoice", "/TheGrid/SlaveView?prefix=p91", true, Badge1(cpo51.p91_Count)));
                    }
                        
                    if (_f.CurrentUser.j04IsModule_p90)
                    {
                        _tabs.Add(AddTab("Zálohy", "p90Proforma", "/TheGrid/SlaveView?prefix=p90", true, Badge1(cpo51.p90_Count)));
                    }
                    
                    _tabs.Add(AddTab("Uživatelé", "j02User", "/TheGrid/SlaveView?prefix=j02", true, Badge1(cpo51.j02_Count)));
                    if (_f.CurrentUser.j04IsModule_o43)
                    {
                        _tabs.Add(AddTab("Inbox", "o43Inbox", "/TheGrid/SlaveView?prefix=o43", true, Badge1(cpo51.o43_Count)));
                    }
                    
                    break;
                case "p90":
                    var cp90 = _f.p90ProformaBL.LoadSumRow(pid);
                    
                    _tabs.Add(AddTab(cp90.p89Name, "tab1", "/p90/Tab1?pid=" + AppendPid2Url(pid)));
                    _tabs.Add(AddTab("Faktury (Vyúčtování)", "p91Invoice", "/TheGrid/SlaveView?prefix=p91", true, Badge1(cp90.p91_Count)));
                    if (_f.CurrentUser.j04IsModule_o43)
                    {
                        _tabs.Add(AddTab("Inbox", "o43Inbox", "/TheGrid/SlaveView?prefix=o43", true, Badge1(cp90.o43_Count)));
                    }
                    _tabs.Add(AddTab("<span class='material-icons-outlined-btn'>speaker_notes</span>", "b05", "/Record/TabB05?prefix=p90", false, Badge1(cp90.b05_Count),"Poznámky, workflow historie"));


                    break;
                case "p91":                    
                    var cp91 = _f.p91InvoiceBL.LoadSumRow(pid);
                    _tabs.Add(AddTab(cp91.p92Name, "tab1", "/p91/Tab1?pid=" + AppendPid2Url(pid)));
                    _tabs.Add(AddTab("Úkony", "p31Worksheet", "/TheGrid/SlaveView?prefix=p31", true, Badge3(cp91.p31_time_count, cp91.p31_expense_count, cp91.p31_fee_count)));
                    _tabs.Add(AddTab("Hodiny", "p31time", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|time", true, Badge1(cp91.p31_time_count)));
                    _tabs.Add(AddTab("Výdaje", "p31expense", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|expense", true, Badge1(cp91.p31_expense_count)));
                    _tabs.Add(AddTab("Odměny", "p31fee", "/TheGrid/SlaveView?prefix=p31&myqueryinline=tabquery|string|fee", true, Badge1(cp91.p31_fee_count)));
                    //_tabs.Add(AddTab("∑", "p31totals", "/p31totals/Index?prefix=p91", false));
                    //if (_f.CurrentUser.IsVysledovkyAccess)
                    //{
                    //    _tabs.Add(AddTab("<span class='material-icons-outlined-btn'>speed</span>", "vysledovka", "/Vysledovka/Index?prefix=p91", false, null, "Výsledovky"));
                    //}
                    
                    _tabs.Add(AddTab("Zálohy", "p90Proforma", "/TheGrid/SlaveView?prefix=p90", true, Badge1(cp91.p90_count)));
                    _tabs.Add(AddTab("Projekty", "p41Project", "/TheGrid/SlaveView?prefix=p41", true, Badge1(cp91.p41_count)));
                    _tabs.Add(AddTab("Zapojení uživatelé", "j02User", "/TheGrid/SlaveView?prefix=j02", true, Badge1(cp91.j02_count)));
                    _tabs.Add(AddTab("Outbox", "x40MailQueue", "/TheGrid/SlaveView?prefix=x40", true));
                    if (_f.CurrentUser.j04IsModule_o43)
                    {
                        _tabs.Add(AddTab("Inbox", "o43Inbox", "/TheGrid/SlaveView?prefix=o43", true, Badge1(cp91.o43_count)));
                    }
                    
                    _tabs.Add(AddTab("<span class='material-icons-outlined-btn'>speaker_notes</span>", "b05", "/Record/TabB05?prefix=p91",false, Badge1(cp91.b05_count),"Poznámky, workflow historie"));


                    break;
                case "p84":
                    var lisB05 = _f.WorkflowBL.GetList_b05("p84", pid, 0, 0, 0);
                    _tabs.Add(AddTab("Upomínka", "tab1", "/p84/Tab1?pid=" + AppendPid2Url(pid), false));
                    _tabs.Add(AddTab("Outbox", "x40MailQueue", "/TheGrid/SlaveView?prefix=x40", true));
                    _tabs.Add(AddTab("<span class='material-icons-outlined-btn'>speaker_notes</span>", "b05", "/Record/TabB05?prefix=p84", false, Badge1(lisB05.Count()), "Poznámky"));



                    break;


            }
            if (_tabs.Count()>0 && _tabs[0].Name == null)
            {
                _tabs[0].Name = "Vyberte záznam v tabulce";
                for(int i = 1; i < _tabs.Count(); i++)
                {
                    _tabs[i].Name = "???";
                }
            }

            if (_f.p07LevelsCount > 1 && (prefix=="le4" || prefix=="le3" || prefix=="le2" || prefix=="le1"))
            {
                int intLevelIndex = Convert.ToInt32(prefix.Substring(2, 1));
                int intBadgeChild = 0;
                for(int i = intLevelIndex+1; i <= 5; i++)
                {
                    if (_f.getP07Level(i,true) != null)
                    {
                        if (i == 5) intBadgeChild = cp41.le5_Count;
                        if (i == 4) intBadgeChild = cp41.le4_Count;
                        if (i == 3) intBadgeChild = cp41.le3_Count;
                        if (i == 2) intBadgeChild = cp41.le2_Count;
                        _tabs.Add(AddTab(_f.getP07Level(i, false), "le"+i.ToString(), "/TheGrid/SlaveView?prefix=le"+i.ToString(), true,Badge1(intBadgeChild)));
                    }
                }
            }

            return _tabs;
            
        }

        
        private string Badge1(int num1, string cssclassname = "badge bg-primary")
        {
            if (num1 == 0)
            {
                return null;
            }
            return $"<span class='{cssclassname}'>{num1}</span>";
        }
        private string Badge2(int num1, int num2, string cssclassname1 = "badge bg-warning text-dark", string cssclassname2 = "badge bg-success")
        {
            string s = null;
            if (cssclassname1== cssclassname2)
            {
                return $"<span class='{cssclassname1}'>{num1}+{num2}</span>";
            }
            if (num1 !=0)
            {
                s = $"<span class='{cssclassname1}'>{num1}</span>";
            }
            if (num2 != 0)
            {
                s += $"<span class='{cssclassname2}'>{num2}</span>";
            }
            return s;
        }
        private string Badge3(int num1,int num2,int num3,string cssclassname= "badge bg-primary")
        {            
            if (num1==0 && num2==0 && num3 == 0)
            {
                return null;
            }

            return $"<span class='{cssclassname}'>{num1}+{num2}+{num3}</span>";
        }

        public NavTab AddTab(string strName, string strTabKey, string strUrl, bool istranslate = true, string strBadge = null,string strTooltip=null)
        {
            if (istranslate)
            {
                strName = _f.tra(strName);
            }
            
            return new NavTab() { Name = strName, Entity = strTabKey, Url = strUrl, Badge = strBadge,ClientID= "tab" + strTabKey,Tooltip=strTooltip};
            }

        private string AppendPid2Url(int go2pid)
        {
            if (go2pid > 0)
            {
                return go2pid.ToString();
            }
            else
            {
                return "@pid";
            }
        }
    }
}
