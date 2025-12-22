using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BL;
using BO;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;
using UI.Models.Tab1;
using UI.Models.Tags;

namespace UI.Controllers
{
    public class o51Controller : BaseController
    {
        public IActionResult Info(int pid)
        {
            return Tab1(pid, "info");
        }
        public IActionResult Tab1(int pid, string caller)
        {
            var v = new o51Tab1() { Factory = this.Factory, pid = pid, caller = caller };

            v.Rec = Factory.o51TagBL.Load(v.pid);
            if (v.Rec != null)
            {
                v.RecSum = Factory.o51TagBL.LoadSumRow(v.Rec.pid);
                

            }

            return View(v);
        }

        public IActionResult MultiSelect(string entity,string o51ids)
        {
            var v = new TagsMultiSelect() { Entity = entity };

            string strPrefix = entity.Substring(0, 3);
            string strPrefixDruhy = "-1";            
            if (entity.Substring(0, 2) == "le")
            {
                strPrefixDruhy = "p41";
            }

            IEnumerable<BO.o51Tag> lisTags = Factory.o51TagBL.GetList(new BO.myQueryO51());
            v.ApplicableTags_Multi = lisTags.Where(p => p.o53IsMultiSelect == true && (p.o53Entities == null || p.o53Entities.Contains(strPrefix) || p.o53Entities.Contains(strPrefixDruhy)));
            v.ApplicableTags_Single = lisTags.Where(p => p.o53IsMultiSelect == false && (p.o53Entities == null || p.o53Entities.Contains(strPrefix) || p.o53Entities.Contains(strPrefix)));

            if (String.IsNullOrEmpty(o51ids) == false)
            {
                var mqx = new BO.myQueryO51();
                mqx.SetPids(o51ids);
                lisTags = Factory.o51TagBL.GetList(mqx);
            }
            

            var mq = new BO.myQuery("o53TagGroup");
            var lisGroups = Factory.o53TagGroupBL.GetList(mq).Where(p =>p.o53IsMultiSelect==false && ( p.o53Entities == null || p.o53Entities.Contains(strPrefix) || p.o53Entities.Contains(strPrefixDruhy))).ToList();
            v.SingleCombos = new List<SingleSelectCombo>();
            foreach (var group in lisGroups)
            {
                var c = new SingleSelectCombo() { o53ID = group.pid, o53Name = group.o53Name };
                if (String.IsNullOrEmpty(o51ids) == false)
                {
                    if (lisTags.Where(p => p.o53ID == group.pid).Count() > 0)
                    {
                        c.o51ID = lisTags.Where(p => p.o53ID == group.pid).First().pid;
                        c.o51Name = lisTags.Where(p => p.o53ID == group.pid).First().o51Name;
                    }
                }
                    
                v.SingleCombos.Add(c);
            }
            
            v.CheckedO51IDs = BO.Code.Bas.ConvertString2ListInt(o51ids);
            v.SelectedO51IDs = BO.Code.Bas.ConvertString2ListInt(o51ids);

          

            return View(v);
        }
        public IActionResult Record(int pid, bool isclone)
        {           
            
            var v = new o51Record() { rec_pid = pid, rec_entity = "o51" };
            v.Rec = new BO.o51Tag();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.o51TagBL.Load(v.rec_pid);                
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.Combo_o53Name = v.Rec.o53Name;
            }
                       
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }
            return ViewTup(v,BO.PermValEnum.GR_o51_Admin);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(o51Record v)
        {
            
            if (ModelState.IsValid)
            {
                BO.o51Tag c = new BO.o51Tag();
                if (v.rec_pid > 0) c = Factory.o51TagBL.Load(v.rec_pid);
                c.o53ID = v.Rec.o53ID;                
                c.o51Name = v.Rec.o51Name;
                c.o51Ordinary = v.Rec.o51Ordinary;
                c.o51IsColor = v.Rec.o51IsColor;
                c.o51BackColor = v.Rec.o51BackColor;
                c.o51ForeColor = v.Rec.o51ForeColor;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.o51TagBL.Save(c);
                if (c.pid > 0)
                {
                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }

           
            this.Notify_RecNotSaved();
            return View(v);
        }


        public IActionResult Batch(int j72id, string pids, string guid_pids)
        {
            if (!string.IsNullOrEmpty(guid_pids))
            {
                pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            var v = new TagsBatch();
            RefreshState_Batch(v, j72id, pids);

            return View(v);
        }
        [HttpPost]
        public IActionResult Batch(Models.Tags.TagsBatch v,string oper)
        {
            RefreshState_Batch(v, v.j72ID, v.Record_Pids);

            if (ModelState.IsValid)
            {
                if (oper == "postback")
                {
                    RefreshState_Batch(v, v.j72ID, v.Record_Pids);
                    return View(v);
                }
                if (v.SelectedO53ID == 0)
                {
                    this.AddMessage("Musíte vybrat štítek.");
                    RefreshState_Batch(v, v.j72ID, v.Record_Pids);
                    return View(v);
                }


                List<int> o51ids = new List<int>();
                if (v.RecO53.o53IsMultiSelect == true)
                {
                    o51ids=v.SelectedO51IDs.Where(p => p > 0).ToList();
                }
                else
                {
                    o51ids.Add(v.SelectedRadioO51ID);
                }

                List<int> pids = BO.Code.Bas.ConvertString2ListInt(v.Record_Pids);

                if (o51ids.Count > 0 || oper=="clear")
                {
                    string strO51IDs = string.Join(",", o51ids);
                    foreach (int pid in pids)
                    {
                        switch (oper)
                        {
                            case "replace":
                                Factory.o51TagBL.SaveTagging(v.Record_Entity, pid, strO51IDs, v.SelectedO53ID);
                                break;
                            case "clear":
                                Factory.o51TagBL.SaveTagging(v.Record_Entity, pid,"", v.SelectedO53ID);
                                break;
                            case "append":
                                var c = Factory.o51TagBL.GetTagging(v.Record_Entity, pid);
                                if (c.TagPids == null)
                                {
                                    c.TagPids = strO51IDs;
                                }
                                else
                                {
                                    c.TagPids += ","+strO51IDs;
                                }
                                Factory.o51TagBL.SaveTagging(v.Record_Entity, pid, c.TagPids,v.SelectedO53ID);

                                break;
                            case "remove":
                                var d = Factory.o51TagBL.GetTagging(v.Record_Entity, pid);
                                if (d.TagPids != null)
                                {
                                    foreach(int o51id in o51ids)
                                    {
                                        d.TagPids = BO.Code.Bas.RemoveValueFromDelimitedString(d.TagPids, o51id.ToString());
                                    }                                    
                                    Factory.o51TagBL.SaveTagging(v.Record_Entity, pid, d.TagPids,v.SelectedO53ID);
                                   
                                }
                                break;
                        }
                        
                    }
                    v.SetJavascript_CallOnLoad(v.j72ID);
                    return View(v);
                }
               
                
            }
            
            
           
            return View(v);
        }
        
        private void RefreshState_Batch(TagsBatch v,int j72id,string pids)
        {
            var gridState = Factory.j72TheGridTemplateBL.LoadState(j72id,Factory.CurrentUser.pid);            
            v.j72ID = j72id;
            v.Record_Entity = gridState.j72Entity;
            v.Record_Pids = pids;

            string strPrefix = v.Record_Entity.Substring(0, 3);
            
            string strPrefixDruhy ="-1";
            if (strPrefix.Substring(0, 2) == "le")
            {
                strPrefixDruhy = "p41";
            }

            v.lisO53 = Factory.o53TagGroupBL.GetList(new BO.myQuery("o53")).Where(p => p.o53Entities == null || p.o53Entities.Contains(strPrefix) || p.o53Entities.Contains(strPrefixDruhy));
            if (v.SelectedO53ID > 0)
            {
                v.RecO53 = Factory.o53TagGroupBL.Load(v.SelectedO53ID);
                var mq = new BO.myQueryO51() { o53id = v.SelectedO53ID };                             
                v.ApplicableTags = Factory.o51TagBL.GetList(mq);
            }

            
            string strMyQuery = "pids|list_int|" + v.Record_Pids;            
            v.gridinput = new Views.Shared.Components.myGrid.myGridInput() {is_enable_selecting=false, entity = v.Record_Entity, master_entity = "inform", myqueryinline = strMyQuery, oncmclick = "", ondblclick = "" };
            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load(v.Record_Entity.Substring(0,3), null, 0, strMyQuery);
            
        }

       

        public string GetTagHtml(string o51ids)
        {
            if (String.IsNullOrEmpty(o51ids) == true)
            {
                return "";
            }
            List<int> lis = BO.Code.Bas.ConvertString2ListInt(o51ids);
            return Factory.o51TagBL.GetTagging(lis).TagHtml;
        }

        
    }

    
}