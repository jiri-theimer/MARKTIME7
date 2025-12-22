using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;

using UI.Models;


namespace UI.Controllers
{
    public class TheGridController : BaseController        
    {
        private readonly BL.Singleton.ThePeriodProvider _pp;
        public TheGridController(BL.Singleton.ThePeriodProvider pp)
        {
            _pp = pp;            
        }

        public IActionResult FpView(string prefix, int go2pid, string myqueryinline,string rez)    //grid pro statisitky finančních plánů
        {
            if (!TestGridPermissions(prefix))
            {
                return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
            }
            if (string.IsNullOrEmpty(prefix))
            {
                prefix = Factory.CBL.LoadUserParam("fpview-prefix", "fp2");
            }

            if (go2pid == 0)
            {
                go2pid = LoadLastUsedPid(prefix,rez);
            }

            
            //zde je master-entity natvrdo výraz: approve_aio
            TheGridViewModel v = LoadFsmViewModel(prefix, go2pid, "fpview", "fpview_aio", 0, myqueryinline, true, rez);
            
            v.gridinput.j72id = Factory.CBL.LoadUserParamInt($"fpview-j72id-{prefix}-{rez}");

            return View(v);
        }

        public IActionResult ApproveView(string prefix, int go2pid, string myqueryinline, string rez)    //grid pro smart-schvalování
        {
            if (string.IsNullOrEmpty(prefix))
            {
                prefix = Factory.CBL.LoadUserParam("approveview-prefix", "p41");
            }

            if (go2pid == 0)
            {
                go2pid = LoadLastUsedPid(prefix, rez);
            }


            //zde je master-entity natvrdo výraz: approve_aio
            int intJ27ID = Factory.CBL.LoadUserParamInt("approveview-j27id");
            if (intJ27ID > 0)
            {
                if (string.IsNullOrEmpty(myqueryinline))
                {
                    myqueryinline = $"j27id_query|int|{intJ27ID}";
                }
                else
                {
                    myqueryinline += $"|j27id_query|int|{intJ27ID}";
                }
            }
            TheGridViewModel v = LoadFsmViewModel(prefix, go2pid, "approveview", "approve_aio", 0, myqueryinline,true, rez);
            v.j27id_query = intJ27ID;

            v.gridinput.j72id = Factory.CBL.LoadUserParamInt($"approveview-j72id-{prefix}-{rez}");

            Handle_InhaleQuery(v, $"approveview-query-j72id-{prefix}-{rez}");


            return View(v);
        }

