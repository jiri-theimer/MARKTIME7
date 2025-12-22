using Microsoft.AspNetCore.Mvc;
using UI.Models.p31oper;

namespace UI.Controllers.p31oper
{
    public class p31korekceController : BaseController
    {
        public IActionResult Index(string pids, string guid_pids)
        {
            if (!string.IsNullOrEmpty(guid_pids))
            {
                pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            var v = new p31KorekceViewModel() { pids = pids, pids_valid = pids };

            RefreshState(v);


            if (v.lisP31.Count() < BO.Code.Bas.ConvertString2ListInt(v.pids).Count())
            {
                AddMessageTranslated("Schválené nebo vyúčtované úkony jsem vyřadil z výběru.", "info");
            }

            if (v.lisP31.Count() == 0)
            {
                return this.StopPage(true, "Na vstupu nejsou rozpracované úkony.");
            }


            return View(v);
        }

        private void RefreshState(p31KorekceViewModel v)
        {
            var mq = new BO.myQueryP31() { pids = BO.Code.Bas.ConvertString2ListInt(v.pids), p31statequery = 1 };


            v.lisP31 = Factory.p31WorksheetBL.GetList(mq);
            v.pids_valid = string.Join(",", v.lisP31.Select(p => p.pid));

            



            v.gridinput = new Views.Shared.Components.myGrid.myGridInput() { is_enable_selecting = false, entity = "p31Worksheet", master_entity = "inform", myqueryinline = $"pids|list_int|{v.pids_valid}", oncmclick = "", ondblclick = "" };
            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load("p31", null, 0, $"pids|list_int|{v.pids_valid}");
            v.gridinput.fixedcolumns = "a__p31Worksheet__p31Date,p31_j02__j02User__fullname_desc,p31_p41_client__p28Contact__p28Name,p31_p41__p41Project__p41Name,p31_p32__p32Activity__p32Name,p31_p32__p32Activity__p32IsBillable,a__p31Worksheet__p31Hours_Orig,a__p31Worksheet__p31Rate_Billing_Orig,a__p31Worksheet__p31Amount_WithoutVat_Orig,a__p31Worksheet__p31Text";
        }

        [HttpPost]
        public IActionResult Index(p31KorekceViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {

                return View(v);
            }

            if (ModelState.IsValid)
            {
                int intOks = 0;int intErrs = 0;


                foreach (var c in v.lisP31)
                {
                    var rec = Factory.p31WorksheetBL.CovertRec2Input(c, false);
                    rec.p72ID_AfterTrimming = (BO.p72IdENUM)v.p72ID_AfterTrimming;

                    if (Factory.p31WorksheetBL.SaveOrigRecord(rec, c.p33ID, null) > 0)
                    {
                        intOks += 1;
                    }
                    else
                    {
                        intErrs += 1;
                    }
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
