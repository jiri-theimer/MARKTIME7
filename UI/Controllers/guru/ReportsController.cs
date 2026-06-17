using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers.guru
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
