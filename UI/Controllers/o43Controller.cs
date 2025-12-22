using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using UI.Models;
using UI.Models.Imap;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class o43Controller : BaseController
    {
        private readonly BL.Singleton.ThePeriodProvider _pp;
        public o43Controller(BL.Singleton.ThePeriodProvider pp)
        {
            _pp = pp;
        }
        public IActionResult Info(int pid)
        {
            return Tab1(pid, "info");
        }

        public IActionResult Tab1(int pid, string caller)
        {
            var v = new o43Tab1() { Factory = this.Factory, prefix = "o43", pid = pid, caller = caller };

            RefreshStateTab1(v);
            return View(v);
        }
        private void RefreshStateTab1(o43Tab1 v)
        {
            v.Rec = Factory.o43InboxBL.Load(v.pid);
            if (v.Rec != null)
            {

                v.SetTagging();
            }
        }

        public IActionResult MyInbox()
        {
            var v = new MyInboxViewModel() {SelectedFolder="Inbox"};
            

            v.SelectedJ40ID = Factory.CBL.LoadUserParamInt("MyInbox-j40ID");
            v.SelectedFolder = Factory.CBL.LoadUserParam("MyInbox-Folder","Inbox");
            v.TakeLastTop = Factory.CBL.LoadUserParamInt("MyInbox-TakeLastTop",10);
            v.TakeSeenFlag = Factory.CBL.LoadUserParam("MyInbox-TakeSeenFlag","all");
            v.TakeFlaggedFlag = Factory.CBL.LoadUserParam("MyInbox-TakeFlaggedFlag","all");
            v.NahazovatZpravamPriznak = Factory.CBL.LoadUserParam("MyInbox-NahazovatZpravamPriznak","Deleted");
            v.MaVazbu = Factory.CBL.LoadUserParamInt("MyInbox-MaVazbu");
            

            RefreshStateMyInbox(v);
            
            if (v.lisJ40.Count() == 0)
            {
                return this.StopPage(false, "Zatím nemáte nastavený IMAP poštovní účet. Na stránce [Můj profil] nebo [Administrace] je třeba nastavit poštovní účet pro stahování zpráv do MARKTIME.");
            }

            return ViewTup(v, BO.PermValEnum.GR_MyInbox);
        }

        private void RefreshStateMyInbox(MyInboxViewModel v)
        {
            v.lisJ40 = Factory.j40MailAccountBL.GetList(new BO.myQueryJ40() { isimap=true });
            
            if (v.SelectedJ40ID>0 && !v.lisJ40.Any(p => p.pid == v.SelectedJ40ID))
            {
                v.SelectedJ40ID = 0;
            }
            if (v.SelectedJ40ID == 0 && v.lisJ40.Count() > 0) v.SelectedJ40ID = v.lisJ40.First().pid;
            if (v.SelectedJ40ID > 0)
            {
                v.RecJ40 = Factory.j40MailAccountBL.Load(v.SelectedJ40ID);                
                v.Folders = BO.Code.Bas.ConvertString2List(v.RecJ40.ReadyImapFolders);
            }

            v.periodinput = new Views.Shared.Components.myPeriod.myPeriodViewModel() { UserParamKey = "imap-myinfolist-period" };
            v.periodinput.LoadUserSetting(_pp, Factory);

            v.recordbinquery = new RecordBinQueryViewModel() { UserParamKey = "grid-o43-recordbinquery" };
            v.recordbinquery.Value= Factory.CBL.LoadUserParamInt(v.recordbinquery.UserParamKey, 1);

            var strMyQuery = $"j02ID_Owner|int|{Factory.CurrentUser.pid}|j40ID|int|{v.SelectedJ40ID}";
            if (v.MaVazbu == 1)
            {
                strMyQuery += "|mavazbu|bool|false";
            }
            if (v.MaVazbu == 2)
            {
                strMyQuery += "|mavazbu|bool|true";
            }
            v.gridinput = new Views.Shared.Components.myGrid.myGridInput() { entity = "o43Inbox", myqueryinline = strMyQuery };
            v.gridinput.j72id = Factory.CBL.LoadUserParamInt("MyInbox/j72id");
            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load("o43", null, 0, strMyQuery);
            v.gridinput.query.global_d1 = v.periodinput.d1;
            v.gridinput.query.global_d2 = v.periodinput.d2;
            v.gridinput.query.period_field = v.periodinput.PeriodField;
            switch (v.recordbinquery.Value)
            {
                case 1:
                    v.gridinput.query.IsRecordValid = true; break;
                case 2:
                    v.gridinput.query.IsRecordValid = false; break;
                default:
                    v.gridinput.query.IsRecordValid = null; break;
            }

        }

        [HttpPost]
        public IActionResult MyInbox(MyInboxViewModel v)
        {
            RefreshStateMyInbox(v);
            if (v.PostbackOper == "j40id")
            {
                Factory.CBL.SetUserParam("MyInbox-j40ID", v.SelectedJ40ID.ToString());
            }
            if (v.PostbackOper == "vazba")
            {
                Factory.CBL.SetUserParam("MyInbox-MaVazbu", v.MaVazbu.ToString());
            }
            if (v.PostbackOper == "load")
            {
                //var lis=Factory.o43InboxBL.InhaleMessageList(v.SelectedJ40ID, DateTime.Today.AddDays(-10), DateTime.Today.AddDays(1), 10,v.SelectedFolder);
                //Factory.o43InboxBL.SaveMessages2Inbox(v.SelectedJ40ID,v.SelectedFolder, lis);
                Factory.o43InboxBL.TryImportMessages(v.SelectedJ40ID, v.TakeLastTop, v.SelectedFolder, v.TakeSeenFlag, v.TakeFlaggedFlag,v.NahazovatZpravamPriznak);

                var folders = Factory.o43InboxBL.GetFolderList(v.SelectedJ40ID);
                Factory.j40MailAccountBL.UpdateFolders(v.SelectedJ40ID, string.Join(",", folders));

                Factory.CBL.SetUserParam("MyInbox-j40ID", v.SelectedJ40ID.ToString());
                Factory.CBL.SetUserParam("MyInbox-TakeLastTop", v.TakeLastTop.ToString());
                Factory.CBL.SetUserParam("MyInbox-TakeSeenFlag", v.TakeSeenFlag);
                Factory.CBL.SetUserParam("MyInbox-TakeFlaggedFlag", v.TakeFlaggedFlag);
                Factory.CBL.SetUserParam("MyInbox-NahazovatZpravamPriznak", v.NahazovatZpravamPriznak);
                Factory.CBL.SetUserParam("MyInbox-Folder", v.SelectedFolder);
                
                RefreshStateMyInbox(v);

                
            }
            
           
          
            return View(v);
        }

        public IActionResult BodyHtml(int pid)
        {
            var v = new InboxRecordViewModel() { rec_pid = pid, rec_entity = "o43" };
            v.Rec = Factory.o43InboxBL.Load(v.rec_pid);
            return View(v);
        }
        public IActionResult Record(int pid)
        {
            var v = new InboxRecordViewModel() { rec_pid = pid, rec_entity = "o43",CurrentBody="html" };
            
            v.Rec = Factory.o43InboxBL.Load(v.rec_pid);
            if (!Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_o43_Owner))
            {
                if (v.Rec.j02ID_Owner != Factory.CurrentUser.pid)
                {
                    return this.StopPage(true, "Nemáte oprávnění upravovat tento Inbox záznam.");
                }                
            }

            RefreshStateRecord(v);            
            if (v.Rec == null)
            {
                return RecNotFound(v);
            }
           
            if (v.RecPreview.o43BodyHtml == null)
            {
                v.CurrentBody = "text";
            }

            if (v.Rec.p41ID > 0)
            {
                v.ProjectCombo.SelectedP41ID = v.Rec.p41ID;
                v.ProjectCombo.SelectedProject = Factory.CBL.GetObjectAlias("p41", v.Rec.p41ID);
            }
            if (v.Rec.p28ID > 0)
            {
                v.ComboP28 = v.Rec.p28Name;
            }
            if (v.Rec.p91ID > 0)
            {
                v.ComboP91 = Factory.p91InvoiceBL.Load(v.Rec.p91ID).p91Code;
            }
            if (v.Rec.j02ID > 0)
            {
                v.ComboJ02 = Factory.j02UserBL.Load(v.Rec.j02ID).FullnameDesc;            
            }
            if (v.Rec.p56ID > 0)
            {
                v.ComboP56 = Factory.p56TaskBL.Load(v.Rec.p56ID).FullName;
            }
            if (v.Rec.o23ID > 0)
            {
                v.ComboO23 = Factory.o23DocBL.Load(v.Rec.o23ID).o23Name;
            }


            v.Toolbar = new MyToolbarViewModel(v.Rec) { AllowClone = false };
           
            return View(v);
        }
        private void RefreshStateRecord(InboxRecordViewModel v)
        {
            v.RecPreview = Factory.o43InboxBL.Load(v.rec_pid);
            v.RecPreview.j02ID = 0;v.RecPreview.p41ID = 0;v.RecPreview.p56ID = 0;v.RecPreview.p28ID = 0;v.RecPreview.o23ID = 0;v.RecPreview.p91ID = 0;
            if (v.ProjectCombo == null)
            {
                v.ProjectCombo = new ProjectComboViewModel() { SelectedP41ID = v.Rec.p41ID };
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(InboxRecordViewModel v)
        {
            RefreshStateRecord(v);
            if (v.IsPostback)
            {
               
                if (v.PostbackOper == "p56id" && v.Rec.p56ID > 0)
                {
                    v.ComboP56 = Factory.p56TaskBL.Load(v.Rec.p56ID).FullName;
                }
                if (v.PostbackOper == "text" || v.PostbackOper == "html")
                {
                    v.CurrentBody = v.PostbackOper;
                    return View(v);
                }
                //if (v.PostbackOper == "msg" || v.PostbackOper == "eml")
                //{
                //    return DownloadAsFile(v.rec_pid, v.PostbackOper);


                //}
                if (v.PostbackOper == "load")
                {
                    if (Factory.o43InboxBL.TryReloadOneMessage(v.rec_pid,null))
                    {
                        v.SetJavascript_CallOnLoad(v.rec_pid);
                        return View(v);
                    }

                    return View(v);
                }

                return View(v);
            }
            

            if (ModelState.IsValid)
            {
                BO.o43Inbox c = Factory.o43InboxBL.Load(v.rec_pid);

                c.o43Subject = v.Rec.o43Subject;
                c.j02ID = v.Rec.j02ID;
                c.p41ID = v.ProjectCombo.SelectedP41ID;
                c.p28ID = v.Rec.p28ID;
                c.p91ID = v.Rec.p91ID;
                c.p56ID = v.Rec.p56ID;
                c.o23ID = v.Rec.o23ID;
                
                c.o43ImapFolder = v.Rec.o43ImapFolder;
                if (string.IsNullOrEmpty(c.o43ImapFolder)) c.o43ImapFolder = "Inbox";

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
               
                c.pid = Factory.o43InboxBL.Save(c);
                if (c.pid > 0)
                {
                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }

            this.Notify_RecNotSaved();
            return View(v);

        }

        public IActionResult DownloadAsFile(int pid,string format)
        {
            format = format.Trim();
            var files = Factory.o43InboxBL.GetInboxFiles(pid, true);

            if (files.Any(p => p.Key == format))
            {
                return RedirectToAction("FileDownloadInbox", "FileUpload", new { pid = pid, format = format });
            }
            //var rec = Factory.o43InboxBL.Load(pid);
            //if (rec.o43ArchiveFolder == null) rec.o43ArchiveFolder = $"{rec.o43DateMessage.Value.Year}\\{rec.o43DateMessage.Value.Month}"; 
            //string strFolder = $"{Factory.UploadFolder}\\{rec.o43ArchiveFolder}";
            
            //if ((format == "eml" || format == "msg") && System.IO.File.Exists($"{strFolder}\\{rec.o43MessageID}.{format}"))
            //{                
            //    return RedirectToAction("FileDownloadInbox", "FileUpload", new { pid = pid,format=format });
            //}
            
            //if (System.IO.File.Exists($"{strFolder}\\{rec.o43MessageID}-{format}"))
            //{                
            //    return RedirectToAction("FileDownloadInbox", "FileUpload", new { pid = pid, format = format });
            //}

            //var ret = Factory.o43InboxBL.TryImportAsFile(pid, "eml");
            //if (ret.Flag == BO.ResultEnum.Success)
            //{
            //    return RedirectToAction("FileDownloadInbox", "FileUpload", new { pid = pid, format = format });

            //}

            return RedirectToAction("FileDownloadNotFound", "FileUpload");
        }
    }



}

