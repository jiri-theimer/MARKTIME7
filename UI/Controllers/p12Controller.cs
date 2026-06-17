using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.p31view;


namespace UI.Controllers
{
    public class p12Controller : BaseController
    {
        public IActionResult Index(int j02id, string d,string status)
        {
            if (!Factory.CurrentUser.IsMasterPerson && !Factory.CurrentUser.IsAdmin)
            {
                return this.StopPage(false, "Nemáte přístup k internímu schvalování.");
            }
            var v = new p12ApproveViewModel() { d0 = DateTime.Today, j02ID = j02id,ApproveStatus=status };

            handle_defaults(v, d);

            v.lisP12 = Factory.p12ApproveUserDayBL.GetList(new BO.myQueryP12() { j02id = v.j02ID,global_d1=v.d1,global_d2=v.d2 });
            v.lisP11 = Factory.p11AttendanceBL.GetList(v.d1, v.d2, v.j02ID, true);
          

            v.lisC26 = Factory.c26HolidayBL.GetList(new BO.myQueryC26() { global_d1 = v.d1, global_d2 = v.d2 });
            var mq = new BO.myQueryP31() { j02id = v.j02ID, global_d1 = v.d1, global_d2 = v.d2 };
            v.lisP31 = Factory.p31WorksheetBL.GetList(mq).OrderBy(p => p.p31DateTimeFrom_Orig);

            v.lisC24 = Factory.c24DayColorBL.GetList(new BO.myQuery("c24") { IsRecordValid = true });
            v.lisC23 = Factory.c24DayColorBL.GetList_c23(v.d1, v.d2, v.j02ID);

            v.lisP32FUT = Factory.p32ActivityBL.GetList(new BO.myQueryP32() { isabsence = true });
            v.lisP32FUT_OsaX = v.lisP32FUT.Where(p => p.p32AbsenceBreakFlag == BO.p32AbsenceBreakFlagENUM._None).Take(10);
            v.lisC22 = Factory.c21FondCalendarBL.GetList_c22(v.RecJ02.c21ID, v.RecJ02.j02CountryCode, v.d1, v.d2);

            v.LeftPanel = new calendarViewModel() { lisP31 = v.lisP31, j02ID = v.j02ID, RecJ02 = v.RecJ02, d0 = v.d0, d1 = v.d1, d2 = v.d2, m0 = v.d0.Month, y0 = v.d0.Year };
            v.LeftPanel.StatTotalsByPrefix = Factory.CBL.LoadUserParam("p31calendar-totalsby", "p28");
            v.LeftPanel.CurrentView = CalendarViewEnum.Month;

            return View(v);
        }


        private void handle_defaults(p12ApproveViewModel v, string d)
        {
            v.IsAgendaDescending = Factory.CBL.LoadUserParamBool("p12-agendadescending", false);
            v.JakPocitatPrescas = Factory.CBL.LoadUserParam("absence-jakpocitatprescas", "prescas1");

            
           
            v.j02ID = Factory.CBL.LoadUserParamInt("p12-j02id", Factory.CurrentUser.j02ID);

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

        public BO.Result SaveApproving(DateTime d,int j02id, string daylist,string status)
        {
            var days = BO.Code.Bas.ConvertString2ListInt(daylist);
            if (days.Count() == 0)
            {
                return new BO.Result(true, "Na vstupu chybí výběr dnů.");

            }
            var lisSaved = Factory.p12ApproveUserDayBL.GetList(new BO.myQueryP12() { j02id = j02id,global_d1=new DateTime(d.Year,d.Month,1),global_d2= new DateTime(d.Year, d.Month, 1).AddMonths(1).AddDays(-1) });
            BO.p12StatusFlagENUM p12status = (BO.p12StatusFlagENUM) Convert.ToInt32(status);

            int intMonth = d.Month;
            int intYear = d.Year;
            foreach(int day in days)
            {
                var datum = new DateTime(intYear, intMonth, day);
                var rec = lisSaved.FirstOrDefault(p => p.p12Date == datum);
                if (rec == null)
                {
                    rec = new BO.p12ApproveUserDay() { j02ID = j02id, p12Date = datum };
                }
                rec.j02ID_ApprovedBy = Factory.CurrentUser.j02ID;
                rec.p12StatusFlag = p12status;

                Factory.p12ApproveUserDayBL.Save(rec);
            }

            return new BO.Result(false);
            
        }
    }
}
