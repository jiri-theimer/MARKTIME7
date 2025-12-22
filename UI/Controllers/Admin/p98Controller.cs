using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class p98Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p98Record() { rec_pid = pid, rec_entity = "p98" };
            v.Rec = new BO.p98Invoice_Round_Setting_Template();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p98Invoice_Round_Setting_TemplateBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }

              
                var lis = Factory.p98Invoice_Round_Setting_TemplateBL.GetList_p97(v.rec_pid).ToList();
                v.lisP97 = new List<p97Repeater>();
                foreach (var c in lis)
                {
                                        
                    v.lisP97.Add(new p97Repeater()
                    {
                        TempGuid = BO.Code.Bas.GetGuid(),p97Scale=c.p97Scale,p97AmountFlag=c.p97AmountFlag,
                        j27ID = c.j27ID,           
                        ComboJ27 = Factory.FBL.LoadCurrencyByID(c.j27ID).j27Code
                        
                    });
                }
            }
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(p98Record v)
        {
            if (v.lisP97 == null)
            {
                v.lisP97 = new List<p97Repeater>();
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p98Record v, string guid)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper == "add_row")
                {
                    var c = new p97Repeater() { TempGuid = BO.Code.Bas.GetGuid() };
                    v.lisP97.Add(c);

                }

                if (v.PostbackOper == "delete_row")
                {
                    v.lisP97.First(p => p.TempGuid == guid).IsTempDeleted = true;

                }

                return View(v);
            }
            
            
            


         
            if (ModelState.IsValid)
            {

                BO.p98Invoice_Round_Setting_Template c = new BO.p98Invoice_Round_Setting_Template();
                if (v.rec_pid > 0) c = Factory.p98Invoice_Round_Setting_TemplateBL.Load(v.rec_pid);
                c.p98Name = v.Rec.p98Name;
                c.p98IsDefault = v.Rec.p98IsDefault;
                c.p98IsIncludeInVat = v.Rec.p98IsIncludeInVat;
               
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
              
                var lis = new List<BO.p97Invoice_Round_Setting>();
                foreach (var row in v.lisP97.Where(p => p.IsTempDeleted == false))
                {
                    var cc = new BO.p97Invoice_Round_Setting() { j27ID = row.j27ID,p97AmountFlag=row.p97AmountFlag,p97Scale=row.p97Scale };
                    lis.Add(cc);
                }

                c.pid = Factory.p98Invoice_Round_Setting_TemplateBL.Save(c, lis);
                if (c.pid > 0)
                {
                  
                   
                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }





            }


            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
