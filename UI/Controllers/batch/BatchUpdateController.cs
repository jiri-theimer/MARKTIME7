using BO.Rejstrik;
using Microsoft.AspNetCore.Mvc;
using UI.Models.batch;

namespace UI.Controllers.batch
{
    public class BatchUpdateController : BaseController
    {
        public IActionResult Index(string pids, string prefix, int j72id, string guid_pids)
        {
            if (!string.IsNullOrEmpty(guid_pids))
            {
                pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            var v = new BatchUpdateViewModel() { prefix = BO.Code.Entity.GetPrefixDb(prefix), pids = pids, j72id = j72id, IsSetValue = true };

            var lisPids = BO.Code.Bas.ConvertString2ListInt(v.pids);
            if (string.IsNullOrEmpty(v.prefix))
            {
                return this.StopPage(true, "Na vstupu chybí Entita.");
            }
            if (lisPids.Count() == 0)
            {
                return this.StopPage(true, "Na vstupu chybí seznam záznamů.");
            }
            if (!ValidateRecOwner(v))
            {
                return this.StopPage(true, "U minimálně jednoho záznamu chybí oprávnění vlastníka k záznamu.");
            }

            RefreshState(v);

            return View(v);
        }

        private void RefreshState(BatchUpdateViewModel v)
        {
            InitElements(v);
            if (v.oper != null)
            {
                v.CurElement = v.lisElements.Where(p => p.Field == v.oper).First();
            }
            BO.TheGridState gridState = null;
            if (v.j72id > 0)
            {
                gridState = Factory.j72TheGridTemplateBL.LoadState(v.j72id, Factory.CurrentUser.pid);
            }
            else
            {
                gridState = Factory.j72TheGridTemplateBL.LoadState(v.prefix, Factory.CurrentUser.pid, null, null);
            }


            v.gridinput = new Views.Shared.Components.myGrid.myGridInput() { j72id = v.j72id, is_enable_selecting = false, entity = gridState.j72Entity, myqueryinline = $"pids|list_int|{v.pids}", oncmclick = "", ondblclick = "" };
            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load(gridState.j72Entity.Substring(0, 3), null, 0, $"pids|list_int|{v.pids}");

            
            if (v.prefix == "p28")
            {
                v.lisX67 = Factory.x67EntityRoleBL.GetList(new BO.myQuery("x67")).Where(p => p.x67Entity == v.prefix || p.x67Entity=="p41");
            }
            else
            {
                v.lisX67 = Factory.x67EntityRoleBL.GetList(new BO.myQuery("x67")).Where(p => p.x67Entity == v.prefix);
            }
        }

        private void InitElements(BatchUpdateViewModel v)
        {
            v.lisElements = new List<BatchUpdateElement>();
            if (v.prefix == "p41" || v.prefix == "le5")
            {
                AE(v, "validity", "Časová platnost záznamu", "validity", true);
                AE(v, "roles", "Role v záznamu", "roles",true);
                AE(v, "j02ID_Owner", "Vlastník záznamu", "combo", true, true);
                AE(v, "p42ID", "Typ projektu", "combo", true);
                AE(v, "p92ID", "Výchozí typ faktury", "combo");
                AE(v, "j18ID", "Středisko", "combo");
                AE(v, "p92ID", "Výchozí typ faktury", "combo");
                AE(v, "p61ID", "Klastr aktivit", "combo", false, true);
                AE(v, "p51ID_Billing", "Ceník fakturačních sazeb", "combo");
                AE(v, "p41BillingLangIndex", "Fakturační jazyk", "combo", true);
                AE(v, "p41InvoiceDefaultText1", "Hlavní text faktury", "memo");
                AE(v, "p41InvoiceDefaultText2", "Technický text faktury", "memo", false, true);
                AE(v, "p41Plan_Hours_Billable", "Plán hodin", "num");
                AE(v, "p41Plan_Expenses", "Plán výdajů", "num");
                AE(v, "p41Plan_Revenue", "Plánovaný příjem", "num");
                AE(v, "p41PlanUntil", "Plán dokončení", "date");
                AE(v, "p41PlanFrom", "Plán zahájení", "date");
            }
            if (v.prefix == "p28")
            {
                AE(v, "validity", "Časová platnost záznamu", "validity",true);
                AE(v, "roles", "Role v záznamu", "roles", true);
                AE(v, "j02ID_Owner", "Vlastník záznamu", "combo", true, true);
                AE(v, "p29ID", "Typ kontaktu", "combo", true);
                AE(v, "p28BillingLangIndex", "Fakturační jazyk", "combo", true);
                AE(v, "p51ID_Billing", "Ceník fakturačních sazeb", "combo",false,true);
                AE(v, "p92ID", "Výchozí typ faktury", "combo");
                AE(v, "j61ID_Invoice", "Šablona zprávy pro elektronickou fakturaci", "combo",false,true);
                AE(v, "p28InvoiceDefaultText1", "Hlavní text faktury", "memo");
                AE(v, "p28InvoiceDefaultText2", "Technický text faktury", "memo", false, true);
                AE(v, "p28Country1", "Stát #1", "text");
                AE(v, "p28Country2", "Stát #2", "text");
                AE(v, "p28CountryCode", "ISO kód státu", "text", true,true);
                AE(v, "p24ID", "Přidat do skupiny kontaktů", "combo",true);
                AE(v,"rejstrik", "Aktualizovat kontakt z rejstříků", "rejstrik", true);

            }
            if (v.prefix == "p56")
            {
                AE(v, "validity", "Časová platnost záznamu", "validity");
                AE(v, "roles", "Role v záznamu", "roles", true);
                AE(v, "j02ID_Owner", "Vlastník záznamu", "combo", true, true);
                AE(v, "p57ID", "Typ úkolu", "combo", true);
                AE(v, "p56PlanUntil", "Plán dokončení", "datetime");
                AE(v, "p56PlanFrom", "Plán zahájení", "datetime");
                AE(v, "p56IsStopNotify", "Vypnout notifikace", "bool", true);
                AE(v, "p56Plan_Hours", "Plán/Limit hodin", "num");
                AE(v, "p56Plan_Expenses", "Plán/Limit výdajů", "num", false, true);

                AE(v, "p56PlanUntil", "Plán dokončení", "date");
                AE(v, "p56PlanFrom", "Plán zahájení", "date");

            }
            if (v.prefix == "p40")
            {
                AE(v, "validity", "Časová platnost záznamu", "validity");                
                AE(v, "j02ID_Owner", "Vlastník záznamu", "combo", true, true);
                AE(v, "p32ID", "Aktivita", "combo", true);
                AE(v, "p40FirstSupplyDate", "Datum prvního úkonu", "datetime",false,false,true);
                AE(v, "p40LastSupplyDate", "Datum konce generování", "datetime",false,true,true);
                AE(v, "p40Text", "Maska textu úkonu", "text",false,false,true);
                AE(v, "p40Value", "Částka (hodnota úkonu)", "num",false,true,true);
                AE(v, "p40Percentage", "Procento změny částky", "num", false, false, true);
                AE(v, "p40GenerateDayAfterSupply", "Kolik dní před/po spouštět generování", "num",false,false,true);
                AE(v, "p40FreeHours", "Volné Fa hodiny zdarma","num");
                AE(v, "p40FreeFee", "Honorář zdarma","num");


            }
            if (v.prefix == "p91")
            {
                AE(v, "validity", "Časová platnost záznamu", "validity");
                AE(v, "roles", "Role v záznamu", "roles", true);                
                AE(v, "j02ID_Owner", "Vlastník záznamu", "combo", true,true);
                AE(v, "p91locks", "Zámky nad vyúčtováním", "p91locks", true, true);
                AE(v, "p92ID", "Typ faktury", "combo", true);
                AE(v, "j19ID", "Druh úhrady", "combo");
                AE(v, "p98ID", "Zaokrouhlovací pravidlo", "combo");
                AE(v, "p80ID", "Struktura cenového rozpisu", "combo", false, true);
                AE(v, "p63ID", "Režijní přirážka k faktuře", "combo", false, true);
                AE(v, "p91Date", "Datum vystavení", "date", true);
                AE(v, "p91DateMaturity", "Datum splatnosti", "date", true);
                AE(v, "p91DateSupply", "Datum plnění", "date", true);
                AE(v, "p91Datep31_From", "Období od", "date", true);
                AE(v, "p91Datep31_Until", "Období od", "date", true, true);
                AE(v, "p91Text1", "Text faktury", "memo");
                AE(v, "p91Text2", "Technický text faktury", "memo");
                AE(v, "p91PortalFlag", "Publikovat na portále", "bool",true);
                

            }
            if (v.prefix == "p90")
            {
                AE(v, "validity", "Časová platnost záznamu", "validity");
                AE(v, "roles", "Role v záznamu", "roles", true);
                AE(v, "j02ID_Owner", "Vlastník záznamu", "combo", true, true);                
                AE(v, "p89ID", "Typ zálohy", "combo", true);
               
                AE(v, "p90Date", "Datum vystavení", "date", true);
                AE(v, "p90DateMaturity", "Datum splatnosti", "date", true);
                
               
                AE(v, "p90Text1", "Text zálohy", "memo");
                AE(v, "p90Text2", "Technický text zálohy", "memo");

            }
            if (v.prefix == "o22")
            {
                AE(v, "validity", "Časová platnost záznamu", "validity");
                AE(v, "roles", "Role v záznamu", "roles", true);
                AE(v, "j02ID_Owner", "Vlastník záznamu", "combo", true, true);
                AE(v, "o21ID", "Typ události/termínu", "combo", true);
                AE(v, "o22Name", "Název", "text");
                AE(v, "o22Location", "Lokalita", "text");

            }
            if (v.prefix == "o23")
            {
                AE(v, "validity", "Časová platnost záznamu", "validity");
                AE(v, "roles", "Role v záznamu", "roles", true);
                AE(v, "j02ID_Owner", "Vlastník záznamu", "combo", true, true);
                AE(v, "o18ID", "Typ dokumentu", "combo", true);
                AE(v, "o23Name", "Název dokumentu", "text");

            }
            if (v.prefix == "j02")
            {
                AE(v, "validity", "Časová platnost záznamu", "validity");
                AE(v, "j04ID", "Aplikační role", "combo", true);
                AE(v, "j07ID", "Pozice", "combo");
                AE(v, "j18ID", "Středisko", "combo");
                AE(v, "c21ID", "Pracovní fond", "combo",false,true);
                AE(v, "j02IsMustChangePassword", "Povinnost si změnit heslo", "bool", true);
                AE(v, "j02IsLoginManualLocked", "Účet uzavřený pro přihlašování", "bool", true,true);
                AE(v, "pwd", "Nové přístupové heslo", "pwd",true);

            }



        }

        private BatchUpdateElement AE(BatchUpdateViewModel v, string strField, string strName, string strType, bool bolIsNoClearValue = false, bool bolIsBreak = false,bool bolIsRequiredValue=false)
        {
            var c = new BatchUpdateElement() { Entity = v.prefix, Field = strField, Name = strName, DataType = strType,IsRequiredValue= bolIsRequiredValue };
            c.IsNoClearValue = bolIsNoClearValue;
            c.IsBreak = bolIsBreak;
            v.lisElements.Add(c);

            return c;
        }


        [HttpPost]
        public IActionResult Index(BatchUpdateViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "oper")
                {
                    v.DestComboValue = 0;
                    v.DestComboText = null;
                }
                if (v.oper == "p92ID")
                {
                    return RedirectToAction("p91changetype", "p91oper", new { pids = v.pids }); //změna typu faktury má extra režim
                }
                if (v.oper == "pwd" && string.IsNullOrEmpty(v.NewPassword))
                {
                    v.NewPassword = new BL.Code.PasswordSupport().GetRandomPassword();
                    v.NewPasswordVerify = v.NewPassword;
                }
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(v.oper))
                {
                    this.AddMessage("Musíte specifikovat [Co upravit].");
                    return View(v);
                }
                if (v.oper == "roles" && v.SelectedX67ID==0)
                {
                    this.AddMessage("Chybí výběr role.");
                    return View(v);
                }
                if (v.oper == "roles" && !v.IsClearRoleAssign)
                {
                    if (!v.x69IsAllUsers && BO.Code.Bas.ConvertString2ListInt(v.SelectedJ02IDs).Count() == 0 && BO.Code.Bas.ConvertString2ListInt(v.SelectedJ11IDs).Count() == 0)
                    {
                        this.AddMessage("Chybí přiřazení role.");
                        return View(v);
                    }
                }
                if (v.oper == "pwd")
                {
                    var res = new BL.Code.PasswordSupport().CheckPassword(v.NewPassword);
                    if (res.Flag == BO.ResultEnum.Failed)
                    {
                        this.AddMessage(res.Message); return View(v);
                    }
                    if (v.NewPassword != v.NewPasswordVerify)
                    {
                        this.AddMessage("Heslo nesouhlasí s jeho ověřením."); return View(v);
                    }
                }
                
