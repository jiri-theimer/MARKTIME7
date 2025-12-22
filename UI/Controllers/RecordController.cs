
using Microsoft.AspNetCore.Mvc;
using System.Data;
using UI.Models;
using UI.Models.a55;
using UI.Models.Record;
using UI.Models.Tab1;
using UI.Views.Shared.Components.myGrid;

namespace UI.Controllers
{
    public class RecordController : BaseController
    {
        private readonly BL.Singleton.ThePeriodProvider _pp;
        public RecordController(BL.Singleton.ThePeriodProvider pp)
        {
            _pp = pp;
        }
        public IActionResult FreeFieldsAutoComplete(string x28field,string controlid,string header)
        {
            var v = new FreeFieldsAutoCompleteViewModel() { x28field = x28field, controlid = controlid,header=header };
            string strTab = BO.Code.Entity.GetEntity(x28field.Substring(0, 3));
            string strSQL = $"select distinct {v.x28field} as Value FROM {strTab}_FreeField";

            var dt = Factory.FBL.GetDataTable(strSQL);
            v.Values = new List<string>();
            foreach(DataRow dbrow in dt.Rows)
            {
                if (dbrow["Value"] != System.DBNull.Value)
                {
                    v.Values.Add(dbrow["Value"].ToString());
                }
                
            }
            return View(v);
        }
        public IActionResult NajitText(string field,int j02id)
        {
            var v = new NajitTextViewModel() {field=field, prefix = field.Substring(0,3) };
            int intJ02ID = j02id;
            if (intJ02ID == 0)
            {
                intJ02ID = Factory.CurrentUser.pid;
            }
            switch (v.field.ToLower())
            {
                case "p31text":
                    v.gridinput = new Views.Shared.Components.myGrid.myGridInput() { is_enable_selecting = true, entity = "p31Worksheet", master_entity = "inform", myqueryinline = $"j02id|int|{intJ02ID}", ondblclick = "grid_dblclick" };
                    v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load("p31", null, 0, $"j02id|int|{intJ02ID}");
                    v.gridinput.fixedcolumns = "a__p31Worksheet__p31Text,a__p31Worksheet__p31Date,p31_p41_client__p28Contact__p28Name,p31_p41__p41Project__p41Name,p31_p32__p32Activity__p32Name,a__p31Worksheet__p31Hours_Orig";

                    break;
                case "p91text1":
                    v.gridinput = new Views.Shared.Components.myGrid.myGridInput() { is_enable_selecting = true, entity = "p91Invoice", master_entity = "inform", ondblclick = "grid_dblclick" };
                    v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load("p91");
                    v.gridinput.fixedcolumns = "a__p91Invoice__p91Text1,a__p91Invoice__p91Code,a__p91Invoice__p91Client,a__p91Invoice__p91DateSupply,a__p91Invoice__p91Amount_WithoutVat,a__p91Invoice__j27Code";
                    break;
                case "p91text2":
                    v.gridinput = new Views.Shared.Components.myGrid.myGridInput() { is_enable_selecting = true, entity = "p91Invoice", master_entity = "inform", ondblclick = "grid_dblclick" };
                    v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load("p91");
                    v.gridinput.fixedcolumns = "a__p91Invoice__p91Text2,a__p91Invoice__p91Text1,a__p91Invoice__p91Code,a__p91Invoice__p91Client,a__p91Invoice__p91DateSupply,a__p91Invoice__p91Amount_WithoutVat,a__p91Invoice__j27Code";

                    break;
            }
           
            


            return View(v);
        }
        public IActionResult QuickStat(string prefix,int pid,string p31guid)
        {
            var v = new BaseTab1ViewModel() { prefix = prefix, pid = pid,p31guid=p31guid };

            return View(v);
        }
        public IActionResult Barcode(string value)
        {
            ViewBag.value = value;
            return View(new BaseViewModel());
        }
        public IActionResult TabB05(string prefix,string master_entity,int master_pid,string caller)
        {
            var v = new BaseTab1ViewModel() { prefix = prefix, pid = master_pid };

            if (caller == "grid")
            {
                Factory.CBL.SaveLastCallingRecPid(master_entity, master_pid, "grid", true, false, null);
            }

            return View(v);
        }
        public IActionResult DynamicPage(int pid, string prefix,int a55id,int a59id,int b02id)    //individuální stránka záznamu (dokument/projekt/kontakt/vyúčtování
        {
            var v = new DynamicPageViewModel() { rec_pid = pid, rec_prefix = prefix,a55ID=a55id,b02ID=b02id, portlet_cssclass="portlet" };
            if (v.a55ID == 0 || v.rec_pid == 0 || string.IsNullOrEmpty(v.rec_prefix))
            {
                return this.StopPage(false, "pid or prefix or a55id missing");
            }

            v.RecA55 = Factory.a55RecPageBL.Load(v.a55ID);
            if (v.RecA55.a55Entity != v.rec_prefix)
            {
                this.AddMessageTranslated("Šablona stránky je určena pro entitu: " + BO.Code.Entity.GetAlias(v.RecA55.a55Entity));                
            }
            if (a59id > 0)
            {
                v.RecA59 = Factory.a59RecPageLayerBL.Load(a59id);   //simulace předává explicitně id plochy
                v.portlet_cssclass = "portlet_simulation";
            }
            else
            {
                v.RecA59 = Factory.a59RecPageLayerBL.LoadByWorkflow(v.a55ID, v.b02ID);
            }
            

            if (v.RecA59 == null)
            {
                return this.StopPage(false, "Nelze načíst vhodnou plochu individuální stránky záznamu.");
            }

            if (v.RecA59.a59StructureFlag == BO.a59StructureFlagENUM.CustomHtml)
            {
                //vlastní html struktura
                v.lisAllWidgets = new List<BO.a58RecPageBox>();
                
                var codes = BO.Code.MergeContent.GetAllMergeFieldsInContent(v.RecA59.a59CustomHtmlStructure);
                foreach (string onecode in codes)
                {
                    var recA58 = Factory.a58RecPageBoxBL.LoadByCode(onecode, 0);
                    if (recA58 != null)
                    {
                        v.RecA59.a59CustomHtmlStructure = v.RecA59.a59CustomHtmlStructure.Replace("[%" + onecode + "%]", $"<div id='final{onecode}'>[{onecode}%]</div>");
                        v.lisAllWidgets.Add(recA58);
                    }
                }                

                return View(v);                 //zde vypadnout a dále nepokračovat! 
            }

            v.ColumnsPerPage = (v.RecA59.a59ColumnsPerPage <= 0) ? 1 : v.RecA59.a59ColumnsPerPage;
            v.DockStructure = new WebpageLayerEnvironment(v.RecA59.a59DockState);
            v.lisAllWidgets = Factory.a58RecPageBoxBL.GetList(new BO.myQuery("a58")).Where(p => p.a59ID == v.RecA59.pid).ToList();

            v.lisUserWidgets = new List<BO.a58RecPageBox>();
            var boxes = BO.Code.Bas.ConvertString2List(v.RecA59.a59Boxes);
            foreach (string s in boxes)
            {
                if (v.lisAllWidgets.Where(p => p.a58Guid.ToString() == s).Count() > 0)
                {
                    v.lisUserWidgets.Add(v.lisAllWidgets.Where(p => p.a58Guid.ToString() == s).First());
                }
            }

            foreach (var onestate in v.DockStructure.States)
            {
                if (v.lisUserWidgets.Where(p => p.pid.ToString() == onestate.Value).Count() > 0)
                {
                    var c = v.lisUserWidgets.Where(p => p.pid.ToString() == onestate.Value).First();
                    switch (onestate.Key)
                    {
                        case "2":
                            if (v.ColumnsPerPage >= 2) v.DockStructure.Col2.Add(c);
                            break;
                        case "3":
                            if (v.ColumnsPerPage >= 3) v.DockStructure.Col3.Add(c);
                            break;
                        default:
                            v.DockStructure.Col1.Add(c);
                            break;
                    }
                }
            }

            foreach (var c in v.lisUserWidgets)
            {
                if ((v.DockStructure.Col1.Contains(c) || v.DockStructure.Col2.Contains(c) || v.DockStructure.Col3.Contains(c)) == false)
                {
                    switch (v.ColumnsPerPage)
                    {
                        case 2 when (v.DockStructure.Col1.Count() >= 2):
                            v.DockStructure.Col2.Add(c);
                            break;
                        case 3 when (v.DockStructure.Col1.Count() >= 2 && v.DockStructure.Col2.Count() >= 2):
                            v.DockStructure.Col3.Add(c);
                            break;
                        case 3 when (v.DockStructure.Col1.Count() >= 2 && v.DockStructure.Col2.Count() < 2):
                            v.DockStructure.Col2.Add(c);
                            break;
                        default:
                            v.DockStructure.Col1.Add(c);
                            break;
                    }

                }

            }

            switch (v.ColumnsPerPage)
            {
                case 1:
                    v.BoxColCss = "col-12";
                    break;
                case 2:
                    v.BoxColCss = "col-lg-6";
                    break;
                case 3:
                    v.BoxColCss = "col-sm-6 col-lg-4";
                    break;
            }

            

            
            return View(v);

        }

