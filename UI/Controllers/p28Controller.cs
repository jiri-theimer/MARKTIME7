
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using UI.Models;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class p28Controller : BaseController
    {
        private readonly IHttpClientFactory _httpclientfactory;
        public p28Controller(IHttpClientFactory hcf)
        {
            _httpclientfactory = hcf;
        }

        public IActionResult Info(int pid)
        {
            return Tab1(pid, "info");
        }
        public IActionResult Tab1(int pid, string caller)
        {
            var v = new p28Tab1() { Factory = this.Factory, prefix = "p28", pid = pid, caller = caller };

            v.Rec = Factory.p28ContactBL.Load(v.pid);
           
            if (v.Rec != null)
            {
                v.RecSum = Factory.p28ContactBL.LoadSumRow(v.Rec.pid);
               

                //v.JeKontaktniOsoba = false;
                //if (v.Rec.p28IsCompany)
                //{
                //    v.lisP30 = Factory.p28ContactBL.GetList_p30(v.pid);
                //}
                //else
                //{
                //    v.lisP30 = Factory.p28ContactBL.GetList_p30(v.pid);
                //    if (v.lisP30.Count() == 0)
                //    {
                //        v.JeKontaktniOsoba = true;
                //        v.lisP30 = Factory.p28ContactBL.GetList_p30_mother(v.pid);
                //    }


                //}

                //if (this.Factory.CurrentUser.j04IsModule_p41)
                //{
                //    v.lisP41 = Factory.p41ProjectBL.GetList(new BO.myQueryP41("p41") { p28id = v.Rec.pid, IsRecordValid = true });
                //    v.lisP26 = Factory.p28ContactBL.GetList_p26(v.pid);
                //}

                v.SetTagging();

                v.SetFreeFields(0);
            }

            return View(v);
        }

        public IActionResult Portal(int pid)
        {
            var v = new p28Record() { rec_pid = pid, IsCompany = 1, rec_entity = "p28", TempGuid = BO.Code.Bas.GetGuid() };
            
            v.Rec = Factory.p28ContactBL.Load(pid);
            if (v.Rec == null) return RecNotFound(v);

            var lisO32 = Factory.p28ContactBL.GetList_o32(v.rec_pid, 0,0);
            v.lisO32 = new List<o32Repeater>();
            foreach (var c in lisO32)
            {
                v.lisO32.Add(new o32Repeater()
                {
                    TempGuid = BO.Code.Bas.GetGuid(),
                    pid = c.pid,
                    o33ID = c.o33ID,
                    o32Value = c.o32Value,
                    o32Description = c.o32Description,
                    o32Person=c.o32Person,
                    o32IsDefaultInInvoice = c.o32IsDefaultInInvoice
                });
            }

            v.Toolbar = new MyToolbarViewModel(v.Rec);
            RefreshState_Portal(v);
            return View(v);
        }
        public void RefreshState_Portal(p28Record v)
        {
            v.form_action = "Portal";
            if (v.lisO32 == null)
            {
                v.lisO32 = new List<o32Repeater>();
            }

            v.Toolbar.AllowDelete = false;
            v.Toolbar.AllowClone = false;
            v.Toolbar.AllowArchive = false;
            v.lisAutocomplete = Factory.o15AutoCompleteBL.GetList(new BO.myQuery("o15"));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Portal(p28Record v,string guid)
        {
            RefreshState_Portal(v);
            if (v.IsPostback)
            {
                switch (v.PostbackOper)
                {

                    case "add_o32":
                        v.lisO32.Add(new o32Repeater() { TempGuid = BO.Code.Bas.GetGuid(), o33ID = BO.o33FlagEnum.Email,o32IsDefaultInInvoice=true });
                        break;
                    case "delete_o32":
                        v.lisO32.First(p => p.TempGuid == guid).IsTempDeleted = true;
                        break;

                }
                return View(v);
            }

            if (ModelState.IsValid)
            {
                var c = Factory.p28ContactBL.Load(v.rec_pid);

                if (v.IsCompany == 1)
                {
                    c.p28IsCompany = true;
                    c.p28CompanyName = v.Rec.p28CompanyName;
                    c.p28FirstName = null; c.p28LastName = null; c.p28TitleAfterName = null; c.p28TitleBeforeName = null;
                }
                else
                {
                    c.p28IsCompany = false;
                    c.p28CompanyName = null;
                    c.p28FirstName = v.Rec.p28FirstName;
                    c.p28LastName = v.Rec.p28LastName;
                    c.p28TitleAfterName = v.Rec.p28TitleAfterName;
                    c.p28TitleBeforeName = v.Rec.p28TitleBeforeName;

                }
                c.p28CountryCode = v.Rec.p28CountryCode;
                c.p28RegID = v.Rec.p28RegID;
                c.p28VatID = v.Rec.p28VatID;
                c.p28ICDPH_SK = v.Rec.p28ICDPH_SK;

                c.p28Street1 = v.Rec.p28Street1;
                c.p28City1 = v.Rec.p28City1;
                c.p28PostCode1 = v.Rec.p28PostCode1;
                c.p28Country1 = v.Rec.p28Country1;
                
                var lisO32 = new List<BO.o32Contact_Medium>();
                foreach (var cc in v.lisO32)
                {
                    lisO32.Add(new BO.o32Contact_Medium()
                    {
                        IsSetAsDeleted = cc.IsTempDeleted,
                        pid = cc.pid,
                        o33ID = cc.o33ID,
                        o32Value = cc.o32Value,
                        o32Description = cc.o32Description,
                        o32Person=cc.o32Person,
                        o32IsDefaultInInvoice = cc.o32IsDefaultInInvoice
                    });
                }

                c.UserInsert = "portal";
                c.pid = Factory.p28ContactBL.Save(c, lisO32, null, v.TempGuid, null, null);
                if (c.pid > 0)
                {
                    Factory.MailBL.SendMessageWithoutFactory($"Info o aktualizaci záznamu kontaktu {c.p28Name}<hr>Databáze: {Factory.Lic.x01LoginDomain}", "Portál záznam kontaktu", "jiri.theimer@marktime.cz", null, "portal@marktime.net", "Portál");
                    v.SetJavascript_CallOnLoad(c.pid);

                }
                else
                {
                    this.Notify_RecNotSaved();
                }

                return View(v);


            }
            else
            {
                this.Notify_RecNotSaved();
                return View(v);
            }
        }

        public IActionResult Record(int pid, bool isclone, int p29id, bool kontaktniosoba)
        {
            var v = new p28Record() { rec_pid = pid, rec_entity = "p28", IsCompany = 1, TempGuid = BO.Code.Bas.GetGuid(), UploadGuid = BO.Code.Bas.GetGuid(), SelectedComboOwner = Factory.CurrentUser.FullnameDesc };
            v.BillingMemo = new PellEditorViewModel();
            if (pid == 0 && kontaktniosoba)
            {
                var lisP29 = Factory.p29ContactTypeBL.GetList(new BO.myQuery("p29")).Where(p => p.p29ScopeFlag == BO.p29ScopeFlagENUM.ContactPerson);
                if (lisP29.Count() > 0)
                {
                    p29id = lisP29.First().pid;

                    v.IsCompany = 0;
                }
            }
            v.Element2Focus = "Rec_p28CompanyName";
            v.Rec = new BO.p28Contact() { p28BillingFlag = BO.p28BillingFlagENUM.CenikDedit, p28CountryCode = Factory.Lic.x01CountryCode, j02ID_Owner = Factory.CurrentUser.pid, p29ID = p29id };

            v.disp = new DispoziceViewModel();
            v.disp.InitItems("p28", Factory);

            if (v.rec_pid == 0 && v.Rec.p29ID > 0)
            {
                v.RecP29 = Factory.p29ContactTypeBL.Load(v.Rec.p29ID);
                v.SelectedComboP29Name = v.RecP29.p29Name;
            }

            if (v.rec_pid == 0 && !isclone && v.Rec.p29ID == 0) //výchozí hodnoty nového kontaktu natáhnout z naposledy mnou vytvořeného
            {
                var qry = Factory.p28ContactBL.GetList(new BO.myQueryP28() { vyloucit_kontaktni_osoby = true, j02id_owner = Factory.CurrentUser.pid, TopRecordsOnly = 1, explicit_orderby = "a.p28ID DESC" });
                if (qry.Count() > 1)
                {
                    v.Rec.p28CountryCode = qry.First().p28CountryCode;
                    v.Rec.p29ID = qry.First().p29ID;
                    v.SelectedComboP29Name = qry.First().p29Name;
                    v.RecP29 = Factory.p29ContactTypeBL.Load(v.Rec.p29ID);
                    v.Rec.p28IsCompany = qry.First().p28IsCompany;
                    if (v.RecP29.p29ScopeFlag == BO.p29ScopeFlagENUM.ContactPerson)
                    {
                        v.Rec.p28IsCompany = false;
                    }
                    v.IsCompany = (v.Rec.p28IsCompany ? 1 : 0);


                }
                if (v.Rec.p29ID == 0)
                {
                    var lisP29 = Factory.p29ContactTypeBL.GetList(new BO.myQuery("p29"));
                    if (lisP29.Count() > 0)
                    {
                        v.SelectedComboP29Name = lisP29.First().p29Name;
                        v.Rec.p29ID = lisP29.First().pid;
                    }
                }
            }


            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p28ContactBL.Load(v.rec_pid);
                if (v.Rec == null) return RecNotFound(v);

                if (!InhalePermissions(v)) return this.StopPage(true, "Nemáte oprávnění k editaci karty záznamu.");
                Handle_LoadRecord(v);

            }

            if (v.rec_pid == 0 && !Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_p28_Creator))
            {
                if (Factory.p29ContactTypeBL.GetList_ContactCreate().Count() == 0)
                {
                    return this.StopPage(true, "Nemáte oprávnění zakládat záznamy nových kontaktů.");
                }
            }

            RefreshState_Record(v,isclone);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
                if (v.lisO32 != null)
                {
                    foreach (var c in v.lisO32)
                    {
                        c.pid = 0;
                    }
                }
                if (v.lisP30 != null)
                {
                    foreach (var c in v.lisP30)
                    {
                        c.pid = 0;
                    }
                }


            }

            return View(v);
        }

        private void Handle_LoadRecord(p28Record v)
        {
            v.SetTagging(Factory.o51TagBL.GetTagging("p28", v.rec_pid));
            v.RecP29 = Factory.p29ContactTypeBL.Load(v.Rec.p29ID);

            if (!v.Rec.p28IsCompany)
            {
                v.IsCompany = 0;
                v.Element2Focus = "Rec_j02LastName";
            }
            if (v.Rec.j61ID_Invoice > 0)
            {
                v.SelectedComboJ61Name = Factory.j61TextTemplateBL.Load(v.Rec.j61ID_Invoice).j61Name;
            }
            if (v.Rec.p63ID > 0)
            {
                v.SelectedComboP63Name = Factory.p63OverheadBL.Load(v.Rec.p63ID).p63Name;
            }
            v.SelectedComboP92Name = v.Rec.p92Name;
            v.SelectedComboP29Name = v.Rec.p29Name;

            v.SelectedComboOwner = v.Rec.Owner;
            if (v.Rec.p28ParentID > 0)
            {
                v.SelectedComboParentP28Name = Factory.p28ContactBL.Load(v.Rec.p28ParentID).p28Name;
            }
            if (v.Rec.p28BillingMemo200 != null && v.BillingMemo !=null)
            {
                v.BillingMemo.HtmlValue = Factory.p28ContactBL.LoadBillingMemo(v.rec_pid);
            }
            

            InhaleDisp(v, v.Rec.p28BitStream);


            if (v.disp.IsContactMedia)
            {
                var lisO32 = Factory.p28ContactBL.GetList_o32(v.rec_pid, 0,0);
                v.lisO32 = new List<o32Repeater>();
                foreach (var c in lisO32)
                {
                    v.lisO32.Add(new o32Repeater()
                    {
                        TempGuid = BO.Code.Bas.GetGuid(),
                        pid = c.pid,
                        o33ID = c.o33ID,
                        o32Value = c.o32Value,
                        o32Description = c.o32Description,
                        o32Person=c.o32Person,
                        o32IsDefaultInInvoice = c.o32IsDefaultInInvoice
                    });
                }
            }
            if (v.disp.IsContactPersons)
            {
                var lisP30 = Factory.p28ContactBL.GetList_p30(v.rec_pid);
                v.lisP30 = new List<p30Repeater>();
                foreach (var c in lisP30)
                {
                    v.lisP30.Add(new p30Repeater()
                    {
                        TempGuid = BO.Code.Bas.GetGuid(),
                        pid = c.pid,
                        p30Name = c.p30Name,
                        p28ID_Person = c.p28ID_Person,
                        ComboPerson = c.PersonWithInvoiceEmail
                    });
                }
            }

            if (v.Rec.p28BillingFlag == BO.p28BillingFlagENUM._NotSpecified) v.Rec.p28BillingFlag = BO.p28BillingFlagENUM.CenikDedit;
            if (v.Rec.p51ID_Billing > 0)
            {
                var recP51 = Factory.p51PriceListBL.Load(v.Rec.p51ID_Billing);
                v.SelectedComboP51Name = recP51.p51Name;
                if (recP51.p51IsCustomTailor)
                {
                    v.Rec.p28BillingFlag = BO.p28BillingFlagENUM.CenikIndividualni;  //ceník na míru
                    v.SelectedP51ID_Flag3 = v.Rec.p51ID_Billing;
                }
                else
                {
                    v.Rec.p28BillingFlag = BO.p28BillingFlagENUM.CenikPrirazeny;
                    v.SelectedP51ID_Flag2 = v.Rec.p51ID_Billing;
                }

            }
        }

        private void InhaleDisp(p28Record v, int bitstream)
        {
            if (v.RecP29 == null) return;

            //int intCache = (v.rec_pid == 0 ? Factory.j02UserBL.LoadBitstreamFromUserCache("p28", v.RecP29.pid) : 0);    //pro nový záznam načíst uložená rozšíření z cache
            int intCache = 0;   //cache nepoužívat


            v.disp.SetVal(PosEnum.Files, v.RecP29.p29FilesTab, bitstream, v.rec_pid, intCache);
            v.disp.SetVal(PosEnum.BillingTab, v.RecP29.p29BillingTab, bitstream, v.rec_pid, intCache);
            v.disp.SetVal(PosEnum.Roles, v.RecP29.p29RolesTab, bitstream, v.rec_pid, intCache);
            v.disp.SetVal(PosEnum.ContactPersons, v.RecP29.p29ContactPersonsTab, bitstream, v.rec_pid, intCache);
            v.disp.SetVal(PosEnum.ContactMedia, v.RecP29.p29ContactMediaTab, bitstream, v.rec_pid, intCache);

        }
        private void InhaleRoles(p28Record v)
        {
            if (v.disp.IsRoles && v.roles == null)
            {
                v.roles = new RoleAssignViewModel() { RecPid = v.rec_pid, RecPrefix = "p28", RolePrefix = "p28", RolePrefixSecond = "p41", Header = "Obsazení rolí v záznamu Kontaktu + Automaticky dosazené projektové role v projektech klienta" };
            }

        }

        private void RefreshState_Record(p28Record v,bool isclone)
        {
            if (v.Rec.p29ID > 0 && v.RecP29 == null)
            {
                v.RecP29 = Factory.p29ContactTypeBL.Load(v.Rec.p29ID);
            }
            if (!v.disp.IsInhaled)
            {
                InhaleDisp(v, 0);
            }
            InhaleRoles(v);

            if (v.ff1 == null)
            {
                v.ff1 = new FreeFieldsViewModel();
                v.ff1.InhaleFreeFieldsView(Factory, v.rec_pid, "p28");
            }
            v.ff1.RefreshInputsVisibility(Factory, v.rec_pid, "p28", v.Rec.p29ID);

            if (v.reminder == null)
            {
                v.reminder = new ReminderViewModel() { is_static_date = true, record_pid = v.rec_pid, record_prefix = "p28" };
            }
            if (v.hlidac == null)
            {
                v.hlidac = new HlidacViewModel() { rec_entity = "p28", rec_pid = v.rec_pid };


            }

            if (v.disp.IsContactMedia && v.lisO32 == null)
            {
                v.lisO32 = new List<o32Repeater>();
            }
            if (v.disp.IsContactPersons && v.lisP30 == null)
            {
                v.lisP30 = new List<p30Repeater>();
            }

            if ((v.rec_pid == 0 || isclone) && v.Notepad == null && v.disp.IsBilling)
            {
                v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "p28", SelectedX04ID = Factory.Lic.x04ID_Default, PlaceHolder = "Notepad: fakturační poznámka..." };
            }

            if (v.Rec.j02ID_Owner == 0)
            {
                v.Rec.j02ID_Owner = Factory.CurrentUser.pid;
                v.SelectedComboOwner = Factory.CurrentUser.FullnameDesc;
            }
            v.lisAutocomplete = Factory.o15AutoCompleteBL.GetList(new BO.myQuery("o15"));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p28Record v, string guid, int j02id, string j02ids)
        {
            RefreshState_Record(v,false);
            if (v.IsPostback)
            {
                switch (v.PostbackOper)
                {
                    case "p29id":
                        if (v.Rec.p29ID > 0)
                        {
                            v.RecP29 = Factory.p29ContactTypeBL.Load(v.Rec.p29ID);
                            InhaleDisp(v, v.disp.GetBitStream());
                            if (v.rec_pid == 0) v.disp.RecoveryDefaultCheckedStates();
                            RefreshState_Record(v,false);
                            //InhaleRoles(v);

                        }
                        break;

                    case "add_o32":
                        v.lisO32.Add(new o32Repeater() { TempGuid = BO.Code.Bas.GetGuid(), o33ID = BO.o33FlagEnum.Email,o32IsDefaultInInvoice=true });
                        break;
                    case "delete_o32":
                        v.lisO32.First(p => p.TempGuid == guid).IsTempDeleted = true;
                        break;
                    case "add_p30":
                        v.lisP30.Add(new p30Repeater() { TempGuid = BO.Code.Bas.GetGuid() });
                        break;
                    case "delete_p30":
                        v.lisP30.First(p => p.TempGuid == guid).IsTempDeleted = true;
                        break;
                    case "ares-ico":
                        Handle_Load_Profile_ByRejstrik(v, "ico", v.Rec.p28RegID);
                        break;
                    case "ares-dic":
                        Handle_Load_Profile_ByRejstrik(v, "dic", v.Rec.p28VatID);
                        break;
                    case "sk-dic":
                        Handle_Load_Profile_ByRejstrik(v, "dic", v.Rec.p28ICDPH_SK);
                        break;
                }

                return View(v);
            }



            if (ModelState.IsValid)
            {
                return Handle_Save(v);
            }
            else
            {
                this.Notify_RecNotSaved();
                return View(v);
            }



        }

        private IActionResult Handle_Save(p28Record v)
        {
            if (v.reminder.lisReminder != null && v.reminder.lisReminder.Where(p => p.IsTempDeleted == false && p.o24StaticDate == null).Count() > 0)
            {
                this.AddMessage("V upozornění chybí vyplnit datum+čas."); return View(v);
            }

            BO.p28Contact c = new BO.p28Contact();
            if (v.rec_pid > 0) c = Factory.p28ContactBL.Load(v.rec_pid);

            if (v.IsCompany == 1)
            {
                c.p28IsCompany = true;
                c.p28CompanyName = v.Rec.p28CompanyName;
                c.p28FirstName = null; c.p28LastName = null; c.p28TitleAfterName = null; c.p28TitleBeforeName = null;
            }
            else
            {
                c.p28IsCompany = false;
                c.p28CompanyName = null;
                c.p28FirstName = v.Rec.p28FirstName;
                c.p28LastName = v.Rec.p28LastName;
                c.p28TitleAfterName = v.Rec.p28TitleAfterName;
                c.p28TitleBeforeName = v.Rec.p28TitleBeforeName;

            }
            c.p28CountryCode = v.Rec.p28CountryCode;
            c.p28RegID = v.Rec.p28RegID;
            c.p28VatID = v.Rec.p28VatID;
            c.p28ICDPH_SK = v.Rec.p28ICDPH_SK;

            c.p28Street1 = v.Rec.p28Street1;
            c.p28City1 = v.Rec.p28City1;
            c.p28PostCode1 = v.Rec.p28PostCode1;
            c.p28Country1 = v.Rec.p28Country1;
            c.p28BeforeAddress1 = v.Rec.p28BeforeAddress1;
            c.p28Street2 = v.Rec.p28Street2;
            c.p28City2 = v.Rec.p28City2;
            c.p28PostCode2 = v.Rec.p28PostCode2;
            c.p28Country2 = v.Rec.p28Country2;

            c.p28ShortName = v.Rec.p28ShortName;
            c.j02ID_Owner = v.Rec.j02ID_Owner;
            c.p29ID = v.Rec.p29ID;
            c.p63ID = v.Rec.p63ID;
            c.p28BillingLangIndex = v.Rec.p28BillingLangIndex; ;
            c.j61ID_Invoice = v.Rec.j61ID_Invoice;
            c.p28VatCodePohoda = v.Rec.p28VatCodePohoda;
                        
            if (v.BillingMemo != null)
            {
                c.p28BillingMemo200 = v.BillingMemo.GetText200();
            }
            


            c.p51ID_Billing = 0;
            if (v.Rec.p28BillingFlag == BO.p28BillingFlagENUM.CenikPrirazeny)
            {
                c.p51ID_Billing = v.SelectedP51ID_Flag2;
                if (c.p51ID_Billing == 0)
                {
                    this.AddMessage("Chybí vybrat ceník fakturačních hodinových sazeb."); return View(v);
                }
            }
            if (v.Rec.p28BillingFlag == BO.p28BillingFlagENUM.CenikIndividualni)
            {
                if (v.SelectedP51ID_Flag3 == 0)
                {
                    var recP51 = Factory.p51PriceListBL.LoadByTempGuid(v.TempGuid);
                    if (recP51 == null)
                    {
                        this.AddMessage("Chybí ceník hodinových sazeb na míru."); return View(v);
                    }
                    c.p51ID_Billing = recP51.pid;
                }
                else
                {
                    c.p51ID_Billing = v.SelectedP51ID_Flag3;
                }
            }

            c.p92ID = v.Rec.p92ID;


            c.p28InvoiceDefaultText1 = v.Rec.p28InvoiceDefaultText1;
            c.p28InvoiceDefaultText2 = v.Rec.p28InvoiceDefaultText2;

            c.p28InvoiceMaturityDays = v.Rec.p28InvoiceMaturityDays;
            c.p28Round2Minutes = v.Rec.p28Round2Minutes;

            c.p28BankAccount = v.Rec.p28BankAccount;
            c.p28BankCode = v.Rec.p28BankCode;
            c.p28JobTitle = v.Rec.p28JobTitle;
            c.p28Salutation = v.Rec.p28Salutation;
            c.p28ExternalCode = v.Rec.p28ExternalCode;

            c.p28ParentID = v.Rec.p28ParentID;

            c.p28BitStream = v.disp.GetBitStream();
            c.p28BillingFlag = v.Rec.p28BillingFlag;


            c.ValidUntil = v.Toolbar.GetValidUntil(c);
            c.ValidFrom = v.Toolbar.GetValidFrom(c);


            List<BO.o32Contact_Medium> lisO32 = null;
            if (v.disp.IsContactMedia)
            {
                lisO32 = new List<BO.o32Contact_Medium>();
                foreach (var cc in v.lisO32)
                {
                    lisO32.Add(new BO.o32Contact_Medium()
                    {
                        IsSetAsDeleted = cc.IsTempDeleted,
                        pid = cc.pid,
                        o33ID = cc.o33ID,
                        o32Value = cc.o32Value,
                        o32Description = cc.o32Description,
                        o32Person=cc.o32Person,
                        o32IsDefaultInInvoice = cc.o32IsDefaultInInvoice
                    });
                }
            }
            List<BO.p30ContactPerson> lisP30 = null;
            if (v.disp.IsContactPersons)
            {
                lisP30 = new List<BO.p30ContactPerson>();
                foreach (var cc in v.lisP30)
                {
                    lisP30.Add(new BO.p30ContactPerson()
                    {
                        IsSetAsDeleted = cc.IsTempDeleted,
                        pid = cc.pid,
                        p30Name = cc.p30Name,
                        p28ID_Person = cc.p28ID_Person
                    });
                }
            }

            c.pid = Factory.p28ContactBL.Save(c, lisO32, v.ff1.inputs, v.TempGuid, v.roles == null ? null : v.roles.getList4Save(Factory), lisP30, (v.BillingMemo != null ? v.BillingMemo.HtmlValue : null));
            if (c.pid > 0)
            {
                Factory.o51TagBL.SaveTagging("p28", c.pid, v.TagPids);

                if (v.rec_pid == 0 && v.Notepad !=null && !string.IsNullOrWhiteSpace(v.Notepad.HtmlContent))  //zda uložit fakturační poznámku
                {
                    var recB05 = new BO.b05Workflow_History() { b05RecordEntity = "p28", b05IsCommentOnly = true, b05IsManualStep = true, x04ID = v.Notepad.SelectedX04ID, b05RecordPid = c.pid, b05Notepad = v.Notepad.HtmlContent, b05Tab1Flag = 6 };
                    Factory.WorkflowBL.Save2History(recB05);

                }

                

                if (v.disp.IsFiles)
                {
                    Factory.o27AttachmentBL.SaveChangesAndUpload(v.UploadGuid, "p28", c.pid);
                }
                if (v.reminder != null)
                {
                    v.reminder.SaveChanges(Factory, c.pid);
                }
                if (v.hlidac != null)
                {
                    v.hlidac.SaveChanges(Factory, c.pid);
                }
                v.SetJavascript_CallOnLoad(c.pid);
                return View(v);
            }

            return View(v);
        }

        private bool InhalePermissions(p28Record v)
        {
            var perm = Factory.p28ContactBL.InhaleRecDisposition(v.Rec.pid, v.Rec);
            if (!perm.OwnerAccess)
            {
                return false;
            }
            if (v.Rec.x38ID > 0)
            {
                v.CanEditRecordCode = Factory.x38CodeLogicBL.CanEditRecordCode(v.Rec.x38ID, perm);
            }
            else
            {
                v.CanEditRecordCode = perm.OwnerAccess;
            }
            return true;
        }

        public BO.Rejstrik.DefaultZaznam LoadRejstrikSubjekt(string strPole, string strHodnota, string strCountryCode)
        {
            var engine = new BL.Code.RejstrikySupport();
            var ret = engine.LoadDefaultZaznam(strPole, strHodnota, strCountryCode, _httpclientfactory.CreateClient());
            try
            {
                return ret.Result;
            }
            catch (Exception e)
            {
                this.AddMessageTranslated(e.Message + "<hr>Jste vůbec připojeni k internetu?");
                return null;
            }



        }


        private void Handle_Load_Profile_ByRejstrik(p28Record v, string strPole, string strHodnota)
        {
            if (string.IsNullOrEmpty(v.Rec.p28CountryCode))
            {
                this.AddMessage("Na vstupu chybí zadat ISO kód státu."); return;
            }

            if (strPole == "ico" && string.IsNullOrEmpty(strHodnota))
            {
                this.AddMessage("Na vstupu chybí zadat IČO."); return;
            }
            if (strPole == "dic" && string.IsNullOrEmpty(strHodnota) && v.Rec.p28CountryCode == "SK")
            {
                this.AddMessage("Na vstupu chybí zadat IČ-DPH."); return;
            }
            if (strPole == "dic" && string.IsNullOrEmpty(strHodnota))
            {
                this.AddMessage("Na vstupu chybí zadat DIČ."); return;
            }

            var subjekt = LoadRejstrikSubjekt(strPole, strHodnota, v.Rec.p28CountryCode);
            if (subjekt == null || subjekt.ico == null && subjekt.dic == null)
            {
                this.AddMessage("Subjekt nebyl nalezen."); return;
            }
            v.IsCompany = 1;
            v.Rec.p28IsCompany = true;
            v.Rec.p28CompanyName = subjekt.name;
            v.Rec.p28Street1 = subjekt.street;
            v.Rec.p28City1 = subjekt.city;
            v.Rec.p28PostCode1 = subjekt.zipcode;
            if (subjekt.country == "CZ")
            {
                v.Rec.p28Country1 = "Česká republika";
            }
            if (subjekt.country == "SK")
            {
                v.Rec.p28Country1 = "Slovenská republika";
            }

            if (strPole == "ico" && !string.IsNullOrEmpty(subjekt.dic))
            {
                if (!string.IsNullOrEmpty(v.Rec.p28VatID) && subjekt.dic != v.Rec.p28VatID)
                {
                    this.AddMessage("DIČ v kartě kontaktu se liší od DIČ v rejstříku!");
                }
                var recDupl = Factory.p28ContactBL.LoadByICO(subjekt.ico, v.rec_pid);
                if (recDupl != null)
                {
                    this.AddMessageTranslated($"Subjekt {recDupl.p28Name} již existuje v databázi s IČO={recDupl.p28RegID}.");
                }
                if (v.Rec.p28CountryCode == "SK")
                {
                    v.Rec.p28ICDPH_SK = subjekt.dic;
                }
                else
                {
                    v.Rec.p28VatID = subjekt.dic;
                }

            }
            if (strPole == "dic" && !string.IsNullOrEmpty(subjekt.ico))
            {
                if (!string.IsNullOrEmpty(v.Rec.p28RegID) && v.Rec.p28RegID != subjekt.ico)
                {
                    this.AddMessage("IČO v kartě kontaktu se liší od IČO v rejstřík!");
                }
                var recDupl = Factory.p28ContactBL.LoadByDIC(subjekt.dic, v.rec_pid);
                if (recDupl != null)
                {
                    this.AddMessageTranslated($"Subjekt {recDupl.p28Name} již existuje v databázi s tímto DIČ.");
                }

                v.Rec.p28RegID = subjekt.ico;

            }


        }
    }
}
