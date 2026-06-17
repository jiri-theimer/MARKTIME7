using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;


namespace UI.Controllers
{
    public class a59Controller : BaseController
    {
        public IActionResult Record(int pid, int a55id)
        {

            var v = new a59Record() { rec_pid = pid, rec_entity = "a59" };

            v.Rec = new BO.a59RecPageLayer() { a55ID = a55id,a59ColumnsPerPage=2,a59StructureFlag=BO.a59StructureFlagENUM.Boxes,a59CssClassContainer="container" };
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.a59RecPageLayerBL.Load(v.rec_pid);
                //v.HtmlContentHelp = v.Rec.a59HelpText;
                //v.SelectedB02IDs = Factory.b02WorkflowStatusBL.GetList(new BO.myQueryB02() { a59id = v.rec_pid }).Select(p => p.pid).ToList();
            }
            else
            {
                if (v.Rec.a55ID == 0)
                {
                    return this.StopPage(true, "a55id missing.");
                }
            }
            v.Toolbar = new MyToolbarViewModel(v.Rec);

            RefreshState(v);


            return ViewTup(v,BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(a59Record v)
        {
            v.RecA55 = Factory.a55RecPageBL.Load(v.Rec.a55ID);
            
            //v.lisB02 = Factory.b02WorkflowStatusBL.GetList(new BO.myQueryB02() { a55id = v.RecA55.pid });

        }

        [HttpPost]        
        public IActionResult Record(Models.Record.a59Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
               
                BO.a59RecPageLayer c = new BO.a59RecPageLayer();
                if (v.rec_pid > 0) c = Factory.a59RecPageLayerBL.Load(v.rec_pid);

                c.a55ID = v.Rec.a55ID;
                
                c.a59Name = v.Rec.a59Name;
                c.a59ColumnsPerPage = v.Rec.a59ColumnsPerPage;
                c.a59CssClassContainer = v.Rec.a59CssClassContainer;
                c.a59StructureFlag = v.Rec.a59StructureFlag;
                c.a59CustomHtmlStructure = v.Rec.a59CustomHtmlStructure;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                if (v.SelectedB02IDs != null)
                {
                    v.SelectedB02IDs = v.SelectedB02IDs.Where(p => p > 0).ToList();
                }
                c.pid = Factory.a59RecPageLayerBL.Save(c,v.SelectedB02IDs);
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
