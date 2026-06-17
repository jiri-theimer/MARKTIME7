using Microsoft.AspNetCore.Mvc;
using UI.Models.p31invoice;

namespace UI.Controllers.p31oper
{
    public class p31InvoiceFinishController : BaseController
    {
        public IActionResult Index(int p91id)
        {
            var v = new p91InvoiceAfterFinishViewModel() { p91id = p91id };
            
            RefreshState(v);

            return View(v);
        }

        private void RefreshState(p91InvoiceAfterFinishViewModel v)
        {
            v.Rec = Factory.p91InvoiceBL.Load(v.p91id);

            v.GridUrl = $"/TheGrid/MasterView?prefix=p91&go2pid={v.p91id}";
            if (!Factory.CBL.LoadUserParamBool($"grid-p91-show11", true))
            {
                v.GridUrl = $"/TheGrid/FlatView?prefix=p91&go2pid={v.p91id}";
            }
        }

        [HttpPost]
        public IActionResult Index(p91InvoiceAfterFinishViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                switch (v.PostbackOper)
                {
                    case "closeandrefresh":
                        v.SetJavascript_CallOnLoad(1);
                        return View(v);

                        
                   

                }
                return View(v);
            }



            return View(v);
        }
    }
}
