using Microsoft.AspNetCore.Mvc;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class o27Controller : BaseController
    {
        public IActionResult Info(int pid)
        {
            return Tab1(pid, "info");
        }

        public IActionResult Tab1(int pid, string caller)
        {
            var v = new o27Tab1() { Factory = this.Factory, prefix = "o27", pid = pid, caller = caller };

            RefreshStateTab1(v);
            return View(v);
        }
        private void RefreshStateTab1(o27Tab1 v)
        {
            v.Rec = Factory.o27AttachmentBL.Load(v.pid);
            if (v.Rec != null)
            {

                v.SetTagging();

                
            }
        }
    }
}