        public IActionResult NotepadPreview(string prefix,int pid)
        {
            var v = new UI.Models.Notepad.PreviewViewModel() { pid = pid, prefix = prefix };
            switch (v.prefix)
            {
                
                case "o23":
                    var reco23 = Factory.o23DocBL.Load(pid);
                    if (reco23.o23IsEncrypted)
                    {
                        if (Factory.p85TempboxBL.LoadRemember(pid) != null)
                        {
                            reco23.o23Notepad = Factory.o23DocBL.EncryptDescryptNotepad(reco23.o23Notepad, false);
                            reco23.o23IsEncrypted = false;
                        }
                        else
                        {
                            return RedirectToAction("Decrypt", new { pid = pid, caller = "Notepad" });
                        }
                    }
                    v.NotepadContent = reco23.o23Notepad; v.x04ID = reco23.x04ID;
                    break;
                
                
               
                case "p56":
                    var recp56 = Factory.p56TaskBL.Load(pid);
                    v.NotepadContent = recp56.p56Notepad; v.x04ID = recp56.x04ID;
                    break;
                case "o22":
                    var recO22 = Factory.o22MilestoneBL.Load(pid);
                    v.NotepadContent = recO22.o22Notepad; v.x04ID = recO22.x04ID;
                    break;
                case "p58":
                    var recP58 = Factory.p58TaskRecurrenceBL.Load(pid);
                    v.NotepadContent = recP58.p58Notepad; v.x04ID = recP58.x04ID;
                    break;
                case "b05":
                    var recB05 = Factory.WorkflowBL.GetList_b05(null, 0, 0, pid,0).First();
                    v.NotepadContent = recB05.b05Notepad; v.x04ID = recB05.x04ID;
                    break;
            }

            return View(v);

        }
        public IActionResult RecPageMobile(string prefix, int pid, string tab, string rez)
        {
            var v = new RecPageViewModel() { Factory = this.Factory, pid = pid, prefix = prefix, DefTab = tab };
            if (v.pid == 0)
            {
                v.pid = v.LoadLastUsedPid(v.prefix, null);
            }
            if (v.pid == 0)
            {
                v.NavTabs = new List<NavTab>();
                return View(v); //prázdná stránka
            }

            switch (v.prefix)
            {
                case "o23":
                    var permO23 = Factory.o23DocBL.InhaleRecDisposition(v.pid, null);
                    if (!permO23.ReadAccess)
                    {
                        return this.StopPage(false, "Nemáte oprávnění číst tento dokument.");
                    }                    
                    break;
                case "p28":
                    var permP28 = Factory.p28ContactBL.InhaleRecDisposition(v.pid, null);
                    if (!permP28.ReadAccess)
                    {
                        return this.StopPage(false, "Nemáte oprávnění číst tento kontakt.");
                    }                   
                    break;
                case "p41":
                case "le5":
                case "le4":
                case "le3":
                case "le2":
                case "le1":
                    if (v.pid > 0)
                    {
                        var permP41 = Factory.p41ProjectBL.InhaleRecDisposition(v.pid, null);
                        if (!permP41.ReadAccess)
                        {
                            return this.StopPage(false, "Nemáte oprávnění číst tento projekt.");
                        }                        
                    }
                    break;
                case "p91":
                    var permP91 = Factory.p91InvoiceBL.InhaleRecDisposition(v.pid, null);
                    if (!permP91.ReadAccess)
                    {
                        return this.StopPage(false, "Nemáte oprávnění číst toto vyúčtování.");
                    }                    
                    break;

            }
            
            v.MenuCode = Factory.CBL.GetObjectAlias(BO.Code.Entity.GetPrefixDb(v.prefix), v.pid);
            if (v.MenuCode == null)
            {
                
                this.Notify_RecNotFound();
                v.pid = 0;
            }
            else
            {
                
                
                Factory.CBL.SaveLastCallingRecPid(v.prefix, v.pid, "recpage", true, true, rez);  //uložit info o naposledy navštíveném záznamu, kvůli le1-le4 pracujeme s původním prefixem

                RefreshState_RecPage(v);

            }

            

            return View(v);
        }

