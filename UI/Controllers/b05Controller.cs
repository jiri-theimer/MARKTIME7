using Microsoft.AspNetCore.Mvc;
using UI.Models.wrk;
using UI.Models;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class b05Controller : BaseController
    {
        public IActionResult Tab1(int pid, string caller)
        {
            var v = new b05Tab1() { Factory = this.Factory, prefix = "b05", pid = pid, caller = caller };

            RefreshStateTab1(v);
            return View(v);
        }
        public IActionResult Dashboard(int pid)
        {
            var v = new b05Tab1() { Factory = this.Factory, prefix = "b05", pid = pid, caller = "dashboard" };

            RefreshStateTab1(v);
            return View(v);
        }
        private void RefreshStateTab1(b05Tab1 v)
        {
            var lis = Factory.WorkflowBL.GetList_b05(null, 0, 0, v.pid,0);
            if (lis.Count() == 0) return;
            v.Rec = lis.First();
            v.lisO27 = Factory.o27AttachmentBL.GetList(new BO.myQueryO27() { entity = "b05", recpid = v.pid });

            if (v.Rec.b05RecordEntity == "p41")
            {
                v.RecP41 = Factory.p41ProjectBL.Load(v.Rec.b05RecordPid);
            }
            if (v.Rec.b05RecordEntity == "p28")
            {
                v.RecP28 = Factory.p28ContactBL.Load(v.Rec.b05RecordPid);
            }
            if (v.Rec.b05RecordEntity == "o23")
            {
                v.RecO23 = Factory.o23DocBL.Load(v.Rec.b05RecordPid);
            }

        }
        public IActionResult Record(int pid,int portalflag,int o43id_source,int recpid,string recprefix,int tab1flag)
        {
            var v = new b05Record() { b05ID = pid,o43ID_Source=o43id_source,RecPrefix=recprefix,RecPid=recpid };                       

            if (v.b05ID == 0 && v.RecPrefix==null)
            {
                v.RecPrefix = "j02";
            }
            
            
            RefreshState(v);
            if (v.RecB05.pid == 0)
            {
                v.RecB05.b05Tab1Flag = tab1flag;
                if (v.RecPrefix != null)
                {
                    v.RecAlias = Factory.CBL.GetObjectAlias(v.RecPrefix, v.RecPid);
                    if (v.RecPrefix == "p41")
                    {                        
                        v.ProjectCombo.SelectedProject = v.RecAlias;
                        v.ProjectCombo.SelectedP41ID = v.RecPid;
                    }
                }
                
            }
            if (portalflag ==1)
            {
                v.IsPortalAccess = true;
            }
            if (v.b05ID>0 && !v.RecB05.b05IsManualStep)
            {
                return this.StopPage(true, "Záznam nelze upravovat.");
            }
            if (v.b05ID>0 && (!Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_b05OwnerAll) && v.RecB05.j02ID_Sys !=Factory.CurrentUser.pid))
            {
                return this.StopPage(true, "Nemáte oprávnění upravovat tento záznam.");
            }
            v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "b05", SelectedX04ID = v.RecB05.x04ID, HtmlContent = v.RecB05.b05Notepad };
            if (v.b05ID > 0)
            {
                var lisO27 = Factory.o27AttachmentBL.GetList(new BO.myQueryO27() { entity = "b05", recpid = v.b05ID });
                if (lisO27.Where(p => p.o27CallerFlag == BO.o27CallerFlagENUM._None).Count() > 0)
                {
                    v.UploadGuid = BO.Code.Bas.GetGuid();
                }
                
                
            }
            if (v.o43ID_Source > 0)
            {
                v.UploadGuid = BO.Code.Bas.GetGuid();
                var recO43 = Factory.o43InboxBL.Load(v.o43ID_Source);
                v.RecB05.b05Name = recO43.o43Subject;
                if (recO43.o43IsBodyHtml)
                {
                    v.Notepad.HtmlContent = recO43.o43BodyHtml;
                }
                else
                {
                    v.Notepad.HtmlContent = recO43.o43BodyText;
                }
                v.RecB05.b05Date = recO43.o43DateMessage;

                var files = Factory.o43InboxBL.GetInboxFiles(recO43.pid, true).Where(p => p.Key != "eml" && p.Key != "msg");
                foreach (BO.StringPair sp in files)
                {
                    System.IO.File.Copy(sp.Value, $"{Factory.TempFolder}\\{sp.Key}", true);
                    Factory.o27AttachmentBL.CreateTempInfoxFile(v.UploadGuid, "b05", sp.Key, sp.Key, BO.Code.File.GetContentType(sp.Value));

                }
            }
            v.b05Date = v.RecB05.b05Date; v.b05Name = v.RecB05.b05Name;v.IsPortalAccess = (v.RecB05.b05PortalFlag == 1 ? true : false);

           

            v.IsTab1 = BO.Code.Bas.bit_compare_or(v.RecB05.b05Tab1Flag, 2);
            v.IsBillingMemo = BO.Code.Bas.bit_compare_or(v.RecB05.b05Tab1Flag, 4);
            return View(v);
        }
        private void RefreshState(b05Record v)
        {
            if (v.b05ID > 0)
            {
                v.RecB05 = Factory.WorkflowBL.GetList_b05(null, 0, 0, v.b05ID,0).First();
                v.RecPrefix = v.RecB05.b05RecordEntity;
                v.RecPid = v.RecB05.b05RecordPid;
            }
            else
            {
                v.RecB05 = new BO.b05Workflow_History() { x04ID = Factory.Lic.x04ID_Default, b05IsManualStep=true,b05IsCommentOnly=true };
            }
            if (v.ProjectCombo == null)
            {
                v.ProjectCombo = new ProjectComboViewModel();
            }
            if (v.ProjectCombo != null)
            {
                v.ProjectCombo.IsHideLabel = true;
                
            }
            

            if (v.reminder == null)
            {
                v.reminder = new ReminderViewModel() { is_static_date = true, record_pid = v.b05ID, record_prefix = "b05" };
            }
            if (v.b05ID > 0)
            {
                v.lisO27_CanDelete = Factory.o27AttachmentBL.GetList(new BO.myQueryO27() { entity = "b05", recpid = v.b05ID }).Where(p => p.o27CallerFlag == BO.o27CallerFlagENUM.Notepad);
            }

          
            
        }
        [HttpPost]
        public IActionResult Record(b05Record v,string o27id)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                
                if (v.PostbackOper== "delete-o27id" && BO.Code.Bas.InInt(o27id)>0)
                {
                    Factory.CBL.DeleteRecord("o27", BO.Code.Bas.InInt(o27id));
                    RefreshState(v);
                }
                if (v.PostbackOper == "delete")
                {
                    Factory.CBL.DeleteRecord("b05", v.b05ID);
                    v.SetJavascript_CallOnLoad(v.b05ID);
                }
                if (v.PostbackOper == "prefix")
                {
                    v.RecAlias = null;
                    v.RecPid = 0;
                }
               
                if (v.PostbackOper == "mail")
                {
                    if (SaveChanges(v))
                    {
                        return RedirectToAction("SendMail", "Mail", new { record_entity = v.RecB05.b05RecordEntity, record_pid = v.RecB05.b05RecordPid, b05id = v.b05ID });
                    }

                }
                return View(v);
            }
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(v.b05Name) && (string.IsNullOrEmpty(v.Notepad.HtmlContent) || v.Notepad.HtmlContent.Length < 10))
                {
                    this.AddMessage("Chybí vyplnit [Notepad] nebo [Název]."); return View(v);
                }
                if (SaveChanges(v))
                {
                    v.SetJavascript_CallOnLoad(v.b05ID);
                }

            }

            return View(v);
        }

        private bool SaveChanges(b05Record v)
        {
            
            if (v.b05ID == 0)
            {
                v.RecB05.b05RecordEntity = v.RecPrefix;
                if (v.RecPrefix == "j02")
                {
                    v.RecB05.b05RecordPid = Factory.CurrentUser.pid;
                }
                else
                {
                    if (v.RecPrefix == "p41")
                    {
                        v.RecB05.b05RecordPid = v.ProjectCombo.SelectedP41ID;
                    }
                    else
                    {
                        v.RecB05.b05RecordPid = v.RecPid;
                    }                    
                }
                if (v.RecB05.b05RecordPid == 0)
                {
                    this.AddMessage("Chybí vyplnit záznam vazby.");
                    return false;
                }
                if (v.o43ID_Source>0 && string.IsNullOrEmpty(v.b05Name))
                {
                    this.AddMessage("Notepad zakládaný z INBOX záznamu má povinný název.");
                    return false;
                }

                v.b05ID=Factory.WorkflowBL.Save2History(v.RecB05);
                if (v.o43ID_Source > 0)
                {
                    var recO43 = Factory.o43InboxBL.Load(v.o43ID_Source);
                    recO43.b05ID = v.b05ID;
                    Factory.o43InboxBL.Save(recO43);
                }
            }
            int intTab1Flag = 0;
            if (v.IsTab1) intTab1Flag += 2;
            if (v.IsBillingMemo) intTab1Flag += 4;

            if (Factory.WorkflowBL.SaveChangesInNotepad(v.b05ID, v.Notepad.HtmlContent, v.Notepad.SelectedX04ID, v.b05Date, v.b05Name,(v.IsPortalAccess==true? 1: 0), intTab1Flag))
            {
                Factory.o27AttachmentBL.CommitNotepdChanges(v.Notepad.TempGuid, "b05", v.b05ID);

                if (v.reminder != null)
                {
                    v.reminder.SaveChanges(Factory, v.b05ID);
                }

                if (v.UploadGuid != null)
                {
                    Factory.o27AttachmentBL.SaveChangesAndUpload(v.UploadGuid, "b05", v.b05ID);
                }

                return true;
            }
            return false;
        }
    }
}
