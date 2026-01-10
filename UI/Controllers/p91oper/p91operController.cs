using BL;
using BO.Rejstrik;
using Microsoft.AspNetCore.Mvc;

using UI.Models;
using UI.Models.p91oper;
using UI.Views.Shared.Components.myGrid;

namespace UI.Controllers
{
    public class p91operController : BaseController
    {
        BL.TheColumnsProvider _cp;
        public p91operController(BL.TheColumnsProvider cp)
        {
            _cp = cp;
        }

       
        public IActionResult checkaddress(string pids_prefix,string pids, string guid_pids)
        {            
            if (string.IsNullOrEmpty(pids_prefix))
            {
                pids_prefix = "p91";
            }
            var v = new checkaddressViewModel();

            if (!string.IsNullOrEmpty(guid_pids))
            {
                if (pids_prefix == "p91")
                {
                    v.input_p91ids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
                }
                else
                {
                    v.input_p28ids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
                }
            }
            else
            {
                if (pids_prefix == "p91")
                {
                    v.input_p91ids = pids;
                }
                else
                {
                    v.input_p28ids = pids;
                }
            }

            if (!string.IsNullOrEmpty(v.input_p91ids))
            {
                var lis = BO.Code.Bas.ConvertString2ListInt(v.input_p91ids);
                if (lis.Count() == 0)
                {
                    return this.StopPage(true, "p91ids missing");
                }
                var lisP91 = Factory.p91InvoiceBL.GetList(new BO.myQueryP91() { IsRecordValid = null, pids = lis });
                if (lisP91.Count() == 0)
                {
                    return this.StopPage(true, "Na vstupu nejsou faktury");
                }
                v.input_p28ids = string.Join(",", lisP91.Select(p => p.p28ID));
            }

            if (BO.Code.Bas.ConvertString2ListInt(v.input_p28ids).Count() == 0)
            {
                return this.StopPage(true, "Na vstupu chybí seznam kontaktů ke kontrole.");
            }
            RefreshState_checkaddress(v);

            if (BO.Code.Bas.ConvertString2ListInt(v.input_p28ids).Count() <= 10)
            {
                Handle_CheckSubjects(v);
            }
            

            return View(v);
        }

        private void RefreshState_checkaddress(checkaddressViewModel v)
        {
            var mq = new BO.myQueryP28() { pids = BO.Code.Bas.ConvertString2ListInt(v.input_p28ids) };
            if (mq.pids.Count() > 0)
            {
                v.lisP28 = Factory.p28ContactBL.GetList(mq).Where(p => p.p28RegID != null || p.p28VatID != null || p.p28ICDPH_SK != null);
                foreach(var c in v.lisP28)
                {
                    c.UserInsert = null;c.UserUpdate = null;c.b02ID = -1;
                }
            }

            
            
        }

        [HttpPost]        
        public IActionResult checkaddress(checkaddressViewModel v)
        {
            RefreshState_checkaddress(v);
            if (v.IsPostback)
            {                
                return View(v);
            }

            if (ModelState.IsValid)
            {
                Handle_CheckSubjects(v);

            }

            return View(v);
        }

        private void Handle_CheckSubjects(checkaddressViewModel v)
        {
            
            var cr = new BL.Code.RejstrikySupport();
            var hc = new HttpClient();

            foreach (var recP28 in v.lisP28)
            {
                recP28.b02ID = -2;
                DefaultZaznam c = null;
                if (recP28.p28RegID != null)
                {
                    c = cr.LoadDefaultZaznam("ico", recP28.p28RegID, recP28.p28CountryCode, hc).Result;
                }
                else
                {
                    if (recP28.p28VatID != null)
                    {
                        c = cr.LoadDefaultZaznam("dic", recP28.p28VatID, recP28.p28CountryCode, hc).Result;
                    }
                }
                recP28.UserInsert = null; recP28.UserUpdate = null;
                if (c != null)
                {
                    string strZmena = null;
                    if (BO.Code.Bas.Diff2String(recP28.p28Street1, c.street)) strZmena = c.street;
                    if (strZmena == null && BO.Code.Bas.Diff2String(recP28.p28City1, c.city)) strZmena = $"{recP28.p28City1} <kbd>x</kbd> {c.city}";
                    if (strZmena == null && BO.Code.Bas.Diff2String(recP28.p28PostCode1, c.zipcode)) strZmena =$"{recP28.p28PostCode1} <kbd>x</kbd> {c.zipcode}";
                    if (recP28.p28IsCompany)
                    {
                        if (BO.Code.Bas.Diff2String(recP28.p28CompanyName, c.name))
                        {                            
                            strZmena = $"{recP28.p28CompanyName} <kbd>x</kbd> {c.name}";
                        }
                    }


                    if (strZmena != null)
                    {
                        recP28.UserInsert = strZmena;
                    }
                    recP28.UserUpdate = c.fulladdress;
                    recP28.p28Street2 = c.street;
                    recP28.p28City2 = c.city;
                    recP28.p28PostCode2 = c.zipcode;
                    recP28.p28ShortName = c.name;
                }
                else
                {
                    recP28.UserInsert = "<kbd>Chybí IČO/DIČ</kbd>";
                }
            }

            
        }