        public IActionResult FlatView(string prefix, int go2pid, string myqueryinline,string tab,string rez)    //pouze grid bez subform
        {
            if (!TestGridPermissions(prefix))
            {
                return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
            }
            if (go2pid == 0 && (prefix=="p28" || prefix=="p91" || prefix=="p41" || prefix=="j02" || prefix == "o23" || prefix=="p90" || prefix == "p51" || prefix=="p40" || prefix=="o43" || prefix=="p84"))
            {
                go2pid = LoadLastUsedPid(prefix,rez);
            }
            
            if (tab==null && (prefix == "p31" || prefix == "j02"))
            {
                tab = Factory.CBL.LoadUserParam("overgrid-tab-" + prefix,"zero",1);
                if (tab != null && tab != "zero")
                {
                    myqueryinline = $"tabquery|string|{tab}";
                }
            }
            
            TheGridViewModel v = LoadFsmViewModel(prefix, go2pid,"flatview",null,0, myqueryinline, TestIfPeriodOverGrid(prefix),rez);
            
            v.gridinput.j72id = Factory.CBL.LoadUserParamInt($"flatview-j72id-{prefix}-{rez}");            
            Handle_InhaleQuery(v, $"flatview-query-j72id-{prefix}-{rez}");

            var cTabs = new UI.Code.NavTabsSupport(Factory);
            v.OverGridTabs = cTabs.getOverGridTabs(v.prefix, tab,true,v.rez);

            

            return View(v);
        }

        
        private void Handle_InhaleQuery(TheGridViewModel v,string strKey)
        {            
            v.gridinput.j72id_query = Factory.CBL.LoadUserParamInt(strKey);
            v.TheGridQueryButton = new UI.Models.TheGridQueryViewModel() { j72id = v.gridinput.j72id_query,paramkey=strKey,prefix=BO.Code.Entity.GetPrefixDb(v.prefix) };

            if (v.TheGridQueryButton.j72id > 0)
            {
                v.TheGridQueryButton.j72name = Factory.j72TheGridTemplateBL.LoadName(v.TheGridQueryButton.j72id);
            }            
        }
        public IActionResult MasterView(string prefix,int go2pid,string myqueryinline, string tab,string rez)    //grid horní + spodní panel, není zde filtrovací pruh s fixním filtrem
        {
            //rez je o17id z o17DocMenu

            if (!TestGridPermissions(prefix))
            {
                return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
            }
            if (go2pid == 0)
            {
                go2pid = LoadLastUsedPid(prefix,rez);
            }
            

            TheGridViewModel v = LoadFsmViewModel(prefix, go2pid,"masterview",null,0, myqueryinline, TestIfPeriodOverGrid(prefix),rez);
            
            v.gridinput.j72id = Factory.CBL.LoadUserParamInt($"masterview-j72id-{prefix}-{rez}");
            Handle_InhaleQuery(v, $"masterview-query-j72id-{prefix}-{rez}");
            

            var cTabs = new UI.Code.NavTabsSupport(Factory);
            v.OverGridTabs = cTabs.getOverGridTabs(v.prefix, tab,false,v.rez);
            v.NavTabs = cTabs.getMasterTabs(v.prefix, go2pid,false);
            
            string strDefTab = tab;
            if (string.IsNullOrEmpty(strDefTab))
            {
                strDefTab=Factory.CBL.LoadUserParam($"masterview-tab-{prefix}");
            }
            NavTab deftab = v.NavTabs[0];
            
            foreach (var ntab in v.NavTabs)
            {
               if (!ntab.Url.Contains("?pid"))
                {
                    //ntab.Url += "&master_entity=" + v.entity + "&master_pid=" + AppendPid2Url(v.gridinput.go2pid);
                    ntab.Url += $"&master_entity={v.entity}&master_pid={AppendPid2Url(v.gridinput.go2pid)}";

                }
                ntab.Url += "&caller=grid";
             
                if (strDefTab !="" && ntab.Entity== strDefTab)
                {
                    deftab = ntab;  //uživatelem naposledy vybraná záložka
                }
            }
            deftab.CssClass += " active";
            if (go2pid > 0)
            {
                v.go2pid_url_in_iframe = deftab.Url;
                
            }

            
            
            return View(v);
        }

        private string AppendPid2Url(int go2pid)
        {
            if (go2pid > 0)
            {
                return go2pid.ToString();
            }
            else
            {
                return  "@pid";
            }
        }
        private bool TestIfPeriodOverGrid(string prefix)
        {
            if (prefix=="p51" || prefix == "p44" || prefix == "p15")
            {
                return false;
            }
            if (this.Factory.CurrentUser.j04IsModule_p31)
            {
                return true;
            }
            return false;    //filtrování období nabízet, pokud je zapnuté vykazování aktivit
            
        }
        public IActionResult SlaveView(string master_entity,int master_pid, string prefix, int go2pid,string myqueryinline,string caller,string rez)    //podřízený subform v rámci MasterView
        {            
            TheGridViewModel v = LoadFsmViewModel(prefix, go2pid, "slaveview", master_entity, master_pid, myqueryinline, TestIfPeriodOverGrid(prefix),rez);
            v.caller = caller;

            v.gridinput.j72id = Factory.CBL.LoadUserParamInt("slaveview-j72id-" + prefix + "-" + master_entity.Substring(0, 3) + "-"+rez);
            //if (d != null)
            //{
            //    v.gridinput.query.global_d1 = BO.Code.Bas.String2Date(d);
            //    v.gridinput.query.global_d2 = v.gridinput.query.global_d1;
            //}
            

            Handle_InhaleQuery(v, $"slaveview-query-j72id-{prefix}-{master_entity.Substring(0, 3)}-{rez}");
            if (String.IsNullOrEmpty(master_entity) || master_pid == 0)
            {
                AddMessage("Musíte vybrat záznam z nadřízeného panelu.");
            }

            if (caller != null)
            {
                Factory.CBL.SaveLastCallingRecPid(master_entity.Substring(0,3), master_pid, caller, true, false,rez);
            }


            return View(v);
        }

