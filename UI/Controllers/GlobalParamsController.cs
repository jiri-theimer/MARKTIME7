using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class GlobalParamsController : BaseController
    {
        public IActionResult Index(string prefix,int pid)
        {
            var v = new GlobalParamsViewModel() { prefix = prefix, pid = pid };
            if (v.pid == 0)
            {
                return this.StopPage(true, "Doplňující parametry je možné vyplnit až po uložení záznamu.");
            }

            v.lisParams = Factory.CBL.GetList_GlobalParam(v.prefix, v.pid).ToList();

            return View(v);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(GlobalParamsViewModel v)
        {
            
            if (v.IsPostback)
            {
                
                return View(v);
            }

            if (ModelState.IsValid)
            {
                for(int i = 0; i < v.lisParams.Count(); i++)
                {
                    Factory.CBL.SaveGlobalParam(v.lisParams[i].o58ID, v.pid, v.lisParams[i].o59Value);
                }

                v.SetJavascript_CallOnLoad(0, null, "window.parent._window_close()");
                return View(v);

            }

            this.Notify_RecNotSaved();            
            return View(v);
        }
    }
}