        //převést draft fakturu na oficiální číslo
        public string converfromdraft(int p91id)
        {
            if (Factory.p91InvoiceBL.ConvertFromDraft(p91id))
            {
                return "1";
            }
            else
            {
                return "0";
            }

        }

        //spárovat fakturu s uhrazenou zálohou
        public IActionResult proforma(int p91id)
        {
            var v = new proformaViewModel() { p91ID = p91id, FilterCustomerOnly = true, PodilProcento = 100 };
            if (v.p91ID == 0)
            {
                return this.StopPage(true, "Na vstupu chybí faktura.");
            }
            RefreshStateProforma(v);

            return View(v);
        }
        private void RefreshStateProforma(proformaViewModel v)
        {
            if (v.RecP91 == null)
            {
                v.RecP91 = Factory.p91InvoiceBL.Load(v.p91ID);
            }
            if (v.RecP91.p91VatRate_Standard > 0.00 && v.RecP91.p91Amount_Vat_Standard > 0.00 && v.RecP91.p91Amount_Vat_Low == 0.00)
            {
                v.ZbyvaUhraditBezDph = v.RecP91.p91Amount_Debt / (1 + v.RecP91.p91VatRate_Standard / 100);
            }
            if (v.RecP91.p91VatRate_Low > 0.00 && v.RecP91.p91Amount_Vat_Low > 0.00 && v.RecP91.p91Amount_Vat_Standard == 0.00)
            {
                v.ZbyvaUhraditBezDph = v.RecP91.p91Amount_Debt / (1 + v.RecP91.p91VatRate_Low / 100);
            }
            if (v.RecP91.p91Amount_Debt == v.RecP91.p91Amount_TotalDue)
            {
                v.ZbyvaUhraditBezDph = v.RecP91.p91Amount_WithoutVat;
            }
            
            v.lisP99 = Factory.p91InvoiceBL.GetList_p99(0, v.p91ID, 0);
            if (v.SelectedP90ID > 0)
            {
                v.lisP82 = Factory.p82Proforma_PaymentBL.GetList(v.SelectedP90ID);
                if (v.SelectedP82ID == 0 && v.lisP82.Count() > 0)
                {
                    v.SelectedP82ID = v.lisP82.First().pid;
                }
            }
        }
        private void Handle_Recalc_Proforma(proformaViewModel v)
        {
            if (v.SelectedP82ID == 0) return;
            if (v.PodilProcento == 0) v.PodilProcento = 100;
            var recP82 = Factory.p82Proforma_PaymentBL.Load(v.SelectedP82ID);
            var proc = Convert.ToDouble(v.PodilProcento);
            v.PodilBezDph = Math.Round(recP82.p82Amount_WithoutVat * proc / 100, 2);
            v.PodilCelkem = recP82.p82Amount;
            v.PodilDph = v.PodilCelkem - v.PodilBezDph;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult proforma(proformaViewModel v, int p99id)
        {
            RefreshStateProforma(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper == "delete" && p99id > 0)
                {
                    if (Factory.p91InvoiceBL.DeleteP99(p99id))
                    {
                        v.SetJavascript_CallOnLoad(v.p91ID);
                        return View(v);
                    }
                }
                if (v.PostbackOper == "filter")
                {
                    v.SelectedP90ID = 0; v.SelectedP90Alias = null; v.SelectedP82ID = 0;
                }
                if (v.PostbackOper == "procento" || v.PostbackOper == "p82id" || v.PostbackOper == "p90id")
                {
                    Handle_Recalc_Proforma(v);
                }


                return View(v);
            }





            if (ModelState.IsValid)
            {
                var c = new BO.p99Invoice_Proforma() { p91ID = v.p91ID };
                var proc = Convert.ToDouble(v.PodilProcento);
                if (v.PodilBezDph > 0)
                {
                    proc = v.PodilBezDph * 100000;
                }
                if (Factory.p91InvoiceBL.SaveP99(v.p91ID, v.SelectedP90ID, v.SelectedP82ID, proc))
                {
                    v.SetJavascript_CallOnLoad(v.p91ID);
                    return View(v);
                }


            }

            this.Notify_RecNotSaved();
            return View(v);
        }



