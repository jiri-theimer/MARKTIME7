using BL;
using Microsoft.AspNetCore.Mvc;
using UI.Models.p31oper;

namespace UI.Controllers.p31oper
{
    public class p31miscController : BaseController
    {
        public BO.Result Move2ExcludeBillingFlag(int p31id, int flag)
        {
            var rec = Factory.p31WorksheetBL.Load(p31id);
            var disp = Factory.p31WorksheetBL.InhaleRecDisposition(rec);
            if (disp.OwnerAccess && disp.RecordState == BO.p31RecordState.ExcludedFromBilling)
            {                
                if (Factory.p31WorksheetBL.Move2ExcludeBillingFlag(flag, new List<int>() { p31id }))
                {
                    return new BO.Result(false);
                }
                else
                {
                    return new BO.Result(true, Factory.GetFirstNotifyMessage());
                }
            }
            else
            {
                return new BO.Result(true, Factory.tra("Chybí oprávnění k editaci úkonu"));
            }
        }
        public BO.Result UpdateText(int p31id, string s)
        {
            var rec = Factory.p31WorksheetBL.Load(p31id);
            var disp = Factory.p31WorksheetBL.InhaleRecDisposition(rec);
            if (disp.OwnerAccess && disp.RecordState == BO.p31RecordState.Editing)
            {
                if (Factory.p31WorksheetBL.UpdateText(p31id, s))
                {
                    return new BO.Result(false);
                }
                else
                {
                    return new BO.Result(true, Factory.GetFirstNotifyMessage());
                }
            }
            else
            {
                return new BO.Result(true, Factory.tra("Chybí oprávnění k editaci úkonu"));
            }
        }

        public IActionResult RateSimulation(int flag, int p41id, int j02id, int p32id, string d,int p54id)
        {
            if (!Factory.CurrentUser.IsRatesAccess)
            {
                return StopPage(true, "Nemáte oprávnění vidět sazby úkonů.");
            }
            var v = new p31RateViewModel() { j02ID = j02id, p41ID = p41id, p32ID = p32id, d = BO.Code.Bas.String2Date(d) };
            if (v.p41ID == 0 || v.p32ID == 0)
            {
                return StopPage(true, "Na vstupu chybí projekt nebo aktivita.");
            }
            v.RecP41 = Factory.p41ProjectBL.Load(v.p41ID);
            v.p51ID_BillingRate = v.RecP41.p51ID_Billing;
            v.p51ID_CostRate = v.RecP41.p51ID_Internal;
            if (v.p51ID_BillingRate == 0 && v.RecP41.p28ID_Client > 0)
            {
                v.p51ID_BillingRate = Factory.p28ContactBL.Load(v.RecP41.p28ID_Client).p51ID_Billing;
            }

            var c = Factory.p31WorksheetBL.LoadRate(BO.p51TypeFlagENUM.BillingRates, BO.Code.Bas.String2Date(d), j02id, p41id, p32id,p54id);
            v.BillRate = c.Value;
            v.j27Code_BillingRate = c.j27Code;
            v.p51ID_BillingRate = c.p51ID;
            //if (c.Value != 0 && v.p51ID_BillingRate == 0)
            //{
            //    //root ceník fakturačních sazeb
            //    var recP51Root = Factory.p51PriceListBL.LoadRootPriceList(DateTime.Now);
            //    v.p51ID_BillingRate = recP51Root.pid;

            //}
            c = Factory.p31WorksheetBL.LoadRate(BO.p51TypeFlagENUM.CostRates, BO.Code.Bas.String2Date(d), j02id, p41id, p32id,p54id);
            v.CostRate = c.Value;
            v.j27Code_CostRate = c.j27Code;

            return View(v);
        }
    }
}
