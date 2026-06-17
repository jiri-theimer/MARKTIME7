using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UI.Code;
using UI.Models;
using UI.Models.p31view;
using UI.Models.Tab1;
using UI.Models.Widgets;

namespace UI.Controllers
{
    public class WidgetsController : BaseController
    {
        private readonly BL.Singleton.ThePeriodProvider _pp;
        private readonly BL.TheColumnsProvider _colsProvider;
        public WidgetsController(BL.Singleton.ThePeriodProvider pp, BL.TheColumnsProvider cp)
        {
            _pp = pp;
            _colsProvider = cp;
        }

        public IActionResult MiniAbsence(string d0)
        {
            if (string.IsNullOrEmpty(d0))
            {
                d0 = DateTime.Today.ToString("dd.MM.yyyy");
            }
            var v = new MiniAbsenceViewModel() {d0=BO.Code.Bas.String2Date(d0),j02ID=Factory.CurrentUser.pid };

            var lisP11 = Factory.p11AttendanceBL.GetList(v.d0, v.d0, v.j02ID, false);
            if (lisP11.Any(p => p.j02ID == v.j02ID && p.p11Date == v.d0))
            {
                var recP11 = lisP11.Where(p => p.j02ID == v.j02ID && p.p11Date == v.d0).First();
                v.Prichod = recP11.Prichod;
                v.Odchod = recP11.Odchod;
            }

            v.lisP32FUT = Factory.p32ActivityBL.GetList(new BO.myQueryP32() { isabsence = true });
            v.lisP11 = Factory.p11AttendanceBL.GetList(new DateTime(v.d0.Year,v.d0.Month,1), new DateTime(v.d0.Year, v.d0.Month, 1).AddMonths(1).AddDays(-1), v.j02ID, false);
            v.lisP31 = Factory.p31WorksheetBL.GetList(new BO.myQueryP31() { j02id = v.j02ID,global_d1=v.d0,global_d2=v.d0 });

            return View(v);
        }
        [HttpPost]
        public IActionResult MiniAbsence(MiniAbsenceViewModel v)
        {
            
            if (v.IsPostback)
            {
                
                return View(v);
            }
            if (ModelState.IsValid)
            {
                
                return View(v);

            }


            
            return View(v);
        }

        public IActionResult MiniLeftPanel()
        {
            var v = new BaseViewModel();
            return View(v);
            
        }

        public IActionResult MiniCalendar()
        {
            var v = new BaseViewModel();
            return View(v);
        }
        public IActionResult Index(string d,int j02id, int x54id, int p28id)
        {
            

            if (!Factory.CurrentUser.j04IsModule_Widgets)
            {
                if (Factory.CurrentUser.j04IsModule_p31)
                {
                    return Redirect("/p31calendar/Index");
                }
                else
                {
                    return Redirect("/Portal/Index");
                }
                

            }
            
            if (HttpContext.Request.Path.Value.Length <= 5)
            {
                //automatické spuštění stránky po přihlášení nebo bez uvedení URL adresy
                //úvodní spuštění: otestovat nastavení domovské stránky

                
                if (basUI.DetectIfMobileFromUserAgent(Request))
                {
                    string s = Factory.CBL.LoadUserParam("j02-homepageurl-mobile");
                    if (!string.IsNullOrEmpty(s))
                    {
                        return Redirect(s);  //pryč na jinou stránku
                    }
                    
                    return Redirect("/Home/Mobile");  //stránky pro mobilní zařízení
                }
                
               

                if (!string.IsNullOrEmpty(Factory.CurrentUser.j02HomePageUrl))
                {

                    return Redirect(Factory.CurrentUser.j02HomePageUrl);  //pryč na jinou stránku
                }
            }

            if (!Factory.CurrentUser.j04IsModule_Widgets)
            {
                return Redirect("/Home/Index");
            }


            
            
            return Widgets(d,j02id,x54id,p28id);
        }