        //hromadné operace nad položkami vyúčtování
        public IActionResult p31operbatch(string baseoper, int p91id, string p31ids)
        {
            var v = new p31operbatchViewModel() { p91ID = p91id, p31ids = p31ids, BaseOper = baseoper };
            if (v.p91ID == 0 || string.IsNullOrEmpty(v.BaseOper))
            {
                return this.StopPage(true, "p91id or baseoper missing");
            }
            if (string.IsNullOrEmpty(v.p31ids))
            {
                return this.StopPage(true, "Na vstupu chybí úkony.");
            }
            if (v.BaseOper == "p70-6") v.SelectedOper = 6;
            if (v.BaseOper == "p70-2") v.SelectedOper = 2;
            if (v.BaseOper == "p70-3") v.SelectedOper = 3;
            RefreshStateOperBatch(v);

            return View(v);
        }
        private void RefreshStateOperBatch(p31operbatchViewModel v)
        {
            v.RecP91 = Factory.p91InvoiceBL.Load(v.p91ID);
            var pids = BO.Code.Bas.ConvertString2ListInt(v.p31ids);
            v.lisP31 = Factory.p31WorksheetBL.GetList(new BO.myQueryP31() { pids = pids });

            v.gridinput = new myGridInput() { is_enable_selecting = false, entity = "p31Worksheet", master_entity = "inform", myqueryinline = "pids|list_int|" + v.p31ids, oncmclick = "", ondblclick = "" };
            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load("p31", null, 0, "pids|list_int|" + v.p31ids);
            v.gridinput.fixedcolumns = "p31Date,p31_j02__j02User__fullname_desc,p31_p41__p41Project__p41Name,p31_p32__p32Activity__p32Name,p31Rate_Billing_Invoiced,p31Amount_WithoutVat_Invoiced,p31VatRate_Invoiced,p31Text";

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult p31operbatch(p31operbatchViewModel v)
        {
            RefreshStateOperBatch(v);
            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                var p31ids = v.lisP31.Select(p => p.pid).ToList();
                if (v.BaseOper == "remove")
                {
                    if (v.SelectedOper == 0)
                    {
                        this.AddMessage("Chybí cílový stav úkonu."); return View(v);
                    }

                    if (!Factory.p31WorksheetBL.RemoveFromInvoice(v.RecP91.pid, p31ids))
                    {
                        return View(v);
                    }
                    switch (v.SelectedOper)
                    {
                        case 1:
                            break;
                        case 2:
                            Factory.p31WorksheetBL.RemoveFromApprove(p31ids);
                            break;
                        case 3:
                            Factory.p31WorksheetBL.RemoveFromApprove(p31ids);
                            //Factory.p31WorksheetBL.Move2Bin(true, p31ids);
                            break;
                    }
                }
                if (v.BaseOper.Substring(0, 3) == "p70")
                {
                    var lis = new List<BO.p31WorksheetInvoiceChange>();
                    foreach (var c in v.lisP31)
                    {
                        lis.Add(new BO.p31WorksheetInvoiceChange() { p31IsInvoiceManual=true, p31ID = c.pid, p70ID = (BO.p70IdENUM)v.SelectedOper });
                    }
                    if (!Factory.p31WorksheetBL.UpdateInvoice(v.p91ID, lis, null))
                    {
                        return View(v);
                    }
                }

                v.SetJavascript_CallOnLoad(p31ids.First());
                return View(v);
            }

            this.Notify_RecNotSaved();
            return View(v);
        }


