using Microsoft.AspNetCore.Mvc;
using UI.Models.p11;
using UI.Models.p31view;

namespace UI.Controllers.p11
{
    public class AbsenceController : BaseController
    {
        public IActionResult Index(int j02id, string d,int go2pid)
        {
            if (go2pid > 0)
            {
                //p31id - reload stránky po zápisu úkonu
                var recP31 = Factory.p31WorksheetBL.Load(go2pid);
                if (recP31 != null) d = recP31.p31Date.ToString("dd.MM.yyyy");
            }
            var v = new AbsenceViewModel() { d0 = DateTime.Today, j02ID = j02id };
            
            handle_defaults(v, d);
           
            v.lisP11 = Factory.p11AttendanceBL.GetList(v.d1, v.d2, v.j02ID,true);
            if (v.lisP11.Any(p=>p.j02ID==v.j02ID && p.p11Date == v.d0))
            {
                v.RecP11 = v.lisP11.Where(p => p.j02ID == v.j02ID && p.p11Date == v.d0).First();
                v.Prichod = v.RecP11.Prichod;
                v.Odchod = v.RecP11.Odchod;
            }

            v.lisP12 = Factory.p12ApproveUserDayBL.GetList(new BO.myQueryP12() {j02id=v.j02ID, global_d1 = v.d1, global_d2 = v.d2,isstatus=true });

            v.lisC26 = Factory.c26HolidayBL.GetList(new BO.myQueryC26() { global_d1 = v.d1, global_d2 = v.d2 });
            var mq = new BO.myQueryP31() { j02id = v.j02ID, global_d1 = v.d1, global_d2 = v.d2 };
            v.lisP31 = Factory.p31WorksheetBL.GetList(mq).OrderBy(p => p.p31DateTimeFrom_Orig);

            v.lisC24 = Factory.c24DayColorBL.GetList(new BO.myQuery("c24") { IsRecordValid = true });
            v.lisC23 = Factory.c24DayColorBL.GetList_c23(v.d1, v.d2, v.j02ID);

            v.lisP32FUT = Factory.p32ActivityBL.GetList(new BO.myQueryP32() { isabsence = true });
            v.lisP32FUT_OsaX = v.lisP32FUT.Where(p => p.p32AbsenceBreakFlag == BO.p32AbsenceBreakFlagENUM._None).Take(10);
            v.lisC22 = Factory.c21FondCalendarBL.GetList_c22(v.RecJ02.c21ID, v.RecJ02.j02CountryCode, v.d1, v.d2);

            v.LeftPanel = new calendarViewModel() { lisP31 = v.lisP31,j02ID=v.j02ID,RecJ02=v.RecJ02,d0=v.d0,d1=v.d1,d2=v.d2,m0=v.d0.Month,y0=v.d0.Year };            
            v.LeftPanel.StatTotalsByPrefix = Factory.CBL.LoadUserParam("p31calendar-totalsby", "p28");
            v.LeftPanel.CurrentView = CalendarViewEnum.Month;
            return View(v);
        }


        private void handle_defaults(AbsenceViewModel v, string d)
        {
            v.IsAgendaDescending = Factory.CBL.LoadUserParamBool("absence-agendadescending", false);
            v.JakPocitatPrescas= Factory.CBL.LoadUserParam("absence-jakpocitatprescas", "prescas1");

            v.ShowHHMM = false;
            if (Factory.CurrentUser.j02DefaultHoursFormat == "T") v.ShowHHMM = true;

            v.j02ID = Factory.CBL.LoadUserParamInt("absence-j02id", Factory.CurrentUser.j02ID);

            v.RecJ02 = Factory.j02UserBL.Load(v.j02ID);
            if (v.RecJ02 == null)
            {
                v.j02ID = Factory.CurrentUser.j02ID;
                v.RecJ02 = Factory.j02UserBL.Load(v.j02ID);
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
            v.m0 = v.d0.Month;
            v.y0 = v.d0.Year;

            v.d1 = new DateTime(v.d0.Year, v.d0.Month, 1);
            v.d2 = v.d1.AddMonths(1).AddDays(-1);
            

        }
    }
}