                if (v.IsSetValue)
                {
                    if (v.oper != "p41BillingLangIndex" && v.oper != "p28BillingLangIndex")
                    {
                        if ((v.CurElement.DataType == "combo" && v.DestComboValue == 0) || (v.CurElement.DataType == "text" && string.IsNullOrEmpty(v.DestTextValue)) || (v.CurElement.DataType == "memo" && string.IsNullOrEmpty(v.DestTextValue)) || (v.CurElement.DataType == "num" && v.DestNumValue == 0))
                        {
                            this.AddMessage("Chybí specifikovat [Nová hodnota].");
                            return View(v);
                        }
                        if ((v.CurElement.DataType == "date" && v.DestDateValue == null) || (v.CurElement.DataType == "datetime" && v.DestDateValue == null))
                        {
                            this.AddMessage("Chybí specifikovat [Nová hodnota].");
                            return View(v);
                        }
                    }

                    if (v.CurElement.IsRequiredValue)
                    {
                        switch (v.CurElement.DataType)
                        {
                            case "text":
                                if (string.IsNullOrEmpty(v.DestTextValue))
                                {
                                    this.AddMessage("Chybí zadat [Hodnota].");
                                    return View(v);
                                }
                                break;
                            case "date":
                            case "datetime":
                                break;
                            case "num":
                                if (v.DestNumValue == 0)
                                {
                                    this.AddMessage("Chybí zadat [Hodnota].");
                                    return View(v);
                                }
                                break;
                        }
                    }
                }
                else
                {
                    v.DestDateValue = null; v.DestComboValue = 0; v.DestNumValue = 0; v.DestTextValue = null;
                }


