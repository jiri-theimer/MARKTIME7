
using Microsoft.AspNetCore.Mvc;
using UI.Models;

namespace UI.Controllers
{
    public class TheGridDesignerController : BaseController
    {
        private readonly BL.TheColumnsProvider _colsProvider;
        private readonly BL.Singleton.ThePeriodProvider _pp;

        public TheGridDesignerController(BL.TheColumnsProvider cp, BL.Singleton.ThePeriodProvider pp)
        {
            _colsProvider = cp;
            _pp = pp;
        }

        public IActionResult Index(int j72id, string prefix, string cols,string pathname,bool saveas)
        {

            var v = new TheGridDesignerViewModel() { ParentLayoutName = this.ParentLayoutName, CallerPathName=pathname,ForceSaveAsAtStart=saveas, HasOwnerPermissions=false };
            
            if (!string.IsNullOrEmpty(prefix))
            {
                //návrhář sloupců pro notifikační šablonu
                v.IsFieldsDesignerOnly = true;
                v.Rec = new BO.j72TheGridTemplate() { j72Entity =BO.Code.Entity.GetEntity(prefix),j72Columns=cols };
                v.HasOwnerPermissions = true;
                Index_RefreshState(v);
                return View(v);
            }
            
            v.Rec = Factory.j72TheGridTemplateBL.Load(j72id);
            if (v.Rec == null)
            {
                return RecNotFound(v);
            }
            else
            {
                
                if (v.Rec.j72SystemFlag == BO.j72SystemFlagEnum.User && (v.Rec.j02ID == Factory.CurrentUser.pid || Factory.CurrentUser.IsAdmin))
                {
                    v.HasOwnerPermissions = true;
                    
                    var lis = Factory.j04UserRoleBL.GetList(new BO.myQueryJ04() { j72id = j72id });
                    v.j04IDs = string.Join(",", lis.Select(p => p.pid));
                    v.j04Names = string.Join(",", lis.Select(p => p.j04Name));
                }
                if (v.Rec.j72SystemFlag==BO.j72SystemFlagEnum.Grid && v.Rec.j02ID == Factory.CurrentUser.pid)
                {
                    v.HasOwnerPermissions = true;
                }

                
                v.lisJ73 = Factory.j72TheGridTemplateBL.GetList_j73(v.Rec.pid, v.Rec.j72Entity.Substring(0, 3),0).ToList();
                foreach (var c in v.lisJ73)
                {
                    c.TempGuid = BO.Code.Bas.GetGuid();
                }
                Index_RefreshState(v);

                if (!v.HasOwnerPermissions && v.Rec.j72SystemFlag ==BO.j72SystemFlagEnum.User)
                {
                    this.AddMessage("Nemáte oprávnění upravovat tuto pojmenovanou tabulku.", "info");
                }

                return View(v);
            }

            

        }