        public IActionResult RecPageTree(string prefix, int pid, string tab, string rez, string myqueryinline)
        {
            var v = new RecPageViewModel() { Factory = this.Factory, pid = pid, prefix = prefix, DefTab = tab, rez = rez };

            if (v.pid == 0)
            {
                v.pid = v.LoadLastUsedPid(v.prefix, null);
            }



            v.recordbinquery = new RecordBinQueryViewModel() { UserParamKey = $"grid-{v.prefix}-recordbinquery" };
            v.recordbinquery.Value = Factory.CBL.LoadUserParamInt(v.recordbinquery.UserParamKey, 1);
            

            return View(v);

        }

        public IActionResult RecPage(string prefix, int pid, string tab,string rez, string myqueryinline)
        {            
            if (prefix == "dd1" && pid>0)
            {
                
                prefix = "o23";
            }
            if (Factory.CurrentUser.IsMobileDisplay())
            {
                return RedirectToAction("RecPageMobile", new { prefix = prefix, pid = pid, tab = tab, rez = rez }); //mobilní zařízení
            }
            var v = new RecPageViewModel() { Factory = this.Factory, pid = pid, prefix = prefix,DefTab=tab,rez=rez };
            
            if (v.pid == 0)
            {
                v.pid = v.LoadLastUsedPid(v.prefix,null);  //kvůli le1-le4 pracujeme s původním prefixem
            }            

            if (v.pid == 0 && !BL.Code.UserUI.IsShowLeftPanel(v.prefix,Factory.CurrentUser.j02UIBitStream))
            {
                v.NavTabs = new List<NavTab>();
                return View(v); //prázdná stránka
            }

            v.IsShowLeftPanel = BL.Code.UserUI.IsShowLeftPanel(v.prefix, Factory.CurrentUser.j02UIBitStream);

            switch (v.prefix)
            {
                case "o23":
                    if (!Factory.CurrentUser.j04IsModule_o23)
                    {
                        return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
                    }
                    if (v.pid > 0)
                    {
                        var permO23 = Factory.o23DocBL.InhaleRecDisposition(v.pid, null);
                        if (permO23 == null)
                        {
                            return Handle_RecordNotExists(prefix, rez);
                        }
                        if (!permO23.ReadAccess)
                        {
                            return this.StopPage(false, "Nemáte oprávnění číst tento dokument.");
                        }
                        if (permO23.a55ID > 0) return RedirectToAction("DynamicPage", new { prefix = v.prefix, pid = v.pid, a55id = permO23.a55ID });  //přesměrovat na individuál
                    }
                    
                    break;
                case "p28":
                    if (!Factory.CurrentUser.j04IsModule_p28)
                    {
                        return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
                    }
                    if (v.pid > 0)
                    {
                        var permP28 = Factory.p28ContactBL.InhaleRecDisposition(v.pid, null);
                        if (permP28 == null)
                        {
                            return Handle_RecordNotExists(prefix, rez);
                        }
                        if (!permP28.ReadAccess)
                        {
                            return this.StopPage(false, "Nemáte oprávnění číst tento kontakt.");
                        }
                        if (permP28.a55ID > 0) return RedirectToAction("DynamicPage", new { prefix = v.prefix, pid = v.pid, a55id = permP28.a55ID });  //přesměrovat na individuál
                    }
                    
                    
                    break;
                case "p41":
                case "le5":
                case "le4":
                case "le3":
                case "le2":
                case "le1":
                    if (!Factory.CurrentUser.j04IsModule_p41)
                    {
                        return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
                    }
                    if (v.pid > 0)
                    {
                        var permP41 = Factory.p41ProjectBL.InhaleRecDisposition(v.pid, null);
                        if (permP41 == null)
                        {
                            return Handle_RecordNotExists(prefix, rez);                            
                        }
                        if (!permP41.ReadAccess)
                        {
                            return this.StopPage(false, "Nemáte oprávnění číst tento projekt.");
                        }
                        if (permP41.a55ID > 0) return RedirectToAction("DynamicPage", new { prefix = v.prefix, pid = v.pid, a55id = permP41.a55ID });  //přesměrovat na individuál
                    }                    
                    break;
                case "p91":
                    if (!Factory.CurrentUser.j04IsModule_p91)
                    {
                        return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
                    }
                    if (v.pid > 0)
                    {
                        var permP91 = Factory.p91InvoiceBL.InhaleRecDisposition(v.pid, null);
                        if (permP91 == null)
                        {
                            return Handle_RecordNotExists(prefix, rez);
                        }
                        if (!permP91.ReadAccess)
                        {
                            return this.StopPage(false, "Nemáte oprávnění číst toto vyúčtování.");
                        }
                        if (permP91.a55ID > 0) return RedirectToAction("DynamicPage", new { prefix = v.prefix, pid = v.pid, a55id = permP91.a55ID });  //přesměrovat na individuál
                    }
                    
                    
                    v.p91uhrazene = Factory.CBL.LoadUserParamInt("grid-p91-uhrazene");
                    

                    break;
                case "p84":
                    if (!Factory.CurrentUser.j04IsModule_p91)
                    {
                        return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
                    }
                    
                    break;
                case "j02":
                    if (!Factory.CurrentUser.j04IsModule_j02)
                    {
                        return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
                    }
                    break;
                case "p56":
                    if (!Factory.CurrentUser.j04IsModule_p56)
                    {
                        return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
                    }
                    break;
                case "p40":
                    if (!Factory.CurrentUser.j04IsModule_p41)
                    {
                        return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
                    }
                    break;

            }            
            

            v.MenuCode = Factory.CBL.GetObjectAlias(BO.Code.Entity.GetPrefixDb(v.prefix), v.pid);
            
            if (v.MenuCode == null)
            {
                RefreshState_TabName(v);
                if (v.IsShowLeftPanel)
                {
                    this.AddMessage("Vyberte záznam z tabulky v levém panelu.", "info");
                }
                else
                {
                    if (v.pid > 0)
                    {
                        Factory.CBL.SetUserParam($"recpage-{v.prefix}-{v.rez}-pid", null);  //pro jistotu vyčistit lastpid z cache 

                    }                    
                    this.Notify_RecNotFound();
                }
                
                
                v.pid = 0;
            }
            else
            {

                v.SetGridUrl();
                if (v.prefix == "o23")
                {
                    var recO23 = Factory.o23DocBL.Load(v.pid);
                    if (recO23 !=null && recO23.o17ID > 0)
                    {
                        v.Go2GridUrl += "&rez=" + recO23.o17ID.ToString();
                    }
                    
                }

                Factory.CBL.SaveLastCallingRecPid(v.prefix, v.pid, "recpage", true, true,rez);  //uložit info o naposledy navštíveném záznamu, kvůli le1-le4 pracujeme s původním prefixem
                
                RefreshState_RecPage(v);

            }
            if (BL.Code.UserUI.IsShowLeftPanel(v.prefix,Factory.CurrentUser.j02UIBitStream))
            {
                //zobrazovat v levém panelu úzký grid
                v.gridinput = new myGridInput() { entity = v.entity, ondblclick = "grid_dblclick",master_entity="recpage",myqueryinline=myqueryinline };
                //v.gridinput.j72id = Factory.CBL.LoadUserParamInt($"recpage-j72id-{BO.Code.Entity.GetPrefixDb(v.prefix)}-{v.rez}");
                v.gridinput.j72id = Factory.CBL.LoadUserParamInt($"recpage-j72id-{v.prefix}-{v.rez}");
                
                v.gridinput.j72id_query = Factory.CBL.LoadUserParamInt($"masterview-query-j72id-{prefix}-{rez}");
                v.TheGridQueryButton = new UI.Models.TheGridQueryViewModel() { j72id = v.gridinput.j72id_query, paramkey = $"masterview-query-j72id-{prefix}-{rez}" };
                if (v.TheGridQueryButton.j72id > 0)
                {
                    v.TheGridQueryButton.j72name = Factory.j72TheGridTemplateBL.LoadName(v.TheGridQueryButton.j72id);
                }

                v.gridinput.go2pid = v.pid;
                v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load(v.prefix,null,0,myqueryinline);
                v.gridinput.query.MyRecordsDisponible = !Factory.CurrentUser.IsAdmin;   //postarat se, aby uživatel viděl pouze záznamy, které má mít přístupné

                if (v.prefix=="j02" || v.prefix == "p28"  || v.prefix=="p41" || v.prefix == "p56" || v.prefix=="le5" || v.prefix == "le4" || v.prefix == "le3" || v.prefix == "le2" || v.prefix == "le1")
                {
                    v.p31statequery = new p31StateQueryViewModel() { UserParamKey = $"grid-{v.prefix}-p31statequery" };
                    v.p31statequery.Value = Factory.CBL.LoadUserParamInt(v.p31statequery.UserParamKey);
                    v.gridinput.query.p31statequery = v.p31statequery.Value;
                }
                
                


                v.recordbinquery = new RecordBinQueryViewModel() { UserParamKey = $"grid-{v.prefix}-recordbinquery" };
                v.recordbinquery.Value = Factory.CBL.LoadUserParamInt(v.recordbinquery.UserParamKey, 1);
                switch (v.recordbinquery.Value)
                {
                    case 1:
                        v.gridinput.query.IsRecordValid = true; break;   //pouze otevřené záznamy
                    case 2:
                        v.gridinput.query.IsRecordValid = false; break;  //pouze záznamy v archivu
                    default:
                        v.gridinput.query.IsRecordValid = null; break;
                }

                
            }
            else
            {
                //grid pouze nahoře pro aktuální záznam
                v.gridinput = new myGridInput() { entity = v.entity, myqueryinline = "pids|list_int|" + v.pid.ToString(), ondblclick = "grid_dblclick", isrecpagegrid = true };
                v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load(v.prefix, null, 0, "pids|list_int|" + v.pid.ToString());
                v.gridinput.j72id = Factory.CBL.LoadUserParamInt($"masterview-j72id-{BO.Code.Entity.GetPrefixDb(v.prefix)}");
            }

            if (v.IsShowLeftPanel)
            {
                v.periodinput = new Views.Shared.Components.myPeriod.myPeriodViewModel() { prefix = v.prefix };
                v.periodinput.UserParamKey = $"grid-period-{v.prefix}";
                v.periodinput.LoadUserSetting(_pp, Factory);    //načtení d1/d2/period_field pro query
                v.gridinput.query.global_d1 = v.periodinput.d1;
                v.gridinput.query.global_d2 = v.periodinput.d2;
                v.gridinput.query.period_field = v.periodinput.PeriodField;
            }


            if (v.NavTabs == null)
            {
                v.NavTabs = new List<NavTab>();
            }

            return View(v);

        }

