using BO;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using UI.Models;
using UI.Models.p31approve;

namespace UI.Controllers.p31approve
{
    public class p31approveinputController : BaseController
    {
        private readonly BL.Singleton.ThePeriodProvider _pp;
        const int _limitMaxPocetUkonu = 2000;
        public p31approveinputController(BL.Singleton.ThePeriodProvider pp)
        {
            _pp = pp;
        }

        public IActionResult Index(string guid_pids, string pids, string prefix, int p72id, int p91id, int approvinglevel, string tempguid, long millis, int p31statequery, string p31tabquery, string period)
        {            
            if (p31statequery !=1 && p31statequery !=3 && p31statequery != 4)
            {
                p31statequery = Factory.CBL.LoadUserParamInt("approve-index-p31statequery", 3);
            }
            
            if ((string.IsNullOrEmpty(pids) || string.IsNullOrEmpty(prefix)) && string.IsNullOrEmpty(guid_pids))
            {
                if (string.IsNullOrEmpty(tempguid)) return this.StopPage(true, "pids or prefix or guid_pids missing.");
            }


            var v = new GatewayViewModel() {p31statequery=p31statequery,p31tabquery=p31tabquery, p31guid = tempguid, guid_pids = guid_pids, pidsinline = pids, prefix = prefix, p91id = p91id, p72id = (BO.p72IdENUM)p72id, approvinglevel = approvinglevel };
            
            RefreshState(v);

            if (v.prefix == "p31")
            {

                SetupTempData4Approving(v);
                if (v.lisP31.Count() == 0)
                {
                   
                    this.AddMessage("Na vstupu chybí nevyúčtované úkony nebo se jedná o interní projekt nebo nemáte potřebné oprávnění.");
                    
                    return View(v);
                }

                return LeaveGateview(v);    //na vstupu je výběr konkrétních p31id -> rovnou přejít do schvalovacího rozhraní
            }

            
            
            v.IsSkipGateway = Factory.CBL.LoadUserParamBool("approve-index-IsSkipGateway", false);


            if (BO.Code.Time.CompareMillisecondsFrom1970(millis) < 2000)    //explicitně předaný filtr podle stavu úkonů v časovém limitu
            {
                if (p31statequery > 0 && v.p31statequery != p31statequery)
                {
                    v.p31statequery = p31statequery;
                    
                }
              
                int intStringVersionValue = 0;
                if (!string.IsNullOrEmpty(period) && v.periodinput.IsDifferentWithStringVersion(period, ref intStringVersionValue))
                {
                    if (intStringVersionValue > 0)  //pokud je na vstupu nulové období, pak ho zde přímo nedávat
                    {
                        v.periodinput.LoadFromString(_pp, this.Factory, period);
                        v.periodinput.SaveUserSetting(this.Factory);
                        Factory.CBL.ClearUserParamsCache();
                    }


                }

                
            }
            
            RefreshState(v);

            if (v.lisInputPids.Count() == 1 && (v.prefix == "le5" || v.prefix == "p41" || v.prefix == "p56" || v.prefix == "p28"))
            {
                if (Factory.CBL.LoadUserParamInt($"recpage-{v.prefix}--pid") != v.lisInputPids.First())
                {
                    Factory.CBL.SetUserParam($"recpage-{v.prefix}--pid", v.lisInputPids.First().ToString());    //uložit naposledy vybraný záznam
                }                
            }

            return View(v);
        }

        private void RefreshState(GatewayViewModel v)
        {
            v.lisInputPids = new List<int>();
            if (!string.IsNullOrEmpty(v.pidsinline))
            {
                v.lisInputPids = BO.Code.Bas.ConvertString2ListInt(v.pidsinline);  //vstupní záznamy jsou předávány napřímo
            }
            else
            {
                if (v.guid_pids != null)
                {
                    v.lisInputPids = Factory.p85TempboxBL.GetPidsFromTemp(v.guid_pids);    //vstupní p31 záznamy jsou uložené v tempu p85tempbox, používá se pokud je pidsinline více než 50 kusů
                }
            }


            
            v.periodinput = new Views.Shared.Components.myPeriod.myPeriodViewModel() { prefix = "p31", UserParamKey = "approve-index-period" };
            v.periodinput.LoadUserSetting(_pp, Factory);    //načtení období pro filtr
            
            

            Refresh_p31Query(v);


            
            string myqueryinline = $"p31statequery|int|{v.p31statequery}|p31tabquery|string|{v.p31tabquery}|MyRecordsDisponible_Approve|bool|1";
            switch (v.prefix)
            {
                case "p31":
                    myqueryinline = $"{myqueryinline}|pids|list_int|{string.Join(",", v.lisInputPids)}";
                    break;
                case "le4":
                case "le3":
                case "le2":
                case "le1":
                    myqueryinline = $"{myqueryinline}|p41ids|list_int|{v.pidsinline}|leindex|int|{v.prefix.Substring(2,1)}";
                    break;
                default:
                    myqueryinline = $"{myqueryinline}|{BO.Code.Entity.GetPrefixDb(v.prefix)}ids|list_int|{string.Join(",", v.lisInputPids)}";
                    break;
            }
           

            v.gridinput = new Views.Shared.Components.myGrid.myGridInput() { isperiodovergrid = true, is_enable_selecting = false,myqueryinline=myqueryinline, entity = "p31Worksheet", oncmclick = "", ondblclick = "" };

            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load("p31", null, 0, myqueryinline);
           
            v.gridinput.query.global_d1 = v.periodinput.d1;
            v.gridinput.query.global_d2 = v.periodinput.d2;
            v.gridinput.query.period_field = v.periodinput.PeriodField;
            v.gridinput.isperiodovergrid = true;

        }

        

