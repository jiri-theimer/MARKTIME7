using BO;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class o18Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new o18Record() { rec_pid = pid, rec_entity = "o18" };
            v.Rec = new BO.o18DocType() { o18EntryCodeFlag=BO.o18EntryCodeENUM.NotUsed };
            v.roles = new RoleAssignViewModel() { RecPid = v.rec_pid, RecPrefix = "o18",RolePrefix="o23",Header="Automatické obsazení rolí v dokumentech tohoto typu" };
            v.creates = new CreateAssignViewModel() { j08RecordEntity = "o18", j08RecordPid = v.rec_pid };

            if (v.rec_pid > 0)
            {
                v.Rec = Factory.o18DocTypeBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                if (v.Rec.p34ID_Uctenka > 0)
                {
                    v.ComboP34Name = Factory.p34ActivityGroupBL.Load(v.Rec.p34ID_Uctenka).p34Name;
                }
                if (v.Rec.p32ID_Uctenka > 0)
                {
                    v.ComboP32Name = Factory.p32ActivityBL.Load(v.Rec.p32ID_Uctenka).p32Name;
                }
                
                var lis1 = Factory.o18DocTypeBL.GetList_o16(v.rec_pid);
                v.lisO16 = new List<o16Repeater>();
                foreach (var c in lis1)
                {
                    var cc = new o16Repeater()
                    {
                        TempGuid = BO.Code.Bas.GetGuid(),
                        o16IsEntryRequired = c.o16IsEntryRequired,
                        o16Name = c.o16Name,
                        o16NameGrid=c.o16NameGrid,
                        o16Field = c.o16Field,
                        o16Ordinary = c.o16Ordinary,
                        o16DataSource = c.o16DataSource,
                        o16IsFixedDataSource = c.o16IsFixedDataSource,
                        o16IsGridField = c.o16IsGridField,
                        o16TextboxHeight = c.o16TextboxHeight,
                        o16Format = c.o16Format,
                        o16HelpText=c.o16HelpText,
                        o16ReminderNotifyBefore=c.o16ReminderNotifyBefore
                    };
                    v.lisO16.Add(cc);
                }
                var lis2 = Factory.o18DocTypeBL.GetList_o20(v.rec_pid);
                v.lisO20 = new List<o20Repeater>();
                foreach(var c in lis2)
                {
                    var cc = new o20Repeater()
                    {
                        o20ID=c.o20ID,
                        TempGuid = BO.Code.Bas.GetGuid(),
                        o20Name=c.o20Name,
                        o20Ordinary=c.o20Ordinary,
                        o20Entity=c.o20Entity,
                        o20RecTypeEntity=c.o20RecTypeEntity,
                        o20RecTypePid=c.o20RecTypePid,
                        o20IsEntryRequired = c.o20IsEntryRequired,
                        o20IsClosed=c.o20IsClosed,
                        o20IsMultiSelect=c.o20IsMultiSelect,
                        o20EntryModeFlag=c.o20EntryModeFlag,
                        ComboEntity = GetComboEntityTypeName(c.o20Entity)
                    };
                    
                    if (c.o20RecTypePid > 0)
                    {
                        cc.ComboSelectedText = GetComboEntityValueAlias(c.o20RecTypeEntity, c.o20RecTypePid);
                        cc.o20RecTypePid = c.o20RecTypePid;
                    }
                    v.lisO20.Add(cc);
                }
                if (v.Rec.b01ID > 0)
                {
                    v.ComboB01 = Factory.b01WorkflowTemplateBL.Load(v.Rec.b01ID).b01Name;
                }
                if (v.Rec.o17ID > 0)
                {
                    v.ComboO17 = Factory.o17DocMenuBL.Load(v.Rec.o17ID).o17Name;
                }
                if (v.Rec.x38ID > 0)
                {
                    v.ComboX38 = Factory.x38CodeLogicBL.Load(v.Rec.x38ID).x38Name;
                }

                v.creates.SetInitValues(Factory, "o18", v.rec_pid);

            }
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }
            RefreshState(v);
            
            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }
        private void RefreshState(o18Record v)
        {
            

            if (v.lisO16 == null)
            {
                v.lisO16 = new List<o16Repeater>();
            }
            if (v.lisO20 == null)
            {
                v.lisO20 = new List<o20Repeater>();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]        
        public IActionResult Record(o18Record v,string guid)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                switch (v.PostbackOper)
                {
                    case "o18id":
                        break;
                    case "o20_add_row":
                        if (string.IsNullOrEmpty(v.SelectedEntity))
                        {
                            this.AddMessage("Musíte vybrat z nabídky entitu."); return View(v);
                        }
                        var co20 = new o20Repeater() { o20Entity = v.SelectedEntity, TempGuid = BO.Code.Bas.GetGuid(), ComboEntity = GetComboEntityTypeName(v.SelectedEntity) };
                        v.lisO20.Add(co20);
                        return View(v);
                    case "o20_delete_row":
                        v.lisO20.First(p => p.TempGuid == guid).IsTempDeleted = true;
                        return View(v);
                    case "o16_add_row":
                        var co16 = new o16Repeater() { TempGuid = BO.Code.Bas.GetGuid(), o16IsGridField = true };
                        v.lisO16.Add(co16);
                        return View(v);
                    case "o16_delete_row":
                        v.lisO16.First(p => p.TempGuid == guid).IsTempDeleted = true;
                        return View(v);
                    default:
                        return View(v);                        
                }
            }
            
           
            if (ModelState.IsValid)
            {
                BO.o18DocType c = new BO.o18DocType();
                if (v.rec_pid > 0) c = Factory.o18DocTypeBL.Load(v.rec_pid);
                c.o18Name = v.Rec.o18Name;
                c.o18NotepadTab = v.Rec.o18NotepadTab;
                c.o18FilesTab = v.Rec.o18FilesTab;
                c.o18RolesTab = v.Rec.o18RolesTab;
                c.o18TagsTab = v.Rec.o18TagsTab;
                c.o18Ordinary = v.Rec.o18Ordinary;
                c.o18EntryCodeFlag = v.Rec.o18EntryCodeFlag;
                c.o18EntryNameFlag = v.Rec.o18EntryNameFlag;
                c.o18IsSeparatedNotepadTab = v.Rec.o18IsSeparatedNotepadTab;
                c.o18MaxOneFileSize = v.Rec.o18MaxOneFileSize;
                c.o18AllowedFileExtensions = v.Rec.o18AllowedFileExtensions;
                c.o18IsColors = v.Rec.o18IsColors;
                c.o18IsAllowEncryption = v.Rec.o18IsAllowEncryption;
                c.o18ReportCodes = v.Rec.o18ReportCodes;
                c.o18IsAllowTree = v.Rec.o18IsAllowTree;
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
                c.b01ID = v.Rec.b01ID;
                c.o17ID = v.Rec.o17ID;
                c.o18GeoFlag = v.Rec.o18GeoFlag;
                c.o18BarcodeFlag = v.Rec.o18BarcodeFlag;
                c.o18Code = v.Rec.o18Code;
                c.x38ID = v.Rec.x38ID;
                c.o18TemplateFlag = v.Rec.o18TemplateFlag;
                c.p34ID_Uctenka = v.Rec.p34ID_Uctenka;
                c.p32ID_Uctenka = v.Rec.p32ID_Uctenka;

                var lisO16 = new List<BO.o16DocType_FieldSetting>();
                foreach (var row in v.lisO16.Where(p => p.IsTempDeleted == false))
                {
                    var cc = new BO.o16DocType_FieldSetting() { o16Field = row.o16Field,o16IsGridField=row.o16IsGridField,o16Name=row.o16Name,o16NameGrid=row.o16NameGrid,
                        o16Ordinary=row.o16Ordinary,o16IsEntryRequired=row.o16IsEntryRequired,o16DataSource=row.o16DataSource,o16IsFixedDataSource=row.o16IsFixedDataSource,
                        o16TextboxHeight=row.o16TextboxHeight,o16Format=row.o16Format,o16HelpText=row.o16HelpText,o16ReminderNotifyBefore=row.o16ReminderNotifyBefore
                    };
                    lisO16.Add(cc);
                }
                var lisO20 = new List<BO.o20DocTypeEntity>();
                foreach(var row in v.lisO20.Where(p => !p.IsTempDeleted))
                {
                    var cc = new BO.o20DocTypeEntity() {pid=row.o20ID, o20ID=row.o20ID, o20Name = row.o20Name, o20Ordinary = row.o20Ordinary, o20Entity = row.o20Entity, o20RecTypePid =row.o20RecTypePid, o20IsEntryRequired = row.o20IsEntryRequired, o20IsClosed = row.o20IsClosed,o20IsMultiSelect=row.o20IsMultiSelect, o20EntryModeFlag=row.o20EntryModeFlag };
                    if (row.o20RecTypePid > 0 && !(string.IsNullOrEmpty(row.ComboEntity)) && row.ComboEntity.Length>2)
                    {
                        cc.o20RecTypeEntity = row.ComboEntity.Substring(0, 3);
                    }
                    lisO20.Add(cc);
                }
                
                c.pid = Factory.o18DocTypeBL.Save(c,lisO20,lisO16, v.roles.getList4Save(Factory), v.creates.getList4Save(Factory));
                if (c.pid > 0)
                {

                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }


        private string GetComboEntityTypeName(string entity)
        {
            switch (entity)
            {
                case "p41":
                case "le5":
                case "le4":
                case "le3":
                case "le2":
                case "le1":
                    return "p42ProjectType";
                case "p28":
                    return "p29ContactType";
                case "j02":
                    return "j07PersonPosition";
                case "p91":
                    return "p92InvoiceType";
                case "p90":
                    return "p89ProformaType";
                case "p31":
                    return "p34ActivityGroup";
                
                default:
                    return null;
            }
        }
        private string GetComboEntityValueAlias(string strTypeEntity,int intTypePID)
        {
            if (intTypePID == 0) return null;
            switch (strTypeEntity)
            {
                case "p42":
                    return Factory.p42ProjectTypeBL.Load(intTypePID).p42Name;
                case "p29":
                    return Factory.p29ContactTypeBL.Load(intTypePID).p29Name;
                case "j07":
                    return Factory.j07PersonPositionBL.Load(intTypePID).j07Name;
                case "p92":
                    //return Factory.p92InvoiceTypeBL.Load(intTypePID).p92Name;
                    return "nutno dodělat";
                case "p89":
                    //return Factory.p89ProformaTypeBL.Load(intTypePID).p89Name;
                    return "nutno dodělat";
                case "p34":
                    return Factory.p34ActivityGroupBL.Load(intTypePID).p34Name;
              
                default:
                    return null;
            }
        }
    }
}