        private IActionResult Handle_RecordNotExists(string prefix,string rez)
        {
            //pro jistotu vyčistit lastpid z cache
            Factory.CBL.SetUserParam($"recpage-{prefix}-{rez}-pid", null);

            return this.StopPage(false, "Volaný záznam neexistuje.", new UI.Menu.TheMenuSupport(Factory).GetMainmenuEntityUrl(prefix), "Klikněte na odkaz");

        }

        
        private void RefreshState_RecPage(RecPageViewModel v)
        {

            var cTabs = new UI.Code.NavTabsSupport(Factory);      //nutno později odremovat
            v.NavTabs = cTabs.getMasterTabs(v.prefix, v.pid,true);    //nutno odremovat

            RefreshState_TabName(v);

            
            if (string.IsNullOrEmpty(v.DefTab))
            {
                v.DefTab = Factory.CBL.LoadUserParam($"masterview-tab-{v.prefix}"); //uživatelem naposledy vybraná záložka    
            }

            if (v.NavTabs.Count() > 0)
            {
                var deftab = v.NavTabs[0];

                foreach (var tab in v.NavTabs)
                {

                    tab.Url = $"{tab.Url}&master_entity={v.entity}&master_pid={v.pid}";
                    if (v.DefTab != null && tab.Entity == v.DefTab)
                    {
                        deftab = tab;
                    }
                }
                deftab.CssClass += " active";
                if (!deftab.Url.Contains("@pid"))
                {
                    v.DefaultNavTabUrl = deftab.Url;
                    v.DefaultNavName = deftab.Name;
                }
            }
            
            

        }

