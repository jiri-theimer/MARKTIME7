using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using MO.Models;

namespace MO.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        public BL.Factory Factory;

        /// <summary>možné hodnoty: subform, modal, record nebo nic, pokud je volající stránka z _Layout</summary>
        public string ParentLayoutName { get; set; }


        /// <summary>Chybová zpráva — zobrazí se přes layout (přežije redirect)</summary>
        protected void SetMessage(string msg) => TempData["Message"] = msg;

        /// <summary>Úspěšná zpráva — zobrazí se přes layout (přežije redirect)</summary>
        protected void SetMessageSuccess(string msg) => TempData["MessageSuccess"] = msg;


        /// <summary>
        /// Spočítá data pro sdílenou partial view _Stats.cshtml (souhrn hodin, čerpání fondu,
        /// utilizace a naposledy vykazované projekty za zadané období). Volá se z libovolného
        /// controlleru, který tenhle blok chce zobrazit (Home/Index, Stats/Index, ...).
        /// </summary>
        protected StatsViewModel BuildStats(DateTime date1, DateTime date2, string label = null)
        {
            var v = new StatsViewModel
            {
                d1 = date1,
                d2 = date2,
                PeriodLabel = !string.IsNullOrEmpty(label) ? label : $"{date1:d.M.} – {date2:d.M.yyyy}"
            };

            var qryP31 = Factory.p31WorksheetBL.GetList(new BO.myQueryP31
            {
                j02id = Factory.CurrentUser.pid,
                global_d1 = date1,
                global_d2 = date2
            }).ToList();

            // Vykázané hodiny celkem / fakturovatelné
            v.Hodiny = qryP31.Sum(p => p.p31Hours_Orig);
            v.HodinyFa = qryP31.Where(p => p.p32IsBillable).Sum(p => p.p31Hours_Orig);

            // Rozpracované (stav "Nic", ještě nevyúčtováno)
            v.HodinyWip = qryP31.Where(p => p.p71ID == BO.p71IdENUM.Nic && p.p91ID == 0).Sum(p => p.p31Hours_Orig);
            v.HodinyFaWip = qryP31.Where(p => p.p32IsBillable && p.p71ID == BO.p71IdENUM.Nic && p.p91ID == 0).Sum(p => p.p31Hours_Orig);
            v.HodinyNeFaWip = qryP31.Where(p => !p.p32IsBillable && p.p71ID == BO.p71IdENUM.Nic && p.p91ID == 0).Sum(p => p.p31Hours_Orig);

            // Schváleno, čeká na vyúčtování
            v.Hodiny4Approve = qryP31.Where(p => p.p71ID == BO.p71IdENUM.Schvaleno && p.p91ID == 0).Sum(p => p.p31Hours_Orig);

            // Vyúčtováno (jen s oprávněním na sazby)
            v.HasRatesAccess = Factory.CurrentUser.IsRatesAccess;
            if (v.HasRatesAccess)
            {
                v.HodinyVyfa4 = qryP31.Where(p => p.p91ID > 0 && p.p70ID == BO.p70IdENUM.Vyfakturovano).Sum(p => p.p31Hours_Orig);
                v.HodinyVyfa6 = qryP31.Where(p => p.p91ID > 0 && p.p70ID == BO.p70IdENUM.ZahrnutoDoPausalu).Sum(p => p.p31Hours_Orig);
                v.HodinyVyfa23 = qryP31.Where(p => p.p91ID > 0
                    && (p.p70ID == BO.p70IdENUM.SkrytyOdpis || p.p70ID == BO.p70IdENUM.ViditelnyOdpis))
                    .Sum(p => p.p31Hours_Orig);
            }

            // Fond pracovní doby
            var recJ02 = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
            if (recJ02 != null)
            {
                v.Fond = recJ02.c21ScopeFlag == BO.c21ScopeFlagENUM.PerTimesheet
                    ? v.Hodiny
                    : Factory.c21FondCalendarBL.GetSumHours(recJ02.c21ID, recJ02.j02CountryCode, date1, date2);
            }

            if (v.Fond > 0)
            {
                v.UtilTotal = 100.00 * v.Hodiny / v.Fond;
                v.UtilFa = 100.00 * v.HodinyFa / v.Fond;
            }

            // Top 15 naposledy vykazovaných projektů
            v.Top15Projects = Factory.p41ProjectBL.GetList_MyTop10(Factory.CurrentUser.pid, 15);

            return v;
        }


        // Test probíhá před spuštěním každé Akce
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // přijatého uživatele přemostit do Factory (stejný vzor jako v UI)
            var ru = (BO.RunningUser)HttpContext.RequestServices.GetService(typeof(BO.RunningUser));
            if (string.IsNullOrEmpty(ru.j02Login))
            {
                ru.j02Login = context.HttpContext.User.Identity.Name;
            }

            if (this.Factory == null)
            {
                this.Factory = (BL.Factory)HttpContext.RequestServices.GetService(typeof(BL.Factory));
            }

            if (Factory.CurrentUser == null || Factory.CurrentUser.isclosed)
            {
                context.Result = new RedirectResult("~/Login/UserLogin");
                return;
            }

            if (Factory.CurrentUser.j02IsMustChangePassword && !IsCurrentContextNoRestriction(context))
            {
                // V MO zatím nepodporujeme změnu hesla - přesměrujeme na UI.
                // Až bude MO mít vlastní ChangePassword view, změnit na lokální action.
                context.Result = new RedirectResult("~/Login/UserLogin?msg=mustchangepassword");
                return;
            }

            var queryString = context.HttpContext.Request.Query;
            queryString.TryGetValue("layout", out StringValues someString);
            this.ParentLayoutName = someString;
        }


        private bool IsCurrentContextNoRestriction(ActionExecutingContext context)
        {
            string strPage = context.RouteData.Values["action"].ToString();
            if (strPage == "Logout" || strPage == "UpdateCurrentUserPing")
            {
                return true;
            }
            return false;
        }


        // Test probíhá po spuštění každé Akce
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // Záměrně zjednodušeno oproti UI - bez logovaní modelstate chyb do souboru.
            // Pokud bude třeba, doplnit dle vzoru UI/Controllers/BaseController.cs
            base.OnActionExecuted(context);
        }
    }
}