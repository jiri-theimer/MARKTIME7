using Microsoft.AspNetCore.Mvc;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class p55Controller : BaseController
    {
        public IActionResult Info(int pid)
        {
            return View(new p55Tab1() { pid = pid, prefix = "p55",Rec=Factory.p56TaskBL.LoadToDoList(pid) });
        }
        public IActionResult Tab1(int pid, string caller)
        {
            var v = new p55Tab1() { Factory = this.Factory, prefix = "p55", pid = pid, caller = caller };

            RefreshStateTab1(v);
            return View(v);
        }
        private void RefreshStateTab1(p55Tab1 v)
        {
            v.Rec = Factory.p56TaskBL.LoadToDoList(v.pid);
            if (v.Rec != null)
            {
                v.lisP56 = Factory.p56TaskBL.GetList(new BO.myQueryP56() {IsRecordValid=null, p55id=v.pid}).OrderBy(p=>p.p56Ordinary);

                
            }
        }
    }
}