                var arr = BO.Code.Bas.ConvertString2ListInt(v.pids);
                foreach (int pid in arr)
                {
                    switch (v.prefix)
                    {
                        case "p41":
                            handle_update_p41(pid, v);
                            break;
                        case "p28":
                            handle_update_p28(pid, v);
                            break;
                        case "p56":
                            handle_update_p56(pid, v);
                            break;
                        case "p91":
                            handle_update_p91(pid, v);
                            break;
                        case "o23":
                            handle_update_o23(pid, v);
                            break;
                        case "o22":
                            handle_update_o22(pid, v);
                            break;
                        case "j02":
                            handle_update_j02(pid, v);
                            break;
                        case "p40":
                            handle_update_p40(pid, v);
                            break;
                        case "p90":
                            handle_update_p90(pid, v);
                            break;
                    }




                }



                if (v.ErrsCount > 0)
                {

                    AddMessageTranslated($"Počet aktualizovaných záznamů: {v.OksCount}, počet chyb: {v.ErrsCount}.", "info");
                    RefreshState(v);
                }
                else
                {
                    v.SetJavascript_CallOnLoad(0);
                    return View(v);
                }
            }

            return View(v);
        }


        private bool ValidateRecOwner(BatchUpdateViewModel v)
        {
            var lisPids = BO.Code.Bas.ConvertString2ListInt(v.pids);
            switch (v.prefix)
            {
                case "p41":
                    var lisP41 = Factory.p41ProjectBL.GetList(new BO.myQueryP41("le5") { pids = lisPids, IsRecordValid = null });
                    foreach (var c in lisP41)
                    {
                        var disp = Factory.p41ProjectBL.InhaleRecDisposition(c.pid, c);
                        if (!disp.OwnerAccess)
                        {
                            return false;
                        }
                    }
                    break;
                case "p28":
                    var lisP28 = Factory.p28ContactBL.GetList(new BO.myQueryP28() { pids = lisPids, IsRecordValid = null });
                    foreach (var c in lisP28)
                    {
                        var disp = Factory.p28ContactBL.InhaleRecDisposition(c.pid, c);
                        if (!disp.OwnerAccess)
                        {
                            return false;
                        }
                    }
                    break;
                case "p56":
                    var lisP56 = Factory.p56TaskBL.GetList(new BO.myQueryP56() { pids = lisPids, IsRecordValid = null });
                    foreach (var c in lisP56)
                    {
                        var disp = Factory.p56TaskBL.InhaleRecDisposition(c.pid, c);
                        if (!disp.OwnerAccess)
                        {
                            return false;
                        }
                    }
                    break;
                case "o22":
                    var lisO22 = Factory.o22MilestoneBL.GetList(new BO.myQueryO22() { pids = lisPids, IsRecordValid = null });
                    foreach (var c in lisO22)
                    {
                        var disp = Factory.o22MilestoneBL.InhaleRecDisposition(c.pid, c);
                        if (!disp.OwnerAccess)
                        {
                            return false;
                        }
                    }
                    break;
                case "p91":
                    var lisP91 = Factory.p91InvoiceBL.GetList(new BO.myQueryP91() { pids = lisPids, IsRecordValid = null });
                    foreach (var c in lisP91)
                    {
                        var disp = Factory.p91InvoiceBL.InhaleRecDisposition(c.pid, c);
                        if (!disp.OwnerAccess)
                        {
                            return false;
                        }
                    }
                    break;
                case "p90":
                    var lisP90 = Factory.p90ProformaBL.GetList(new BO.myQueryP90() { pids = lisPids, IsRecordValid = null });
                    foreach (var c in lisP90)
                    {
                        var disp = Factory.p90ProformaBL.InhaleRecDisposition(c.pid, c);
                        if (!disp.OwnerAccess)
                        {
                            return false;
                        }
                    }
                    break;
                case "o23":
                    var lisO23 = Factory.o23DocBL.GetList(new BO.myQueryO23() { pids = lisPids, IsRecordValid = null });
                    foreach (var c in lisO23)
                    {
                        var disp = Factory.o23DocBL.InhaleRecDisposition(c.pid, c);
                        if (!disp.OwnerAccess)
                        {
                            return false;
                        }
                    }
                    break;
                default:
                    return Factory.CurrentUser.IsAdmin;
            }

            return true;
        }

        private void handle_update_p41(int pid, BatchUpdateViewModel v)
        {
            var rec = Factory.p41ProjectBL.Load(pid);
            switch (v.oper)
            {
                case "validity":
                    if (v.DestComboValue == 1)
                    {
                        rec.ValidUntil = DateTime.Now.AddMinutes(-1);
                    }
                    if (v.DestComboValue == 2)
                    {
                        rec.ValidUntil = new DateTime(3000, 1, 1);
                    }
                    break;
                case "roles":
                    handle_update_roles(pid, v); break;
                case "j02ID_Owner":
                    rec.j02ID_Owner = v.DestComboValue; break;
                case "p92ID":
                    rec.p92ID = v.DestComboValue; break;
                case "p42ID":
                    rec.p42ID = v.DestComboValue; break;
                case "p61ID":
                    rec.p61ID = v.DestComboValue; break;
                case "p51ID_Billing":
                    rec.p51ID_Billing = v.DestComboValue; break;
                case "j18ID":
                    rec.j18ID = v.DestComboValue; break;
                case "p41BillingLangIndex":
                    rec.p41BillingLangIndex = v.DestComboValue; break;
                case "p41InvoiceDefaultText1":
                    rec.p41InvoiceDefaultText1 = v.DestTextValue;
                    break;
                case "p41InvoiceDefaultText2":
                    rec.p41InvoiceDefaultText2 = v.DestTextValue;
                    break;
                case "p41Plan_Hours_Billable":
                    rec.p41Plan_Hours_Billable = v.DestNumValue; break;
                case "p41Plan_Expenses":
                    rec.p41Plan_Expenses = v.DestNumValue; break;
                case "p41Plan_Revenue":
                    rec.p41Plan_Revenue = v.DestNumValue; break;
                case "p41PlanUntil":
                    rec.p41PlanUntil = v.DestDateValue; break;
                case "p41PlanFrom":
                    rec.p41PlanFrom = v.DestDateValue; break;

            }

            int intPID = Factory.p41ProjectBL.Save(rec, null, null, null);
            if (intPID == 0)
            {
                v.ErrsCount += 1;
            }
            else
            {
                v.OksCount += 1;
            }
        }

        private void handle_update_p28(int pid, BatchUpdateViewModel v)
        {
             
            var rec = Factory.p28ContactBL.Load(pid);
            switch (v.oper)
            {                
                case "validity":
                    if (v.DestComboValue == 1)
                    {
                        rec.ValidUntil = DateTime.Now.AddMinutes(-1);
                    }
                    if (v.DestComboValue == 2)
                    {
                        rec.ValidUntil = new DateTime(3000, 1, 1);
                    }
                    break;
                case "roles":
                    handle_update_roles(pid, v); break;
                case "j02ID_Owner":
                    rec.j02ID_Owner = v.DestComboValue; break;
                case "p92ID":
                    rec.p92ID = v.DestComboValue; break;
                case "p29ID":
                    rec.p29ID = v.DestComboValue; break;
                case "j61ID_Invoice":
                    rec.j61ID_Invoice = v.DestComboValue; break;

                case "p51ID_Billing":
                    rec.p51ID_Billing = v.DestComboValue; break;

                case "p28BillingLangIndex":
                    rec.p28BillingLangIndex = v.DestComboValue; break;
                case "p28InvoiceDefaultText1":
                    rec.p28InvoiceDefaultText1 = v.DestTextValue;
                    break;
                case "p28InvoiceDefaultText2":
                    rec.p28InvoiceDefaultText2 = v.DestTextValue;
                    break;
                case "p28Country1":
                    rec.p28Country1 = v.DestTextValue;
                    break;
                case "p28Country2":
                    rec.p28Country2 = v.DestTextValue;
                    break;
                case "p28CountryCode":
                    rec.p28CountryCode = v.DestTextValue.ToUpper();
                    break;
                case "p24ID":
                    if (v.IsSetValue)
                    {

                    }
                    Factory.p24ContactGroupBL.Append(v.DestComboValue, new List<int>() { pid });
                    v.OksCount += 1;
                    return;
                   
                case "rejstrik":
                    DefaultZaznam c = null;
                    var cr = new BL.Code.RejstrikySupport();
                    
                    if (rec.p28RegID != null)
                    {
                        c = cr.LoadDefaultZaznam("ico", rec.p28RegID, rec.p28CountryCode).Result;
                    }
                    else
                    {
                        if (rec.p28VatID != null)
                        {
                            c = cr.LoadDefaultZaznam("dic", rec.p28VatID, rec.p28CountryCode).Result;
                        }
                    }
                    if (c != null)
                    {
                        if (c.name != null)
                        {
                            rec.p28CompanyName = c.name;
                            if (c.pravniforma == "101" && rec.p28IsCompany && c.name.Contains(" "))
                            {
                                rec.p28IsCompany = false;
                                rec.p28CompanyName = null;
                                var arr = c.name.Split(" ");
                                rec.p28LastName = arr.Last();
                                rec.p28FirstName = arr[arr.Length - 2];
                                if (arr.Length > 2 && (c.name.ToLower().Contains("ing") || c.name.ToLower().Contains("mgr") || c.name.ToLower().Contains("mudr")))
                                {
                                    rec.p28TitleBeforeName = arr[0];
                                }
                            }
                        }
                        
                        rec.p28Street1 = c.street;
                        rec.p28City1 = c.city;
                        rec.p28PostCode1 = c.zipcode;
                        rec.p28Country1 = c.country;
                        if (c.dic != null)
                        {
                            rec.p28VatID = c.dic;
                        }
                       
                    }
                    break;


            }

            int intPID = Factory.p28ContactBL.Save(rec, null, null, null, null, null);
            if (intPID == 0)
            {
                v.ErrsCount += 1;
            }
            else
            {
                v.OksCount += 1;
            }
        }

        private void handle_update_p56(int pid, BatchUpdateViewModel v)
        {
            var rec = Factory.p56TaskBL.Load(pid);
            switch (v.oper)
            {
                case "validity":
                    if (v.DestComboValue == 1)
                    {
                        rec.ValidUntil = DateTime.Now.AddMinutes(-1);
                    }
                    if (v.DestComboValue == 2)
                    {
                        rec.ValidUntil = new DateTime(3000, 1, 1);
                    }
                    break;
                case "roles":
                    handle_update_roles(pid, v); break;
                case "j02ID_Owner":
                    rec.j02ID_Owner = v.DestComboValue; break;
                case "p57ID":
                    rec.p57ID = v.DestComboValue; break;
                case "p56Plan_Hours":
                    rec.p56Plan_Hours = v.DestNumValue; break;
                case "p56Plan_Expenses":
                    rec.p56Plan_Expenses = v.DestNumValue; break;

                case "p56PlanUntil":
                    rec.p56PlanUntil = v.DestDateValue; break;
                case "p56PlanFrom":
                    rec.p56PlanFrom = v.DestDateValue; break;
                case "p56IsStopNotify":
                    rec.p56IsStopNotify = v.DestBoolValue; break;



            }

            int intPID = Factory.p56TaskBL.Save(rec, null, null);
            if (intPID == 0)
            {
                v.ErrsCount += 1;
            }
            else
            {
                v.OksCount += 1;
            }
        }

        private void handle_update_p40(int pid, BatchUpdateViewModel v)
        {
            var rec = Factory.p40WorkSheet_RecurrenceBL.Load(pid);
            switch (v.oper)
            {
                case "validity":
                    if (v.DestComboValue == 1)
                    {
                        rec.ValidUntil = DateTime.Now.AddMinutes(-1);
                    }
                    if (v.DestComboValue == 2)
                    {
                        rec.ValidUntil = new DateTime(3000, 1, 1);
                    }
                    break;
                
                case "p40FirstSupplyDate":
                    rec.p40FirstSupplyDate = v.DestDateValue; break;
                case "p40LastSupplyDate":
                    rec.p40LastSupplyDate = v.DestDateValue; break;
                case "p32ID":
                    rec.p32ID = v.DestComboValue; break;
                case "p40Percentage":
                    rec.p40Value = Math.Round(rec.p40Value * (double)(100 + v.DestNumValue) / 100.00, 2);
                    break;
                case "p40Value":
                    rec.p40Value = v.DestNumValue; break;
                case "p40GenerateDayAfterSupply":
                    rec.p40GenerateDayAfterSupply = (int) v.DestNumValue; break;

                case "p40Text":
                    rec.p40Text = v.DestTextValue; break;

                case "p40Name":
                    rec.p40Name = v.DestTextValue; break;
                case "p40FreeFee":
                    rec.p40FreeFee = v.DestNumValue; break;
                case "p40FreeHours":
                    rec.p40FreeHours = v.DestNumValue; break;

            }

            int intPID = Factory.p40WorkSheet_RecurrenceBL.Save(rec);
            if (intPID == 0)
            {
                v.ErrsCount += 1;
            }
            else
            {
                v.OksCount += 1;
            }
        }

        private void handle_update_p91(int pid, BatchUpdateViewModel v)
        {
            var rec = Factory.p91InvoiceBL.Load(pid);
            switch (v.oper)
            {
                case "validity":
                    if (v.DestComboValue == 1)
                    {
                        rec.ValidUntil = DateTime.Now.AddMinutes(-1);
                    }
                    if (v.DestComboValue == 2)
                    {
                        rec.ValidUntil = new DateTime(3000, 1, 1);
                    }
                    break;
                case "roles":
                    handle_update_roles(pid, v);break;
                case "j02ID_Owner":
                    rec.j02ID_Owner = v.DestComboValue; break;
                case "p92ID":
                    rec.p92ID = v.DestComboValue; break;
                case "p98ID":
                    rec.p98ID = v.DestComboValue; break;
                case "p80ID":
                    rec.p80ID = v.DestComboValue; break;
                case "j19ID":
                    rec.j19ID = v.DestComboValue; break;
                
                case "p63ID":
                    rec.p63ID = v.DestComboValue; break;
                case "p91Date":
                    rec.p91Date = v.DestDateValue.Value; break;
                case "p91DateMaturity":
                    rec.p91DateMaturity = v.DestDateValue.Value; break;
                case "p91DateSupply":
                    rec.p91DateSupply = v.DestDateValue.Value; break;
                case "p91Datep31_From":
                    rec.p91Datep31_From = v.DestDateValue; break;
                case "p91Datep31_Until":
                    rec.p91Datep31_Until = v.DestDateValue; break;
                case "p91Text1":
                    rec.p91Text1 = v.DestTextValue; break;
                case "p91Text2":
                    rec.p91Text2 = v.DestTextValue; break;
                case "p91locks":
                    rec.p91LockFlag = 0;
                    if (v.Isp91LockFlag2) rec.p91LockFlag += 2;
                    if (v.Isp91LockFlag4) rec.p91LockFlag += 4;
                    if (v.Isp91LockFlag8) rec.p91LockFlag += 8;
                    break;
                case "p91PortalFlag":
                    if (v.DestBoolValue)
                    {
                        rec.p91PortalFlag = BO.p91PortalFlagEnum.Published;
                    }
                    else
                    {
                        rec.p91PortalFlag = BO.p91PortalFlagEnum.None;
                    }
                    
                    break;

            }

            int intPID = Factory.p91InvoiceBL.Update(rec, null, null);
            if (intPID == 0)
            {
                v.ErrsCount += 1;
            }
            else
            {
                v.OksCount += 1;
            }
        }


        private void handle_update_p90(int pid, BatchUpdateViewModel v)
        {
            var rec = Factory.p90ProformaBL.Load(pid);
            switch (v.oper)
            {
                case "validity":
                    if (v.DestComboValue == 1)
                    {
                        rec.ValidUntil = DateTime.Now.AddMinutes(-1);
                    }
                    if (v.DestComboValue == 2)
                    {
                        rec.ValidUntil = new DateTime(3000, 1, 1);
                    }
                    break;
                case "roles":
                    handle_update_roles(pid, v); break;
                case "j02ID_Owner":
                    rec.j02ID_Owner = v.DestComboValue; break;
                case "p89ID":
                    rec.p89ID = v.DestComboValue; break;
                
                case "p90Date":
                    rec.p90Date = v.DestDateValue.Value; break;
                case "p90DateMaturity":
                    rec.p90DateMaturity = v.DestDateValue.Value; break;
               
              
                case "p90Text1":
                    rec.p90Text1 = v.DestTextValue; break;
                case "p90Text2":
                    rec.p90Text2 = v.DestTextValue; break;
                

            }

            int intPID = Factory.p90ProformaBL.Save(rec,null,null);
            if (intPID == 0)
            {
                v.ErrsCount += 1;
            }
            else
            {
                v.OksCount += 1;
            }
        }

        private void handle_update_o23(int pid, BatchUpdateViewModel v)
        {
            var rec = Factory.o23DocBL.Load(pid);
            switch (v.oper)
            {
                case "validity":
                    if (v.DestComboValue == 1)
                    {
                        rec.ValidUntil = DateTime.Now.AddMinutes(-1);
                    }
                    if (v.DestComboValue == 2)
                    {
                        rec.ValidUntil = new DateTime(3000, 1, 1);
                    }
                    break;
                case "roles":
                    handle_update_roles(pid, v); break;
                case "j02ID_Owner":
                    rec.j02ID_Owner = v.DestComboValue; break;
                case "o18ID":
                    rec.o18ID = v.DestComboValue; break;
               
                case "o23Name":
                    rec.o23Name = v.DestTextValue; break;
                

            }

            int intPID = Factory.o23DocBL.Save(rec, null, null);
            if (intPID == 0)
            {
                v.ErrsCount += 1;
            }
            else
            {
                v.OksCount += 1;
            }
        }

        private void handle_update_o22(int pid, BatchUpdateViewModel v)
        {
            var rec = Factory.o22MilestoneBL.Load(pid);
            switch (v.oper)
            {
                case "validity":
                    if (v.DestComboValue == 1)
                    {
                        rec.ValidUntil = DateTime.Now.AddMinutes(-1);
                    }
                    if (v.DestComboValue == 2)
                    {
                        rec.ValidUntil = new DateTime(3000, 1, 1);
                    }
                    break;
                case "roles":
                    handle_update_roles(pid, v); break;
                case "j02ID_Owner":
                    rec.j02ID_Owner = v.DestComboValue; break;
                case "o21ID":
                    rec.o21ID = v.DestComboValue; break;
                
                case "o22Name":
                    rec.o22Name = v.DestTextValue; break;
                case "o22Location":
                    rec.o22Location = v.DestTextValue; break;
                
            }

            int intPID = Factory.o22MilestoneBL.Save(rec, null);
            if (intPID == 0)
            {
                v.ErrsCount += 1;
            }
            else
            {
                v.OksCount += 1;
            }
        }
        private void handle_update_j02(int pid, BatchUpdateViewModel v)
        {
            var rec = Factory.j02UserBL.Load(pid);
            switch (v.oper)
            {
                case "validity":
                    if (v.DestComboValue == 1)
                    {
                        rec.ValidUntil = DateTime.Now.AddMinutes(-1);
                    }
                    if (v.DestComboValue == 2)
                    {
                        rec.ValidUntil = new DateTime(3000, 1, 1);
                    }
                    break;
                
                case "j04ID":
                    rec.j04ID = v.DestComboValue; break;
                case "c21ID":
                    rec.c21ID = v.DestComboValue; break;
                case "j07ID":
                    rec.j07ID = v.DestComboValue; break;
                case "j18ID":
                    rec.j18ID = v.DestComboValue; break;
                case "j02IsMustChangePassword":
                    rec.j02IsMustChangePassword = v.DestBoolValue;break;
                case "j02IsLoginManualLocked":
                    rec.j02IsLoginManualLocked = v.DestBoolValue; break;
                case "pwd":

                    Factory.j02UserBL.SaveNewPassword(rec.pid, v.NewPassword, false);
                    break;


            }

            int intPID = Factory.j02UserBL.Save(rec,null);
            if (intPID == 0)
            {
                v.ErrsCount += 1;
            }
            else
            {
                v.OksCount += 1;
            }
        }
        private void handle_update_roles(int pid, BatchUpdateViewModel v)
        {
            var j02ids = BO.Code.Bas.ConvertString2ListInt(v.SelectedJ02IDs);
            var j11ids = BO.Code.Bas.ConvertString2ListInt(v.SelectedJ11IDs);
            if (v.IsClearRoleAssign)
            {
                Factory.x67EntityRoleBL.ClearOneRoleAssign(pid, v.prefix, v.SelectedX67ID);
            }
            else
            {
                Factory.x67EntityRoleBL.UpdateOneRoleAssign(pid, v.prefix, v.SelectedX67ID, j02ids, j11ids, v.x69IsAllUsers);
            }
            
        }
    }
}