        [HttpPost]
        public IActionResult Index(Models.TheGridDesignerViewModel v, string guid, string j72name)    //uložení grid sloupců
        {
            Index_RefreshState(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper == "saveas" && j72name != null)
                {
                    var recJ72 = Factory.j72TheGridTemplateBL.Load(v.Rec.pid);
                    var lisJ73 = Factory.j72TheGridTemplateBL.GetList_j73(recJ72.pid, recJ72.j72Entity.Substring(0, 3),0).ToList();
                    recJ72.j72SystemFlag = BO.j72SystemFlagEnum.User; recJ72.j72ID = 0; recJ72.pid = 0; recJ72.j72Name = j72name; recJ72.j02ID = Factory.CurrentUser.pid;
                    List<int> j04ids = BO.Code.Bas.ConvertString2ListInt(v.j04IDs);
                    List<int> j11ids = BO.Code.Bas.ConvertString2ListInt(v.j11IDs);
                    var intJ72ID = Factory.j72TheGridTemplateBL.Save(recJ72, lisJ73, j04ids, j11ids);
                    return RedirectToActionPermanent("Index", new { j72id = intJ72ID,pathname=v.CallerPathName });
                }
                if (v.PostbackOper == "rename" && j72name != null)
                {
                    var recJ72 = Factory.j72TheGridTemplateBL.Load(v.Rec.pid);
                    recJ72.j72Name = j72name;
                    var intJ72ID = Factory.j72TheGridTemplateBL.Save(recJ72, null, null, null);
                    return RedirectToActionPermanent("Index", new { j72id = intJ72ID, pathname = v.CallerPathName });
                }

                if (v.PostbackOper == "delete" && v.HasOwnerPermissions)
                {
                    if (Factory.CBL.DeleteRecord("j72", v.Rec.pid) == "1")
                    {
                        v.Rec.pid = Factory.j72TheGridTemplateBL.LoadState(v.Rec.j72Entity, Factory.CurrentUser.pid, v.Rec.j72MasterEntity,v.Rec.j72Rez).pid;
                        v.SetJavascript_CallOnLoad(v.Rec.j72ID);

                    }
                }
                if (v.PostbackOper == "changefield" && guid != null)
                {
                    if (v.lisJ73.Where(p => p.TempGuid == guid).Count() > 0)
                    {
                        var c = v.lisJ73.Where(p => p.TempGuid == guid).First();
                        c.j73Value = null; c.j73ValueAlias = null;
                        c.j73ComboValue = 0;
                        c.j73Date1 = null; c.j73Date2 = null;
                        c.j73Num1 = 0; c.j73Num2 = 0;

                        
                        c.MyQueryInline = v.lisQueryFields.Where(p => p.Field == c.j73Column).First().MyQueryInline;
                        c.j73IsOfferPersonsAdd = v.lisQueryFields.Where(p => p.Field == c.j73Column).First().IsOfferPersonsAdd;


                    }


                }

                if (v.PostbackOper == "add_j73")
                {
                    var c = new BO.j73TheGridQuery() { TempGuid = BO.Code.Bas.GetGuid(), j73Column = v.lisQueryFields.First().Field };
                    c.FieldType = v.lisQueryFields.Where(p => p.Field == c.j73Column).First().FieldType;
                    c.FieldEntity = v.lisQueryFields.Where(p => p.Field == c.j73Column).First().SourceEntity;
                    c.MyQueryInline = v.lisQueryFields.Where(p => p.Field == c.j73Column).First().MyQueryInline;
                    v.lisJ73.Add(c);


                }
                if (v.PostbackOper == "delete_j73")
                {
                    v.lisJ73.First(p => p.TempGuid == guid).IsTempDeleted = true;

                }
                if (v.PostbackOper == "clear_j73")
                {
                    v.lisJ73.Clear();

                }

                if (v.PostbackOper == "restore2factory")
                {
                    Factory.CBL.DeleteRecord("j72", v.Rec.pid);
                    v.SetJavascript_CallOnLoad(v.Rec.pid);

                }

                if (v.PostbackOper== "restore2global")
                {
                    Factory.j72TheGridTemplateBL.SetAsDefaultGlobalColumns(v.Rec.j72Entity.Substring(0,3), v.Rec.j72Columns,v.Rec.j72MasterEntity);                    
                }
                if (v.PostbackOper== "update2allusers")
                {
                    Factory.j72TheGridTemplateBL.UpdateColumns2AllUsers(v.Rec.j72Entity, v.Rec.j72Columns);
                }
                if (v.PostbackOper== "catalog")
                {
                    v.UniqueNamesCatalog = string.Join("<br>", v.AllColumns.Where(p=>p.Rezerva=="1").Select(p => $"{p.DesignerGroup}: <kbd>{p.Header}</kbd><code>{p.UniqueName}</code>"));
                }
                if (v.PostbackOper == "saveas_j73")
                {
                    return RedirectToAction("Index", "TheQueryDesigner", new { j72id = v.Rec.pid, saveas = true });
                }
                return View(v);
            }








