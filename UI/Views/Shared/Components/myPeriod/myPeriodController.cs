using Microsoft.AspNetCore.Mvc;
using UI.Controllers;
using UI.Models;

namespace UI.Views.Shared.Components.myPeriod
{    
    public class myPeriodController : BaseController
    {
        private readonly BL.Singleton.ThePeriodProvider _pp;
        public myPeriodController(BL.Singleton.ThePeriodProvider pp)
        {
            _pp = pp;
        }

        
        public IActionResult selector(string prefix,string userparamkey,int periodvalue)
        {
            var v = new myPeriodViewModel() { prefix = prefix,UserParamKey=userparamkey };
            

            v.LoadUserSetting(_pp, Factory, periodvalue);    //načtení naposledy nastaveného období uživatelem


            return View(v);
        }


        public IActionResult UserPeriods()
        {
            var v = new UserPeriodsViewModel();

            v.lisX21 = Factory.FBL.GetListX21(Factory.CurrentUser.pid).ToList();

            foreach (var c in v.lisX21)
            {
                c.TempGuid = BO.Code.Bas.GetGuid();
            }

            return View(v);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UserPeriods(UserPeriodsViewModel v, string guid)
        {
            if (v.lisX21 == null)
            {
                v.lisX21 = new List<BO.x21DatePeriod>();
            }
            if (v.IsPostback)
            {
                if (v.PostbackOper == "add")
                {
                    var c = new BO.x21DatePeriod() { TempGuid = BO.Code.Bas.GetGuid(), x21ValidFrom = DateTime.Today, x21ValidUntil = DateTime.Today.AddDays(30) };
                    v.lisX21.Add(c);

                }
                if (v.PostbackOper == "delete")
                {
                    v.lisX21.First(p => p.TempGuid == guid).IsTempDeleted = true;

                }

                return View(v);
            }

            if (ModelState.IsValid)
            {

                if (Factory.FBL.SaveX21Batch(v.lisX21))
                {

                    v.SetJavascript_CallOnLoad(0);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
