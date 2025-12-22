using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using UI.Models;
using UI.Models.a55;
using UI.Models.Record;

namespace UI.Controllers
{
    public class AdminOneWebpageController : BaseController
    {
        public IActionResult CssFile(string filename)
        {
            var v = new CssFileViewModel() { FileName = filename };

            if (!string.IsNullOrEmpty(v.FileName))
            {
                string strPath = Factory.App.WwwRootFolder + "\\_users\\" +Factory.Lic.x01Guid+"\\CSS\\"+ v.FileName;
                if (System.IO.File.Exists(strPath))
                {
                    v.Content= System.IO.File.ReadAllText(strPath);
                }
            }


            RefreshState_CssFile(v);



            return View(v);

        }

        private void RefreshState_CssFile(CssFileViewModel v)
        {
            v.lisFileNames = BO.Code.File.GetFileNamesInDir(Factory.App.WwwRootFolder + "\\users\\"+Factory.Lic.x01Guid +"\\CSS\\", "*.css", false);

        }
        [HttpPost]
        public IActionResult CssFile(CssFileViewModel v, string oper)
        {
            RefreshState_CssFile(v);

            if (oper == "postback")
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(v.FileName))
                {
                    this.AddMessage("Chybí název souboru.");
                    return View(v);
                }
                if (string.IsNullOrEmpty(v.Content))
                {
                    this.AddMessage("Chybí obsah souboru.");
                    return View(v);
                }

                string strPath = Factory.App.AppRootFolder + "\\wwwroot\\css\\user\\" + v.FileName;
                BO.Code.File.WriteText2File(strPath, v.Content);

                v.SetJavascript_CallOnLoad(1);
                return View(v);
            }