        private void RefreshState_TabName(RecPageViewModel v)
        {
            if (v.IsShowLeftPanel)
            {
                switch (v.prefix)
                {
                    case "le5":
                        v.TabName = Factory.getP07Level(5, false).ToUpper();
                        break;
                    case "le4":
                        v.TabName = Factory.getP07Level(4, false).ToUpper();
                        break;
                    case "le3":
                        v.TabName = Factory.getP07Level(3, false).ToUpper();
                        break;
                    case "le2":
                        v.TabName = Factory.getP07Level(2, false).ToUpper();
                        break;
                    default:
                        v.TabName = Factory.EProvider.ByPrefix(v.prefix).AliasPlural.ToUpper();
                        break;
                }                 
                return;
            }




            switch (v.prefix)
            {
                case "j02":
                    v.TabName = Factory.tra("Stránka uživatele");
                    break;
                case "o23":
                    v.TabName = Factory.tra("Stránka dokumentu");
                    break;
                case "p28":
                    v.TabName = Factory.tra("Stránka kontaktu");
                    break;
                case "p41":
                    v.TabName = Factory.tra("Stránka projektu");
                    break;
                case "p56":
                    v.TabName = Factory.tra("Stránka úkolu");
                    break;
                case "o22":
                    v.TabName = Factory.tra("Stránka záznamu");
                    break;
                case "p58":
                    v.TabName = Factory.tra("Stránka opakovaného úkolu");
                    break;
                case "p40":
                    v.TabName = Factory.tra("Stránka předpisu opakovaného úkonu");
                    break;
                case "le1":
                case "le2":
                case "le3":
                case "le4":
                case "le5":
                    
                    v.TabName =$"{Factory.tra("Stránka")} {Factory.getP07Level_Inflection(Convert.ToInt32(v.prefix.Substring(2, 1)))}";
                    break;
                case "p90":
                    v.TabName = Factory.tra("Stránka zálohy");
                    break;
                case "p91":
                    v.TabName = Factory.tra("Stránka vyúčtování");
                    break;
                case "p84":
                    v.TabName = Factory.tra("Stránka upomínky");
                    break;
                case "o43":
                    v.TabName = Factory.tra("Inbox záznam");
                    break;
                case "o51":
                    v.TabName = Factory.tra("Štítek");
                    break;
                case "p51":
                    v.TabName = Factory.tra("Ceník sazeb");
                    break;
                case "b05":
                    v.TabName = Factory.tra("Poznámka"); break;
                default:
                    v.TabName = Factory.tra("Stránka");
                    break;
            }
        }