        //přesunout položku do jiného vyúčtování
        public IActionResult p31move2invoice(int p31id)
        {
            var v = new p31move2invoiceViewModel() { p31ID = p31id };
            if (v.p31ID == 0)
            {
                return this.StopPage(true, "Na vstupu chybí úkon.");
            }
            RefreshStateMove2Invoice(v);

            return View(v);
        }
        private void RefreshStateMove2Invoice(p31move2invoiceViewModel v)
        {
            v.RecP91 = Factory.p91InvoiceBL.LoadByP31ID(v.p31ID);
            v.Rec = Factory.p31WorksheetBL.Load(v.p31ID);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult p31move2invoice(p31move2invoiceViewModel v)
        {
            RefreshStateMove2Invoice(v);
            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (!Factory.p31WorksheetBL.Move2Invoice(v.SelectedP91ID, v.p31ID))
                {
                    return View(v);
                }

                v.SetJavascript_CallOnLoad(v.p31ID);
                return View(v);
            }

            this.Notify_RecNotSaved();
            return View(v);
        }

        public IActionResult p31info(int p31id)
        {            
            return View(new BaseViewModel());
        }

        //vyjmout položku z vyúčtování
        public IActionResult p31remove(int p31id)
        {
            var v = new p31removeViewModel() { p31ID = p31id };
            if (v.p31ID == 0)
            {
                return this.StopPage(true, "Na vstupu chybí úkon.");
            }
            RefreshStateRemove(v);

            return View(v);
        }
        private void RefreshStateRemove(p31removeViewModel v)
        {
            v.RecP91 = Factory.p91InvoiceBL.LoadByP31ID(v.p31ID);
            v.Rec = Factory.p31WorksheetBL.Load(v.p31ID);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult p31remove(p31removeViewModel v)
        {
            RefreshStateRemove(v);
            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (v.SelectedOper == 0)
                {
                    this.AddMessage("Chybí cílový stav úkonu."); return View(v);
                }
                var p31ids = new List<int> { v.p31ID };
                if (!Factory.p31WorksheetBL.RemoveFromInvoice(v.RecP91.pid, p31ids))
                {
                    return View(v);
                }

                switch (v.SelectedOper)
                {
                    case 1:
                        break;
                    case 2:
                        Factory.p31WorksheetBL.RemoveFromApprove(p31ids);
                        break;
                    case 3:
                        Factory.p31WorksheetBL.RemoveFromApprove(p31ids);
                        Factory.p31WorksheetBL.Move2ExcludeBillingFlag(1, p31ids);
                        break;
                }



                v.SetJavascript_CallOnLoad(v.p31ID);
                return View(v);
            }

            this.Notify_RecNotSaved();
            return View(v);
        }

