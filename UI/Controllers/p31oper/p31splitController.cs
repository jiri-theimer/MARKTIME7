using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.p31oper;

namespace UI.Controllers.p31oper
{
    public class p31SplitController : BaseController
    {
        public IActionResult Index(int pid, string approve_guid)
        {
            var v = new p31splitViewModel() { pid = pid, approve_guid = approve_guid };

            RefreshState(v);

            v.Text1 = v.Rec.p31Text;
            v.Text2 = v.Text1;
            if (Factory.CurrentUser.j02DefaultHoursFormat == "T" || v.Rec.IsRecommendedHHMM())
            {
                v.Hours1 = v.Rec.p31HHMM_Orig;
            }
            else
            {
                v.Hours1 = v.Rec.p31Hours_Orig.ToString();
            }
            v.Text1Internal = v.Rec.p31TextInternal;
            v.Text2Internal = v.Rec.p31TextInternal;

            return View(v);
        }

        private void RefreshState(p31splitViewModel v)
        {
            v.Rec = Factory.p31WorksheetBL.Load(v.pid);
        }

        [HttpPost]
        public IActionResult Index(p31splitViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {

                return View(v);
            }

            if (ModelState.IsValid)
            {

                var rec = Factory.p31WorksheetBL.Load(v.pid);
                var cinput1 = Factory.p31WorksheetBL.CovertRec2Input(rec, false);
                cinput1.p31Text = v.Text1;
                cinput1.p31TextInternal = v.Text1Internal;
                cinput1.Value_Orig = v.Hours1;
                cinput1.approve_guid = v.approve_guid;

                var vlds = Factory.p31WorksheetBL.ValidateBeforeSaveOrigRecord(cinput1);
                if (!string.IsNullOrEmpty(vlds.First().ErrorMessage))
                {
                    this.AddMessageTranslated(vlds.First().ErrorMessage);
                    return View(v);
                }
                var cinput2 = Factory.p31WorksheetBL.CovertRec2Input(rec, false);
                cinput2.SetPID(0);
                cinput2.p31Text = v.Text2;
                cinput2.p31TextInternal = v.Text2Internal;
                cinput2.Value_Orig = v.Hours2;
                cinput2.approve_guid = v.approve_guid;
                vlds = Factory.p31WorksheetBL.ValidateBeforeSaveOrigRecord(cinput2);
                if (!string.IsNullOrEmpty(vlds.First().ErrorMessage))
                {
                    this.AddMessageTranslated(vlds.First().ErrorMessage);
                    return View(v);
                }

                var ff1 = new FreeFieldsViewModel();
                ff1.InhaleFreeFieldsView(Factory, rec.pid, "p31");

                Factory.p31WorksheetBL.SaveOrigRecord(cinput1, BO.p33IdENUM.Cas, ff1.inputs);

                int intPID2 = Factory.p31WorksheetBL.SaveOrigRecord(cinput2, BO.p33IdENUM.Cas, ff1.inputs);
                if (intPID2 == 0)
                {
                    return View(v);
                }
                else
                {
                    if (v.approve_guid != null)
                    {
                        var cai = new BO.p31WorksheetApproveInput() { };

                        //úprava úkonu, který se právě schvaluje
                        var mq = new BO.myQueryP31();
                        mq.SetPids(intPID2.ToString() + "," + v.pid.ToString());
                        var lisP31 = this.Factory.p31WorksheetBL.GetList(mq);



                        BO.p72IdENUM p72id = BO.p72IdENUM.Fakturovat;
                        var recTemp = Factory.p31WorksheetBL.LoadTempRecord(intPID2, v.approve_guid);
                        if (recTemp != null) p72id = recTemp.p72ID_AfterApprove;
                        Factory.p31WorksheetBL.DeleteTempRecord(v.approve_guid, intPID2);
                        Factory.p31WorksheetBL.DeleteTempRecord(v.approve_guid, v.pid);
                        BL.Code.p31Support.SetupTempApproving(this.Factory, lisP31, v.approve_guid, 0, true, p72id);
                    }
                }


                v.SetJavascript_CallOnLoad(intPID2);
                return View(v);
            }

            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