        public IActionResult Widgets(string d,int j02id,int x54id,int p28id)
        {
            
            var v = new WidgetsViewModel() { IsSubform = false, d0 = DateTime.Today,j02id_me=Factory.CurrentUser.pid,Skin= "index" };
            if (!string.IsNullOrEmpty(d))
            {
                v.d0 = BO.Code.Bas.String2Date(d);
            }
            if (j02id > 0)
            {
                v.j02id_me = j02id;
            }
            int intJ04ID = Factory.CurrentUser.j04ID;
            if (v.j02id_me != Factory.CurrentUser.pid)
            {
                intJ04ID = Factory.j02UserBL.Load(v.j02id_me).j04ID;
            }

            v.lisX54 = Factory.j04UserRoleBL.GetList_x54(intJ04ID).Where(p => !p.isclosed);
            if (v.lisX54.Count() == 0)
            {
                return this.StopPage(false, "Vaše aplikační role nemá nastavenou widget skupinu.", "/Admin/Page?area=widgets&prefix=x54");
            }
            v.SelectedX54ID = x54id;
            if (v.SelectedX54ID == 0)
            {
                v.SelectedX54ID = Factory.CBL.LoadUserParamInt("Widgets-x54ID");
            }            
            if (v.SelectedX54ID==0 || v.lisX54.Where(p => p.pid == v.SelectedX54ID).Count() == 0)
            {
                v.SelectedX54ID = v.lisX54.First().pid;
            }
            v.RecX54 = Factory.x54WidgetGroupBL.Load(v.SelectedX54ID);
                        
            if (v.RecX54.x54IsParamP28ID)
            {
                v.SelectedP28ID = p28id;
                if (v.SelectedP28ID == 0)
                {
                    v.SelectedP28ID = Factory.CBL.LoadUserParamInt("Widgets-p28ID");
                }
                v.IsAllScopeAllClients = Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_p28_Owner);
                if (!v.IsAllScopeAllClients)
                {
                    v.lisP28 = Factory.p28ContactBL.GetList(new BO.myQueryP28() { isportal = true });
                    
                    if (v.SelectedP28ID == 0 && v.lisP28.Count() > 0)
                    {
                        v.SelectedP28ID = v.lisP28.First().pid;                       
                    }
                }
                else
                {
                    if (v.SelectedP28ID > 0)
                    {
                        v.SelectedP28Name = Factory.p28ContactBL.Load(v.SelectedP28ID).p28Name;
                    }
                }
            }
            if (v.RecX54.x54IsAllowSkins)
            {
                v.Skin = Factory.CBL.LoadUserParam($"Widgets-Skin-{v.SelectedX54ID}", "index");
            }
            
            if (string.IsNullOrEmpty(v.Skin))
            {
                v.Skin = "index";
            }

            v.lisJ02 = Factory.j02UserBL.GetList(new BO.myQueryJ02() { explicit_orderby="a.j02Name",MyRecordsDisponible=true});

            PrepareWidgets(v);

            return View(v);
        }

        public BO.Result ChangeX54ID(int x54id)
        {
            Factory.CBL.SetUserParam("Widgets-x54ID", x54id.ToString());
            return new BO.Result(false);
        }
        public BO.Result ChangeP28ID(int p28id)
        {
            Factory.CBL.SetUserParam("Widgets-p28ID", p28id.ToString());
            return new BO.Result(false);
        }

        public BO.Result ChangeSkin(string skin,int x54id)
        {
            Factory.CBL.SetUserParam($"Widgets-Skin-{x54id}", skin);
            return new BO.Result(false);
        }




        private string parse_badge(int intCount)
        {
            if (intCount > 0)
            {
                return intCount.ToString();
            }
            return null;
        }

        public BO.Result SaveWidgetState(string s, string skin,int x54id)
        {
            var rec = Factory.x55WidgetBL.LoadState(Factory.CurrentUser.pid, skin,x54id);
            rec.x56DockState = s;
            rec.x56Skin = skin;
            Factory.x55WidgetBL.SaveState(rec);
            return new BO.Result(false);
        }
        public BO.Result RemoveWidget(int x55id, string skin,int x54id)
        {
            var recX55 = Factory.x55WidgetBL.Load(x55id);
            var recX56 = Factory.x55WidgetBL.LoadState(Factory.CurrentUser.pid, skin,x54id);
            var boxes = BO.Code.Bas.ConvertString2List(recX56.x56Boxes);
            if (boxes.Where(p => p == recX55.x55Code).Count() > 0)
            {
                boxes.Remove(recX55.x55Code);
                recX56.x56Boxes = string.Join(",", boxes);
                Factory.x55WidgetBL.SaveState(recX56);
                return new BO.Result(false);
            }

            return new BO.Result(true, "widget not found");
        }
        public BO.Result InsertWidget(int x55id, string skin, int x54id)
        {
            var recX55 = Factory.x55WidgetBL.Load(x55id);
            var recX56 = Factory.x55WidgetBL.LoadState(Factory.CurrentUser.pid, skin,x54id);
            var boxes = BO.Code.Bas.ConvertString2List(recX56.x56Boxes);
            if (boxes.Where(p => p == recX55.x55Code).Count() == 0)
            {
                boxes.Add(recX55.x55Code);
                recX56.x56Boxes = string.Join(",", boxes);
                Factory.x55WidgetBL.SaveState(recX56);
                return new BO.Result(false);
            }
            return new BO.Result(true, "widget not found");
        }
        
