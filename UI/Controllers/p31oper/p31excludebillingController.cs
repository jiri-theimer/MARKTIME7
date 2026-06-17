
using Microsoft.AspNetCore.Mvc;
using UI.Models.p31oper;

namespace UI.Controllers.p31oper
{
    public class p31excludebillingController : BaseController
    {
        private readonly BL.Singleton.ThePeriodProvider _pp;
        public p31excludebillingController(BL.Singleton.ThePeriodProvider pp)
        {
            _pp = pp;
        }

        public IActionResult Index(string pids,int flag,string prefix,string guid_pids)
        {
            if ((string.IsNullOrEmpty(pids) || string.IsNullOrEmpty(prefix)) && string.IsNullOrEmpty(guid_pids))
            {
                return this.StopPage(true, "pids/guid_pids missing.");
            }
            if (!string.IsNullOrEmpty(guid_pids))
            {
                pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }

           
            var v = new p31ExcludeBillingViewModel() {pids = pids, p31ExcludeBillingFlag=flag,prefix=prefix };
            v.IsOnlyHours = Factory.CBL.LoadUserParamBool("p31excludebilling-IsOnlyHours", true);
            RefreshState(v);

            
            if (v.lisP31.Count() == 0)
            {
                if (v.p31ExcludeBillingFlag == 1)
                {
                    v.p31ExcludeBillingFlag = 0;
                }
                else
                {
                    v.p31ExcludeBillingFlag = 1;
                }
                
                RefreshState(v);
            }
            
            if (v.prefix=="p31" && v.lisP31.Count() == 1 && BO.Code.Bas.ConvertString2ListInt(pids).Count()==1)
            {
                if (SaveChanges(v))
                {
                    v.SetJavascript_CallOnLoad(0);
                    return View(v);
                }
            }

            return View(v);
        }

        private void RefreshState(p31ExcludeBillingViewModel v)
        {
            v.periodinput = new Views.Shared.Components.myPeriod.myPeriodViewModel() { prefix = "p31", UserParamKey = "p31exclude-period" };
            v.periodinput.LoadUserSetting(_pp, Factory);    //načtení období pro filtr

            var mq = new BO.myQueryP31() {MyRecordsDisponible=true };
            if (v.prefix != "p31")
            {
                mq.period_field = v.periodinput.PeriodField;
                mq.global_d1 = v.periodinput.d1;
                mq.global_d2 = v.periodinput.d2;
            }
            if (v.IsOnlyHours)
            {
                mq.tabquery = "time";   //pouze hodiny
            }
            if (v.p31ExcludeBillingFlag == 1)
            {
                
                mq.explicit_sqlwhere = "a.p71ID IS NULL AND a.p91ID IS NULL";
            }
            if (v.p31ExcludeBillingFlag == 0)
            {
                mq.p31statequery = 15;  //vyloučené z vyúčtování
            }
            else
            {
                mq.p31statequery = 20;   //p91id is null
            }
            
           
            string strMyqueryinline = $"p31statequery|int|{mq.p31statequery}";
            switch (v.prefix)
            {
                case "p31":                    
                    mq.pids = BO.Code.Bas.ConvertString2ListInt(v.pids);
                    strMyqueryinline = $"{strMyqueryinline}|pids|list_int|{v.pids}";
                    
                    break;
                case "p41":
                case "le5":                
                    mq.p41ids = BO.Code.Bas.ConvertString2ListInt(v.pids);
                    strMyqueryinline = $"{strMyqueryinline}|p41ids|list_int|{v.pids}";
                    break;
                case "le4":
                case "le3":
                case "le2":
                case "le1":
                    mq.p41ids = BO.Code.Bas.ConvertString2ListInt(v.pids);
                    mq.leindex = Convert.ToInt32(v.prefix.Substring(2, 1));
                    strMyqueryinline = $"{strMyqueryinline}|p41ids|list_int|{v.pids}|leindex|int|{mq.leindex}";
                    break;
                case "p28":
                    mq.p28ids = BO.Code.Bas.ConvertString2ListInt(v.pids);
                    strMyqueryinline = $"{strMyqueryinline}|p28ids|list_int|{v.pids}";
                    break;
                case "p56":
                    mq.p56ids = BO.Code.Bas.ConvertString2ListInt(v.pids);
                    strMyqueryinline = $"{strMyqueryinline}|p56ids|list_int|{v.pids}";
                    break;
                case "j02":
                    mq.j02ids = BO.Code.Bas.ConvertString2ListInt(v.pids);
                    strMyqueryinline = $"{strMyqueryinline}|j02ids|list_int|{v.pids}";
                    break;
            }

            v.lisP31 = Factory.p31WorksheetBL.GetList(mq);

            if (v.lisP31.Count() == 10000)
            {
                this.AddMessageTranslated("Na jednu transakci dokážu zpracovat maximálně 10.000 záznamů.", "info");
            }

            v.gridinput = new Views.Shared.Components.myGrid.myGridInput() { is_enable_selecting = false, entity = "p31Worksheet", master_entity = "inform", myqueryinline = strMyqueryinline, oncmclick = "", ondblclick = "" };
            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load("p31", null, 0, strMyqueryinline);
            v.gridinput.query.period_field = v.periodinput.PeriodField;
            if (v.IsOnlyHours)
            {
                v.gridinput.query.p31tabquery = "time";
            }            
            v.gridinput.query.global_d1 = v.periodinput.d1;
            v.gridinput.query.global_d2 = v.periodinput.d2;
        }

        [HttpPost]
        public IActionResult Index(p31ExcludeBillingViewModel v)
        {
            

            RefreshState(v);

            if (v.IsPostback)
            {

                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (SaveChanges(v))
                {
                    Factory.CBL.SetUserParam("p31excludebilling-IsOnlyHours", v.IsOnlyHours.ToString());
                    v.SetJavascript_CallOnLoad(0);
                    return View(v);
                }

                
            }

            this.Notify_RecNotSaved();
            return View(v);
        }

        private bool SaveChanges(p31ExcludeBillingViewModel v)
        {
            if (v.lisP31.Count() == 0)
            {
                this.AddMessageTranslated("Na vstupu chybí úkony.");
                return false;
            }
            if (v.lisP31.Count() > 15000)
            {
                this.AddMessageTranslated("Na vstupu je více než 15.000 úkonů. Kvůli výkonu je třeba snížit počet záznamů.");
                return false;
            }
            var arr = new List<int>();
            foreach (var c in v.lisP31)
            {
                var disp = Factory.p31WorksheetBL.InhaleRecDisposition(c);
                if (disp.OwnerAccess)
                {
                    arr.Add(c.pid);
                }
            }
            if (arr.Count() == 0)
            {
                this.AddMessageTranslated("Na vstupu chybí úkony s editačním oprávněním.");
                return false;
            }

            bool bolOK = Factory.p31WorksheetBL.Move2ExcludeBillingFlag(v.p31ExcludeBillingFlag, arr);

            if (!bolOK)
            {
                this.AddMessageTranslated("Nedošlo k nahození požadovaného stavu u vybraných úkonů.");
                return false;
            }

            return true;
        }
    }
}
