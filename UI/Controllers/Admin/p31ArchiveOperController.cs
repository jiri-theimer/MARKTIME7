using Microsoft.AspNetCore.Mvc;
using UI.Models.Admin;

namespace UI.Controllers.Admin
{
    public class p31ArchiveOperController : BaseController
    {
        public IActionResult Index(string oper)
        {
            if (oper != "restore")
            {
                oper = "archive";
            }
            var v = new p31ArchiveOperViewModel() { SelectedYear = 2010,oper=oper };

            RefreshState(v);

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(p31ArchiveOperViewModel v)
        {
            if (v.oper == "archive")
            {
                v.lisStat = Factory.FBL.GetListTotalsByYear(BL.p31TableEnum.p31Worksheet).OrderBy(p => p.Rok);
            }
            else
            {
                v.lisStat = Factory.FBL.GetListTotalsByYear(BL.p31TableEnum.p31Worksheet_Del).OrderBy(p => p.Rok);
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(p31ArchiveOperViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {

                return View(v);
            }

            if (ModelState.IsValid)
            {

                
                if (v.oper == "archive")
                {
                    Factory.p31WorksheetBL.Move_To_Del(null,v.SelectedYear);
                }
                else
                {
                    Factory.p31WorksheetBL.Move_From_Del(null,v.SelectedYear);
                }
                this.AddMessage("Operace dokončena", "info");
                

                RefreshState(v);

                return View(v);

            }


            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
