using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models.p31approve;
using UI.Models;
using System.Web;
using BO;
using DocumentFormat.OpenXml.Wordprocessing;

namespace UI.Controllers.p31approve
{
    public class p31approveController : BaseController
    {
        private readonly BL.Singleton.ThePeriodProvider _pp;
        public p31approveController(BL.Singleton.ThePeriodProvider pp)
        {
            _pp = pp;
        }
        public IActionResult Stat(string p31guid)
        {
            var v = new p31approveMother() { p31guid = p31guid };
            return View(v);
        }
        public IActionResult Memos(string p31guid,int p28id,int p41id)
        {
            var v = new p31approveMother() { p31guid = p31guid };
            ViewBag.p28id = p28id;
            ViewBag.p41id = p41id;
            this.StopPage(true, $"p28id: {ViewBag.p28id}, p41id: {ViewBag.p41id}");
            return View(v);
        }
        public IActionResult Record(int p31id, int p72id, int approvinglevel, string p31guid)  //reczoom íčko
        {
            var v = new ZoomRecordViewModel() { p31ID = p31id, p31guid = p31guid, prefix = "p31" };

            v.Rec = v.LoadGridRecord(Factory, v.p31ID, v.p31guid);
            if (v.Rec == null)
            {
                return StopPageSubform("rec is null");
            }


            return View(v);
        }





        private void RefreshState_Inline(InlineViewModel v)
        {
            if (v.p91id > 0)
            {
                v.RecP91_Append2Invoice = Factory.p91InvoiceBL.Load(v.p91id);
            }

            if (v.lisP31 != null)
            {
                var p41ids = v.lisP31.Select(p => p.p41ID).Distinct();
                if (p41ids.Count() > 0)
                {
                    v.lisP41 = Factory.p41ProjectBL.GetList(new BO.myQueryP41("le5") { pids = p41ids.ToList() });
                }
                v.InitStatistic = CalculateStat(v.lisP31);
            }

            
        }

        

        
        public IActionResult Grid(int p91id, int approvinglevel, string p31guid)
        {
            //zde už vstupní úkony musí být uložené v p31worksheet_temp v rámci tempguid
            if (string.IsNullOrEmpty(p31guid))
            {
                return StopPage(true, "p31guid missing.");
            }


            var v = new GridViewModel() { p31guid = p31guid, p91id = p91id, approvinglevel = approvinglevel };
            v.Rec = new GridRecord();



            if (Factory.CBL.LoadUserParamInt("grid-p31-approve-p31statequery", 0) > 0 && Factory.CBL.LoadUserParamValidityMinutes("grid-p31-approve-p31statequery") > 1)
            {
                //nechceme mít na úvod filtrování podle stavu úkonů
                Factory.CBL.SetUserParam("grid-p31-approve-p31statequery", "0");
                Factory.CBL.ClearUserParamsCache();
            }

            v.j72ID = Factory.CBL.LoadUserParamInt("p31approve-j72id");
            

            RefreshState_Grid(v);
            return View(v);



        }



        [HttpPost]
        public IActionResult Grid(GridViewModel v)
        {
            RefreshState_Grid(v);
            if (v.IsPostback)
            {
                switch (v.PostbackOper)
                {
                    case "saveonly":
                    case "saveandbilling":
                    case "append2invoice":
                        return Handle_Save_Approving(v);
                   

                    case "rate":
                        UpdateTempRate(v.batchpids, v.BatchInvoiceRate, v.p31guid);
                        return View(v);
                    case "approvinglevel":
                        UpdateTempApprovingLevel(v.batchpids, v.BatchApproveLevel, v.p31guid);
                        return View(v);

                }


                return View(v);
            }



            return View(v);

        }

