using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers.Admin
{
    public class p92Controller : BaseController
    {

        public IActionResult Info(int pid)
        {
            var v = new BaseTab1ViewModel() { prefix = "p92", pid = pid };
            return View(v);
        }
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p92Record() { rec_pid = pid, rec_entity = "p92" };
            v.Rec = new BO.p92InvoiceType();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p92InvoiceTypeBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }

                v.ComboJ27Code = v.Rec.j27Code;
                v.ComboJ61Name = v.Rec.j61Name;
                v.ComboP93Name = v.Rec.p93Name;
                v.ComboP83Name = v.Rec.p83Name;
                try
                {
                    if (v.Rec.p80ID > 0) v.ComboP80Name = Factory.p80InvoiceAmountStructureBL.Load(v.Rec.p80ID).p80Name;
                    if (v.Rec.p98ID > 0) v.ComboP98Name = Factory.p98Invoice_Round_Setting_TemplateBL.Load(v.Rec.p98ID).p98Name;
                    if (v.Rec.x31ID_Invoice > 0) v.ComboX31_Report = Factory.x31ReportBL.Load(v.Rec.x31ID_Invoice).x31Name;
                    if (v.Rec.x31ID_Attachment > 0) v.ComboX31_Attachment = Factory.x31ReportBL.Load(v.Rec.x31ID_Attachment).x31Name;
                    if (v.Rec.x31ID_Letter > 0) v.ComboX31_Letter = Factory.x31ReportBL.Load(v.Rec.x31ID_Letter).x31Name;
                  
                    if (v.Rec.x38ID > 0) v.ComboX38Name = Factory.x38CodeLogicBL.Load(v.Rec.x38ID).x38Name;
                    if (v.Rec.p32ID_CreditNote > 0) v.ComboP32Name = Factory.p32ActivityBL.Load(v.Rec.p32ID_CreditNote).p32Name;
                    if (v.Rec.b01ID > 0) v.ComboB01 = Factory.b01WorkflowTemplateBL.Load(v.Rec.b01ID).b01Name;
                    
                }
                catch
                {
                    ///nic
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

        private void RefreshState(p92Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p92Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                BO.p92InvoiceType c = new BO.p92InvoiceType();
                if (v.rec_pid > 0) c = Factory.p92InvoiceTypeBL.Load(v.rec_pid);
                c.p92Name = v.Rec.p92Name;
                c.p92Ordinary = v.Rec.p92Ordinary;
                c.p92TypeFlag = v.Rec.p92TypeFlag;
                c.x38ID = v.Rec.x38ID;
                c.j27ID = v.Rec.j27ID;
                c.p93ID = v.Rec.p93ID;
                c.p80ID = v.Rec.p80ID;
                c.p98ID = v.Rec.p98ID;
                c.p83ID = v.Rec.p83ID;
                
                c.p32ID_CreditNote = v.Rec.p32ID_CreditNote;
                c.x15ID = v.Rec.x15ID;

                c.x31ID_Invoice = v.Rec.x31ID_Invoice;
                c.x31ID_Attachment = v.Rec.x31ID_Attachment;
                c.x31ID_Letter = v.Rec.x31ID_Letter;
                c.j61ID = v.Rec.j61ID;
               
                c.p92InvoiceDefaultText1 = v.Rec.p92InvoiceDefaultText1;
                c.p92InvoiceDefaultText2 = v.Rec.p92InvoiceDefaultText2;
                c.p92ReportConstantPreText1 = v.Rec.p92ReportConstantPreText1;
                c.p92ReportConstantText = v.Rec.p92ReportConstantText;

                
                c.p92FilesTab = v.Rec.p92FilesTab;
                c.p92RolesTab = v.Rec.p92RolesTab;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
                c.b01ID = v.Rec.b01ID;
                c.p92RepDocName = v.Rec.p92RepDocName;
                c.p92RepDocNumber = v.Rec.p92RepDocNumber;

                c.p92QrCodeFlag = v.Rec.p92QrCodeFlag;

                c.pid = Factory.p92InvoiceTypeBL.Save(c);
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
