using Microsoft.AspNetCore.Mvc;
using UI.Models;

namespace UI.Controllers
{
    public class TreePageController : BaseController
    {
        public IActionResult Index(string prefix, int pid, string tab, string rez, string myqueryinline)
        {
          
            var v = new TreePageViewModel() { pid = pid, prefix =(string.IsNullOrEmpty(prefix)? "p41": prefix), DefTab = tab, rez = rez };

            if (v.pid == 0)
            {
                v.pid = LoadLastUsedPid(v.prefix, null);
            }

            if (v.pid == 0 && !BL.Code.UserUI.IsShowLeftPanel(v.prefix, Factory.CurrentUser.j02UIBitStream))
            {
                v.NavTabs = new List<NavTab>();
                return View(v); //prázdná stránka
            }


            return View(v);
        }


        private int LoadLastUsedPid(string prefix, string rez)
        {
            return Factory.CBL.LoadUserParamInt($"treepage-{prefix}-{rez}-pid");
            
        }
    }
}
