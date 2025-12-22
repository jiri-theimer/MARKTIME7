
using BL;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using UI.Models;
using UI.Models.capacity;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class j02Controller : BaseController
    {
        public IActionResult PermSimulation(int j02id, int p34id)
        {
            var v = new j02PermSimulation() { j02ID = j02id, SelectedP34ID = p34id };

            RefreshState_PermSimulation(v);
            if (v.j02ID == 0)
            {
                return this.StopPage(true, "Na vstupu chybí id uživatele");
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }
        private void RefreshState_PermSimulation(j02PermSimulation v)
        {
            v.ProjectCombo = new ProjectComboViewModel() { MyQueryInline = "" };
            v.ProjectCombo.MyQueryInline = $"j02id_query|int|{v.j02ID}";

            if (v.SelectedP34ID > 0)
            {
                var recP34 = Factory.p34ActivityGroupBL.Load(v.SelectedP34ID);
                v.SelectedP34Name = recP34.p34Name;
                v.ProjectCombo.MyQueryInline = $"{v.ProjectCombo.MyQueryInline}|p34id_for_p31_entry|int|{v.SelectedP34ID}|p33id_for_p31_entry|int|{(int)recP34.p33ID}|p34incomestatementflag_for_p31_entry|int|{(int)recP34.p34IncomeStatementFlag}";

            }
        }
        [HttpPost]
        public IActionResult PermSimulation(j02PermSimulation v)
        {
            RefreshState_PermSimulation(v);
            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {



            }



            return View(v);
        }

        public IActionResult Tab1Setting(string prefix)
        {
            var v = new BaseTab1ViewModel() { prefix = prefix };
            v.Tab1BitStream = Factory.j02UserBL.LoadBitstreamFromUserCache($"t{v.prefix.Substring(1, 2)}", 0);

            RefreshState_Tab1Setting(v);

            v.SelectedBoxes = new List<int>();
            foreach (var c in v.lisAllBoxes)
            {
                if (v.Tab1BitStream == 0 || BO.Code.Bas.bit_compare_or(v.Tab1BitStream, Convert.ToInt32(c.Key)))
                {
                    v.SelectedBoxes.Add(Convert.ToInt32(c.Key));
                }
            }

            return View(v);
        }

        private void RefreshState_Tab1Setting(BaseTab1ViewModel v)
        {
            v.ab(Tab1BoxEnum.recheader, Factory.tra("Hlavička záznamu"));

            v.ab(Tab1BoxEnum.tags, Factory.tra("Štítky"));
            if (v.prefix == "p91")
            {
                v.ab(Tab1BoxEnum.quickstat, Factory.tra("Rychlá statistika"));
            }



            switch (BO.Code.Entity.GetPrefixDb(v.prefix))
            {
                case "p28":
                    v.ab(Tab1BoxEnum.navigor, Factory.tra("Strom navigátor"));

                    v.ab(Tab1BoxEnum.p28billingbox, Factory.tra("Výsledovka a fakturační nastavení"));
                    break;
                case "p41":
                    v.ab(Tab1BoxEnum.navigor, Factory.tra("Strom navigátor"));
                    v.ab(Tab1BoxEnum.p41billingbox, Factory.tra("Výsledovka, rozpočet a fakturační nastavení"));
                    break;
                case "p56":
                    v.ab(Tab1BoxEnum.navigor, Factory.tra("Strom navigátor"));
                    break;
                case "p91":
                    v.ab(Tab1BoxEnum.p91clientbox, Factory.tra("Strom navigátor"));
                    break;
                case "p90":
                    v.ab(Tab1BoxEnum.p82list, Factory.tra("Úhrady a jejich párování s daňovou fakturou"));
                    break;
            }

            v.ab(Tab1BoxEnum.o27list, Factory.tra("Seznam nahraných příloh"));


        }

        [HttpPost]
        public IActionResult Tab1Setting(BaseTab1ViewModel v,bool? quickstat)
        {
            RefreshState_Tab1Setting(v);
            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {
                int x = v.SelectedBoxes.Sum(p => p);
                if (x == 0)
                {
                    this.AddMessage("Musíte zaškrtnout minimálně jeden box.");
                    return View(v);
                }
                Factory.j02UserBL.SaveBitStreamUserCache($"t{v.prefix.Substring(1, 2)}", x, 0, 0);

                if (quickstat != null)
                {
                    Factory.CBL.SetUserParam($"{v.prefix}-quickstat-immediately", quickstat.ToString());
                }

                v.SetJavascript_CallOnLoad(v.Tab1BitStream);
                return View(v);

            }


            this.Notify_RecNotSaved();
            return View(v);
        }
        public IActionResult Info(int pid)
        {
            return Tab1(pid, "info");
        }
        public IActionResult Tab1(int pid, string caller)
        {
            var v = new j02Tab1() { Factory = this.Factory, prefix = "j02", pid = pid, caller = caller };

            v.Rec = Factory.j02UserBL.Load(v.pid);
            if (v.Rec != null)
            {
                //v.RecSum = Factory.j02UserBL.LoadSumRow(v.Rec.pid);

                v.SetTagging();

                v.SetFreeFields(0);
            }

            return View(v);
        }

        public IActionResult TabR01(string master_entity, int master_pid, string caller)
        {
            if (master_pid == 0) return this.StopPageSubform("master_pid missing");

            var v = new j02TabR01ViewModel() { j02id = master_pid, timeline = new CapacityTimelineP41ViewModel() };
            v.timeline.j02ID = v.j02id;
            v.timeline.UserKeyBase = "TabR01";


            v.HasOwnerPermissions = true;
            v.timeline.IsReadOnly = !v.HasOwnerPermissions;

            //v.lisR04 = Factory.p41ProjectBL.GetList_r04(v.p41id, 0);

            if (caller == "grid")
            {
                Factory.CBL.SaveLastCallingRecPid(master_entity, master_pid, "grid", true, false, null);
            }

            v.RecJ02 = Factory.j02UserBL.Load(v.j02id);
            v.timeline.RecJ02 = v.RecJ02;


            return View(v);
        }

        public IActionResult VirtualRecord(int pid, int parent_pid)
        {
            var v = new j02VirtualUserVIewModel() { j02VirtualParentID = parent_pid, pid = pid };

            RefreshState_VirtualRecord(v);

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }
        private void RefreshState_VirtualRecord(j02VirtualUserVIewModel v)
        {
            if (v.pid > 0)
            {
                v.Rec = Factory.j02UserBL.Load(v.pid);
                v.j02VirtualParentID = v.Rec.j02VirtualParentID;
            }

            v.RecParent = Factory.j02UserBL.Load(v.j02VirtualParentID);


        }
        [HttpPost]
        public IActionResult VirtualRecord(j02VirtualUserVIewModel v)
        {
            RefreshState_VirtualRecord(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper == "delete")
                {
                    if (Factory.CBL.DeleteRecord("j02", v.pid) == "1")
                    {
                        v.SetJavascript_CallOnLoad(v.pid);
                        return View(v);
                    }
                }
                if (v.PostbackOper == "archive")
                {
                    if (Factory.CBL.Archive("j02", v.pid))
                    {
                        v.SetJavascript_CallOnLoad(v.pid);
                        return View(v);
                    }
                }
                if (v.PostbackOper == "restore")
                {
                    if (Factory.CBL.Restore("j02", v.pid))
                    {
                        v.SetJavascript_CallOnLoad(v.pid);
                        return View(v);
                    }
                }
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (v.pid == 0)
                {
                    if (Factory.j02UserBL.CreateVirtualRecord(v.j02VirtualParentID))
                    {
                        v.pid = Factory.j02UserBL.LoadVirtualUser(v.j02VirtualParentID).pid;
                    }

                }
                v.SetJavascript_CallOnLoad(v.pid);
                return View(v);


            }

            Notify_RecNotSaved();
            return View(v);
        }
        public IActionResult Record(int pid, bool isclone, int j04id)
        {
            var v = new j02Record() { rec_pid = pid, rec_entity = "j02", UploadGuid = BO.Code.Bas.GetGuid(), UploadGuidSignature = BO.Code.Bas.GetGuid(), Element2Focus = "Rec_j02FirstName" };

            v.disp = new DispoziceViewModel();
            v.disp.InitItems("j02", Factory);

            if (v.rec_pid == 0)
            {
                v.IsDefinePassword = true;
                v.NewPassword = new BL.Code.PasswordSupport().GetRandomPassword();
                v.VerifyPassword = v.NewPassword;
            }


            v.Rec = new BO.j02User() { j02LangIndex = Factory.Lic.x01LangIndex, j04ID = j04id };
            if (Factory.Lic.x01LoginDomain != null)
            {
                v.Rec.j02Login = "@" + Factory.Lic.x01LoginDomain;
            }

            if (v.rec_pid == 0 && v.Rec.j04ID > 0)
            {
                v.RecJ04 = Factory.j04UserRoleBL.Load(v.Rec.j04ID);
                v.ComboJ04Name = v.RecJ04.j04Name;
            }

            if (v.rec_pid > 0)
            {
                v.Rec = Factory.j02UserBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                if (v.Rec.j02VirtualParentID > 0)
                {
                    return RedirectToAction($"VirtualRecord", new { pid = v.rec_pid });
                }
                v.RecJ04 = Factory.j04UserRoleBL.Load(v.Rec.j04ID);
                v.SetTagging(Factory.o51TagBL.GetTagging("j02", v.rec_pid));
                v.ComboC21Name = v.Rec.c21Name;
                v.ComboJ07Name = v.Rec.j07Name;
                v.ComboJ04Name = v.Rec.j04Name;
                v.ComboJ18Name = v.Rec.j18Name;


                InhaleDisp(v, v.Rec.j02BitStream);


                if (v.Rec.j02InvoiceSignatureFile != null)
                {
                    v.SignatureInvoiceFile = "/_users/" + Factory.Lic.x01Guid + "/PLUGINS/" + v.Rec.j02InvoiceSignatureFile;
                }



            }


            RefreshState_Record(v);




            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
                v.IsChangeLogin = true;
                v.IsDefinePassword = true;
                v.NewPassword = new BL.Code.PasswordSupport().GetRandomPassword();
                v.VerifyPassword = v.NewPassword;
            }
            if (!Factory.CurrentUser.IsAdmin)
            {
                v.Toolbar.AllowDelete = false;
                v.Toolbar.AllowClone = false;
            }
            if (v.rec_pid !=0 && !Factory.CurrentUser.IsAdmin && Factory.CurrentUser.IsMasterPerson)
            {
                //ověřit, zda má oprávnění podle nadřízeného uživatele
                var lisSlaves = Factory.j05MasterSlaveBL.GetList_Slaves_J02IDs_j02Edit(Factory.CurrentUser.pid);
                if (lisSlaves.Where(p => p == v.rec_pid).Count() > 0)
                {
                    return View(v); //nadřízený uživatel má oprávnění k editaci profilu
                }
                
            }
            return ViewTup(v, BO.PermValEnum.GR_Admin);

        }

        private void RefreshState_Record(j02Record v)
        {

            if (v.Rec.j04ID > 0 && v.RecJ04 == null)
            {
                v.RecJ04 = Factory.j04UserRoleBL.Load(v.Rec.j04ID);
            }
            if (!v.disp.IsInhaled)
            {
                InhaleDisp(v, 0);
            }



            v.lisAutocomplete = Factory.o15AutoCompleteBL.GetList(new BO.myQuery("o15"));
            if (v.ff1 == null)
            {
                v.ff1 = new FreeFieldsViewModel();
                v.ff1.InhaleFreeFieldsView(Factory, v.rec_pid, "j02");
            }
            v.ff1.RefreshInputsVisibility(Factory, v.rec_pid, "j02", v.Rec.j07ID);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(j02Record v, string guid)
        {
            RefreshState_Record(v);

            if (v.IsPostback)
            {
                switch (v.PostbackOper)
                {

                    case "j04id":
                        if (v.Rec.j04ID > 0)
                        {
                            v.RecJ04 = Factory.j04UserRoleBL.Load(v.Rec.j04ID);
                            InhaleDisp(v, v.disp.GetBitStream());
                            if (v.rec_pid == 0) v.disp.RecoveryDefaultCheckedStates();

                        }
                        break;
                    case "newpwd":
                        v.IsDefinePassword = true;
                        v.NewPassword = new BL.Code.PasswordSupport().GetRandomPassword();
                        v.VerifyPassword = v.NewPassword;
                        v.Element2Focus = "NewPassword";
                        this.AddMessage("Změnu hesla je třeba potvrdit tlačítkem [Uložit změny].", "info");
                        break;
                    case "changelogin":
                        v.IsDefinePassword = true;
                        v.IsChangeLogin = true;
                        if (Factory.Lic.x01LoginDomain != null)
                        {
                            v.Rec.j02Login = "@" + Factory.Lic.x01LoginDomain;
                        }

                        v.NewPassword = new BL.Code.PasswordSupport().GetRandomPassword();
                        v.VerifyPassword = v.NewPassword;
                        v.Element2Focus = "Rec_j02Login";
                        this.AddMessage("Se změnou přihlašovacího jména je třeba resetovat i přístupové heslo.", "info");
                        //this.AddMessage("Změnu přihlašovacího jména a hesla je třeba potvrdit tlačítkem [Uložit změny].", "info");
                        break;
                    case "unblock":
                        Factory.j02UserBL.UpdateAccessFailedCount(v.rec_pid, 0);
                        v.SetJavascript_CallOnLoad(v.rec_pid);  //zavřít okno

                        break;
                    case "delete_signature":
                        v.UploadGuidSignature = BO.Code.Bas.GetGuid();
                        v.IsDeleteSignature = true;
                        this.AddMessage("Změny se potvrdí až po uložení záznamu.", "info");
                        break;

                }




                return View(v);
            }


            if (ModelState.IsValid)
            {
                if (v.IsDefinePassword && !ValidateUserPassword(v))
                {
                    return View(v);

                }

                BO.j02User c = new BO.j02User();
                if (v.rec_pid > 0) c = Factory.j02UserBL.Load(v.rec_pid);
                if (v.IsChangeLogin || v.rec_pid == 0)
                {
                    c.j02Login = v.Rec.j02Login;
                }
                c.j04ID = v.Rec.j04ID;
                c.j07ID = v.Rec.j07ID;
                c.c21ID = v.Rec.c21ID;
                c.j18ID = v.Rec.j18ID;
                c.j02LangIndex = v.Rec.j02LangIndex;

                c.j02IsMustChangePassword = v.Rec.j02IsMustChangePassword;
                c.j02TwoFactorVerifyFlag = v.Rec.j02TwoFactorVerifyFlag;

                c.j02InvoiceSignatureText = v.Rec.j02InvoiceSignatureText;
                if (v.IsDeleteSignature)
                {
                    c.j02InvoiceSignatureFile = null;
                }
                else
                {
                    c.j02InvoiceSignatureFile = v.Rec.j02InvoiceSignatureFile;
                }

                c.j02Email = v.Rec.j02Email;
                c.j02EmailSignature = v.Rec.j02EmailSignature;


                c.j02TitleBeforeName = v.Rec.j02TitleBeforeName;
                c.j02FirstName = v.Rec.j02FirstName;
                c.j02LastName = v.Rec.j02LastName;
                c.j02TitleAfterName = v.Rec.j02TitleAfterName;
                c.j02Code = v.Rec.j02Code;
                c.j02Mobile = v.Rec.j02Mobile;
                c.j02Code = v.Rec.j02Code;

                c.j02TimesheetEntryDaysBackLimit = v.Rec.j02TimesheetEntryDaysBackLimit;
                c.j02TimesheetEntryDaysBackLimit_p34IDs = v.Rec.j02TimesheetEntryDaysBackLimit_p34IDs;
                c.p72ID_NonBillable = v.Rec.p72ID_NonBillable;

                c.j02BitStream = v.disp.GetBitStream();
                c.j02WorksheetOperFlag = v.Rec.j02WorksheetOperFlag;

                c.j02CountryCode = v.Rec.j02CountryCode;

                c.j02IsLoginManualLocked = v.Rec.j02IsLoginManualLocked;

                c.j02IsDebugLog = v.Rec.j02IsDebugLog;
                c.j02Plan_Internal_Rate = v.Rec.j02Plan_Internal_Rate;
                c.j02DefaultHoursFormat = v.Rec.j02DefaultHoursFormat;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
                if (!ValidatePreSave(c))
                {
                    return View(v);
                }


                c.pid = Factory.j02UserBL.Save(c, v.ff1.inputs);
                if (c.pid > 0)
                {
                    if (v.rec_pid == 0 || v.IsDefinePassword)
                    {
                        Factory.j02UserBL.SaveNewPassword(c.pid, v.NewPassword, false);
                    }
                    Factory.o51TagBL.SaveTagging("j02", c.pid, v.TagPids);

                    if (v.disp.IsFiles)
                    {
                        Factory.o27AttachmentBL.SaveChangesAndUpload(v.UploadGuid, "j02", c.pid);
                    }

                    if (Factory.o27AttachmentBL.GetTempFiles(v.UploadGuidSignature).Count() > 0)
                    {
                        var tempfile = Factory.o27AttachmentBL.GetTempFiles(v.UploadGuidSignature).First();
                        var strOrigFileName = "j02_signature_" + c.pid.ToString() + "_original" + tempfile.o27FileExtension;
                        System.IO.File.Copy(tempfile.FullPath, Factory.PluginsFolder + "\\" + strOrigFileName, true);

                        var strDestFileName = "j02_signature_" + c.pid.ToString() + tempfile.o27FileExtension;
                        if (Factory.App.HostingMode != BL.Singleton.HostingModeEnum.None)
                        {
                            strDestFileName = BO.Code.Bas.ParseDbNameFromCloudLogin(Factory.CurrentUser.j02Login) + "_" + strDestFileName;
                        }
                        Code.basUI.ResizeImage(tempfile.FullPath, Factory.PluginsFolder + "\\" + strDestFileName, 300, 130);

                        c = Factory.j02UserBL.Load(c.pid);
                        c.j02InvoiceSignatureFile = strDestFileName;
                        Factory.j02UserBL.Save(c, null);
                    }


                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }


            }


            this.Notify_RecNotSaved();
            return View(v);

        }

        private bool ValidateUserPassword(j02Record v)
        {
            if (string.IsNullOrEmpty(v.NewPassword) || string.IsNullOrEmpty(v.VerifyPassword))
            {
                this.AddMessage("Heslo nemůže být prázdné."); return false;
            }
            var res = new BL.Code.PasswordSupport().CheckPassword(v.NewPassword);
            if (res.Flag == BO.ResultEnum.Failed)
            {
                this.AddMessage(res.Message); return false;
            }
            if (v.NewPassword != v.VerifyPassword)
            {
                this.AddMessage("Heslo nesouhlasí s jeho ověřením."); return false;
            }

            return true;
        }
        private bool ValidatePreSave(BO.j02User recj02)
        {
            if (string.IsNullOrEmpty(recj02.j02Login) || recj02.j04ID == 0)
            {
                this.AddMessage("Přihlašovací jméno (login) a Aplikační role jsou povinná pro uživatelský účet."); return false;
            }

            return true;
        }

        private void InhaleDisp(j02Record v, int bitstream)
        {
            if (v.RecJ04 == null) return;

            //int intCache = (v.rec_pid == 0 ? Factory.j02UserBL.LoadBitstreamFromUserCache("j02", v.RecJ04.pid) : 0);    //pro nový záznam načíst uložená rozšíření z cache
            int intCache = 0;   //cache nepoužívat


            v.disp.SetVal(PosEnum.Files, v.RecJ04.j04FilesTab, bitstream, v.rec_pid, intCache);


        }


    }
}