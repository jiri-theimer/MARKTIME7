using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using UI.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using System.Net.Http;
using SQLitePCL;
using DocumentFormat.OpenXml.InkML;

namespace UI.Controllers    
{
    [Authorize]
    public class BaseController : Controller
    {        
        public BL.Factory Factory;
        public BO.PermValEnum MustHavePerm;
        public string ParentLayoutName { get; set; }    //možné hodnoty: subform, modal, record nebo nic, pokud je volající stránka z _Layout
        


        //Test probíhá před spuštěním každé Akce!
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            
            //předání přihlášeného uživatele do Factory
            BO.RunningUser ru = (BO.RunningUser)HttpContext.RequestServices.GetService(typeof(BO.RunningUser));
            if (string.IsNullOrEmpty(ru.j02Login))
            {
                ru.j02Login = context.HttpContext.User.Identity.Name;
                
            }
            if (this.Factory == null)
            {
                this.Factory = (BL.Factory)HttpContext.RequestServices.GetService(typeof(BL.Factory));
            }
          
            if (Factory.CurrentUser==null || Factory.CurrentUser.isclosed)
            {
                context.Result = new RedirectResult("~/Login/UserLogin");
                return;
            }
            //Logout

            if (Factory.CurrentUser.j02IsMustChangePassword && !IsCurrentContextNoRestriction(context))    //test, zda si uživatel nemusí povinně změnit heslo
            {
                context.Result = new RedirectResult("~/Home/ChangePassword"); return;
            }
            if (Factory.CurrentUser.j02SmsVerifyCode != null && !IsCurrentContextNoRestriction(context))   //test, zda systém vyžaduje 2faktorové ověření
            {
                context.Result = new RedirectResult("~/Home/SmsVerify");    //systém vyžaduje po uživatel SMS 2faktorové ověření
                return;
            }            
            

            var queryString = context.HttpContext.Request.Query;
            StringValues someString;
            queryString.TryGetValue("layout", out someString);
            this.ParentLayoutName = someString;

            

         

            //Příklad přesměrování stránky jinam:
            //context.Result = new RedirectResult("~/Home/Index");

        }

        private bool IsCurrentContextNoRestriction(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context) //test zda aktuální stránka je mimo kontrolu ověření
        {
            if (ControllerContext.ActionDescriptor.ControllerName == "Menu")
            {
                return true;
            }
            string strPage = context.RouteData.Values["action"].ToString();
            if (strPage == "ChangePassword" || strPage == "SmsVerify" || strPage == "UpdateCurrentUserPing" || strPage == "main_me" || strPage == "Logout")
            {
                return true;
            }

            return false;
        }

        //Test probíhá po spuštění každé Akce:
        public override void OnActionExecuted(Microsoft.AspNetCore.Mvc.Filters.ActionExecutedContext context)
        {
            if (!ModelState.IsValid)
            {
                foreach (var modelError in ModelState)
                {
                    string propertyName = modelError.Key;                    
                    if (modelError.Value.Errors.Count > 0)
                    {
                        var s = context.HttpContext.Request.Path + " | " + Factory.tra("Chyba v poli") + " [" + propertyName + "]: " + modelError.Value.Errors.First().ErrorMessage;
                        Factory.CurrentUser.AddMessage(s);
                    }
                }

             
            }
            
            base.OnActionExecuted(context);
        }


        

        public IActionResult StopPage(bool bolModal,string strMessage,string strShowUrl=null,string strMessageToUrl=null)
        {
            var v = new StopPageViewModel() { Message = strMessage, IsModal = bolModal,ShowUrl=strShowUrl,MessageToUrl=strMessageToUrl };
            
            return View("_StopPage",v);
        }
        public IActionResult StopPageSubform(string strMessage)
        {
            var v = new StopPageViewModel() { Message = strMessage, IsSubform = true,IsModal=false };

            return View("_StopPage", v);
        }
        
        
        public ViewResult RecNotFound(UI.Models.BaseRecordViewModel v)
        {
            AddMessage("Hledaný záznam neexistuje!","error");            
            return View(v);
        }
        public ViewResult RecNotFound(UI.Models.BaseViewModel v)
        {
            AddMessage("Hledaný záznam neexistuje!", "error");
            return View(v);
        }

        public void Notify_RecNotSaved()
        {
            AddMessage("Záznam zatím nebyl uložen.", "warning");
        }
        public void Notify_RecNotFound()
        {
            AddMessage("Hledaný záznam neexistuje!", "warning");
        }

        public void AddMessage(string strMessage,string template="error")
        {
            
            Factory.CurrentUser.AddMessage(Factory.tra(strMessage), template);
        }
        public void AddMessageTranslated(string strMessage, string template = "error")
        {
            Factory.CurrentUser.AddMessage(strMessage, template);
        }
        public bool TUP(BO.PermValEnum oneperm)
        {
            return Factory.CurrentUser.TestPermission(oneperm);
        }

        

        public virtual ViewResult ViewTup(object model, BO.PermValEnum oneperm)
        {
            if (!TUP(oneperm))
            {
                var v = new StopPageViewModel() { Message = "Pro tuto stránku nemáte oprávnění!", IsModal = true };
                return View("_StopPage",v);
            }
            return View(model);
        }
       

        public NavTab AddTab(string strName, string strTabKey, string strUrl,bool istranslate=true,string strBadge=null)
        {
            if (istranslate)
            {
                strName = Factory.tra(strName);
            }
            return new NavTab() { Name = strName, Entity = strTabKey, Url = strUrl,Badge=strBadge };
        }
        public NavTab AddTab(string strName, string strTabKey, string strUrl, bool istranslate = true, int intBadgeNum=0)
        {
            if (istranslate)
            {
                strName = Factory.tra(strName);
            }
            return new NavTab() { Name = strName, Entity = strTabKey, Url = strUrl, Badge = $"<span class='badge bg-primary'>{intBadgeNum}</span>" };
        }
    }
}