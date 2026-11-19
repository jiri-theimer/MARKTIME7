using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.p31oper;

namespace UI.Controllers.p31oper
{
    public class p31recalcController : BaseController
    {
        public IActionResult Index(string pids,string guid_pids)
        {
            if (!string.IsNullOrEmpty(guid_pids))
            {
                pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            var v = new p31RecalcViewModel() { pids = pids, pids_valid = pids,WhatRates="fakturacni" };

            RefreshState(v);
            //if (v.lisP31.Count() == 0)
            //{
            //    AddMessageTranslated("Pro zvolený druh sazeb nevyhovují vybrané časové úkony.", "info");

            //}

            //RefreshState(v);

            

            if (v.WhatRates=="fakturacni" && v.lisP31.Count() < BO.Code.Bas.ConvertString2ListInt(v.pids).Count())
            {
                AddMessageTranslated("Schválené nebo vyúčtované úkony jsem vyřadil z výběru.", "info");
            }


            return View(v);
        }

        private const int MaxCheckRows = 3000;      //nad tento počet úkonů se kontrola sazeb neprovádí (jeden dotaz na sazbu za každou kombinaci den+osoba+projekt+aktivita)

        private void RefreshState(p31RecalcViewModel v)
        {
            var mq = new BO.myQueryP31() {tabquery= "time_or_kusovnik", pids = BO.Code.Bas.ConvertString2ListInt(v.pids) };
            if (v.WhatRates == "fakturacni")
            {
                mq = new BO.myQueryP31() { tabquery = "time_or_kusovnik", p31statequery = 1, pids = BO.Code.Bas.ConvertString2ListInt(v.pids) };
            }            

            v.lisP31 = Factory.p31WorksheetBL.GetList(mq);
            
            //kontrola fakturačních sazeb: uložená sazba proti sazbě podle ceníku
            v.lisCheck = null; v.IsCheckSkipped = false;
            if (v.WhatRates == "fakturacni")
            {
                if (v.lisP31.Count() > MaxCheckRows)
                {
                    v.IsCheckSkipped = true;
                }
                else
                {
                    v.lisCheck = Factory.p31WorksheetBL.CheckBillingRates(v.lisP31);
                }
            }

            v.pids_valid = string.Join(",", v.lisP31.Select(p => p.pid));
            if (string.IsNullOrEmpty(v.pids_valid))
            {
                v.pids_valid = "-999";
            }

            v.gridinput = new Views.Shared.Components.myGrid.myGridInput() {is_enable_selecting=false, entity = "p31Worksheet", master_entity = "inform", myqueryinline = $"pids|list_int|{v.pids_valid}", oncmclick = "", ondblclick = "" };
            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load("p31", null, 0, $"pids|list_int|{v.pids_valid}");
            v.gridinput.fixedcolumns = "a__p31Worksheet__p31Date,p31_j02__j02User__fullname_desc,p31_p41_client__p28Contact__p28Name,p31_p41__p41Project__p41Name,p31_p32__p32Activity__p32Name,p31_p32__p32Activity__p32IsBillable,a__p31Worksheet__p31Hours_Orig,a__p31Worksheet__p31Rate_Billing_Orig,a__p31Worksheet__p31Amount_WithoutVat_Orig,a__p31Worksheet__p31Rate_Internal_Orig,a__p31Worksheet__p31Rate_Overhead,a__p31Worksheet__p31Text";
        }

        [HttpPost]
        public IActionResult Index(p31RecalcViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {

                return View(v);
            }

            if (ModelState.IsValid)
            {
                int intOks = 0;
                switch (v.WhatRates)
                {
                    case "fakturacni":
                        var pidsRecalc = v.lisP31.Select(p => p.pid).ToList();
                        if (v.lisCheck != null)
                        {
                            //úkony s ručně zadaným honorářem se nepřepočítávají nikdy - přepočet by jejich částku přepsal sazbou z ceníku
                            var manual = v.lisCheck.Where(p => p.IsManualFee).Select(p => p.p31ID).ToHashSet();
                            pidsRecalc = pidsRecalc.Where(p => !manual.Contains(p)).ToList();
                            if (v.IsOnlyDifferent)
                            {
                                var diff = v.lisCheck.Where(p => p.IsDifferent).Select(p => p.p31ID).ToHashSet();
                                pidsRecalc = pidsRecalc.Where(p => diff.Contains(p)).ToList();
                            }
                        }
                        if (pidsRecalc.Count == 0)
                        {
                            this.AddMessage("Všechny sazby odpovídají ceníku, není co přepočítat.", "info");
                            return View(v);
                        }
                        intOks = Factory.p31WorksheetBL.Recalc(pidsRecalc, 1);
                        break;
                    case "nakladove":
                        intOks = Factory.p31WorksheetBL.Recalc(v.lisP31.Select(p => p.pid).ToList(), 2);
                        break;
                    case "rezijni":
                        intOks = Factory.p31WorksheetBL.Recalc(v.lisP31.Select(p => p.pid).ToList(), 3);
                        break;
                    case "efektivni":
                        intOks = Factory.p31WorksheetBL.Recalc(v.lisP31.Select(p => p.pid).ToList(), 4);
                        break;
                }                
                if (intOks == 0)
                {
                    this.AddMessage("Chyba");
                    return View(v);
                }

                v.SetJavascript_CallOnLoad(0);
                return View(v);
            }

            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
