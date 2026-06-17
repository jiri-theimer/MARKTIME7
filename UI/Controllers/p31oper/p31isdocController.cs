using BL;
using Microsoft.AspNetCore.Mvc;
using UI.Models.p31oper;

namespace UI.Controllers.p31oper
{
    public class p31isdocController : BaseController
    {
        public IActionResult Index(string guid)
        {
            var v = new p31IsdocViewModel() { UploadGuid=guid };

          

            return View(v);
        }


        [HttpPost]
        public IActionResult Index(p31KorekceViewModel v)
        {
         
            if (v.IsPostback)
            {

                return View(v);
            }

            if (ModelState.IsValid)
            {
                

                v.SetJavascript_CallOnLoad(0);
                return View(v);
            }

            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
