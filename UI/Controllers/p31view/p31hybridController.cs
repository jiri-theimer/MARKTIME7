using Microsoft.AspNetCore.Mvc;
using UI.Models.p31view;

namespace UI.Controllers.p31view
{
    public class p31hybridController : BaseController
    {
        public IActionResult Index(int j02id, string d, string d0)
        {
            var v = new hybridViewModel() { d0 = DateTime.Today, j02ID = j02id };
            v.StatTotalsByPrefix = Factory.CBL.LoadUserParam("p31hybrid-totalsby", "topp41");
            if (!string.IsNullOrEmpty(d0))
            {
                d = d0; //d0 posílá _minicalendar zevnitř
            }
            if (!string.IsNullOrEmpty(d))
            {
                try
                {
                    v.d0 = BO.Code.Bas.String2Date(d);
                }
                catch
                {
                    v.d0 = DateTime.Today;
                }

            }
            if (v.j02ID == 0)
            {
                v.j02ID = Factory.CBL.LoadUserParamInt("p31calendar-j02id", Factory.CurrentUser.j02ID);
            }
            v.m0 = v.d0.Month;
            v.y0 = v.d0.Year;
            v.d1 = new DateTime(v.d0.Year, v.d0.Month, 1);
            v.d2 = v.d1.AddMonths(1).AddDays(-1);
            //v.d1 = BO.Code.Bas.get_first_prev_monday(v.d1);
            //v.d2 = BO.Code.Bas.get_first_prev_monday(v.d2).AddDays(6);

            RefreshState(v);

            return View(v);
        }


        private void RefreshState(hybridViewModel v)
        {
            v.InitFrameSrc = $"/TheGrid/SlaveView?prefix=p31&master_entity=j02&master_pid={v.j02ID}&caller=hybrid&myqueryinline=global_d1|date|{v.d0.ToString("dd.MM.yyyy")}|global_d2|date|{v.d0.ToString("dd.MM.yyyy")}";
            v.MonthFrameSrc = $"/TheGrid/SlaveView?prefix=p31&master_entity=j02&master_pid={v.j02ID}&caller=hybrid&myqueryinline=global_d1|date|{v.d1.ToString("dd.MM.yyyy")}|global_d2|date|{v.d2.ToString("dd.MM.yyyy")}";
            
            v.RecJ02 = Factory.j02UserBL.Load(v.j02ID);
        }
    }
}
