using Microsoft.AspNetCore.Mvc;
using UI.Models;

namespace UI.Controllers
{
    public class TheQueryDesignerController : BaseController
    {
        public IActionResult Index(int j72id,string prefix, string pathname, bool saveas)
        {

            var v = new TheQueryDesignerViewModel() { CallerPathName = pathname, ForceSaveAsAtStart = saveas };
            
            v.Rec = new BO.j72TheGridTemplate() { j72Entity =BO.Code.Entity.GetEntity( prefix),j72SystemFlag=BO.j72SystemFlagEnum.QueryOnly,j02ID=Factory.CurrentUser.pid };

           
            if (j72id > 0)
            {
                v.Rec = Factory.j72TheGridTemplateBL.Load(j72id);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
            }
            if (saveas)
            {                
                v.Rec.j72SystemFlag = BO.j72SystemFlagEnum.QueryOnly;                
                v.Rec.j02ID = Factory.CurrentUser.pid;
            }

            if (v.Rec.j02ID == Factory.CurrentUser.pid)
            {
                v.HasOwnerPermissions = true;
                if (v.Rec.pid > 0)
                {
                    var lis = Factory.j04UserRoleBL.GetList(new BO.myQueryJ04() { j72id = v.Rec.pid });
                    v.j04IDs = string.Join(",", lis.Select(p => p.pid));
                    v.j04Names = string.Join(",", lis.Select(p => p.j04Name));
                }
                
            }
            
            v.lisJ73 = Factory.j72TheGridTemplateBL.GetList_j73(v.Rec.pid, v.Rec.j72Entity.Substring(0, 3),0).ToList();
            foreach (var c in v.lisJ73)
            {
                c.TempGuid = BO.Code.Bas.GetGuid();
            }

            if (saveas)
            {                
                v.Rec.pid = 0;                
            }

            Index_RefreshState(v);

            return View(v);

        }

        private void Index_RefreshState(Models.TheQueryDesignerViewModel v)
        {
                       
            v.lisQueryFields = new BL.TheQueryFieldProvider(v.Rec.j72Entity.Substring(0, 3)).getPallete();
            
            if (Factory.CurrentUser.j02LangIndex > 0)
            {   //překlad do cizího jazyku
                foreach (var c in v.lisQueryFields)
                {
                    c.Header = Factory.tra(c.Header);
                }
            }

            
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

            
        }


        [HttpPost]
        public IActionResult Index(Models.TheQueryDesignerViewModel v, string guid, string j72name)    //uložení grid filtru
        {
            Index_RefreshState(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper == "saveas" && j72name != null)
                {
                    var recJ72 = Factory.j72TheGridTemplateBL.Load(v.Rec.pid);
                    var lisJ73 = Factory.j72TheGridTemplateBL.GetList_j73(recJ72.pid, recJ72.j72Entity.Substring(0, 3),0).ToList();
                    recJ72.j72SystemFlag = BO.j72SystemFlagEnum.QueryOnly; recJ72.j72ID = 0; recJ72.pid = 0; recJ72.j72Name = j72name; recJ72.j02ID = Factory.CurrentUser.pid;
                    List<int> j04ids = BO.Code.Bas.ConvertString2ListInt(v.j04IDs);
                    
                    var intJ72ID = Factory.j72TheGridTemplateBL.Save(recJ72, lisJ73, j04ids,null);
                    
                    return RedirectToActionPermanent("Index", new { j72id = intJ72ID,prefix=recJ72.j72Entity.Substring(0,3), pathname = v.CallerPathName });
                }
                if (v.PostbackOper == "new" && j72name != null)
                {
                    v.Rec.j72Name = j72name;
                    v.Rec.pid = 0;
                    v.lisJ73.Clear();
                }
                

                if (v.PostbackOper == "delete" && v.HasOwnerPermissions)
                {
                    if (Factory.CBL.DeleteRecord("j72", v.Rec.pid) == "1")
                    {
                        v.Rec.pid = Factory.j72TheGridTemplateBL.LoadState(v.Rec.j72Entity, Factory.CurrentUser.pid, v.Rec.j72MasterEntity, v.Rec.j72Rez).pid;
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

                return View(v);
            }



            if (ModelState.IsValid)
            {

                BO.j72TheGridTemplate recJ72 = new BO.j72TheGridTemplate() { j72SystemFlag = BO.j72SystemFlagEnum.QueryOnly,j72Entity=v.Rec.j72Entity,j02ID=Factory.CurrentUser.pid };
                if (v.Rec.pid > 0)
                {
                    recJ72 = Factory.j72TheGridTemplateBL.Load(v.Rec.pid);
                }
                
                recJ72.j72Name = v.Rec.j72Name;
              
                recJ72.j72IsPublic = v.Rec.j72IsPublic;
                recJ72.j72IsQueryNegation = v.Rec.j72IsQueryNegation;
               
                List<int> j04ids = BO.Code.Bas.ConvertString2ListInt(v.j04IDs);
               
                int intJ72ID = Factory.j72TheGridTemplateBL.Save(recJ72, v.lisJ73.Where(p => p.j73ID > 0 || p.IsTempDeleted == false).ToList(), j04ids, null);
                if (intJ72ID > 0)
                {
                    // Factory.j72TheGridTemplateBL.SaveState(gridState, Factory.CurrentUser.pid);
                    SaveQueryToUserCache(v, intJ72ID);


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

        private void SaveQueryToUserCache(Models.TheQueryDesignerViewModel v,int intJ72ID)
        {
            if (v.CallerPathName.Contains("MasterView"))
            {
                Factory.CBL.SetUserParam($"masterview-query-j72id-{v.Rec.j72Entity.Substring(0, 3)}-", intJ72ID.ToString());
            }
            if (v.CallerPathName.Contains("FlatView"))
            {
                Factory.CBL.SetUserParam($"flatview-query-j72id-{v.Rec.j72Entity.Substring(0, 3)}-", intJ72ID.ToString());
            }

        }

    }
}
