using Microsoft.AspNetCore.Mvc;
using UI.Models.p91oper;

namespace UI.Controllers.p91oper
{
    public class p91CloneController : BaseController
    {
        public IActionResult Index(int pid)
        {
            var v = new p91CloneViewModel() { pid = pid };
            v.recDestP91 = new BO.p91Invoice();
            v.lisDestP31 = new List<p91Clonep31DestRecord>();

            if (v.pid == 0)
            {
                return this.StopPage(true, "pid missing.");
            }

            RefreshState(v);

            v.recDestP91.p91Text1 = v.recOrigP91.p91Text1;
            v.recDestP91.p91Text2 = v.recOrigP91.p91Text2;

            foreach (var c in v.lisOrigP31)
            {
                var rec = new p91Clonep31DestRecord() { p31ID = c.pid, recOrig = c, p31Date = c.p31Date,p31Text=c.p31Text };
                v.lisDestP31.Add(rec);
            }

            return View(v);
        }

        private void RefreshState(p91CloneViewModel v)
        {
            
            v.recOrigP91 = Factory.p91InvoiceBL.Load(v.pid);
            v.lisOrigP31 = Factory.p31WorksheetBL.GetList(new BO.myQueryP31() { p91id = v.pid });
            if (v.lisDestP31 != null)
            {
                foreach (var c in v.lisDestP31)
                {
                    c.recOrig = v.lisOrigP31.Where(p => p.pid == c.p31ID).First();

                }
            }
            
            
        }

        [HttpPost]
        public IActionResult Index(p91CloneViewModel v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                
                return View(v);
            }
            if (ModelState.IsValid)
            {
                foreach(var c in v.lisDestP31)
                {
                    var rec = c.recOrig;
                    rec.p31Date = c.p31Date;
                    rec.p31Text = c.p31Text;
                    var recInput=Factory.p31WorksheetBL.CovertRec2Input(rec, false);
                }
            }




            return View(v);
        }

    }


    
}