        public BO.Result SavePocetSloupcu(int x, string skin,int x54id)
        {
            
            Factory.CBL.SetUserParam($"Widgets-ColumnsPerPage-{skin}-{x54id}", x.ToString());
            return new BO.Result(false);
        }
        public BO.Result SavePageAutoRefresh(int x, string skin,int x54id)
        {
            Factory.CBL.SetUserParam($"Widgets-PageAutoRefresh-{skin}-{x54id}", x.ToString());
            return new BO.Result(false);
        }

        public BO.Result Clear2FactoryState(string skin,int x54id)    //vyčistí plochu do továrního nastavení
        {
            Factory.x55WidgetBL.Clear2FactoryState(Factory.x55WidgetBL.LoadState(Factory.CurrentUser.pid, skin,x54id));
            return new BO.Result(false);
        }

        public BO.Result SetAsDefaultGlobalBoxes(string skin,int x54id)    //vyčistí plochu do továrního nastavení
        {
            Factory.x55WidgetBL.SetAsDefaultGlobalBoxes(Factory.x55WidgetBL.LoadState(Factory.CurrentUser.pid, skin,x54id).x56Boxes);
            return new BO.Result(false);
        }

        public BO.Result UpdateBoxes2AllUsers(string skin,int x54id)    //vyčistí plochu do továrního nastavení
        {
            Factory.x55WidgetBL.UpdateBoxes2AllUsers(Factory.x55WidgetBL.LoadState(Factory.CurrentUser.pid, skin,x54id).x56Boxes,skin,x54id);
            return new BO.Result(false);
        }