        private void RefreshState_Grid(GridViewModel v)
        {
            v.p72Query = Factory.CBL.LoadUserParam("p31approve-query-p72id", "0");

            v.lisP31 = Factory.p31WorksheetBL.GetList(new myQueryP31() { tempguid = v.p31guid });
            v.p31RecordsCount = v.lisP31.Count();
            HandleNavTabs(v);
            v.Rec = new GridRecord();
            string strMyQuery = $"tempguid|string|{v.p31guid}";
            if (v.SelectedTab != null)
            {
                strMyQuery += $"|tabquery|string|{v.SelectedTab}";
            }
            if (!string.IsNullOrEmpty(v.p72Query) && v.p72Query != "0")
            {
                strMyQuery += $"|p72id_afterapprove|int|{v.p72Query}";
            }
            

            v.gridinput = new Views.Shared.Components.myGrid.myGridInput() { entity = "p31Worksheet", j72id = v.j72ID, myqueryinline = strMyQuery, oncmclick = "cm_p31_local(event)", ondblclick = "", master_entity = "approve" };


            if (Factory.CurrentUser.IsHes(262144))
            {
                v.gridinput.reczoomflag = 2;    //zda zobrazovat íčko
            }


            v.gridinput.query = new InitMyQuery(Factory.CurrentUser).Load("p31", "app", 0, strMyQuery);



            if (v.p91id > 0)
            {
                v.RecP91_Append2Invoice = Factory.p91InvoiceBL.Load(v.p91id);
            }

            if (v.lisP31 != null)
            {
                var p41ids = v.lisP31.Select(p => p.p41ID).Distinct();
                if (p41ids.Count() > 0)
                {
                    v.lisP41 = Factory.p41ProjectBL.GetList(new BO.myQueryP41("le5") { pids = p41ids.ToList() });
                }
            }
            
        }


        private void HandleNavTabs(p31approveMother v)
        {

            if (v.OverGridTabs == null) v.OverGridTabs = new List<NavTab>();

            var tab = AddTab("<span class='material-icons-outlined-nosize'>functions</span>", "zero", "javascript:changetab('zero')", false, v.lisP31.Count());
            v.OverGridTabs.Add(tab);
            int x = v.lisP31.Where(p => p.p33ID == p33IdENUM.Cas).Count();
            if (x > 0)
            {
                v.OverGridTabs.Add(AddTab("Hodiny", "time", "javascript:changetab('time')", true, x));
            }
            x = v.lisP31.Where(p => (p.p33ID == p33IdENUM.PenizeBezDPH || p.p33ID == p33IdENUM.PenizeVcDPHRozpisu) && p.p34IncomeStatementFlag == p34IncomeStatementFlagENUM.Vydaj).Count();
            if (x > 0)
            {
                v.OverGridTabs.Add(AddTab("Výdaje", "expense", "javascript:changetab('expense')", true, x));
            }
            x = v.lisP31.Where(p => (p.p33ID == p33IdENUM.PenizeBezDPH || p.p33ID == p33IdENUM.PenizeVcDPHRozpisu) && p.p34IncomeStatementFlag == p34IncomeStatementFlagENUM.Prijem).Count();
            if (x > 0)
            {
                v.OverGridTabs.Add(AddTab("Odměny", "fee", "javascript:changetab('fee')", true, x));
            }
            x = v.lisP31.Where(p => p.p33ID == p33IdENUM.Kusovnik).Count();
            if (x > 0)
            {
                v.OverGridTabs.Add(AddTab("Kusovník", "kusovnik", "javascript:changetab('kusovnik')", true, x));
            }

        }


