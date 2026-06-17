using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class o53Controller : BaseController
        
    {
        private readonly BL.TheColumnsProvider _cp;
        public o53Controller(BL.TheColumnsProvider cp)
        {
            _cp = cp;
        }
        ///skupina štítku
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new o53Record() { rec_pid = pid, rec_entity = "o53" };
            v.Rec = new  BO.o53TagGroup();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.o53TagGroupBL.Load(v.rec_pid);
                
                

            }
            else
            {                
                v.Rec.o53IsMultiSelect = true;
            }
            v.Toolbar = new MyToolbarViewModel(v.Rec);

            
            v.ApplicableEntities = GetApplicableEntities();
           
            if (isclone)
            {
                v.MakeClone();
            }
            return ViewTup(v,BO.PermValEnum.GR_o51_Admin);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(o53Record v)
        {
            v.ApplicableEntities = GetApplicableEntities();

            if (ModelState.IsValid)
            {
                BO.o53TagGroup c = new BO.o53TagGroup();
                if (v.rec_pid > 0) c = Factory.o53TagGroupBL.Load(v.rec_pid);

                
                c.o53Name = v.Rec.o53Name;
                c.o53Entities = v.Rec.o53Entities;
                c.o53IsMultiSelect = v.Rec.o53IsMultiSelect;
                c.o53Ordinary = v.Rec.o53Ordinary;
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.o53TagGroupBL.Save(c);
                if (c.pid > 0)
                {
                    _cp.Refresh();   //obnovit názvy sloupců kategorií
                   
                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }
                            
            }
            
            
            
            this.Notify_RecNotSaved();
            return View(v);
            
        }


        private List<BO.TheEntity> GetApplicableEntities()
        {
            var lis = new List<BO.TheEntity>();            
            lis.Add(Factory.EProvider.ByPrefix("o23"));
            lis.Add(Factory.EProvider.ByPrefix("p28"));
            lis.Add(Factory.EProvider.ByPrefix("p41"));
            lis.Add(Factory.EProvider.ByPrefix("j02"));
            lis.Add(Factory.EProvider.ByPrefix("p31"));            
            lis.Add(Factory.EProvider.ByPrefix("p91"));
            lis.Add(Factory.EProvider.ByPrefix("p90"));
            lis.Add(Factory.EProvider.ByPrefix("p56"));
            lis.Add(Factory.EProvider.ByPrefix("o22"));
            lis.Add(Factory.EProvider.ByPrefix("p84"));


            for (int i = 1; i <= 5; i++)
            {
                if (this.Factory.getP07Level(i,false) != null)
                {
                    Factory.EProvider.ByPrefix("le" + (i).ToString()).AliasPlural = this.Factory.getP07Level(i, false)+" ["+Factory.tra("Pouze verze 7")+"]";
                    lis.Add(Factory.EProvider.ByPrefix("le" + i.ToString()));
                }
                //Factory.EProvider.ByPrefix("le" + (6 - i).ToString()).AliasPlural = this.Factory.CurrentUser.getP07Level(5 - i,false);
                
            }
            
            

            return lis;
        }



    }
}