        public IActionResult MobileView(string prefix, int go2pid, string myqueryinline, string tab, string rez)    //pouze grid bez subform
        {
            

            if (!TestGridPermissions(prefix))
            {
                return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
            }
            if (go2pid == 0 && (prefix == "p28" || prefix == "p91" || prefix == "p41" || prefix == "j02" || prefix == "o23" || prefix == "p90" || prefix == "p51" || prefix == "p40" || prefix == "o43" || prefix=="p84"))
            {
                go2pid = LoadLastUsedPid(prefix, rez);
            }

            if (tab == null && (prefix == "p31" || prefix == "j02"))
            {
                tab = Factory.CBL.LoadUserParam("overgrid-tab-" + prefix, "zero", 1);
                if (tab != null && tab != "zero")
                {
                    myqueryinline = $"tabquery|string|{tab}";
                }
            }

            TheGridViewModel v = LoadFsmViewModel(prefix, go2pid, "flatview", "mobile", 0, myqueryinline, TestIfPeriodOverGrid(prefix), rez);

            v.gridinput.j72id = Factory.CBL.LoadUserParamInt($"flatview-j72id-{prefix}-{rez}");            
            Handle_InhaleQuery(v, $"flatview-query-j72id-{prefix}-{rez}");

            var cTabs = new UI.Code.NavTabsSupport(Factory);
            v.OverGridTabs = cTabs.getOverGridTabs(v.prefix, tab, true, v.rez);



            return View(v);
        }