        private IEnumerable<p31Worksheet> GetRecords(string prefix, string pidsinline, string guid_pids, baseQuery ApplicableGridQuery)
        {
            var lisPIDs = new List<int>();
            var p31ids = new List<int>();
            if (!string.IsNullOrEmpty(pidsinline))
            {
                lisPIDs = BO.Code.Bas.ConvertString2ListInt(pidsinline);  //vstupní záznamy jsou předávány napřímo
            }
            else
            {
                if (guid_pids != null)
                {
                    lisPIDs = Factory.p85TempboxBL.GetPidsFromTemp(guid_pids);    //vstupní záznamy jsou uložené v tempu p85tempbox, používá se pokud je pidsinline více než 50 kusů
                }
            }


            var mq = new myQueryP31() { MyRecordsDisponible = true, p31statequery = 3 }; //nevyúčtované            
            switch (prefix)
            {
                case "p31":
                    p31ids = lisPIDs;
                    break;
                case "j02":
                case "p28":
                case "o23":
                case "p56":
                case "p41":
                    foreach (int pid in lisPIDs)
                    {
                        BO.Code.Reflexe.SetPropertyValue(mq, $"{prefix}id", pid);
                        p31ids.InsertRange(0, Factory.p31WorksheetBL.GetList(mq).Select(p => p.pid));
                    }
                    break;

            }

            if (p31ids.Count() == 0)
            {
                p31ids.Add(-1);
            }

            mq = new myQueryP31() { MyRecordsDisponible = true, p31statequery = 3, pids = p31ids, explicit_orderby = "a.p41ID,a.p31ID" };

            if (ApplicableGridQuery != null)
            {
                if (ApplicableGridQuery.p31statequery > 0)
                {
                    mq.p31statequery = ApplicableGridQuery.p31statequery;
                }


                mq.global_d1 = ApplicableGridQuery.global_d1;
                mq.global_d2 = ApplicableGridQuery.global_d2;
                mq.period_field = ApplicableGridQuery.period_field;
            }

            return Factory.p31WorksheetBL.GetList(mq);
        }




        public GridRecord LoadGridRecord(int p31id, string guid)
        {
            return new GatewayViewModel().LoadGridRecord(Factory, p31id, guid);
        }
        public Result SaveTempBatch(string pids, int p71id, int p72id, string guid)
        {
            var ret = new Result(false);
            var p31ids = BO.Code.Bas.ConvertString2ListInt(pids);
            int x = 0;
            var errs = new List<string>();

            foreach (int p31id in p31ids)
            {
                x += 1;
                var rec = Factory.p31WorksheetBL.Load(p31id);
                var recTemp = Factory.p31WorksheetBL.LoadTempRecord(p31id, guid);
                var c = new p31WorksheetApproveInput() { p31ID = p31id, Guid = guid, p33ID = recTemp.p33ID, p31Date = recTemp.p31Date };
                c.p71id = (p71IdENUM)p71id;
                c.p31ApprovingLevel = recTemp.p31ApprovingLevel;

                switch (c.p71id)
                {
                    case p71IdENUM.Nic:
                        if (!Factory.p31WorksheetBL.Save_Approving(c, true, true))
                        {
                            errs.Add("#" + x.ToString() + ": " + Factory.GetFirstNotifyMessage());
                        }
                        break;
                    case p71IdENUM.Neschvaleno:
                        if (!Factory.p31WorksheetBL.Save_Approving(c, true, true))
                        {
                            errs.Add("#" + x.ToString() + ": " + Factory.GetFirstNotifyMessage());
                        }
                        break;
                    case p71IdENUM.Schvaleno:
                        c.p72id = (p72IdENUM)p72id;
                        if (c.p72id == p72IdENUM.Fakturovat || c.p72id == p72IdENUM.FakturovatPozdeji)
                        {
                            c.Rate_Billing_Approved = recTemp.p31Rate_Billing_Orig;
                            c.Value_Approved_Billing = recTemp.p31Value_Orig;
                            c.VatRate_Approved = recTemp.p31VatRate_Approved;

                            if (c.Rate_Billing_Approved == 0) c.Rate_Billing_Approved = rec.p31Rate_Billing_Orig;
                            if (c.Value_Approved_Billing == 0) c.Value_Approved_Billing = rec.p31Value_Orig;

                        }
                        if (!Factory.p31WorksheetBL.Save_Approving(c, true, true))
                        {
                            errs.Add("#" + x.ToString() + ": " + Factory.GetFirstNotifyMessage());
                        }
                        break;
                }
            }

            if (errs.Count() > 0)
            {
                ret.Message = string.Join("<hr>", errs);
            }

            return ret;
        }