        //rozvržení prostoru na ploše
        private void PrepareWidgets(WidgetsViewModel v)
        {
            var mq = new BO.myQuery("x55") {x54id=v.SelectedX54ID, IsRecordValid = true, MyRecordsDisponible = true, CurrentUser = Factory.CurrentUser };
            var hodnoty = new List<string>() { null, v.Skin };

            v.lisAllWidgets = Factory.x55WidgetBL.GetList(mq).OrderBy(p => p.x55Category).ThenBy(p => p.x55Ordinal);
            
            string ss = null;
            foreach(var c in v.lisAllWidgets)
            {                
                if (c.x55Category != ss)
                {
                    c.Rezerva = c.x55Category;
                }
                ss = c.x55Category;
            }

            v.lisUserWidgets = new List<BO.x55Widget>();
            if (Factory.CurrentUser.IsMobileDisplay())
            {
                v.ColumnsPerPage = 1;
                v.PageAutoRefreshPerSeconds = 0;
                Factory.CurrentUser.j02GridCssBitStream = 564;   //napevno grid nastavení
            }
            else
            {
                v.ColumnsPerPage = Factory.CBL.LoadUserParamInt($"Widgets-ColumnsPerPage-{v.Skin}-{v.SelectedX54ID}", 2);
                if (v.RecX54.x54IsAllowAutoRefresh)
                {
                    v.PageAutoRefreshPerSeconds = Factory.CBL.LoadUserParamInt($"Widgets-PageAutoRefresh-{v.Skin}-{v.SelectedX54ID}", 0);
                }
                
            }
            
            
            v.recX56 = Factory.x55WidgetBL.LoadState(Factory.CurrentUser.pid, v.Skin,v.SelectedX54ID);
            v.DockStructure = new WidgetsEnvironment(v.recX56.x56DockState);

            if (Factory.CurrentUser.j02LangIndex == 2)
            {
                v.DataTables_Localisation = "/lib/datatables/localisation/uk_UA.json";
            }
            else
            {
                v.DataTables_Localisation = "/lib/datatables/localisation/cs_CZ.json";
            }

            if (v.recX56 == null || v.recX56.x56Boxes == null)
            {
                return; //uživatel nemá na ploše žádný widget, dál není třeba pokračovat
            }

            var boxes = BO.Code.Bas.ConvertString2List(v.recX56.x56Boxes);
            foreach (string s in boxes)
            {
                if (v.lisAllWidgets.Where(p => p.x55Code == s).Count() > 0)
                {
                    v.lisUserWidgets.Add(v.lisAllWidgets.Where(p => p.x55Code == s).First());
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
            if (v.lisUserWidgets.Exists(p => p.x55TableSql != null && p.x55TableColHeaders != null))
            {
                v.IsDataTables = true;
            }
            if (v.IsDataTables)
            {
                if (v.lisUserWidgets.Exists(p => p.x55DataTablesButtons > BO.x55DataTablesBtns.None))
                {
                    v.IsExportButtons = true;   //zobrazovat tlačítka XLS/CSV/COPY
                }
                if (v.lisUserWidgets.Exists(p => p.x55DataTablesButtons == BO.x55DataTablesBtns.ExportPrintPdf))
                {
                    v.IsPdfButtons = true;      //zobrazovat i tlačítko PDF
                }
                if (v.IsPdfButtons || v.lisUserWidgets.Exists(p => p.x55DataTablesButtons == BO.x55DataTablesBtns.ExportPrint))
                {
                    v.IsPrintButton = true;      //zobrazovat i tlačítko PDF
                }
            }
            if (v.lisUserWidgets.Exists(p => p.x55ChartSql != null && p.x55ChartHeaders != null))
            {
                v.IsCharts = true;
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
        }


        //načtení html obsahu jednoho boxu
        public BO.x55Widget GetWidgetHtmlContent(int x55id, int columnsperpage, string d,int j02id,int p28id)
        {
            var rec = Factory.x55WidgetBL.Load(x55id);
            string strHtml = rec.x55Content;
            if (strHtml != null)
            {
                strHtml = strHtml.Replace("@j02id", j02id.ToString());
                strHtml = strHtml.Replace("@x01id", rec.x01ID.ToString());
                strHtml = strHtml.Replace("@p28id", p28id.ToString());
            }
            
            
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(strHtml);

            if (rec.x55ReportCodes != null)
            {
                rec.x55ReportCodes = rec.x55ReportCodes.Replace("|", ";").Replace(",", ";");
                var arr = BO.Code.Bas.ConvertString2List(rec.x55ReportCodes, ";");
                foreach(var strX31Code in arr)
                {
                    var recX31 = Factory.x31ReportBL.LoadByCode(strX31Code,0);
                    if (recX31 != null)
                    {
                        sb.AppendLine($"<div style='padding:4px'><a href=\"javascript:_window_open('/ReportsClient/reportNoContext?noframework=true&x31id={recX31.pid}')\"><span class='material-icons-outlined-btn'>print</span>{recX31.x31Name}</a></div>");
                    }
                }
            }

            DateTime datD0 = DateTime.Today;
            if (!string.IsNullOrEmpty(d))
            {
                datD0 = BO.Code.Bas.String2Date(d);
            }
            if (j02id == 0) j02id = Factory.CurrentUser.pid;


            if (rec.x55ChartSql != null && rec.x55ChartHeaders != null)
            {
                string s = rec.x55ChartSql;
                s = DL.BAS.ParseMergeSQL(s, j02id.ToString());
                s = s.Replace("@j02id", j02id.ToString());
                s = s.Replace("@x01id", rec.x01ID.ToString());
                s = s.Replace("@p28id", p28id.ToString());
                //s = s.Replace("$$", "''''");

                if (s.Contains("@d"))
                {
                    s = s.Replace("@d", BO.Code.Bas.GD(datD0));
                }
                var dt = Factory.gridBL.GetListFromPureSql(s);
                var cGen = new BL.Code.Datatable2Chart();
                sb.AppendLine(cGen.CreateGoogleChartHtml(dt, rec));
                
                
            }
            if (rec.x55TableSql != null && rec.x55TableColHeaders != null)
            {
                string s = rec.x55TableSql;
                s = DL.BAS.ParseMergeSQL(s, Factory.CurrentUser.pid.ToString());
                s = s.Replace("@j02id", j02id.ToString());
                s = s.Replace("@x01id", rec.x01ID.ToString());
                s = s.Replace("@p28id", p28id.ToString());
                

                if (s.Contains("@d"))
                {
                    s = s.Replace("@d", BO.Code.Bas.GD(datD0));
                }
                var dt = Factory.gridBL.GetListFromPureSql(s);
                if (dt.Rows.Count >= rec.x55DataTablesLimit && rec.x55DataTablesLimit > 0)
                {
                    rec.IsUseDatatables = true; //splněna podmínka pro zobrazení tabulky přes plugin DataTables

                }
               
                var cGen = new BL.Code.Datatable2Html(new BL.Code.HtmlTable() { IsCanExport = false, ColHeaders = rec.x55TableColHeaders, ColTypes = rec.x55TableColTypes, ClientID = rec.x55Code, IsUseDatatables = rec.IsUseDatatables,ColFlexSubtotals=rec.x55TableColTotals }, Factory);
                
                sb.AppendLine(cGen.CreateHtmlTable(dt, 1000));

                

            }

            switch (rec.x55Code.ToLower())
            {
                case "pandulak":
                    var strPandulakDir = $"{Factory.App.AppRootFolder}\\wwwroot\\images\\pandulak";
                    sb = new System.Text.StringBuilder();
                    sb.Append($"<img src='/images/pandulak/{Code.basUI.getPandulakImage(strPandulakDir)}'/>");
                    
                    if (columnsperpage <= 2)
                    {
                        sb.Append($"<img src='/images/pandulak/{Code.basUI.getPandulakImage(strPandulakDir)}'/>");
                        
                    }
                    break;
            }

            if (rec.x55Name.Contains("@d"))
            {
                rec.x55Name = rec.x55Name.Replace("@d", datD0.ToString("dd.MM.yyyy ddd"));
            }

            rec.x55Content = sb.ToString();
            return rec;



        }
    }
}