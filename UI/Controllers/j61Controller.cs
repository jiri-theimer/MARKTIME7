using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class j61Controller : BaseController
    {
        private readonly BL.TheColumnsProvider _colsProvider;
        public j61Controller(BL.TheColumnsProvider cp)
        {
            _colsProvider = cp;
        }
        public IActionResult Info(int pid)
        {
            var v = new BaseTab1ViewModel() { prefix = "j61", pid = pid };
            return View(v);
        }
        public IActionResult Record(int pid, bool isclone,string prefix)
        {
            var v = new j61Record() { rec_pid = pid, rec_entity = "j61" };
            v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "j61", SelectedX04ID = Factory.Lic.x04ID_Default };
            v.Rec = new BO.j61TextTemplate() {j61Entity=prefix, j61GridColumns = Factory.MailBL.GetDefaultGridFields(prefix), j61UserSignatureFlag=BO.j61UserFlagEnum.Yes , j61GridColumnsFlag = BO.j61UserFlagEnum.Yes,j61RecordLinkFlag=BO.j61UserFlagEnum.Yes };
           
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.j61TextTemplateBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.ComboOwner = v.Rec.Owner;
                v.Notepad.HtmlContent = v.Rec.j61MailBody;
                v.Notepad.SelectedX04ID = v.Rec.x04ID;

            }
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }
            
            return View(v);
        }

        private void RefreshState(j61Record v)
        {

            if (!string.IsNullOrEmpty(v.Rec.j61GridColumns))
            {
                v.lisGridColumns = _colsProvider.ParseTheGridColumns(v.Rec.j61Entity, v.Rec.j61GridColumns, Factory);

            }
            v.Notepad.PlaceHolder = "Notepad: Text poštovní zprávy";
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(j61Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper== "j61entity" && v.rec_pid==0 && v.Rec.j61Entity !=null)
                {
                    v.Rec.j61GridColumns = Factory.MailBL.GetDefaultGridFields(v.Rec.j61Entity);
                    RefreshState(v);
                }
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.j61TextTemplate c = new BO.j61TextTemplate();
                if (v.rec_pid > 0) c = Factory.j61TextTemplateBL.Load(v.rec_pid);
                c.j02ID_Owner = v.Rec.j02ID_Owner;                
                c.j61Name = v.Rec.j61Name;
                c.j61Ordinary = v.Rec.j61Ordinary;
                c.j61Entity = v.Rec.j61Entity;
                c.j61MailSubject = v.Rec.j61MailSubject;
                c.j61MailBody = v.Notepad.HtmlContent;
                c.x04ID = v.Notepad.SelectedX04ID;
                c.j61MailTO = v.Rec.j61MailTO;
                c.j61MailCC = v.Rec.j61MailCC;
                c.j61MailBCC = v.Rec.j61MailBCC;
                c.j61HtmlTemplateFile = v.Rec.j61HtmlTemplateFile;
                c.j61GridColumns = v.Rec.j61GridColumns;
                c.j61RecordLinkFlag = v.Rec.j61RecordLinkFlag;
                c.j61GridColumnsFlag = v.Rec.j61GridColumnsFlag;
                c.j61UserSignatureFlag = v.Rec.j61UserSignatureFlag;
                c.j61IsPublic = v.Rec.j61IsPublic;
                c.j61SqlSource = v.Rec.j61SqlSource;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);


                c.pid = Factory.j61TextTemplateBL.Save(c);
                if (c.pid > 0)
                {

                    v.SetJavascript_CallOnLoad(c.pid,"j61id");
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }

        public int PrepareTempData(string prefix,string guid,string cols)
        {
            var c = new BO.p85Tempbox() { p85GUID = guid, p85Prefix = prefix,p85Message=cols };
            return Factory.p85TempboxBL.Save(c);
        }
    }
}