        public Result UpdateTempText(int p31id, string s, string guid)
        {
            if (Factory.p31WorksheetBL.UpdateTempText(p31id, s, guid))
            {
                return new Result(false);
            }
            else
            {
                return new Result(true, Factory.GetFirstNotifyMessage());
            }
        }
        public ApproveStat LoadStat(string guid)
        {
            return GenerateStat(guid);
        }
        public Result SaveTempRecord(GridRecord rec, int p31id, string guid,bool isbatch)
        {
            var ret = new Result(false);
            
            var c = new p31WorksheetApproveInput() { p31ID = p31id, Guid = guid, p33ID = (p33IdENUM)rec.p33id };
            var recTemp = Factory.p31WorksheetBL.LoadTempRecord(c.p31ID, c.Guid);
            
            c.p71id = (p71IdENUM)rec.p71id;
            c.p31Date = recTemp.p31Date;
            c.p31Text = rec.Popis;
            
            c.p31ApprovingLevel = rec.uroven;

            switch (c.p71id)
            {
                case p71IdENUM.Nic:
                    //rozpracováno
                                        
                    if (!Factory.p31WorksheetBL.Save_Approving(c, true, true))
                    {
                        ret.Message = Factory.GetFirstNotifyMessage();
                        ret.Flag = ResultEnum.Failed;
                        return ret;
                    }
                    else
                    {
                        return ret;
                    }
                case p71IdENUM.Neschvaleno:
                    //neschváleno
                    c.p31Text = rec.Popis;
                    if (Factory.p31WorksheetBL.Save_Approving(c, true, true))
                    {
                        return ret;
                    }
                    else
                    {
                        ret.Message = Factory.GetFirstNotifyMessage();
                        ret.Flag = ResultEnum.Failed;
                        return ret;
                    }
                case p71IdENUM.Schvaleno:
                    //schváleno
                    c.p72id = (p72IdENUM)rec.p72id;
                    c.p31Text = rec.Popis;
                    c.p31ApprovingLevel = rec.uroven;
                    break;
            }




            switch (c.p33ID)
            {
                case p33IdENUM.Cas:
                    if (c.p72id == p72IdENUM.Fakturovat || c.p72id == p72IdENUM.FakturovatPozdeji)
                    {
                        c.Value_Approved_Billing = BO.Code.Time.ShowAsDec(rec.hodiny);                        
                        if (isbatch && c.Value_Approved_Billing == 0) c.Value_Approved_Billing = recTemp.p31Value_Orig;
                        c.Rate_Billing_Approved = rec.sazba;
                        if (isbatch && c.Rate_Billing_Approved == 0) c.Rate_Billing_Approved = recTemp.p31Rate_Billing_Orig;
                    }
                    if (c.p72id == p72IdENUM.ZahrnoutDoPausalu)
                    {
                        c.p31Value_FixPrice = BO.Code.Time.ShowAsDec(rec.hodinypausal);
                    }

                    c.Value_Approved_Internal = BO.Code.Time.ShowAsDec(rec.hodinyinterni);
                    c.Rate_Internal_Approved = recTemp.p31Rate_Internal_Approved;

                    break;
                case p33IdENUM.Kusovnik:
                    if (c.p72id == p72IdENUM.Fakturovat || c.p72id == p72IdENUM.FakturovatPozdeji)
                    {
                        c.Value_Approved_Billing = Convert.ToDouble(rec.hodiny);
                        if (isbatch && c.Value_Approved_Billing == 0) c.Value_Approved_Billing = recTemp.p31Value_Orig;

                        c.Rate_Billing_Approved = rec.sazba;
                    }


                    break;
                case p33IdENUM.PenizeBezDPH:
                case p33IdENUM.PenizeVcDPHRozpisu:
                    if (c.p72id == p72IdENUM.Fakturovat || c.p72id == p72IdENUM.FakturovatPozdeji)
                    {
                        c.VatRate_Approved = rec.dphsazba;
                        if (isbatch && c.VatRate_Approved == 0) c.VatRate_Approved = recTemp.p31VatRate_Orig;
                        c.Value_Approved_Billing = rec.bezdph;
                        if (isbatch && c.Value_Approved_Billing == 0) c.Value_Approved_Billing = recTemp.p31Value_Orig;
                    }


                    break;
            }
            if (!Factory.p31WorksheetBL.Save_Approving(c, true, !isbatch))
            {
                ret.Message = Factory.GetFirstNotifyMessage();
                ret.Flag = ResultEnum.Failed;
            }
            

            return ret;

        }
        private string RenderStatValue(double n,string suffixe=null)
        {
            return $"{BO.Code.Time.FormatNumeric(n, false)}{suffixe}";
        }
        private ApproveStat CalculateStat(IEnumerable<BO.p31Worksheet> lis)
        {
            var ret = new ApproveStat();
            if (lis.Where(p => p.p71ID == BO.p71IdENUM.Nic).Count() > 0)
            {
                ret.hodiny9 = RenderStatValue(lis.Where(p => p.p71ID == BO.p71IdENUM.Nic).Sum(p => p.p31Hours_Orig), "h");
                ret.honorar_hodiny9 = $"{BO.Code.Bas.Num2StringNull(lis.Where(p => p.p71ID == BO.p71IdENUM.Nic && p.p33ID == BO.p33IdENUM.Cas).Sum(p => p.p31Amount_WithoutVat_Orig))}";
            }
            var qry = lis.Where(p => p.p71ID == BO.p71IdENUM.Schvaleno);
            ret.hodiny0 = RenderStatValue(qry.Sum(p => p.p31Hours_Orig),"h");
            ret.hodiny4 = RenderStatValue(qry.Where(p => p.p72ID_AfterApprove == BO.p72IdENUM.Fakturovat).Sum(p => p.p31Hours_Approved_Billing), "h");
            ret.hodiny2 = RenderStatValue(qry.Where(p => p.p72ID_AfterApprove == BO.p72IdENUM.ViditelnyOdpis).Sum(p => p.p31Hours_Orig), "h");
            ret.hodiny3 = RenderStatValue(qry.Where(p => p.p72ID_AfterApprove == BO.p72IdENUM.SkrytyOdpis).Sum(p => p.p31Hours_Orig), "h");
            ret.hodiny6 = RenderStatValue(qry.Where(p => p.p72ID_AfterApprove == BO.p72IdENUM.ZahrnoutDoPausalu).Sum(p => p.p31Hours_Orig), "h");
            ret.hodiny7 = RenderStatValue(qry.Where(p => p.p72ID_AfterApprove == BO.p72IdENUM.FakturovatPozdeji).Sum(p => p.p31Hours_Approved_Billing), "h");
            
            
            if (qry.Where(p => p.p72ID_AfterApprove == BO.p72IdENUM.ZahrnoutDoPausalu && p.p31Value_FixPrice != 0).Count() > 0)
            {
                ret.hodiny6 += "/" + RenderStatValue(qry.Where(p => p.p72ID_AfterApprove == BO.p72IdENUM.ZahrnoutDoPausalu).Sum(p => p.p31Value_FixPrice), "h");
            }
            if (qry.Where(p => p.p72ID_AfterApprove == BO.p72IdENUM.Fakturovat && p.j27ID_Billing_Orig == 2).Count() > 0)
            {
                ret.bezdph4czk = BO.Code.Bas.Num2StringNull(qry.Where(p => p.p72ID_AfterApprove == BO.p72IdENUM.Fakturovat && p.j27ID_Billing_Orig == 2).Sum(p => p.p31Amount_WithoutVat_Approved)) + " czk";
            }
            if (qry.Where(p => p.p72ID_AfterApprove == BO.p72IdENUM.Fakturovat && p.j27ID_Billing_Orig == 3).Count() > 0)
            {
                ret.bezdph4eur = BO.Code.Bas.Num2StringNull(qry.Where(p => p.p72ID_AfterApprove == BO.p72IdENUM.Fakturovat && p.j27ID_Billing_Orig == 3).Sum(p => p.p31Amount_WithoutVat_Approved)) + " eur";
            }

            
            double n = qry.Where(p => p.j27ID_Billing_Orig == 2).Sum(p => p.p31Amount_WithoutVat_Orig);
            if (n > 0)
            {
                ret.penize_czk0 = $"{BO.Code.Bas.Num2StringNull(n)} czk";
            }
            n = qry.Where(p => p.j27ID_Billing_Orig == 3).Sum(p => p.p31Amount_WithoutVat_Orig);
            if (n > 0)
            {
                ret.penize_eur0 = $"{BO.Code.Bas.Num2StringNull(n)} eur";
            }
            
            if (qry.Where(p =>p.p71ID==BO.p71IdENUM.Schvaleno && p.p31Amount_WithoutVat_Orig != p.p31Amount_WithoutVat_Approved).Count() > 0)
            {
                n = qry.Where(p => p.p33ID == BO.p33IdENUM.Cas && p.p31Amount_WithoutVat_Orig != p.p31Amount_WithoutVat_Approved).Sum(p => p.p31Hours_Orig - p.p31Hours_Approved_Billing);
                if (n > 0)
                {
                    ret.hodiny_minus = $"-{BO.Code.Time.FormatNumeric(n,false)}";
                }                

                n = qry.Where(p => p.j27ID_Billing_Orig == 2).Sum(p => p.p31Amount_WithoutVat_Orig - p.p31Amount_WithoutVat_Approved);
                if (n > 0)
                {
                    ret.penize_czk_minus = BO.Code.Bas.Num2StringNull(n);
                    ret.penize_czk_minus = $"-{ret.penize_czk_minus} czk";
                }
                
                n = qry.Where(p => p.p72ID_AfterApprove == BO.p72IdENUM.Fakturovat && p.j27ID_Billing_Orig == 3).Sum(p => p.p31Amount_WithVat_Orig - p.p31Amount_WithoutVat_Approved);
                if (n > 0)
                {
                    ret.penize_eur_minus = BO.Code.Bas.Num2StringNull(n);
                    ret.penize_eur_minus = $"-{ret.penize_eur_minus} eur";
                }
                
            }


            return ret;
        }
        