        public IActionResult GridMultiSelect(string prefix)
        {
            var c = Factory.EProvider.ByPrefix(prefix);
            var v = new GridMultiSelect() { entity = c.TableName,prefix=prefix,entityTitle=c.AliasPlural };

            v.gridinput = GetGridInput(v.entity);
            return View(v);
            
        }

        private myGridInput GetGridInput(string entity)
        {
            var gi = new myGridInput() { entity = entity, ondblclick=null,oncmclick=null };            
            gi.query = new BO.InitMyQuery(Factory.CurrentUser).Load(entity.Substring(0, 3));
            gi.j72id = Factory.CBL.LoadUserParamInt("GridMultiSelect-j72id-"+entity.Substring(0,3));
            gi.ondblclick = "handle_dblclick";
            return gi;
        }

        public IActionResult RecordCode(string prefix,int pid)
        {
            var v = new RecordCode() { pid = pid, prefix = prefix };
            if (v.pid==0 || v.prefix == null)
            {
                return this.StopPage(true, "pid or prefix missing");
            }
            
            v.CodeValue = Factory.CBL.GetCurrentRecordCode(v.prefix, v.pid);
            v.dtLast10 = Factory.CBL.GetList_Last10RecordCode(v.prefix);
            return View(v);
        }
        [HttpPost]
        public IActionResult RecordCode(RecordCode v)
        {
            v.dtLast10 = Factory.CBL.GetList_Last10RecordCode(v.prefix);
            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                
                if (Factory.CBL.SaveRecordCode(v.CodeValue,v.prefix,v.pid)>0)
                {
                    v.SetJavascript_CallOnLoad(v.pid,"recordcode|"+v.CodeValue);
                }

            }

            return View(v);

        }

        public IActionResult RecordValidity(string strD1,string strD2)
        {
            
            var v = new RecordValidity();
            if (strD1 == null)
            {
                v.d1 = DateTime.Now;
            }
            else
            {
                v.d1 = BO.Code.Bas.String2Date(strD1);
            }
            if (strD2 == null)
            {
                v.d2 = new DateTime(3000,1,1);
            }
            else
            {
                v.d2 = BO.Code.Bas.String2Date(strD2);
                if (v.d2.Year < 3000)
                {
                    v.d2= v.d2.AddDays(1).AddMinutes(-1);
                }
            }
            return View(v);
        }
        [HttpPost]
        public IActionResult RecordValidity(RecordValidity v,string oper)
        {
           if (oper == "now")
            {
                v.d2 = DateTime.Now;
                v.IsAutoClose = true;
               
            }
            return View(v);

        }


    }
}