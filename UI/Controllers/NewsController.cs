using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class NewsController : Controller
    {
        private BL.Factory _f;

        public NewsController(BL.Factory f)
        {
            _f = f;
            if (_f.App.HostingMode == BL.Singleton.HostingModeEnum.SharedApp)
            {
                _f.InhaleUserByLogin("admin@marktime.cz");
            }
        }

        public IActionResult Index()
        {
            var v = new NewsViewModel();
          

            var mq = new BO.myQuery("x52");
            mq.explicit_orderby = "a.x52Date DESC,a.x52Ordinary";
            v.lisX52 = _f.x52BlogBL.GetList(mq);

            return View(v);
        }

        public IActionResult Detail(int pid)
        {
            var v = new NewsDetailViewModel() { pid = pid };
            v.Rec = _f.x52BlogBL.Load(pid);

            if (_f.App.HostingMode != BL.Singleton.HostingModeEnum.None)
            {
                if (v.Rec.x52Html != null && v.Rec.x52Html.Contains("/_users/"))
                {
                    v.Rec.x52Html = v.Rec.x52Html.Replace("/_users/8c56ee9c-007e-418c-9db6-66ea7b3710b8/NOTEPAD/x52/", "https://portal.marktime.net/_users/_news/");
                }
            }
            
            
            
            return View(v);
        }
    }
}