        private ApproveStat GenerateStat(string guid)
        {            
            var mq = new BO.myQueryP31() { tempguid = guid };
            var lis = Factory.p31WorksheetBL.GetList(mq);
            return CalculateStat(lis);

        }

        private bool SaveApproving(p31approveMother v)
        {

            var mq = new myQueryP31() { tempguid = v.p31guid };
            v.lisP31 = Factory.p31WorksheetBL.GetList(mq);
            if (v.lisP31.Count() == 0)
            {
                AddMessage("Žádné aktivity ke schválení.");
                return false;
            }

            for (int pokus = 1; pokus <= 2; pokus++)
            {
                //pokus=1:kontrola, pokus=2:uložení
                var errs = new List<string>(); int x = 1;
                if (pokus == 2 && errs.Count > 0)
                {
                    AddMessageTranslated(string.Join("<hr>", errs));
                    return false;
                }
                foreach (var rec in v.lisP31)
                {
                    var c = BL.Code.p31Support.InhaleApprovingInput(rec);

                    if (pokus == 1)
                    {
                        if (!Factory.p31WorksheetBL.Validate_Before_Save_Approving(c, false))
                        {
                            errs.Add("#" + x.ToString() + ": " + Factory.GetFirstNotifyMessage());
                        }
                    }
                    else
                    {
                        Factory.p31WorksheetBL.Save_Approving(c, false, true);
                    }

                    x += 1;
                }
            }



            return true;
        }


