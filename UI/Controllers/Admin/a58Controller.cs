using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class a58Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone,int a59id)
        {

            var v = new a58Record() { rec_pid = pid, rec_entity = "a58" };

            v.Rec = new BO.a58RecPageBox() { a59ID = a59id,a58IsHtmlByPlaintext =false,x04ID=Factory.Lic.x04ID_Default,a58Code=BO.Code.Bas.GetGuid().Substring(0,10) };
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.a58RecPageBoxBL.Load(v.rec_pid);
                 
                v.HtmlContentText = v.Rec.a58HtmlText;

            }

            v.Notepad = new Models.Notepad.EditorViewModel() { HtmlContent = v.Rec.a58HtmlText, SelectedX04ID = v.Rec.x04ID,Prefix="a58" };

            v.Toolbar = new MyToolbarViewModel(v.Rec);


            if (isclone)
            {
                v.MakeClone();

            }

            RefreshState(v);

            return ViewTup(v,BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(a58Record v)
        {
            v.RecA59 = Factory.a59RecPageLayerBL.Load(v.Rec.a59ID);
            v.RecA55 = Factory.a55RecPageBL.Load(v.RecA59.a55ID);
            
            
            switch (v.Rec.a58ContentFlag)
            {
                case BO.a58ContentFlagEnum.ButtonOneWorkflowStep:
                    //v.lisB06 = Factory.b06WorkflowStepBL.GetList(new BO.myQueryB06() { a55id=v.RecA55.pid}).Where(p => p.b06IsManualStep==true);
                    break;
                  
              
                case BO.a58ContentFlagEnum.ButtonOneReport:
                    //v.lisX31 = Factory.x31ReportBL.GetList(new BO.myQueryX31() { entity=101,is=true,IsRecordValid=null });
                    break;
            }
           


        }

        [HttpPost]
        public IActionResult Record(a58Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {
                
                BO.a58RecPageBox c = new BO.a58RecPageBox();
                if (v.rec_pid > 0) c = Factory.a58RecPageBoxBL.Load(v.rec_pid);

                c.a59ID = v.Rec.a59ID;

                c.a58ContentFlag = v.Rec.a58ContentFlag;
                c.a58ButtonText = v.Rec.a58ButtonText;
                c.b06ID_Button = v.Rec.b06ID_Button;
                c.x31ID_Button = v.Rec.x31ID_Button;
                             
                c.a58HtmlText = v.Notepad.HtmlContent;
                c.x04ID = v.Notepad.SelectedX04ID;
                if (c.x04ID == 0)
                {
                    c.a58IsHtmlByPlaintext = true;
                }
                else
                {
                    c.a58IsHtmlByPlaintext = false;
                }
               
                c.a58Name = v.Rec.a58Name;
                c.a58CssClassName = v.Rec.a58CssClassName;
                c.a58ControlFlag = v.Rec.a58ControlFlag;
                c.a58Code = v.Rec.a58Code;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.a58RecPageBoxBL.Save(c);
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
