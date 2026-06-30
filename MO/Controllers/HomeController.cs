using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MO.Models;
using UAParser;

namespace MO.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            var v = new BaseViewModel { PageTitle = Factory.tra("Domů") };
            return View(v);
        }


        public IActionResult Tasks()
        {
            var v = new BaseViewModel { PageTitle = Factory.tra("Úkony") };
            return View(v);
        }


        public IActionResult Todos()
        {
            var v = new BaseViewModel { PageTitle = Factory.tra("Úkoly") };
            return View(v);
        }


        public IActionResult Reports()
        {
            var v = new BaseViewModel { PageTitle = Factory.tra("Tiskové sestavy") };
            return View(v);
        }


        public IActionResult More()
        {
            var v = new BaseViewModel { PageTitle = Factory.tra("Více") };
            return View(v);
        }


        public IActionResult MyProfile()
        {
            var rec = Factory.j02UserBL.Load(Factory.CurrentUser.pid);

            var v = new MyProfileViewModel
            {
                PageTitle = Factory.tra("Můj profil"),
                RecJ02 = rec,
                userAgent = Request.Headers["User-Agent"].ToString()
            };

            try
            {
                v.client_info = Parser.GetDefault().Parse(v.userAgent);
            }
            catch
            {
                // tichá chyba - UA parser nesmí blokovat stránku
            }

            return View(v);
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Identity.Application");
            return View(new BaseViewModel());
        }


        // Error handler - nasměrováno v Startup.cs UseExceptionHandler("/Home/Error")
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult Error()
        {
            return View(new BaseViewModel());
        }
    }
}