            this.Notify_RecNotSaved();
            return View(v);
        }
        public IActionResult Index(int a55id,int a59id)
        {
            var v = new AdminOneWebpageViewModel() { SelectedA55ID = a55id,SelectedA59ID=a59id};
            v.lisA55 = Factory.a55RecPageBL.GetList(new BO.myQuery("a55") { IsRecordValid=null});
            if(v.SelectedA55ID==0 && v.lisA55.Count() > 0)
            {
                v.SelectedA55ID = v.lisA55.First().pid;
            }
            if (v.SelectedA55ID > 0)
            {
                v.RecA55 = Factory.a55RecPageBL.Load(v.SelectedA55ID);                
            }                        

            v.lisA59 = Factory.a59RecPageLayerBL.GetList(new BO.myQuery("a59")).Where(p => p.a55ID == v.SelectedA55ID);
            if (v.SelectedA59ID == 0 && v.lisA59.Count() > 0)
            {
                v.SelectedA59ID = v.lisA59.First().pid;
            }

            if (v.SelectedA59ID > 0)
            {
                v.RecA59 = Factory.a59RecPageLayerBL.Load(v.SelectedA59ID);
                if (v.RecA59 == null)
                {
                    return RedirectToAction("Index");
                }
                if (v.SelectedA55ID != v.RecA59.a55ID || v.RecA55==null)
                {
                    v.SelectedA55ID = v.RecA59.a55ID;
                    v.RecA55 = Factory.a55RecPageBL.Load(v.SelectedA55ID);
                }                
            }

            v.DockStructure = new WebpageLayerEnvironment(null);
            v.lisUserWidgets = new List<BO.a58RecPageBox>();

            if (v.RecA59 != null)
            {
                PrepareWidgets(v);
            }

            if (v.RecA55 != null)
            {
                v.Simulation_RecPid = Factory.CBL.LoadUserParamInt($"DynamicRecPage-simulation-{v.RecA55.a55Entity}-RecPid");
                if (v.Simulation_RecPid > 0)
                {
                    v.Simulation_RecName = Factory.CBL.LoadUserParam($"DynamicRecPage-simulation-{v.RecA55.a55Entity}-RecName");
                }
            }
            

            return View(v);
        }

        private void PrepareWidgets(AdminOneWebpageViewModel v)
        {
            v.lisAllWidgets = Factory.a58RecPageBoxBL.GetList(new BO.myQuery("a58")).Where(p => p.a59ID == v.RecA59.pid);

            v.lisUserWidgets = new List<BO.a58RecPageBox>();
            v.ColumnsPerPage = (v.RecA59.a59ColumnsPerPage<=0) ? 1: v.RecA59.a59ColumnsPerPage;

            v.DockStructure = new WebpageLayerEnvironment(v.RecA59.a59DockState);


            if (v.RecA59==null || string.IsNullOrEmpty(v.RecA59.a59Boxes))
            {
                return; //uživatel nemá na ploše žádný widget, dál není třeba pokračovat
            }

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
        }


        public BO.Result SaveWidgetState(string s,int a59id)
        {
            var rec = Factory.a59RecPageLayerBL.Load(a59id);
            rec.a59DockState = s;            
            Factory.a59RecPageLayerBL.Save(rec,null);
            return new BO.Result(false);
        }

        public BO.Result RemoveWidget(int a58id)
        {
            var recA58 = Factory.a58RecPageBoxBL.Load(a58id);            
            var recA59 = Factory.a59RecPageLayerBL.Load(recA58.a59ID);
            var boxes = BO.Code.Bas.ConvertString2List(recA59.a59Boxes);
            if (boxes.Where(p => p == recA58.a58Guid.ToString()).Count() > 0)
            {
                boxes.Remove(recA58.a58Guid);
                recA59.a59Boxes = string.Join(",", boxes);
                Factory.a59RecPageLayerBL.Save(recA59,null);
                return new BO.Result(false);
            }

            return new BO.Result(true, "widget not found");
        }

        public BO.Result InsertWidget(int a58id)
        {
            var recA58 = Factory.a58RecPageBoxBL.Load(a58id);
            var recA59 = Factory.a59RecPageLayerBL.Load(recA58.a59ID);
            var boxes = BO.Code.Bas.ConvertString2List(recA59.a59Boxes);
            if (boxes.Where(p => p == recA58.a58Guid).Count() == 0)
            {
                boxes.Add(recA58.a58Guid);
                recA59.a59Boxes = string.Join(",", boxes);
                Factory.a59RecPageLayerBL.Save(recA59,null);
                return new BO.Result(false);
            }
            return new BO.Result(true, "widget not found");
        }


        //načtení html obsahu jednoho boxu
        public string GetWidgetHtmlContent(int a58id)
        {
            var recA58 = Factory.a58RecPageBoxBL.Load(a58id);
            //var recA59 = Factory.a59RecPageLayerBL.Load(recA58.a59ID);
            var ret = new List<string>();

            if (recA58.a58HtmlText != null)
            {
                string s = BO.Code.Bas.Html2Text(recA58.a58HtmlText);
                if (s.Length > 100)
                {
                    ret.Add(s.Substring(0, 100) + "...");
                }
                else
                {
                    ret.Add(s);
                }
            }

            if (recA58.IsButton())
            {
                string s = recA58.a58ButtonText;
              
                if (recA58.a58ContentFlag == BO.a58ContentFlagEnum.ButtonOneWorkflowStep && recA58.b06ID_Button > 0)
                {
                    //var recB06 = Factory.b06WorkflowStepBL.Load(recA58.b06ID_Button);
                    //if (recB06 != null) s += " (" + BO.BAS.OM2(recB06.b06Name, 25) + ")";
                }
                ret.Add($"<button type='button'>{s}</button>");
            }
            switch (recA58.a58ContentFlag)
            {                
                case BO.a58ContentFlagEnum.RecHeaderBox:
                    ret.Add("Hlavička záznamu");
                    break;
                case BO.a58ContentFlagEnum.o27List:
                    ret.Add("Seznam příloh/souborů");
                    break;
                case BO.a58ContentFlagEnum.RecNotepad:
                    ret.Add("Notepad záznamu");
                    break;
                
                
            }

            

            return string.Join("<hr>",ret);

            
            

        }
    }
}
