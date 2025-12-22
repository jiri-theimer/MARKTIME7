using BL;
using BO.TimeApi;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class o23Controller : BaseController
    {
        public IActionResult Info(int pid)
        {
            return Tab1(pid, "Info", null);
        }
        public IActionResult Dashboard(int pid)
        {
            return Tab1(pid, "Dashboard", null);
        }

        public IActionResult Decrypt(int pid, string rez, string caller, string pwd)
        {
            var v = new o23Decrypt() { pid = pid, rez = rez, caller = caller, Rec = Factory.o23DocBL.Load(pid) };

            if (!string.IsNullOrEmpty(pwd))
            {
                if (Factory.o23DocBL.LoadPassword(pid) == pwd)
                {

                    Factory.p85TempboxBL.SetRemember(pid.ToString(), pid);
                    v.Rec.o23Notepad = Factory.o23DocBL.EncryptDescryptNotepad(v.Rec.o23Notepad, false);
                    v.Rec.o23IsEncrypted = false;
                    if (caller != null)
                    {
                        switch (caller.ToLower())
                        {
                            case "Tab1":
                                return RedirectToAction("Tab1", new { pid = pid, rez = rez });
                            case "Dashboard":
                                return RedirectToAction("Dashboard", new { pid = pid, rez = rez });
                            case "Info":
                                return RedirectToAction("Info", new { pid = pid, rez = rez });
                            case "Record":
                                return RedirectToAction("Record", new { pid = pid, rez = rez });
                            case "notepad":
                                return RedirectToAction("NotepadPreview", "Record", new { pid = pid, preix = "o23" });

                        }
                    }
                    
                }
                else
                {
                    this.AddMessage("Zadejte správné heslo.");
                }

            }

            return View(v);
        }
        public IActionResult Tab1(int pid, string caller, string rez)
        {
            var v = new o23Tab1() { Factory = this.Factory, prefix = "o23", pid = pid, caller = caller, rez = rez };
            v.Rec = Factory.o23DocBL.Load(v.pid);
            if (v.Rec != null)
            {
                if (v.Rec.o23IsEncrypted)
                {
                    if (Factory.p85TempboxBL.LoadRemember(pid) != null)
                    {
                        v.Rec.o23Notepad = Factory.o23DocBL.EncryptDescryptNotepad(v.Rec.o23Notepad, false);
                        v.Rec.o23IsEncrypted = false;
                    }
                    else
                    {
                        return RedirectToAction("Decrypt", new { pid = pid, caller = "Tab1", rez = rez });
                    }

                }

                v.SetTagging();


            }
            return View(v);
        }





        public IActionResult SelectDocType(string prefix, int recpid, int o17id,int o43id_source)
        {
            var v = new SelectDocTypeViewModel() { prefix = BO.Code.Entity.GetPrefixDb(prefix), recpid = recpid, o17id = o17id,o43id_source=o43id_source };

            //v.lisO18 = Factory.o18DocTypeBL.GetList(new BO.myQueryO18() {entity = v.prefix, o17id = v.o17id }).OrderBy(p => p.o18Ordinary);
            v.lisO18 = Factory.o18DocTypeBL.GetList_DocumentCreate();
            if (v.lisO18.Count() == 0)
            {
                if (!string.IsNullOrEmpty(v.prefix))
                {
                    return this.StopPage(true, "Pro zvolenou entitu chybí v systému typ dokumentu.");
                    //this.AddMessage("Pro zvolenou entitu chybí v systému typ dokumentu.");
                }
                else
                {
                    //this.AddMessage("V systému chybí založené typy dokumentů.");
                    return this.StopPage(true, "V systému chybí založené typy dokumentů nebo nemáte potřebná oprávnění pro zakládání dokumentů.");
                }

            }
            if (v.lisO18.Count() == 1)
            {
                //rovnou vybrat
                return RedirectToAction("Record", new { prefix = prefix, recpid = recpid, o18id = v.lisO18.First().pid });
            }
            return View(v);
        }

        public IActionResult Uctenka(int pid, int o18id)
        {
            var v = new UctenkaViewModel() { rec_pid = pid, rec_entity = "o23", o18ID = o18id, UploadGuid = BO.Code.Bas.GetGuid(),form_action="Uctenka" };
            if (v.o18ID == 0 && v.rec_pid == 0)
            {
                return RedirectToAction("SelectDocType");
            }
            v.Rec = new BO.o23Doc() { j27ID_Expense = Factory.Lic.j27ID };
            v.SelectedComboJ27 = Factory.FBL.LoadCurrencyByID(v.Rec.j27ID_Expense).j27Code;
            var x15id = Factory.Lic.x15ID;
            var lisP53 = Factory.p53VatRateBL.GetList(new BO.myQuery("p53")).Where(p=>p.x15ID== Factory.Lic.x15ID);
            if (lisP53.Count()>0)
            {
                v.Rec.o23FreeNumber02 = lisP53.First().p53Value;
            }
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.o23DocBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }              

                v.o18ID = v.Rec.o18ID;
                v.RecO18 = Factory.o18DocTypeBL.Load(v.o18ID);                
           
                v.SelectedComboOwner = v.Rec.Owner;
               
            }
            else
            {
                
                v.RecO18 = Factory.o18DocTypeBL.Load(v.o18ID);                
            }

            InhaleNotepad_Uctenka(v, v.Rec.x04ID, v.Rec.o23Notepad);

            v.Toolbar = new MyToolbarViewModel(v.Rec);

            RefreshState_Uctenka(v);

            return View(v);
        }

        private void RefreshState_Uctenka(UctenkaViewModel v)
        {
            
            if (v.RecO18 == null)
            {
                v.RecO18 = Factory.o18DocTypeBL.Load(v.o18ID);
            }

            if (v.lisP34 == null)
            {
                v.lisP34 = Factory.p34ActivityGroupBL.GetList(new BO.myQueryP34() { isexpenseinput = true });

            }
            if (v.lisP34.Count() == 1)
            {
                v.Rec.p34ID_Expense = v.lisP34.First().pid;
            }
            if (v.Rec.o23FreeDate01 == null)
            {
                v.Rec.o23FreeDate01 = DateTime.Today;
            }
            if (v.ProjectCombo == null)
            {
                v.ProjectCombo = new ProjectComboViewModel();
            }
            v.ProjectCombo.IsHideLabel = false;

            InhaleNotepad_Uctenka(v);
          
            if (v.Rec.j02ID_Owner == 0)
            {
                v.Rec.j02ID_Owner = Factory.CurrentUser.pid;
                v.SelectedComboOwner = Factory.CurrentUser.FullnameDesc;
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Uctenka(UctenkaViewModel v)
        {
            RefreshState_Uctenka(v);
            if (v.IsPostback)
            {               

                return View(v);
            }

            if (ModelState.IsValid)
            {

                BO.o23Doc c = new BO.o23Doc();
                if (v.rec_pid > 0) c = Factory.o23DocBL.Load(v.rec_pid);
                c.o18ID = v.o18ID;
                c.o23Name = v.Rec.o23Name;
                c.o23FreeDate01 = v.Rec.o23FreeDate01;
                c.p34ID_Expense = v.Rec.p34ID_Expense;
                c.j27ID_Expense = v.Rec.j27ID_Expense;
                
                c.o23Code = v.Rec.o23Code;
                c.o23FreeText01 = v.Rec.o23FreeText01;
                c.o23FreeNumber01 = v.Rec.o23FreeNumber01;
                c.o23FreeNumber02 = v.Rec.o23FreeNumber02;
                c.o23FreeNumber03 = v.Rec.o23FreeNumber03;
                c.o23FreeNumber04 = v.Rec.o23FreeNumber04;

                c.o23Notepad = v.Notepad.HtmlContent;
                c.x04ID = v.Notepad.SelectedX04ID;
                c.p41ID_Expense = v.ProjectCombo.SelectedP41ID;
                


                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);


                c.pid = Factory.o23DocBL.SaveUctenka(c);
                if (c.pid > 0)
                {
                    Factory.o27AttachmentBL.CommitNotepdChanges(v.Notepad.TempGuid, "o23", c.pid);
                    Factory.o27AttachmentBL.SaveChangesAndUpload(v.UploadGuid, "o23", c.pid);

                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }





            }


            this.Notify_RecNotSaved();
            return View(v);
        }

        public IActionResult Record(int pid, bool isclone, int o18id, string prefix, int recpid, string rez,int o43id_source)
        {
            var v = new o23Record() { rec_pid = pid, rec_entity = "o23", o18ID = o18id, UploadGuid = BO.Code.Bas.GetGuid(),o43id_source=o43id_source };
            v.disp = new DispoziceViewModel();
            v.disp.InitItems("o23", Factory);

            if (v.o18ID == 0 && v.rec_pid == 0)
            {
                return RedirectToAction("SelectDocType", new { prefix = prefix, recpid = recpid, o17id = BO.Code.Bas.InInt(rez), o43id_source= o43id_source });
            }
            v.Rec = new BO.o23Doc();


            if (v.rec_pid > 0)
            {
                v.Rec = Factory.o23DocBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }

                if (v.Rec.o23IsEncrypted && Factory.p85TempboxBL.LoadRemember(v.rec_pid) == null)
                {
                    return RedirectToAction("Decrypt", new { pid = v.rec_pid, caller = "Record", rez = rez });

                }

                v.o18ID = v.Rec.o18ID;
                v.RecO18 = Factory.o18DocTypeBL.Load(v.o18ID);
              
                if (!InhalePermissions(v))
                {
                    return this.StopPage(true, "Nemáte oprávnění k editaci karty záznamu.");
                }

                InhaleDisp(v, v.Rec.o23BitStream);
                InhaleNotepad(v, v.Rec.x04ID, v.Rec.o23Notepad);
                if (v.Rec.o23IsEncrypted)
                {
                    v.Notepad.HtmlContent = Factory.o23DocBL.EncryptDescryptNotepad(v.Notepad.HtmlContent, false);
                }

                v.SelectedComboOwner = v.Rec.Owner;
                v.SetTagging(Factory.o51TagBL.GetTagging("o23", v.rec_pid));
            }
            else
            {                                
                v.RecO18 = Factory.o18DocTypeBL.Load(v.o18ID);
                if (v.RecO18.o18IsAllowEncryption)
                {
                    v.Rec.o23IsEncrypted = true;
                }
                InhaleDisp(v, 0);
                if (v.o43id_source > 0)
                {
                    InhaleInboxRecord(v);
                }
                InhaleNotepad(v, v.Rec.x04ID, v.Rec.o23Notepad);
            }

            if (v.RecO18.o18TemplateFlag == BO.o18TemplateENUM.Uctenka)
            {
                return RedirectToAction("Uctenka", new { pid = v.rec_pid,o18id=v.RecO18.pid, caller = "Record", rez = rez });
            }

            v.Toolbar = new MyToolbarViewModel(v.Rec);

            RefreshState(v);


            foreach (var c in v.lisO16)
            {
                var cc = new DocFieldInput() { o16HelpText = c.o16HelpText, o16Field = c.o16Field, o16Name = c.o16Name, o16DataSource = c.o16DataSource, o16IsEntryRequired = c.o16IsEntryRequired, o16IsFixedDataSource = c.o16IsFixedDataSource,o16ReminderNotifyBefore=c.o16ReminderNotifyBefore };
                if (v.Rec != null)  //načtení uživtelských polí dokumentu
                {
                    if (BO.Code.Reflexe.GetPropertyValue(v.Rec, cc.o16Field) != null)
                    {
                        switch (c.FieldType)
                        {
                            case BO.x24IdENUM.tBoolean:
                                cc.CheckInput = Convert.ToBoolean(BO.Code.Reflexe.GetPropertyValue(v.Rec, cc.o16Field));
                                break;
                            case BO.x24IdENUM.tDate:
                            case BO.x24IdENUM.tDateTime:
                                cc.DateInput = Convert.ToDateTime(BO.Code.Reflexe.GetPropertyValue(v.Rec, cc.o16Field));
                                break;
                            case BO.x24IdENUM.tDecimal:
                                cc.NumInput = Convert.ToDouble(BO.Code.Reflexe.GetPropertyValue(v.Rec, cc.o16Field));
                                break;
                            default:
                                cc.StringInput = Convert.ToString(BO.Code.Reflexe.GetPropertyValue(v.Rec, cc.o16Field));
                                break;
                        }
                    }


                }
                v.lisFields.Add(cc);
            }

            var lisO19 = Factory.o23DocBL.GetList_o19(v.rec_pid);
            v.lisO19 = new List<o19Repeator>();
            foreach (var c in lisO19)
            {
                var cc = new o19Repeator() { TempGuid = BO.Code.Bas.GetGuid(), pid = c.pid, o20ID = c.o20ID, o19RecordPid = c.o19RecordPid, o20Entity = c.o20Entity, SelectedO20Name = c.o20Name };
                var co20 = Factory.o18DocTypeBL.LoadO20(c.o20ID);
                cc.SelectedO20Name = co20.BindName;
                cc.SelectedBindText = Factory.CBL.GetObjectAlias(co20.BindPrefix, c.o19RecordPid);
                v.lisO19.Add(cc);


            }

            if (v.Rec.pid > 0 && isclone)
            {
                v.MakeClone();
            }

            if (v.rec_pid == 0)
            {
                v.IsAutoCollapseO20 = true;
            }
            if (v.Rec.pid == 0)
            {
                if (prefix != null && recpid > 0)
                {
                    //Založení nového dokumentu z konkrétního záznamu entity
                    if (v.lisO20.Where(p => p.BindPrefix == prefix).Count() > 0)
                    {
                        v.SelectedO20ID = v.lisO20.First(p => p.BindPrefix == prefix).pid;
                        var co20 = Factory.o18DocTypeBL.LoadO20(v.SelectedO20ID);
                        var c = new o19Repeator() { o20ID = v.SelectedO20ID, TempGuid = BO.Code.Bas.GetGuid(), o19RecordPid = recpid, SelectedBindText = Factory.CBL.GetObjectAlias(prefix, recpid), SelectedO20Name = co20.BindName };
                        v.lisO19.Add(c);
                    }
                }
                else
                {
                    if (v.lisO20.Any(p => p.o20Entity == "j02"))
                    {
                        //automaticky přidat přihlášeného uživatele
                        var recO20 = v.lisO20.Where(p => p.o20Entity == "j02").First();
                        var cc = new o19Repeator() { TempGuid = BO.Code.Bas.GetGuid(), o20ID = recO20.o20ID, o19RecordPid = Factory.CurrentUser.pid, o20Entity = "j02", SelectedO20Name = recO20.BindName, SelectedBindText = Factory.CurrentUser.FullNameAsc };
                        v.lisO19.Add(cc);
                    }
                }



            }



            return View(v);
        }



        private void RefreshState(o23Record v)
        {
            if (v.RecO18 == null)
            {
                v.RecO18 = Factory.o18DocTypeBL.Load(v.o18ID);
            }
            if (!v.disp.IsInhaled)
            {
                InhaleDisp(v, 0);
            }
            InhaleNotepad(v);
            InhaleRoles(v);

            if (v.reminder == null)
            {
                v.reminder = new ReminderViewModel() { is_static_date = true, record_pid = v.rec_pid, record_prefix = "o23" };
            }
            if (v.barcodes == null)
            {
                v.barcodes = new BarcodesViewModel() { record_pid = v.rec_pid, record_prefix = "o23", TempGuid = v.UploadGuid };
            }
            if (v.lisO16 == null)
            {
                v.lisO16 = Factory.o18DocTypeBL.GetList_o16(v.o18ID);
            }
            if (v.lisO19 == null)
            {
                v.lisO19 = new List<o19Repeator>();
            }
            if (v.lisO20 == null)
            {
                v.lisO20 = Factory.o18DocTypeBL.GetList_o20(v.o18ID).Where(p => p.o20EntryModeFlag == BO.o20EntryModeENUM.Combo);
                if (v.lisO20.Any(p => p.o20Name == null))
                {
                    foreach (var c in v.lisO20.Where(p => p.o20Name == null))
                    {
                        c.o20Name = BO.Code.Entity.GetAlias(c.o20Entity);
                    }
                }
                foreach (var c in v.lisO20.Where(p => p.o20IsEntryRequired))
                {
                    c.o20Name += "*";
                }
            }
            if (v.SelectedO20ID == 0 && v.lisO20.Count() > 0)
            {
                v.SelectedO20ID = v.lisO20.First().pid;
                Handle_Changeo20ID(v, v.lisO20.First());
            }
            //if (v.lisO27 == null)
            //{
            //    v.lisO27 = new List<o27Repeator>();
            //}
            if (v.lisFields == null)
            {
                v.lisFields = new List<DocFieldInput>();
            }
            if (v.Rec.j02ID_Owner == 0)
            {
                v.Rec.j02ID_Owner = Factory.CurrentUser.pid;
                v.SelectedComboOwner = Factory.CurrentUser.FullnameDesc;
            }

            
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(Models.Record.o23Record v, string guid)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "o20ID")
                {
                    var co20 = Factory.o18DocTypeBL.LoadO20(v.SelectedO20ID);
                    Handle_Changeo20ID(v, co20);
                    v.IsAutoCollapseO20 = true;
                }
                if (v.PostbackOper == "bindrec")
                {
                    v.IsAutoCollapseO20 = true;
                    if (v.lisO19.Any(p => p.o20ID == v.SelectedO20ID && p.o19RecordPid == v.SelectedBindPid && !p.IsTempDeleted))
                    {
                        this.AddMessage("Tato vazba již existuje."); return View(v);
                    }
                    var c = new o19Repeator() { o20ID = v.SelectedO20ID, TempGuid = BO.Code.Bas.GetGuid(), o19RecordPid = v.SelectedBindPid, SelectedO20Name = v.SelectedBindName };
                    c.SelectedBindText = Factory.CBL.GetObjectAlias(v.SelectedBindEntity.Substring(0, 3), v.SelectedBindPid);
                    v.SelectedBindText = c.SelectedBindText;
                    var co20 = Factory.o18DocTypeBL.LoadO20(v.SelectedO20ID);
                    if (!co20.o20IsMultiSelect)
                    {
                        //není povolen vícenásobný výběr entity
                        if (v.lisO19.Any(p => p.o20ID == v.SelectedO20ID && !p.IsTempDeleted))
                        {
                            c = v.lisO19.First(p => p.o20ID == v.SelectedO20ID && !p.IsTempDeleted);
                            c.o19RecordPid = v.SelectedBindPid;
                            //c.SelectedBindText = v.SelectedBindText;
                            c.SelectedBindText = Factory.CBL.GetObjectAlias(co20.BindPrefix, v.SelectedBindPid);
                            return View(v);
                        }
                    }

                    v.lisO19.Add(c);
                }

                if (v.PostbackOper == "delete_o19")
                {
                    v.lisO19.First(p => p.TempGuid == guid).IsTempDeleted = true;
                    v.IsAutoCollapseO20 = true;

                }
                //if (v.PostbackOper == "delete_o27")
                //{
                //    v.lisO27.First(p => p.TempGuid == guid).IsTempDeleted = true;

                //}


                return View(v);
            }




            if (ModelState.IsValid)
            {
                if (v.reminder.lisReminder != null && v.reminder.lisReminder.Where(p => p.IsTempDeleted == false && p.o24StaticDate == null).Count() > 0)
                {
                    this.AddMessage("V upozornění chybí vyplnit datum+čas."); return View(v);
                }



                BO.o23Doc c = new BO.o23Doc();
                if (v.rec_pid > 0) c = Factory.o23DocBL.Load(v.rec_pid);

                if (c.j95ID == 0 && v.RecO18.o18GeoFlag == BO.o18GeoFlagEnum.LoadFromCurrentUser)
                {
                    var retGeo = Factory.p15LocationBL.AppendWeatherLog(null, 0, BO.Code.Bas.InDouble(v.current_user_lon), BO.Code.Bas.InDouble(v.current_cuser_lat));
                    if (!retGeo.issuccess)
                    {
                        this.AddMessage("Nelze načíst souřadnice."); return View(v);
                    }
                    c.j95ID = retGeo.pid;
                }

                c.o18ID = v.o18ID;
                c.o23Name = v.Rec.o23Name;
                if (v.RecO18.o18EntryCodeFlag == BO.o18EntryCodeENUM.Manual)
                {
                    c.o23Code = v.Rec.o23Code;
                }
                
                c.j02ID_Owner = v.Rec.j02ID_Owner;
                c.o23BitStream = v.disp.GetBitStream();


                if (v.disp.IsNotepad)
                {
                    c.o23Notepad = v.Notepad.HtmlContent;
                    c.x04ID = v.Notepad.SelectedX04ID;
                }
                foreach (var cc in v.lisFields)
                {
                    switch (cc.FieldType)
                    {
                        case BO.x24IdENUM.tBoolean:
                            BO.Code.Reflexe.SetPropertyValue(c, cc.o16Field, cc.CheckInput);
                            break;
                        case BO.x24IdENUM.tDate:
                        case BO.x24IdENUM.tDateTime:
                            BO.Code.Reflexe.SetPropertyValue(c, cc.o16Field, cc.DateInput);
                            if (cc.o16IsEntryRequired && cc.DateInput == null)
                            {
                                this.AddMessageTranslated(Factory.tra(string.Format("Pole [{0}] je povinné k vyplnění.", cc.o16Name))); return View(v);
                            }
                            break;
                        case BO.x24IdENUM.tDecimal:
                            BO.Code.Reflexe.SetPropertyValue(c, cc.o16Field, cc.NumInput);
                            if (cc.o16IsEntryRequired && cc.NumInput == 0)
                            {
                                this.AddMessageTranslated(Factory.tra(string.Format("Pole [{0}] je povinné k vyplnění.", cc.o16Name))); return View(v);
                            }
                            break;
                        default:
                            BO.Code.Reflexe.SetPropertyValue(c, cc.o16Field, cc.StringInput);
                            if (cc.o16IsEntryRequired && string.IsNullOrEmpty(cc.StringInput))
                            {
                                this.AddMessageTranslated(Factory.tra(string.Format("Pole [{0}] je povinné k vyplnění.", cc.o16Name))); return View(v);
                            }

                            break;
                    }


                }

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                var lisO19 = new List<BO.o19DocTypeEntity_Binding>();
                foreach (var cc in v.lisO19)
                {
                    lisO19.Add(new BO.o19DocTypeEntity_Binding()
                    {
                        IsSetAsDeleted = cc.IsTempDeleted,
                        pid = cc.pid,
                        o20ID = cc.o20ID,
                        o19RecordPid = cc.o19RecordPid,
                    });
                }

                string strPwd = null;
                if (v.RecO18.o18IsAllowEncryption)
                {
                    c.o23IsEncrypted = v.Rec.o23IsEncrypted;
                    if (c.o23IsEncrypted)
                    {
                        bool bolNeedPwd = false;
                        if (v.rec_pid == 0) bolNeedPwd = true;
                        if (v.rec_pid > 0 && Factory.o23DocBL.Load(v.rec_pid).o23Password == null) bolNeedPwd = true;

                        if (bolNeedPwd && string.IsNullOrEmpty(v.PasswordNew))
                        {
                            this.AddMessage("Chybí vyplnit [Heslo pro zašifrování].");
                            return View(v);
                        }
                        if ((!string.IsNullOrEmpty(v.PasswordNew) || !string.IsNullOrEmpty(v.PasswordVerify)) && v.PasswordNew != v.PasswordVerify)
                        {
                            this.AddMessage("Heslo nesouhlasí s jeho ověřením.");
                            return View(v);
                        }
                        if (!string.IsNullOrEmpty(v.PasswordNew))
                        {
                            strPwd = v.PasswordNew;
                        }
                    }
                }

                c.pid = Factory.o23DocBL.Save(c, lisO19, v.disp.IsRoles ? v.roles.getList4Save(Factory) : null);
                if (c.pid > 0)
                {
                    if (v.RecO18.o18IsAllowEncryption)
                    {
                        if (strPwd != null)
                        {
                            Factory.o23DocBL.SavePassword(c.pid, strPwd);
                        }
                        if (c.o23IsEncrypted)
                        {
                            string s = Factory.o23DocBL.EncryptDescryptNotepad(c.o23Notepad, true);
                            Factory.o23DocBL.UpdateNotepad(c.pid, s);
                        }
                        else
                        {
                            Factory.o23DocBL.ClearPassword(c.pid);
                        }
                    }

                    if (v.disp.IsTags)
                    {
                        Factory.o51TagBL.SaveTagging("o23", c.pid, v.TagPids);
                    }
                    if (v.disp.IsNotepad)
                    {
                        Factory.o27AttachmentBL.CommitNotepdChanges(v.Notepad.TempGuid, "o23", c.pid);
                    }
                    if (v.disp.IsFiles)
                    {
                        Factory.o27AttachmentBL.SaveChangesAndUpload(v.UploadGuid, "o23", c.pid);

                    }
                    if (c.j95ID == 0 && v.RecO18.o18GeoFlag == BO.o18GeoFlagEnum.LoadFromP15) //načíst polohu+počasí z projektu/úkolu záznamu
                    {
                        var retGeo = Factory.p15LocationBL.AppendWeatherLog("o23", c.pid);
                        if (retGeo.pid > 0)
                        {
                            Factory.o23DocBL.UpdateGeo(c.pid, retGeo.pid);
                        }

                    }
                    v.barcodes.CommitChangesIfNewRecords(Factory, c.pid);

                    if (v.o43id_source > 0)
                    {
                        var recO43 = Factory.o43InboxBL.Load(v.o43id_source);
                        recO43.o23ID = c.pid;
                        Factory.o43InboxBL.Save(recO43);
                    }

                    if (v.reminder != null)
                    {
                        v.reminder.SaveChanges(Factory, c.pid);
                    }

                    //reminder pro o16ReminderNotifyBefore
                    TrySave_dd1Reminder(v, c.pid);


                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }

        private void TrySave_dd1Reminder(o23Record v, int pid)
        {
            var rec = Factory.o23DocBL.Load(pid);
            var lisO16 = Factory.o18DocTypeBL.GetList_o16(rec.o18ID).Where((p => p.FieldType == BO.x24IdENUM.tDate || p.FieldType == BO.x24IdENUM.tDateTime));
            var lisO24 = Factory.o24ReminderBL.GetList_ddx(pid);
            var lisX69 = Factory.x67EntityRoleBL.GetList_X69_OneDoc(rec, false);
            foreach (var fld in lisO16)
            {
                string strEntity = "dd" + fld.o16Field.Substring(12, 1);
                var recO24 = new BO.o24Reminder();
                if (lisO24.Where(p => p.o24RecordPid == pid && p.o24RecordEntity == strEntity).Count() > 0)
                {
                    recO24.pid = lisO24.Where(p => p.o24RecordPid == pid && p.o24RecordEntity == strEntity).First().pid;
                }
                if (fld.o16ReminderNotifyBefore > 0)
                {
                    var c = BO.Code.Reflexe.GetPropertyValue(rec, fld.o16Field);
                    if (c != null && c != System.DBNull.Value)
                    {
                        recO24.o24RecordDate = (DateTime)c;
                        recO24.j02ID = rec.j02ID_Owner;
                        if (lisX69.Count() > 0)
                        {
                            recO24.x67ID = lisX69.First().pid;
                        }
                        recO24.o24MediumFlag = BO.o24MediumFlagEnum.Email;
                        recO24.o24Unit = "h";
                        recO24.o24Memo = $"{fld.o16Name} ({BO.Code.Bas.ObjectDateTime2String(c)})";
                        recO24.o24Count = fld.o16ReminderNotifyBefore * -1;
                        recO24.o24RecordEntity = strEntity;
                        recO24.o24RecordPid = rec.pid;
                        recO24.ValidUntil = rec.ValidUntil;
                        recO24.ValidFrom = rec.ValidFrom;
                        Factory.o24ReminderBL.Save(recO24);

                    }
                    else
                    {
                        if (recO24.pid > 0)
                        {
                            Factory.CBL.DeleteRecord("o24", recO24.pid);
                        }
                    }
                }
                else
                {
                    if (recO24.pid > 0)
                    {
                        Factory.CBL.DeleteRecord("o24", recO24.pid);
                    }
                }
                

            }




        }

        private void Handle_Changeo20ID(Models.Record.o23Record v, BO.o20DocTypeEntity recO20)
        {
            v.SelectedBindName = recO20.BindName;
            v.SelectedBindEntity = recO20.o20Entity;
            v.SelectedBindPid = 0;
            v.SelectedBindText = null;
            if (recO20.o20RecTypePid > 0)
            {
                v.SelectedBindMyQueryInline = $"{recO20.o20RecTypeEntity}id|int|{recO20.o20RecTypePid}";

            }
            else
            {
                v.SelectedBindMyQueryInline = null;
            }

        }


        private void InhaleDisp(o23Record v, int bitstream)
        {
            if (v.RecO18 == null) return;

            //int intCache = (v.rec_pid == 0 ? Factory.j02UserBL.LoadBitstreamFromUserCache("o23", v.RecO18.pid) : 0);    //pro nový záznam načíst uložená rozšíření z cache
            int intCache = 0;   //cache nepoužívat
            v.disp.SetVal(PosEnum.Notepad, v.RecO18.o18NotepadTab, bitstream, v.rec_pid, intCache);
            v.disp.SetVal(PosEnum.Files, v.RecO18.o18FilesTab, bitstream, v.rec_pid, intCache);
            v.disp.SetVal(PosEnum.Tags, v.RecO18.o18TagsTab, bitstream, v.rec_pid, intCache);
            v.disp.SetVal(PosEnum.Roles, v.RecO18.o18RolesTab, bitstream, v.rec_pid, intCache);

        }
        private void InhaleRoles(o23Record v)
        {
            if (v.disp.IsRoles && v.roles == null)
            {
                v.roles = new RoleAssignViewModel() { RecPid = v.rec_pid, RecPrefix = "o23", RolePrefix = "o23" };
            }

        }
        private void InhaleNotepad(o23Record v, int x04id = 0, string htmlcontent = null)
        {
            if (!v.disp.IsNotepad) return;
            if (v.Notepad == null)
            {
                v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "o23", SelectedX04ID = Factory.Lic.x04ID_Default };
            }
            if (!string.IsNullOrEmpty(htmlcontent))
            {
                v.Notepad.HtmlContent = htmlcontent;
            }
            if (x04id > 0)
            {
                v.Notepad.SelectedX04ID = x04id;
            }


        }

        private void InhaleNotepad_Uctenka(UctenkaViewModel v, int x04id = 0, string htmlcontent = null)
        {           
            if (v.Notepad == null)
            {
                v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "o23", SelectedX04ID = Factory.Lic.x04ID_Default };
            }
            if (!string.IsNullOrEmpty(htmlcontent))
            {
                v.Notepad.HtmlContent = htmlcontent;
            }
            if (x04id > 0)
            {
                v.Notepad.SelectedX04ID = x04id;
            }


        }


        private bool InhalePermissions(o23Record v)
        {
            var perm = Factory.o23DocBL.InhaleRecDisposition(v.Rec.pid, v.Rec);

            if (v.RecO18.o18EntryCodeFlag == BO.o18EntryCodeENUM.X38ID)
            {                
                v.CanEditRecordCode = Factory.x38CodeLogicBL.CanEditRecordCode(v.Rec.x38ID, perm);
            }
            else
            {

                v.CanEditRecordCode = perm.OwnerAccess;
            }

            return perm.OwnerAccess;
        }

        private void InhaleInboxRecord(o23Record v)
        {
            if (v.o43id_source > 0)
            {
                var recO43 = Factory.o43InboxBL.Load(v.o43id_source);
                
                v.Rec.o23Name = recO43.o43Subject;
                if (recO43.o43IsBodyHtml)
                {
                    v.Rec.o23Notepad = recO43.o43BodyHtml;
                }
                else
                {
                    v.Rec.o23Notepad = recO43.o43BodyText;
                }
                var files = Factory.o43InboxBL.GetInboxFiles(recO43.pid, true).Where(p => p.Key != "eml" && p.Key != "msg");
                if (files.Count() > 0)
                {
                    v.disp.SetChecked(PosEnum.Files, true); //zapnout rozšíření Nahrávat přílohy                    
                }
                foreach (BO.StringPair sp in files)
                {
                    System.IO.File.Copy(sp.Value, $"{Factory.TempFolder}\\{sp.Key}",true);
                    Factory.o27AttachmentBL.CreateTempInfoxFile(v.UploadGuid, "o23", sp.Key, sp.Key, BO.Code.File.GetContentType(sp.Value));
                    
                }
                

            }
        }
    }
}
