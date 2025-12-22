using Microsoft.AspNetCore.Mvc;
using UI.Models;

namespace UI.Controllers
{
    public class changelogController : BaseController
    {
        public IActionResult p31(int pid)
        {
            var v = new changelogViewModel() { prefix = "p31", pid = pid };

            return View(v);
        }
        public IActionResult p41(int pid)
        {
            var v = new changelogViewModel() { prefix = "p41", pid = pid };

            return View(v);
        }
        public IActionResult p28(int pid)
        {
            var v = new changelogViewModel() { prefix = "p28", pid = pid };

            return View(v);
        }
        public IActionResult p91(int pid)
        {
            var v = new changelogViewModel() { prefix = "p91", pid = pid };

            return View(v);
        }
        public IActionResult p56(int pid)
        {
            var v = new changelogViewModel() { prefix = "p56", pid = pid };

            return View(v);
        }
        public IActionResult o23(int pid)
        {
            var v = new changelogViewModel() { prefix = "o23", pid = pid };

            return View(v);
        }
    }
}