            if (ModelState.IsValid)
            {
                
                if (v.IsFieldsDesignerOnly)
                {
                    //pouze návrhář polí notifikační šablony
                    
                    if (v.ParentLayoutName == "subform")
                    {
                        v.SetJavascript_CallOnLoad(0, v.Rec.j72Columns, "window.parent.document.getElementById('fra_subgrid').contentWindow.reload_after_griddesigner");
                    }
                    else
                    {
                        v.SetJavascript_CallOnLoad(0, v.Rec.j72Columns, "window.parent.reload_after_griddesigner");
                    }
                    return View(v);
                }
                var recJ72 = Factory.j72TheGridTemplateBL.Load(v.Rec.pid);
                var gridState = Factory.j72TheGridTemplateBL.LoadState(v.Rec.pid, Factory.CurrentUser.pid);
                recJ72.j72Columns = v.Rec.j72Columns;
                
                if (recJ72.j72Columns !=null && recJ72.j72Columns.IndexOf("undefined")>=0)
                {
                    recJ72.j72Columns = String.Join(",", recJ72.j72Columns.Split(",").Where(p => p != "undefined"));
                }
                
                recJ72.j72IsPublic = v.Rec.j72IsPublic;
                recJ72.j72IsQueryNegation = v.Rec.j72IsQueryNegation;

                gridState.j75Filter = "";   //automaticky vyčistit aktuální sloupcový filtr
                gridState.j75CurrentPagerIndex = 0;
                gridState.j75CurrentRecordPid = 0;

                if (gridState.j75SortDataField != null)
                {
                    if (recJ72.j72Columns.IndexOf(gridState.j75SortDataField) == -1)
                    { //vyčistit sort field, pokud se již nenachází ve vybraných sloupcích
                        gridState.j75SortDataField = "";
                        gridState.j75SortOrder = "";
                    }
                }
                List<int> j04ids = BO.Code.Bas.ConvertString2ListInt(v.j04IDs);
                List<int> j11ids = BO.Code.Bas.ConvertString2ListInt(v.j11IDs);
                int intJ72ID = Factory.j72TheGridTemplateBL.Save(recJ72, v.lisJ73.Where(p => p.j73ID > 0 || p.IsTempDeleted == false).ToList(), j04ids, j11ids);
                if (intJ72ID > 0)
                {
                    Factory.j72TheGridTemplateBL.SaveState(gridState, Factory.CurrentUser.pid);
                    if (v.CallerPathName != null && v.CallerPathName.Contains("MasterView"))
                    {
                        Factory.CBL.SetUserParam($"masterview-j72id-{recJ72.j72Entity.Substring(0, 3)}-{recJ72.j72Rez}", intJ72ID.ToString());
                    }
                    if (v.CallerPathName != null && v.CallerPathName.Contains("FlatView"))
                    {
                        Factory.CBL.SetUserParam($"flatview-j72id-{recJ72.j72Entity.Substring(0, 3)}-{recJ72.j72Rez}", intJ72ID.ToString());
                    }
                    if (v.CallerPathName != null && v.CallerPathName.Contains("SlaveView") && v.Rec.j72MasterEntity != null)
                    {
                        Factory.CBL.SetUserParam($"slaveview-j72id-{recJ72.j72Entity.Substring(0, 3)}-{v.Rec.j72MasterEntity}-{recJ72.j72Rez}", intJ72ID.ToString());
                    }

                    v.SetJavascript_CallOnLoad(v.Rec.pid);
                    return View(v);
                }
                else
                {
                    return View(v);
                }

            }


            return View(v);

        }

        private void inhale_tree(UI.Models.TheGridDesignerViewModel v)
        {

            v.treeNodes = new List<kendoTreeItem>();

            foreach (var rel in v.Relations)
            {
                var grp = new kendoTreeItem() { id = "group__" + rel.RelName + "__" + rel.TableName, text = rel.AliasSingular, expanded = false };


                switch (Factory.CurrentUser.j02LangIndex)
                {
                    case 1:
                        grp.text = rel.Translate1; break;
                    case 2:
                        grp.text = rel.Translate2; break;
                    default:
                        grp.text = rel.AliasSingular; break;
                }
                if (Factory.p07LevelsCount > 1)
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        if (grp.text == "L" + i.ToString()) grp.text = Factory.getP07Level(i, true);  //doplnit skutečný název úrovně projektu - p07Name
                    }
                }
                else
                {
                    if (grp.text == "L5") grp.text = Factory.getP07Level(5, true);
                }
                