        //upravit položku vyúčtování
        public IActionResult p31edit(int p31id)
        {
            var v = new p31editViewModel() { p31ID = p31id };
            if (v.p31ID == 0)
            {
                return this.StopPage(true, "Na vstupu chybí úkon.");
            }
            RefreshStateEdit(v);
            
            v.p31Text = v.Rec.p31Text;
            v.p31TextInternal = v.Rec.p31TextInternal;
            v.SelectedP70ID = v.Rec.p70ID;
            switch (v.Rec.p33ID)
            {
                case BO.p33IdENUM.Cas:
                    v.p31Value_Invoiced = v.Rec.p31Hours_Invoiced;
                    v.Hours = v.Rec.p31Hours_Invoiced.ToString();
                    v.Hours_FixPrice = v.Rec.p31Value_FixPrice.ToString();
                    v.p31Rate_Billing_Invoiced = v.Rec.p31Rate_Billing_Invoiced;

                    break;
                case BO.p33IdENUM.Kusovnik:
                    v.p31Value_Invoiced = v.Rec.p31Value_Invoiced;
                    v.p31Rate_Billing_Invoiced = v.Rec.p31Rate_Billing_Invoiced;
                    break;
                default:
                    break;
            }

            v.p31VatRate_Invoiced = v.Rec.p31VatRate_Invoiced;
            v.p31Amount_WithoutVat_Invoiced = v.Rec.p31Amount_WithoutVat_Invoiced;
            v.p31Amount_Vat_Invoiced = v.Rec.p31Amount_Vat_Invoiced;
            v.p31Text = v.Rec.p31Text;
            v.p31Code = v.Rec.p31Code;
            v.p31Ordinary = v.Rec.p31Ordinary;
            var tg = Factory.o51TagBL.GetTagging("p31", v.p31ID);
            v.TagPids = tg.TagPids;
            v.TagNames = tg.TagNames;
            v.TagHtml = tg.TagHtml;
            return View(v);
        }
        private void RefreshStateEdit(p31editViewModel v)
        {
            v.RecP91 = Factory.p91InvoiceBL.LoadByP31ID(v.p31ID);
            v.Rec = Factory.p31WorksheetBL.Load(v.p31ID);

            if (v.ff1 == null)
            {
                v.ff1 = new FreeFieldsViewModel();
                v.ff1.InhaleFreeFieldsView(Factory, v.p31ID, "p31");
            }
            v.ff1.RefreshInputsVisibility(Factory, v.p31ID, "p31", v.Rec.p34ID);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult p31edit(p31editViewModel v)
        {
            RefreshStateEdit(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper == "recalc_vat")
                {
                    v.p31Amount_Vat_Invoiced = v.p31Amount_WithoutVat_Invoiced * v.p31VatRate_Invoiced / 100;
                }
                return View(v);
            }

            if (ModelState.IsValid)
            {
                var c = new BO.p31WorksheetInvoiceChange() { p31ID = v.Rec.pid, p33ID = v.Rec.p33ID, p32ManualFeeFlag = v.Rec.p32ManualFeeFlag };
                c.p31IsInvoiceManual = true;    //částky úkonu se odvíjejí z tohoto formuláře a nikoliv ze schválených hodnot
                c.TextUpdate = v.p31Text;
                c.TextInternalUpdate = v.p31TextInternal;
                c.p70ID = v.SelectedP70ID;
                
                c.p31Code = v.p31Code;
                c.p31Ordinary = v.p31Ordinary;

                if (v.SelectedP70ID == BO.p70IdENUM.Vyfakturovano)
                {
                    c.InvoiceVatRate = v.p31VatRate_Invoiced;

                    switch (v.Rec.p33ID)
                    {
                        case BO.p33IdENUM.Cas:
                            c.InvoiceValue = BO.Code.Time.ShowAsDec(v.Hours);
                            c.InvoiceRate = v.p31Rate_Billing_Invoiced;
                            break;
                        case BO.p33IdENUM.Kusovnik:
                            c.InvoiceValue = v.p31Value_Invoiced;
                            c.InvoiceRate = v.p31Rate_Billing_Invoiced;
                            break;
                        default:
                            c.InvoiceValue = v.p31Amount_WithoutVat_Invoiced;
                            c.InvoiceVatAmount = v.p31Amount_Vat_Invoiced;
                            break;
                    }
                }
                if (v.SelectedP70ID == BO.p70IdENUM.ZahrnutoDoPausalu)
                {
                    if (v.Rec.p33ID == BO.p33IdENUM.Cas)
                    {
                        c.FixPriceValue = BO.Code.Time.ShowAsDec(v.Hours_FixPrice);
                    }
                }



                var lis = new List<BO.p31WorksheetInvoiceChange>();
                lis.Add(c);

                if (Factory.p31WorksheetBL.UpdateInvoice(v.RecP91.pid, lis, v.ff1.inputs))
                {
                    Factory.o51TagBL.SaveTagging("p31", c.p31ID, v.TagPids);

                    v.SetJavascript_CallOnLoad(v.p31ID);
                    return View(v);
                }


            }

            this.Notify_RecNotSaved();
            return View(v);
        }