        private void UpdateTempRate(string pids, double newrate, string guid)
        {

            if (newrate <= 0)
            {
                AddMessage("Fakturační sazba musí být větší než NULA.");
                return;
            }
            var p31ids = BO.Code.Bas.ConvertString2ListInt(pids);
            int x = 1;

            foreach (int p31id in p31ids)
            {
                var rec = Factory.p31WorksheetBL.LoadTempRecord(p31id, guid);
                var c = BL.Code.p31Support.InhaleApprovingInput(rec);
                c.Guid = guid;
                c.Rate_Billing_Approved = newrate;
                if (rec.p33ID != p33IdENUM.Cas && rec.p33ID != p33IdENUM.Kusovnik)
                {
                    AddMessageTranslated("#" + x.ToString() + ": " + Factory.tra("Sazbu lze měnit pouze pro časové nebo kusovníkové úkony."));
                }
                else
                {
                    Factory.p31WorksheetBL.Save_Approving(c, true, true);
                }

                x += 1;
            }



        }

        private void UpdateTempApprovingLevel(string pids, int newlevel, string guid)
        {


            var p31ids = BO.Code.Bas.ConvertString2ListInt(pids);
            foreach (int p31id in p31ids)
            {
                var rec = Factory.p31WorksheetBL.LoadTempRecord(p31id, guid);
                var c = BL.Code.p31Support.InhaleApprovingInput(rec);
                c.Guid = guid;
                c.p31ApprovingLevel = newlevel;

                Factory.p31WorksheetBL.Save_Approving(c, true, true);

            }

        }