                if (v.Relations.Count() == 1 && v.treeNodes.Count() == 0)
                {
                    grp.expanded = true;
                }
                grp.customvalue2 = "/images/folder.png";
                grp.customvalue3 = "tree_group";
                
                grp.items = new List<kendoTreeItem>();

                var qry = v.AllColumns.Where(p => p.Entity == rel.TableName);
                if (rel.TableName == "le4" || rel.TableName == "le3" || rel.TableName == "le2" || rel.TableName == "le1")
                {
                    qry=v.AllColumns.Where(p => p.Entity == "p41Project" || p.Entity== rel.TableName);
                }
                
                foreach (string gg in qry.Where(p => p.DesignerGroup != null).Select(p => p.DesignerGroup).Distinct())
                {
                    var cc = new kendoTreeItem() { text = gg, customvalue2 = "/images/folder.png", customvalue3 = "tree_group" };
                    grp.items.Add(cc);
                }



                foreach (var col in qry)
                {
                    var cc = new kendoTreeItem() { id = rel.RelName + "__" + col.Entity + "__" + col.Field, text = col.Header };
                    cc.customvalue1 = rel.RelName + "__" + col.Entity;
                    cc.tooltip = cc.tooltip;
                    switch (Factory.CurrentUser.j02LangIndex)
                    {
                        case 1:
                            cc.text = col.TranslateLang1; break;
                        case 2:
                            cc.text = col.TranslateLang2; break;
                        default:
                            cc.text = col.Header;
                            break;
                    }

                    if (Factory.p07LevelsCount > 1)
                    {
                        for (int i = 1; i <= 5; i++)
                        {
                            if (cc.text == "L" + i.ToString()) cc.text = Factory.getP07Level(i, true);
                        }
                    }
                    else
                    {
                        if (cc.text == "L5") cc.text = Factory.getP07Level(5, true);
                    }

                    if (rel.RelName != null && rel.RelName.Length >= 4 && rel.RelName.Substring(0, 4) == "p41_")
                    {
                        cc.text += " [" + rel.AliasSingular + "]";
                    }

                    cc.text_plus_group = grp.text + " -> " + cc.text;

                    cc.customvalue2 = "/images/" + col.getImage();
                    cc.customvalue3 = "tree_item";
                    if (col.IsTimestamp) cc.customvalue3 += " timestamp";

                    if (col.DesignerGroup == null)
                    {
                        grp.items.Add(cc);
                    }
                    else
                    {
                        var findgrp = grp.items.Where(p => p.customvalue3 == "tree_group" && p.text == col.DesignerGroup);
                        if (findgrp.Count() > 0)
                        {
                            if (findgrp.First().items == null)
                            {
                                findgrp.First().items = new List<kendoTreeItem>();
                            }
                            findgrp.First().items.Add(cc);
                        }

                    }
                    col.Rezerva = "1";
                }
                