        //změnit typ faktury
        public IActionResult p91changetype(string pids)
        {
            var v = new p91changetypeViewModel() { pids = pids,IsUpdateIssuer=true};
            if (BO.Code.Bas.ConvertString2List(pids).Count() == 0)
            {
                return this.StopPage(true, "Na vstupu chybí faktura.");
            }
            RefreshStateChangeType(v);

            
            foreach (var c in v.lisP91)
            {
                var disp = Factory.p91InvoiceBL.InhaleRecDisposition(c.pid, c);
                if (!disp.OwnerAccess)
                {
                    return this.StopPage(true, "U jedné z faktur nemáte vlastnické oprávnění k záznamu.");
                }
                
            }

            


            return View(v);
        }
        private void RefreshStateChangeType(p91changetypeViewModel v)
        {
            v.lisP91 = Factory.p91InvoiceBL.GetList(new BO.myQueryP91() { pids = BO.Code.Bas.ConvertString2ListInt(v.pids) });
            
            if (v.SelectedP92ID == 0 && v.lisP91 !=null && v.lisP91.Count()>0)
            {
                v.SelectedP92ID = v.lisP91.First().p92ID;
            }
            v.lisP92 = Factory.p92InvoiceTypeBL.GetList(new BO.myQuery("p92"));
            v.RecP92 = Factory.p92InvoiceTypeBL.Load(v.SelectedP92ID);
            if (v.RecP92 != null)
            {
                v.RecP93 = Factory.p93InvoiceHeaderBL.Load(v.RecP92.p93ID);
                v.RecX38 = Factory.x38CodeLogicBL.Load(v.RecP92.x38ID);
            }
            
            

        }
        [HttpPost]        
        public IActionResult p91changetype(p91changetypeViewModel v)
        {
            RefreshStateChangeType(v);
            if (v.IsPostback)
            {
                if (v.lisP91.Where(p => !p.p91IsDraft && p.x38ID != v.RecP92.x38ID).Count() > 0)
                {
                    v.IsUpdateP91Code = true;
                }
                
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (v.SelectedP92ID == 0)
                {
                    this.AddMessage("Chybí vybrat cílový typ faktury.");
                    return View(v);
                }
                foreach(var c in v.lisP91)
                {
                    c.p92ID = v.SelectedP92ID;
                    if (v.IsUpdateIssuer)
                    {
                        c.p91Supplier = v.RecP93.p93Company;
                        c.p91Supplier_RegID = v.RecP93.p93RegID;
                        c.p91Supplier_VatID = v.RecP93.p93VatID;
                        c.p91Supplier_ICDPH_SK = v.RecP93.p93ICDPH_SK;
                        c.p91Supplier_City = v.RecP93.p93City;
                        c.p91Supplier_Street = v.RecP93.p93Street;
                        c.p91Supplier_ZIP = v.RecP93.p93Zip;
                        c.p91Supplier_Country = v.RecP93.p93Country;
                        c.p91Supplier_Registration = v.RecP93.p93Registration;
                    }
                    int intPID = Factory.p91InvoiceBL.Update(c, null, null);

                    if (intPID == 0)
                    {
                        this.AddMessageTranslated(Factory.GetFirstNotifyMessage());
                        return View(v);
                    }
                    if (v.IsUpdateP91Code && c.x38ID !=v.RecX38.pid)
                    {
                        Factory.p91InvoiceBL.RecoveryP91Code(c.pid);
                    }

                    if (v.IsUpdateJ27ID)
                    {
                        Factory.p91InvoiceBL.ChangeCurrency(c.pid, v.RecP92.j27ID);
                    }
                    if (v.SelectedX15ID != BO.x15IdEnum.Nic)
                    {
                        var recP91 = Factory.p91InvoiceBL.Load(c.pid);
                        var sazba = Factory.p53VatRateBL.NajdiSazbu(recP91.p91DateSupply, v.SelectedX15ID, recP91.j27ID);
                        Factory.p91InvoiceBL.ChangeVat(recP91.pid, (int)v.SelectedX15ID, sazba);
                    }
                }
                

                
                
                
                v.SetJavascript_CallOnLoad(v.lisP91.First().pid);
                return View(v);
            }

            this.Notify_RecNotSaved();
            return View(v);
        }


        //odstranit vyúčtování
        public IActionResult p91delete(int p91id)
        {
            var v = new p91deleteViewModel() { p91ID = p91id, TempGuid = BO.Code.Bas.GetGuid() };
            if (v.p91ID == 0)
            {
                return this.StopPage(true, "Na vstupu chybí faktura.");
            }
            RefreshStateDelete(v);


            return View(v);
        }
        private void RefreshStateDelete(p91deleteViewModel v)
        {
            v.RecP91 = Factory.p91InvoiceBL.Load(v.p91ID);
            v.gridinput = new myGridInput() { is_enable_selecting = false, entity = "p31Worksheet", master_entity = "inform", myqueryinline = "p91id|int|" + v.p91ID.ToString(), oncmclick = "", ondblclick = "" };
            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load("p31", null, 0, "p91id|int|" + v.p91ID.ToString());
            v.gridinput.fixedcolumns = "p31Date,p31_j02__j02User__fullname_desc,p31_p41__p41Project__p41Name,p31_p32__p32Activity__p32Name,p31Rate_Billing_Invoiced,p31Amount_WithoutVat_Invoiced,p31VatRate_Invoiced,p31Text";
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult p91delete(p91deleteViewModel v)
        {
            RefreshStateDelete(v);
            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (v.SelectedOper == 0)
                {
                    this.AddMessage("Musíte zvolit, jak naložit s úkony ve vyúčtování."); return View(v);
                }


                if (!Factory.p91InvoiceBL.Delete(v.p91ID, v.TempGuid, v.SelectedOper))
                {
                    return View(v);
                }
                v.SetJavascript_CallOnLoad(v.p91ID);
                return View(v);
            }

            this.Notify_RecNotSaved();
            return View(v);
        }