        private bool TestGridPermissions(string prefix)
        {

            switch (prefix)
            {
                case "p31":
                    return Factory.CurrentUser.j04IsModule_p31;
                case "p41":
                case "le5":
                case "le4":
                case "le3":
                case "le2":
                case "le1":
                    return Factory.CurrentUser.j04IsModule_p41;
                case "p28":
                    return Factory.CurrentUser.j04IsModule_p28;
               
                case "p90":
                    return Factory.CurrentUser.j04IsModule_p90;                
                case "p91":
                case "p84":
                    return Factory.CurrentUser.j04IsModule_p91;
                case "j02":
                    return Factory.CurrentUser.j04IsModule_j02;
                case "o23":
                    return Factory.CurrentUser.j04IsModule_o23;
                case "p56":
                    return Factory.CurrentUser.j04IsModule_p56;
                case "p51":
                    return Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_p51_Admin);
                case "p40":
                    if (!Factory.CurrentUser.IsRatesAccess)
                    {
                        return false;
                    }
                    return Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_p41_Reader);
                case "o51":
                case "o53":
                    return Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_o51_Admin);
                case "o43":
                    return Factory.CurrentUser.j04IsModule_o43;
                case "p11":
                    return Factory.CurrentUser.j04IsModule_p11;
                case "p49":
                case "fp1":
                case "fp2":
                case "fp3":
                    return Factory.CurrentUser.j04IsModule_p49;
                case "b05":
                    return true;
                   
                    
                default:
                    return true;
            }
        }
        private TheGridViewModel LoadFsmViewModel(string prefix,int go2pid,string pagename,string masterentity,int master_pid,string myqueryinline,bool isperiodovergrid,string rez)
        {            

            var v = new TheGridViewModel() { prefix = prefix,master_pid=master_pid, myqueryinline = myqueryinline,rez=rez };

            switch (v.prefix)
            {
                case "o43":
                    v.o43mavazbu = Factory.CBL.LoadUserParamInt("grid-o43-mavazbu");
                    if (v.o43mavazbu == 1) v.myqueryinline = "mavazbu|bool|false";
                    if (v.o43mavazbu == 2) v.myqueryinline = "mavazbu|bool|true";
                    break;
               
               
                case "x40":
                    if (v.master_pid == 0 && string.IsNullOrEmpty(masterentity))
                    {
                        v.myqueryinline = $"j02id_creator|int|{Factory.CurrentUser.pid}";   //Můj OUTBOX
                    }
                    break;
                case "p31":
                    if (masterentity == "le4" || masterentity == "le3")
                    {
                        v.show_podrizene = Factory.CBL.LoadUserParamBool("slaveview-show-podrizene", true);
                        if (!v.show_podrizene)
                        {
                            if (v.myqueryinline == null)
                            {
                                v.myqueryinline = $"p41id|int|{v.master_pid}";  //uživatel chce vidět úkony bez podřízených projektů
                            }
                            else
                            {
                                v.myqueryinline += $"|p41id|int|{v.master_pid}";  //uživatel chce vidět úkony bez podřízených projektů
                            }                            
                        }
                    }

                    break;
                case "p41":
                    v.p41treequery = new ProjectTreeQueryViewModel();
                    v.p41treequery.Value = Factory.CBL.LoadUserParamInt("p41-treequery");
                    if (v.p41treequery.Value > 0)
                    {
                        if (v.myqueryinline == null)
                        {
                            v.myqueryinline = $"p07level|int|{v.p41treequery.Value}";
                        }
                        else
                        {
                            v.myqueryinline += $"|p07level|int|{v.p41treequery.Value}";
                        }
                        
                    }
                    break;
                
            }
           

            BO.TheEntity c = Factory.EProvider.ByPrefix(prefix);
            v.entity = c.TableName;
            v.entityTitle = c.AliasPlural;
            if (prefix == "le5") v.entityTitle = Factory.getP07Level(5, false);

            bool bolMasterEntity = false;bool bolAIO = false;
            if (!string.IsNullOrEmpty(masterentity))
            {
                bolMasterEntity = true;
                if (masterentity == "approve_aio") bolAIO = true; //all-in-one schvalování má společné hodnoty pro období/filtrování
            }
            

            v.gridinput = new Views.Shared.Components.myGrid.myGridInput() {entity=v.entity, go2pid = go2pid, master_entity = masterentity,master_pid=master_pid,myqueryinline=v.myqueryinline,ondblclick= "grid_dblclick",isperiodovergrid= isperiodovergrid,rez=v.rez };
            

            if (v.entity == "")
            {
                AddMessage("Grid entita nebyla nalezena.");
            }

            
            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load(prefix, masterentity, master_pid, v.myqueryinline,rez);
            if (v.gridinput.query.j02id_query > 0 && v.gridinput.query.j02id_query != v.gridinput.query.CurrentUser.pid)
            {
                v.gridinput.query.CurrentUser = Factory.j02UserBL.LoadRunningUser(v.gridinput.query.j02id_query);
            }
          
            v.gridinput.query.MyRecordsDisponible = !Factory.CurrentUser.IsAdmin;   //postarat se, aby uživatel viděl pouze záznamy, které má mít přístupné
            if (bolAIO)
            {
                v.gridinput.query.MyRecordsDisponible_Approve = true;
            }
            
            if (isperiodovergrid)
            {                
                v.periodinput = new Views.Shared.Components.myPeriod.myPeriodViewModel() { prefix = v.prefix };                
                if (bolMasterEntity)
                {
                    v.periodinput.UserParamKey = $"grid-period-{v.prefix}-{masterentity}";
                    if (bolAIO) v.periodinput.UserParamKey = "approve-index-period";
                }
                else
                {
                    v.periodinput.UserParamKey = $"grid-period-{v.prefix}";
                }
                if (v.myqueryinline != null && v.gridinput.query.global_d1 !=null)
                {
                    v.periodinput.SetIsLoaded();
                    v.periodinput.d1 = v.gridinput.query.global_d1; //filtrovací období je předáno v myqueryinline
                    v.periodinput.d2 = v.gridinput.query.global_d2;
                    v.periodinput.PeriodValue = 1;
                    v.periodinput.PeriodAlias = v.periodinput.d1.Value.ToString("dd.MM.yyyy");
                    if (v.gridinput.query.global_d1 != v.gridinput.query.global_d2)
                    {
                        v.periodinput.PeriodAlias = $"{v.periodinput.d1.Value.ToString("dd.MM.")} - {v.periodinput.d2.Value.ToString("dd.MM.")}";
                    }
                    v.periodinput.PeriodField = "p31Date";
                    v.periodinput.PeriodFieldAlias = "Datum úkonu";
                    
                }
                else
                {
                    v.periodinput.LoadUserSetting(_pp, Factory);    //načtení d1/d2/period_field pro query
                }
                
                v.gridinput.query.global_d1 = v.periodinput.d1;
                v.gridinput.query.global_d2 = v.periodinput.d2;
                v.gridinput.query.period_field = v.periodinput.PeriodField;
            }
            if (BL.Code.UserUI.GetFlatViewValue(v.prefix)>0)
            {
                v.IsCanbeMasterView = true;
                v.dblClickSetting = Factory.CBL.LoadUserParam($"grid-{v.prefix}-dblclick", "edit");
            }
           
            if (this.Factory.CurrentUser.j04IsModule_p31 && masterentity !="p91Invoice" && (v.prefix == "p31" || v.prefix == "p41" || v.prefix == "p28" || v.prefix == "o23" || v.prefix=="p56" || v.prefix=="j02" || v.prefix == "le5" || v.prefix == "le4"))
            {
                v.p31statequery = new p31StateQueryViewModel();
                if (bolMasterEntity)
                {                    
                    v.p31statequery.UserParamKey = $"grid-{v.prefix}-{masterentity}-p31statequery";
                    if (bolAIO) v.p31statequery.UserParamKey = "approve-p31statequery";
                }
                else
                {                    
                    v.p31statequery.UserParamKey = $"grid-{v.prefix}-p31statequery";
                }
                v.p31statequery.Value = Factory.CBL.LoadUserParamInt(v.p31statequery.UserParamKey, (bolAIO ? 3 : 0));

                v.gridinput.query.p31statequery = v.p31statequery.Value;

                
                
            }

            if (this.Factory.CurrentUser.j04IsModule_p31 && (v.prefix=="p91" || v.p31statequery !=null) && (!bolMasterEntity || bolAIO))
            {
                v.p31tabquery = new p31TabQueryViewModel();
                if (bolMasterEntity)
                {                    
                    v.p31tabquery.UserParamKey = $"grid-{v.prefix}-{masterentity}-p31formatquery";
                    if (bolAIO) v.p31tabquery.UserParamKey= "approve-p31tabquery";
                }
                else
                {                    
                    v.p31tabquery.UserParamKey = $"grid-{v.prefix}-p31formatquery";
                }
                v.p31tabquery.Value = Factory.CBL.LoadUserParam(v.p31tabquery.UserParamKey);
                v.gridinput.query.p31tabquery = v.p31tabquery.Value;
            }

            v.gridinput.query.IsRecordValid = null;
            if (v.prefix !="j92" && v.prefix !="j90" && v.prefix !="x40")
            {
                v.recordbinquery = new RecordBinQueryViewModel() { Prefix=v.prefix};
                if (bolMasterEntity)
                {                    
                    v.recordbinquery.UserParamKey = $"grid-{v.prefix}-{masterentity}-recordbinquery";
                    if (bolAIO) v.recordbinquery.UserParamKey = "approve-recordbinquery";
                }
                else
                {                    
                    v.recordbinquery.UserParamKey = $"grid-{v.prefix}-recordbinquery";
                }
                v.recordbinquery.Value = Factory.CBL.LoadUserParamInt(v.recordbinquery.UserParamKey,1);
                switch (v.recordbinquery.Value)
                {
                    case 1:
                        v.gridinput.query.IsRecordValid = true;break;   //pouze otevřené záznamy
                    case 2:
                        v.gridinput.query.IsRecordValid = false;break;  //pouze záznamy v archivu
                    default:
                        v.gridinput.query.IsRecordValid = null;break;
                }                
            }
            
            
            
           

            return v;

        }


        private int LoadLastUsedPid(string prefix,string rez)
        {
            return Factory.CBL.LoadUserParamInt($"recpage-{prefix}-{rez}-pid", 0, 12);

          
            
        }

        public List<NavTab> getTabs(string prefix,int pid)  //volá se z javascriptu při změně řádky v gridu
        {
            var cTabs = new UI.Code.NavTabsSupport(Factory);
            return cTabs.getMasterTabs(prefix, pid,true);
        }

       

    }
}