        [HttpPost]
        public IActionResult Index(GatewayViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "p31statequery")
                {
                    Factory.CBL.SetUserParam("approve-index-p31statequery", v.p31statequery.ToString());                    
                }
                
                return View(v);
            }
            if (ModelState.IsValid)
            {
                

                SetupTempData4Approving(v); //naplnit tabulku p31worksheet_temp

                if (v.lisP31.Count() > _limitMaxPocetUkonu)
                {
                    this.AddMessage("Do jednoho vyúčtování lze vložit maximálně 2.000 úkonů.");
                    v.IsSkipGateway = false;
                    return View(v);
                }

                if (v.lisP31.Count() == 0)
                {
                    this.AddMessage("Pro schvalování chybí nevyúčtované úkony nebo nemáte potřebné oprávnění.");
                    v.IsSkipGateway = false;
                    return View(v);
                }
            }
               
            return LeaveGateview(v);

        }

        private IActionResult LeaveGateview(GatewayViewModel v) //přechod do schvalovacího rozhraní
        {
            string strDestAction = "Inline";
            if (v.lisP31 != null && v.lisP31.Count() > 500)
            {
                strDestAction = "Grid"; //nad 500 záznamů je výchozí rozhraní Grid (kvůli výkonu)
            }
            else
            {
                strDestAction = Factory.CBL.LoadUserParam("approve-default-ui", "Inline");
            }
            //if (Factory.CurrentUser.j02Login == "lamos@dev01.cz")
            //{
            //    strDestAction = "Inline2";
            //}
            
            return RedirectToAction(strDestAction, "p31approve", new { p31guid = v.p31guid, approvinglevel = v.approvinglevel, p91id = v.p91id });

        }

        private void SetupTempData4Approving(GatewayViewModel v)
        {
            if (string.IsNullOrEmpty(v.p31guid))
            {
                v.lisP31 = Factory.p31WorksheetBL.GetList(v.myQueryP31);               
                
                v.p31guid = BO.Code.Bas.GetGuid();  //vygenerovat p31guid
                if (v.lisP31.Count() <= _limitMaxPocetUkonu)
                {
                    BL.Code.p31Support.SetupTempApproving(this.Factory, v.lisP31, v.p31guid, v.approvinglevel, Factory.CurrentUser.IsHes(131072), v.p72id);

                    
                }
                
            }
            else
            {
                var mq = new BO.myQueryP31() { tempguid = v.p31guid };
                v.lisP31 = Factory.p31WorksheetBL.GetList(mq);

            }

            
        }

        private void Refresh_p31Query(GatewayViewModel v)
        {
            
            v.myQueryP31 = new BO.myQueryP31() { MyRecordsDisponible_Approve = true }; //nevyúčtované            
            switch (v.prefix)
            {
                case "p31":
                    v.myQueryP31.pids = v.lisInputPids;
                    v.myQueryP31.p31statequery = 3;
                    break;
                case "j02":
                    v.myQueryP31.j02ids = v.lisInputPids;
                    break;
                case "p28":
                    v.myQueryP31.p28ids = v.lisInputPids;
                    break;
                case "o23":
                    //v.p31Query.o23id = v.lisInputPids;
                    break;
                case "p56":
                    v.myQueryP31.p56ids = v.lisInputPids;
                    break;
                case "p41":
                case "le5":
                    v.myQueryP31.p41ids = v.lisInputPids;
                    break;
                case "le4":
                case "le3":
                case "le2":
                case "le1":
                    v.myQueryP31.p41ids = v.lisInputPids;
                    v.myQueryP31.leindex = Convert.ToInt32(v.prefix.Substring(2, 1));
                    break;
                    

            }

            if (v.prefix != "p31")
            {
                v.myQueryP31.p31statequery = v.p31statequery;

                if (v.periodinput.PeriodValue > 0)
                {
                    v.myQueryP31.period_field = v.periodinput.PeriodField;
                    v.myQueryP31.global_d1 = v.periodinput.d1;
                    v.myQueryP31.global_d2 = v.periodinput.d2;
                }

                v.myQueryP31.tabquery = v.p31tabquery;
            }






        }
    }
}