        //Vytvořit dobropis
        public IActionResult creditnote(int p91id)
        {
            var v = new creditnoteViewModel() { p91ID = p91id };
            if (v.p91ID == 0)
            {
                return this.StopPage(true, "Na vstupu chybí faktura.");
            }
            RefreshStateCreditNote(v);


            return View(v);
        }
        private void RefreshStateCreditNote(creditnoteViewModel v)
        {
            v.RecP91 = Factory.p91InvoiceBL.Load(v.p91ID);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult creditnote(creditnoteViewModel v)
        {
            RefreshStateCreditNote(v);
            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (v.SelectedP92ID == 0)
                {
                    this.AddMessage("Chybí typ opravného dokladu."); return View(v);
                }

                int intPID = Factory.p91InvoiceBL.CreateCreditNote(v.p91ID, v.SelectedP92ID);
                if (intPID > 0)
                {
                    v.SetJavascript_CallOnLoad(v.p91ID);
                    return View(v);
                }

            }

            this.Notify_RecNotSaved();
            return View(v);
        }





        //Převést kompletně na jinou sazbu DPH
        public IActionResult vat(int p91id)
        {
            var v = new vatViewModel() { p91ID = p91id };
            if (v.p91ID == 0)
            {
                return this.StopPage(true, "Na vstupu chybí faktura.");
            }
            RefreshStateVAT(v);


            return View(v);
        }

