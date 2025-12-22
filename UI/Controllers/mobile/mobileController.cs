using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers.mobile
{
    public class mobileController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