        private IActionResult Handle_Save_Approving(p31approveMother v)
        {
            switch (v.PostbackOper)
            {
                case "saveonly":
                case "saveandbilling":
                case "append2invoice":
                    if (SaveApproving(v))
                    {
                        if (v.PostbackOper== "saveonly")
                        {
                            
                            v.SetJavascript_CallOnLoad(1);  //pouze uložit schvalování a zavřít
                            return View(v);
                        }
                        if (v.PostbackOper == "saveandbilling")
                        {
                            return RedirectToAction("Index", "p31invoice", new { tempguid = v.p31guid });
                        }
                        if (v.PostbackOper == "append2invoice")
                        {
                            var dispP91 = Factory.p91InvoiceBL.InhaleRecDisposition(v.RecP91_Append2Invoice.pid, v.RecP91_Append2Invoice);
                            if (!dispP91.OwnerAccess)
                            {
                                foreach (var rec in v.lisP31)
                                {
                                    var dispP41 = Factory.p41ProjectBL.InhaleRecDisposition(rec.p41ID, null);
                                    if (v.RecP91_Append2Invoice.p91IsDraft && !dispP41.p91_DraftCreate)
                                    {
                                        AddMessageTranslated(rec.Project + ": " + Factory.tra("V projektu nemáte oprávnění vytvářet DRAFT vyúčtování."));
                                        
                                        return View(v);
                                    }
                                    if (!v.RecP91_Append2Invoice.p91IsDraft && !dispP41.p91_Create)
                                    {
                                        AddMessageTranslated(rec.Project + ": " + Factory.tra("V projektu nemáte oprávnění vytvářet DRAFT vyúčtování."));
                                        return View(v);
                                    }
                                    if (BO.Code.Bas.bit_compare_or(v.RecP91_Append2Invoice.p91LockFlag, 2))
                                    {
                                        //zámek ve faktuře pro cenové úpravě
                                        if (rec.p71ID == p71IdENUM.Schvaleno && rec.p72ID_AfterApprove == p72IdENUM.Fakturovat)
                                        {
                                            AddMessageTranslated("Faktura má zámek proti úpravě ceny faktury.");
                                            return View(v);
                                        }
                                    }
                                    if (BO.Code.Bas.bit_compare_or(v.RecP91_Append2Invoice.p91LockFlag, 4))
                                    {
                                        //zámek ve faktuře pro úkony s nulovou cenou
                                        AddMessageTranslated("Faktura má zámek i proti vkládání úkonů s nulovou cenou.");
                                        return View(v);
                                    }
                                    //if (!v.RecP91_Append2Invoice.p91IsDraft && rec.p71ID == p71IdENUM.Schvaleno && rec.p72ID_AfterApprove == p72IdENUM.Fakturovat)
                                    //{
                                    //    AddMessageTranslated(rec.Project + ": " + Factory.tra("S vaším oprávněním můžete do tohoto vyúčtování vkládat pouze úkony s nulovou fakturační cenou!"));
                                    //    return View(v);

                                    //}

                                }
                            }
                            if (Factory.p31WorksheetBL.Append2Invoice(v.p91id, v.lisP31.Select(p => p.pid).ToList()))
                            {                                
                                if (Factory.CurrentUser.j04IsModule_p91)

                                {
                                    v.SetJavascript_CallOnLoad($"/Record/RecPage?prefix=p91&pid={v.p91id}");
                                    //if (!Factory.CBL.LoadUserParamBool("p31invoice-notshowfinishpage", false))
                                    //{
                                    //    return RedirectToAction("Index", "p31InvoiceFinish", new { p91id = v.p91id });
                                    //}
                                    //else
                                    //{
                                    //    v.SetJavascript_CallOnLoad(1);
                                    //}

                                }
                                else
                                {
                                    v.SetJavascript_CallOnLoad(1);
                                }
                                return View(v);
                            }

                        }
                        v.SetJavascript_CallOnLoad(0);
                        return View(v);
                    }
                    else
                    {
                        return View(v);
                    }
                default:
                    return View(v);
            }
        }


