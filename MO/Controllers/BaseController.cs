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