        private void RefreshStateVAT(vatViewModel v)
        {
            v.RecP91 = Factory.p91InvoiceBL.Load(v.p91ID);
            v.gridinput = new myGridInput() { is_enable_selecting = false, entity = "p31Worksheet", master_entity = "inform", myqueryinline = "p91id|int|" + v.p91ID.ToString(), oncmclick = "", ondblclick = "" };
            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load("p31", null, 0, "p91id|int|" + v.p91ID.ToString());
            v.gridinput.fixedcolumns = "p31Date,p31_j02__j02User__fullname_desc,p31_p41__p41Project__p41Name,p31_p32__p32Activity__p32Name,p31Rate_Billing_Invoiced,p31Amount_WithoutVat_Invoiced,p31VatRate_Invoiced,p31Text";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult vat(vatViewModel v)
        {
            RefreshStateVAT(v);
            if (v.IsPostback)
            {
                var rec = Factory.p91InvoiceBL.Load(v.p91ID);
                v.VatRate = Factory.p53VatRateBL.NajdiSazbu(DateTime.Now, v.SelectedX15ID, rec.j27ID,Factory.Lic.x01CountryCode);
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if ((int)v.SelectedX15ID == 0)
                {
                    this.AddMessage("Musíte vybrat cílovou DPH hladinu."); return View(v);
                }

                if (!Factory.p91InvoiceBL.ChangeVat(v.p91ID, (int)v.SelectedX15ID, v.VatRate))
                {
                    return View(v);
                }
                v.SetJavascript_CallOnLoad(v.p91ID);
                return View(v);
            }

            this.Notify_RecNotSaved();
            return View(v);
        }


        //Aktualizovat měnový kurz faktury
        public IActionResult exupdate(int p91id)
        {
            var v = new exupdateViewModel() { p91ID = p91id };
            if (v.p91ID == 0)
            {
                return this.StopPage(true, "Na vstupu chybí faktura.");
            }
            v.RecP91 = Factory.p91InvoiceBL.Load(v.p91ID);
            return View(v);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult exupdate(exupdateViewModel v)
        {
            v.RecP91 = Factory.p91InvoiceBL.Load(v.p91ID);
            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                Factory.p91InvoiceBL.ClearExchangeDate(v.p91ID, true);
                v.SetJavascript_CallOnLoad(v.p91ID);
                return View(v);
            }

            this.Notify_RecNotSaved();
            return View(v);
        }
        public IActionResult p96(int p91id)
        {
            var v = new p96ViewModel() { p91ID = p91id };
            v.lisP96 = Factory.p91InvoiceBL.GetList_p96(v.p91ID,null);
            v.RecP91 = Factory.p91InvoiceBL.Load(v.p91ID);
            return View(v);
        }

        //Převést na jinou měnu:
        public IActionResult j27(int p91id)
        {
            var v = new j27ViewModel() { p91ID = p91id };
            if (v.p91ID == 0)
            {
                return this.StopPage(true, "Na vstupu chybí faktura.");
            }
            RefreshStateJ27(v);

            return View(v);
        }

        private void RefreshStateJ27(j27ViewModel v)
        {
            if (v.RecP91 == null)
            {
                v.RecP91 = Factory.p91InvoiceBL.Load(v.p91ID);
            }
            v.lisJ27 = Factory.FBL.GetListCurrency();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult j27(j27ViewModel v)
        {
            RefreshStateJ27(v);

            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (v.SelectedJ27ID == 0)
                {
                    this.AddMessage("Musíte vybrat cílovou měnu vyúčtování."); return View(v);
                }
                if (Factory.p91InvoiceBL.ChangeCurrency(v.p91ID, v.SelectedJ27ID))
                {
                    v.SetJavascript_CallOnLoad(v.p91ID);
                    return View(v);
                }
            }

            this.Notify_RecNotSaved();
            return View(v);
        }

        //Zapsat úhradu faktury:
        public IActionResult p94(int p91id)
        {
            var v = new p94ViewModel() { p91ID = p91id };
            if (v.p91ID == 0)
            {
                return this.StopPage(true, "Na vstupu chybí faktura.");
            }
            RefreshStateP94(v);
            v.Rec = new BO.p94Invoice_Payment() { p94Date = DateTime.Today, p94Amount = v.RecP91.p91Amount_Debt };


            return View(v);
        }

        private void RefreshStateP94(p94ViewModel v)
        {
            if (v.lisP94 == null)
            {
                v.lisP94 = Factory.p91InvoiceBL.GetList_p94(v.p91ID);
            }
            if (v.RecP91 == null)
            {
                v.RecP91 = Factory.p91InvoiceBL.Load(v.p91ID);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult p94(p94ViewModel v, int p94id)
        {
            RefreshStateP94(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper == "mail")   //uložit a notifikovat
                {
                    var c = new BO.p94Invoice_Payment() { p91ID = v.p91ID, p94Amount = v.Rec.p94Amount, p94Date = v.Rec.p94Date, p94Description = v.Rec.p94Description };
                    c.pid = Factory.p91InvoiceBL.SaveP94(c);
                    if (c.pid > 0)
                    {
                        int intJ61ID = Factory.CBL.LoadUserParamInt("mail-p94-j61id");
                        if (intJ61ID == 0)
                        {
                            var lisJ61 = Factory.j61TextTemplateBL.GetList(new BO.myQueryJ61()).Where(p => p.j61Entity == "p94");
                            if (lisJ61.Count() > 0)
                            {
                                intJ61ID = lisJ61.First().pid;
                            }
                        }                        
                        return RedirectToAction("SendMail", "Mail", new { record_entity = "p94", record_pid = c.pid,j61id=intJ61ID });
                    }
                    
                }

                if (v.PostbackOper == "delete" && p94id > 0)
                {
                    if (Factory.p91InvoiceBL.DeleteP94(p94id, v.p91ID))
                    {
                        v.SetJavascript_CallOnLoad(v.p91ID);
                        return View(v);
                    }
                }

                return View(v);
            }




            if (ModelState.IsValid)
            {
                var c = new BO.p94Invoice_Payment() { p91ID = v.p91ID, p94Amount = v.Rec.p94Amount, p94Date = v.Rec.p94Date, p94Description = v.Rec.p94Description };
                c.pid = Factory.p91InvoiceBL.SaveP94(c);
                if (c.pid > 0)
                {

                    v.SetJavascript_CallOnLoad(v.p91ID);
                    return View(v);
                }

            }

            this.Notify_RecNotSaved();
            return View(v);
        }

    }
}