        public IActionResult Inline(int p91id, int approvinglevel, string p31guid)
        {
            //zde už vstupní úkony musí být uložené v p31worksheet_temp v rámci tempguid
            if (string.IsNullOrEmpty(p31guid))
            {
                return StopPage(true, "p31guid missing.");
            }
            var v = new InlineViewModel() { p31guid = p31guid, p91id = p91id, approvinglevel = approvinglevel, lisRecs = new List<GridRecord>() };
            var xx = Factory.j72TheGridTemplateBL.LoadState("p31Worksheet", Factory.CurrentUser.pid, "approve", null);
            if (xx != null)
            {
                v.j72id_report = xx.j72ID;
            }
            v.OrderBy1 = Factory.CBL.LoadUserParam("p31approve-orderby1","0");
            v.p72Query = Factory.CBL.LoadUserParam("p31approve-query-p72id", "0");
            

            v.IsRenderp28Name = true;v.IsRenderp41Name = true;v.IsRenderPerson = true;v.IsRenderp34Name = true;v.IsRenderp32Name = true;
            var mq = new myQueryP31() { tempguid = v.p31guid };
            if(v.OrderBy1 != "0")
            {
                mq.explicit_orderby = v.OrderBy1;
            }
            
            v.lisP31 = Factory.p31WorksheetBL.GetList(mq);

            switch (v.p72Query)
            {
                case "4":
                    v.lisP31 = v.lisP31.Where(p => p.p72ID_AfterApprove == BO.p72IdENUM.Fakturovat);
                    break;
                case "6":
                    v.lisP31 = v.lisP31.Where(p => p.p72ID_AfterApprove == BO.p72IdENUM.ZahrnoutDoPausalu);
                    break;
                case "7":
                    v.lisP31 = v.lisP31.Where(p => p.p72ID_AfterApprove == BO.p72IdENUM.FakturovatPozdeji);
                    break;
                case "23":
                    v.lisP31 = v.lisP31.Where(p => p.p72ID_AfterApprove == BO.p72IdENUM.ViditelnyOdpis || p.p72ID_AfterApprove == BO.p72IdENUM.SkrytyOdpis);
                    break;
            }

            if (v.lisP31.Select(p => p.p28ID_Client).Distinct().Count() <= 1) v.IsRenderp28Name = false;
            if (v.lisP31.Select(p => p.p41ID).Distinct().Count() <= 1) v.IsRenderp41Name = false;
            if (v.lisP31.Select(p => p.j02ID).Distinct().Count() <= 1) v.IsRenderPerson = false;

            foreach (var c in v.lisP31)
            {
                var grec = v.LoadGridRecord(Factory, c.pid, v.p31guid, c);
               

                v.lisRecs.Add(grec);

               
            }

            RefreshState_Inline(v);
            return View(v);
        }

        [HttpPost]
        public IActionResult Inline(InlineViewModel v)
        {
            RefreshState_Inline(v);
            if (v.IsPostback)
            {
                switch (v.PostbackOper)
                {
                    case "saveonly":
                    case "saveandbilling":
                    case "append2invoice":
                        return Handle_Save_Approving(v);
                    
                }
               
                return View(v);
            }

            return View(v);
        }
    }
}
