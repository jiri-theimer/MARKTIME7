
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Menu;
using UI.Views.Shared.Components.myGrid;
using UI.Code.Menu;

namespace UI.Controllers
{
    public class MenuController : BaseController
    {
        

        private List<MenuItem> _lis;
        private TheMenuSupport _menusup;

        public MenuController()
        {
            _lis = new List<MenuItem>();
            _menusup = new TheMenuSupport(null);
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult main_me()
        {
            return View();
        }
      
        
        public IActionResult main_home()
        {
            return View();
        }
        public IActionResult main_new()
        {
            return View();
        }
        public IActionResult main_search()
        {
            return View();
        }
    
        public IActionResult main_p41(int level)
        {
            
            if (level == 0) level = 5;
            
            return View(new DynamicMenuViewModel() { Prefix = "le"+level.ToString(),p07Level=level });
        }
        

        public IActionResult grid_extension(string prefix)
        {
            return View(new DynamicMenuViewModel() { Prefix = prefix });
        }
        public IActionResult grid_selected(string prefix,string master_prefix,int master_pid)
        {
            if (!string.IsNullOrEmpty(master_prefix)) master_prefix = master_prefix.Substring(0, 3);
            return View(new DynamicMenuViewModel() { Prefix = prefix,MasterPrefix=master_prefix,MasterPid=master_pid });
        }
        public IActionResult grid_menu(string prefix,string master_prefix,string rez)
        {
            return View(new DynamicMenuViewModel() { Prefix = prefix,MasterPrefix=master_prefix,Rez=rez });
        }
        public IActionResult grid_query(string prefix, string master_prefix, string rez,int j72id)
        {
            return View(new DynamicMenuViewModel() { Prefix = BO.Code.Entity.GetPrefixDb(prefix), MasterPrefix = master_prefix, Rez = rez,j72id=j72id });
        }
        public IActionResult tree_query(string prefix, string master_prefix, string rez)
        {
            return View(new DynamicMenuViewModel() {Prefix=prefix, MasterPrefix = master_prefix, Rez = rez });
        }
        public IActionResult recpage_extension(string prefix)
        {
            return View(new DynamicMenuViewModel() { Prefix = prefix });
        }

        public string ContextMenu(string entity, int pid,string source,string master_entity,string period,string device)
        {
            if (Factory.CurrentUser.IsMobileDisplay()) device = "Phone";
            if (device == "Phone")
            {
                _menusup.IsMobile = true;
            }        
            
            if (entity == "p31approve")
            {                
                string p31guid = master_entity; //v master_entity je uložený p31guid
                return _menusup.FlushResult_UL(new p31approveContextMenu(Factory, pid, p31guid).GetItems(), true, true, source);
            }
            string prefix = entity.Substring(0, 3);

            

            List<MenuItem> lis = new List<MenuItem>();
            
            if (master_entity == "workflow")
            {
                lis = new workflowContextMenu(Factory, pid, prefix, source).GetItems();
                return _menusup.FlushResult_UL(lis, true, false);
            }

            switch (prefix)
            {                
                case "j02":
                    lis = new j02ContextMenu(Factory,pid,source,period,device).GetItems();break;
                case "p28":
                case "fp3":
                    lis = new p28ContextMenu(Factory, pid, source,period,device).GetItems(); break;
                case "p56":
                case "fp1":
                    lis = new p56ContextMenu(Factory, pid, source,period, device).GetItems(); break;
                case "o22":
                    lis = new o22ContextMenu(Factory, pid, source).GetItems(); break;
                case "p41":
                case "le5":
                case "le4":
                case "le3":
                case "le2":
                case "le1":
                case "fp2":
                    lis = new p41ContextMenu(Factory, pid, source,period, device).GetItems(); break;
                case "p31":
                    lis = new p31ContextMenu(Factory, pid, source,master_entity, device).GetItems(); break;
                case "o23":
                    lis = new o23ContextMenu(Factory, pid, source, device).GetItems(); break;
                case "p40":
                    lis = new p40ContextMenu(Factory, pid, source, device).GetItems(); break;
                case "p49":
                    lis = new p49ContextMenu(Factory, pid, source, device).GetItems(); break;
                case "p90":
                    lis = new p90ContextMenu(Factory, pid, source, device).GetItems(); break;
                case "p91":
                    lis = new p91ContextMenu(Factory, pid, source, device).GetItems(); break;
                case "p84":
                    lis = new p84ContextMenu(Factory, pid, source, device).GetItems(); break;
                case "o43":
                    lis = new o43ContextMenu(Factory, pid, source, device).GetItems();break;
                case "x40":
                    lis = new x40ContextMenu(Factory, pid, source).GetItems(); break;
                case "b05":
                    lis = new b05ContextMenu(Factory, pid, source, device).GetItems(); break;
                case "p55":
                    lis = new p55ContextMenu(Factory, pid, source, device).GetItems(); break;
                case "p39":
                case "j90":
                case "j92":
                case "j95":
                case "p11":
                case "o24":
                    return "<p>No Menu</p>";    //bez kontextového menu
                default:
                    lis = new defContextMenu(Factory, pid, source,prefix).GetItems();
                    if (prefix == "p44")
                    {
                        

                        lis.Add(AMI("Nový projekt podle šablony", $"javascript: _window_open('/p41cft/Index?p44id={pid}', 3)", "work_outline"));
                    }
                    break;
                    
            }

            return _menusup.FlushResult_UL(lis, true, true, source,device);
        }
      
        //public string CurrentUserLangIndex()
        //{
        //    for (int i = 0; i <= 4; i++)
        //    {
        //        string s = "<img src='/images/small_czechrepublic.gif'/> Česky";
        //        if (i == 1) s = "<img src='/images/small_uk.gif'/> English";
        //        if (i == 2) s = "Deutch";
        //        if (i == 4) s = "<img src='/images/small_slovakia.gif'/> Slovenčina";
        //        if (Factory.CurrentUser.j02LangIndex == i) s += "&#10004;";
        //        if (i != 2 && i!=3) //jazyk 2 a 3 zatím neukazovat!
        //        {
        //            AMI_NOTRA(s, string.Format("javascript: _save_langindex_menu({0})", i));
        //        }
                
        //    }
        //    return _menusup.FlushResult_UL(_lis,true,false);            
        //}
        
       
        


       

        public string get_p31StateQuery_MenuItems(string userparamkey, int explicit_value = 0, string explicit_javascript_onchange = null)  //menu pro zobrazení filtrování stavu úkonů
        {
            _menusup = new TheMenuSupport(Factory);
            string s0 = "radio_button_unchecked"; string s1 = "radio_button_checked";

            
            if (explicit_value == 0 && !string.IsNullOrEmpty(userparamkey))
            {
                explicit_value = Factory.CBL.LoadUserParamInt(userparamkey);
            }
            if (string.IsNullOrEmpty(explicit_javascript_onchange))
            {
                explicit_javascript_onchange = "handle_p31statequery_change";
            }
            
            
            

            AMI("Nefiltrovat", $"javascript:{explicit_javascript_onchange}('0')",(explicit_value == 0 ? s1:s0));
            //var arr = new List<int> { 1, 2,16,17, 3,4,5,6,7,8,9,10,11,12,13,14,15,19 };
            var arr = new List<int> { 1,3,4,2, 16, 17,  5, 6, 7, 8, 9, 10, 11, 12, 13, 14,21, 15, 19 };


            var cv = new p31StateQueryViewModel();
            foreach(int x in arr)
            {
                cv.Value = x;
                MenuItem mi=AMI(cv.getStateAlias(), $"javascript:{explicit_javascript_onchange}('{x}')",(explicit_value == x ? s1: s0));
               
                if (x == explicit_value) mi.IsActive = true;               
                if (x == 4 || x==9)
                {
                    DIV();  //za první skupinou 3 nejpoužívanější položek vizuálně oddělit + oddělit vyúčtované
                }
            }
            
            return _menusup.FlushResult_UL(_lis, true, false);      
            
        }



        public string get_p31TabQuery_MenuItems(myGridUIContext tgi, string userparamkey,string explicit_value=null,string explicit_javascript_onchange=null)  //menu pro zobrazení filtrování formátu úkonů
        {
            _menusup = new TheMenuSupport(Factory);
            string s0 = "radio_button_unchecked"; string s1 = "radio_button_checked";
                        
            
            if (string.IsNullOrEmpty(explicit_value) && !string.IsNullOrEmpty(userparamkey))
            {
                explicit_value = Factory.CBL.LoadUserParam(userparamkey);
            }
            if (string.IsNullOrEmpty(explicit_javascript_onchange))
            {
                explicit_javascript_onchange = "handle_p31tabquery_change";
            }

            AMI("Nefiltrovat", $"javascript:{explicit_javascript_onchange}('')",(explicit_value == ""? s1: s0));
            var arr = new List<string> { "time", "kusovnik","expense","fee" };


            var cv = new p31TabQueryViewModel();
            foreach (string x in arr)
            {
                cv.Value = x;
                MenuItem mi = AMI(cv.getStateAlias(), $"javascript:{explicit_javascript_onchange}('{x}')",(explicit_value == x ? s1: s0));

                if (x == explicit_value) mi.IsActive = true;

            }

            return _menusup.FlushResult_UL(_lis, true, false);

        }




        private MenuItem AMI(string strName,string strUrl,string icon=null, string strParentID = null,string strID=null, string strTarget = null)
        {
            var c = new MenuItem() { Name = Factory.tra(strName), Url = strUrl, Target = strTarget, ID = strID, ParentID = strParentID, Icon = icon };
            _lis.Add(c);
            return c;
        }
        private void AMI_NOTRA(string strName, string strUrl,string icon=null, string strParentID = null, string strID = null, string strTarget = null)
        {           
            _lis.Add(new MenuItem() { Name = strName, Url = strUrl, Target = strTarget, ID = strID, ParentID = strParentID,Icon=icon });
        }
        private void DIV(string strName=null, string strParentID = null)
        {
            _lis.Add(new MenuItem() { IsDivider = true, Name = BO.Code.Bas.OM2(strName,30),ParentID=strParentID });
        }
        private void DIV_TRANS(string strName = null)
        {
            _lis.Add(new MenuItem() { IsDivider = true, Name = BO.Code.Bas.OM2(Factory.tra(strName), 30) });
        }
        private void HEADER(string strName)
        {
            _lis.Add(new MenuItem() { IsHeader = true, Name = BO.Code.Bas.OM2(strName, 100)+":" });
        }

        
       
    }
}