                v.treeNodes.Add(grp);
            }



        }



        private void Index_RefreshState(Models.TheGridDesignerViewModel v)
        {
            var mq = new BO.myQuery(v.Rec.j72Entity);
            var ce = Factory.EProvider.ByPrefix(mq.Prefix);
            v.Relations = Factory.EProvider.getApplicableRelations(mq.Prefix, Factory); //návazné relace
            v.Relations.Insert(0, new BO.EntityRelation() { TableName = ce.TableName, AliasSingular = ce.AliasSingular, SqlFrom = ce.SqlFromGrid, RelName = "a", Translate1 = ce.TranslateLang1, Translate2 = ce.TranslateLang2 });   //primární tabulka a

            

            v.AllColumns = new List<BO.TheGridColumn>();
            if (Factory.CurrentUser.IsRatesAccess && Factory.CurrentUser.IsVysledovkyAccess)
            {
                v.AllColumns.InsertRange(0, _colsProvider.AllColumns());        //přístup ke všem sloupcům        
            }
            else
            {
                if (!Factory.CurrentUser.IsRatesAccess && mq.Prefix == "p31")
                {
                    v.Relations = v.Relations.Where(p => p.RelName != "p31_p91").ToList(); //nenabízet entitu Vyúčtování
                }
                var mycols = _colsProvider.AllColumns().AsEnumerable();
                if (!Factory.CurrentUser.IsRatesAccess)
                {
                    mycols = mycols.Where(p => !p.IHRC);    //nemá právo na fakturační sazby a honoráře
                }
                if (!Factory.CurrentUser.IsVysledovkyAccess)
                {
                    mycols = mycols.Where(p => !p.VYSL);    //nemá právo na výsledovky a nákladové sazby
                }
                v.AllColumns.InsertRange(0, mycols);    //bez billing sloupců
                //v.AllColumns.InsertRange(0, _colsProvider.AllColumns().Where(p => !p.IHRC));    //bez billing sloupců

            }
            if (Factory.CurrentUser.j04GridColumnsExclude != null)  //vyhodit zakázané sloupce
            {
                var exc = Factory.CurrentUser.j04GridColumnsExclude.Split(",");
                for (int i = 0; i < exc.Length; i++)
                {
                    if (v.AllColumns.Any(p=>p.UniqueName==exc[i]))
                    {
                        v.AllColumns.Remove(v.AllColumns.Where(p => p.UniqueName == exc[i]).First());
                    }
                }                
            }
            
            //uživatelská pole, uživatelská pole pro o23Doc:
            string strDbPrefix = BO.Code.Entity.GetPrefixDb(v.Rec.j72Entity.Substring(0, 3));
            
            v.AllColumns.InsertRange(v.AllColumns.Count - 1, new BL.ffColumnsProvider(Factory, strDbPrefix).getColumns());    //katalog uživatelských polí a rolí v entitách

            if (!string.IsNullOrEmpty(v.Rec.j72Columns))
            {
                v.SelectedColumns = _colsProvider.ParseTheGridColumns(mq.Prefix, v.Rec.j72Columns, Factory);
                v.Rec.j72Columns = String.Join(",", v.SelectedColumns.Select(p => p.UniqueName));               
                
            }
            

            if (v.CallerPathName !=null && v.CallerPathName.Contains("p31totals"))
            {
                //vyhodit zakázané sloupce pro nástroj SOUČTY p31totals
                v.AllColumns = v.AllColumns.Where(p => !p.IsNotUseP31TOTALS).ToList();
            }
            
            

            v.lisQueryFields = new BL.TheQueryFieldProvider(v.Rec.j72Entity.Substring(0, 3)).getPallete();
            if (Factory.CurrentUser.j02LangIndex > 0)
            {   //překlad do cizího jazyku
                foreach (var c in v.lisQueryFields)
                {
                    c.Header = Factory.tra(c.Header);
                }
            }

            v.lisPeriods = _pp.getPallete();
            if (v.lisJ73 == null)
            {
                v.lisJ73 = new List<BO.j73TheGridQuery>();
            }
            foreach (var c in v.lisJ73.Where(p => p.j73Column != null))
            {
                if (v.lisQueryFields.Where(p => p.Field == c.j73Column).Count() > 0)
                {
                    var cc = v.lisQueryFields.Where(p => p.Field == c.j73Column).First();
                    c.FieldType = cc.FieldType;
                    c.FieldEntity = cc.SourceEntity;
                    c.MasterPrefix = cc.MasterPrefix;
                    c.MasterPid = cc.MasterPid;
                }
            }

            inhale_tree(v);
        }
    }
}