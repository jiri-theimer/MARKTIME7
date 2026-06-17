using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.p31view;

namespace UI.Controllers.p31view
{
    public class p31calendarController : BaseController
    {
        public IActionResult Mobile(int j02id, string d, int cv)
        {
            var v = new calendarViewModel() { d0 = DateTime.Today, j02ID = j02id, CurrentView = CalendarViewEnum.Month };

            handle_defaults(v, d, cv);
            if (v.CurrentView == CalendarViewEnum.Hybrid)
            {
                cv = 1;
                handle_defaults(v, d, cv);
            }
            RefreshState(v);

            return View(v);
        }
        public IActionResult Index(int j02id,string d,int cv)
        {
            if (cv == 7)
            {
                return RedirectToAction("Index", "p31hybrid", new { d = d });
            }
            var v = new calendarViewModel() { d0 = DateTime.Today,j02ID=j02id,CurrentView= CalendarViewEnum.Month };            
            
            handle_defaults(v,d,cv);
            if (v.CurrentView == CalendarViewEnum.Hybrid)
            {
                return RedirectToAction("Index", "p31hybrid", new { d = d });
            }

            RefreshState(v);

            return View(v);
        }

        private void RefreshState(calendarViewModel v)
        {
            
            v.lisC26 = Factory.c26HolidayBL.GetList(new BO.myQueryC26() { global_d1 = v.d1, global_d2 = v.d2 });

            

            if (Factory.CurrentUser.j04IsModule_p31)
            {
                v.lisSums = Factory.p31WorksheetBL.GetList_TimelineDays(new List<int> { v.j02ID }, v.d1, v.d2, 0, 0, 0).ToList();                
            }
            else
            {
                v.lisSums = new List<BO.p31WorksheetTimelineDay>();
            }

            var mq = new BO.myQueryP31() { j02id = v.j02ID, global_d1 = v.d1, global_d2 = v.d2 };
            v.lisP31 = Factory.p31WorksheetBL.GetList(mq).OrderBy(p => p.p31DateTimeFrom_Orig);

            v.lisC24 = Factory.c24DayColorBL.GetList(new BO.myQuery("c24") { IsRecordValid = true });
            v.lisC23 = Factory.c24DayColorBL.GetList_c23(v.d1, v.d2, v.j02ID);
            
            if (v.ShowP56Recs)
            {
                var mqP56 = new BO.myQueryP56() { x67id = v.x67ID_p56, j02id = v.j02ID, global_d1 = v.d1, global_d2 = v.d2, period_field = "p56PlanUntil" };
                if (v.j02ID != Factory.CurrentUser.pid)
                {
                    mqP56.MyRecordsDisponible = true;
                }
                v.lisP56 = Factory.p56TaskBL.GetList(mqP56);
            }
            if (v.ShowO22Recs)
            {
                var mqO22 = new BO.myQueryO22() { x67id = v.x67ID_o22, j02id = v.j02ID, global_d1 = v.d1.AddDays(-20), global_d2 = v.d2.AddDays(20), period_field = "o22PlanUntil" };
                if (v.j02ID != Factory.CurrentUser.pid)
                {
                    mqO22.MyRecordsDisponible = true;
                }
                v.lisO22 = Factory.o22MilestoneBL.GetList(mqO22);
            }
            v.lisP32FUT = Factory.p32ActivityBL.GetList(new BO.myQueryP32() { isabsence = true });
            if (Factory.CurrentUser.j04IsModule_p11)
            {
                v.lisP11 = Factory.p11AttendanceBL.GetList(v.d1, v.d2, v.j02ID, false);
            }
        }

        private void handle_defaults(calendarViewModel v,string d,int cv)
        {
            if (cv < 0 || cv > 6) cv = 0;            
            if (cv == 0)
            {
                cv = Factory.CBL.LoadUserParamInt("p31calendar-cv",1);
            }
            v.CurrentView = (CalendarViewEnum)cv;
            if (v.CurrentView == CalendarViewEnum.Hybrid)
            {                
                return;
            }
            v.ShowHHMM = false;
            
            if (Factory.CurrentUser.j02DefaultHoursFormat == "T") v.ShowHHMM = true;
            v.StatTotalsByPrefix = Factory.CBL.LoadUserParam("p31calendar-totalsby", "topp41");
            v.ShowLeftPanel = Factory.CBL.LoadUserParamBool("p31calendar-showleftpanel", Factory.CurrentUser.j04IsModule_p31);
            v.ShowWeekend = Factory.CBL.LoadUserParamBool("p31calendar-showweekend", true);
            v.ShowP31Recs = Factory.CBL.LoadUserParamBool("p31calendar-showp31recs", true);
            v.ShowP31RecsNoTime = Factory.CBL.LoadUserParamBool("p31calendar-showp31recsnotime", false);
            v.ShowP56Recs = Factory.CBL.LoadUserParamBool("p31calendar-showp56recs", true);
            v.ShowO22Recs = Factory.CBL.LoadUserParamBool("p31calendar-showo22recs", true);
            v.j02ID = Factory.CBL.LoadUserParamInt("p31calendar-j02id", Factory.CurrentUser.j02ID);
            
            if (v.ShowP56Recs || v.ShowO22Recs)
            {
                var lisX67 = Factory.x67EntityRoleBL.GetList(new BO.myQuery("x67") { explicit_orderby = "x67Ordinary" });
                v.lisX67_p56 = lisX67.Where(p => p.x67Entity == "p56");
                v.lisX67_o22 = lisX67.Where(p => p.x67Entity == "o22");
            }
            if (v.ShowP56Recs)
            {
                v.x67ID_p56 = Factory.CBL.LoadUserParamInt("p31calendar-x67id_p56", v.lisX67_p56.First().pid);
            }
            if (v.ShowO22Recs)
            {
                v.x67ID_o22 = Factory.CBL.LoadUserParamInt("p31calendar-x67id_o22", v.lisX67_o22.First().pid);
            }

            if (v.CurrentView ==CalendarViewEnum.MonthAgenda || v.CurrentView==CalendarViewEnum.WeekAgenda || v.CurrentView == CalendarViewEnum.NAgenda)
            {
                v.IsAgendaDescending = Factory.CBL.LoadUserParamBool("p31calendar-agendadescending", false);
            }
            if (v.CurrentView == CalendarViewEnum.NAgenda)
            {
                v.AgendaNdays = Factory.CBL.LoadUserParamInt("p31calendar-agendandays", 3);
            }
            if (v.CurrentView == CalendarViewEnum.ExactDay)
            {
                v.DayView_MinutesGap = Factory.CBL.LoadUserParamInt("p31calendar-minutesgap", 15);
                v.h0 = Factory.CBL.LoadUserParamInt("p31calendar-h0", 8);
                v.h1 = Factory.CBL.LoadUserParamInt("p31calendar-h1", 19);
            }

            v.RecJ02 = Factory.j02UserBL.Load(v.j02ID);

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

            switch (v.CurrentView)
            {
                
                case CalendarViewEnum.Month:
                    v.d1 = new DateTime(v.d0.Year, v.d0.Month, 1);
                    v.d2 = v.d1.AddMonths(1).AddDays(-1);
                    v.d1 =BO.Code.Bas.get_first_prev_monday(v.d1);
                    v.d2 = BO.Code.Bas.get_first_prev_monday(v.d2).AddDays(6);
                    break;
                case CalendarViewEnum.MonthAgenda:
                    v.d1 = new DateTime(v.d0.Year, v.d0.Month, 1);
                    v.d2 = v.d1.AddMonths(1).AddDays(-1);
                    break;
                case CalendarViewEnum.Week:                    
                case CalendarViewEnum.WeekAgenda:                    
                    v.d1 = BO.Code.Bas.get_first_prev_monday(v.d0);
                    v.d2 = v.d1.AddDays(6);
                    break;
                case CalendarViewEnum.NAgenda:
                    v.d1 = v.d0;
                    v.d2 = v.d1.AddDays(v.AgendaNdays-1);
                    break;
                case CalendarViewEnum.ExactDay:
                    v.d1 = v.d0;
                    v.d2 = v.d0;
                    v.DayView_MinutesGap= Factory.CBL.LoadUserParamInt("p31calendar-minutesgap", 15);
                    break;
                default:                    
                    break;
                
            }
            
        }

        


        

        public int SaveC23Record(string d, int c24id, int j02id)
        {
            return Factory.c24DayColorBL.SaveC23Record(BO.Code.Bas.String2Date(d), c24id, j02id);
        }

        public BO.Result SaveAbsRecord(string d,string prichod,string odchod,int j02id)
        {
            DateTime d0 = BO.Code.Bas.String2Date(d);
            DateTime? d1 = null;
            DateTime? d2 = null;
            if (!string.IsNullOrEmpty(prichod))
            {
                d1 = d0.AddSeconds(BO.Code.Time.ConvertTimeToSeconds(prichod));
            }
            if (!string.IsNullOrEmpty(odchod))
            {
                d2 = d0.AddSeconds(BO.Code.Time.ConvertTimeToSeconds(odchod));
            }
            

            var c = new BO.p11Attendance() { j02ID = j02id, p11Date = d0,p11TodayStart=d1,p11TodayEnd=d2 };
            c.pid = Factory.p11AttendanceBL.Save(c);
            if (c.pid > 0)
            {
                return new BO.Result(false, null, c.pid);
            }
            else
            {
                return new BO.Result(true, Factory.GetFirstNotifyMessage());
            }
        }
        public BO.p11Attendance LoadAbsRecord(string d,int j02id)
        {
            var lis = Factory.p11AttendanceBL.GetList(BO.Code.Bas.String2Date(d), BO.Code.Bas.String2Date(d), j02id,false);
            if (lis.Count() == 0)
            {
                return new BO.p11Attendance();
            }
            else
            {
                return lis.First();
            }
        }

        public BO.Result SaveFutRecord(int p32id,string d,int j02id)  //aktivita plánované absence z kalendáře
        {
            var recP32 = Factory.p32ActivityBL.Load(p32id);
            if (recP32 == null)
            {
                return new BO.Result(true, "recP32 is null");
            }
            if (recP32.p32Value_Default == 0 || recP32.p41ID_Absence == 0)
            {
                return new BO.Result(false, "manual"); //úkon zadat ručně přes formulář, protože není vyplněna výchozí hodnota
                //return new BO.Result(false, "V nastavení aktivity není uvedena výchozí hodnota úkonu - proto nelze vykazovat takto automaticky"); //úkon zadat ručně přes formulář, protože není vyplněna výchozí hodnota
            }

            var recP34 = Factory.p34ActivityGroupBL.Load(recP32.p34ID);
            var recJ02 = Factory.j02UserBL.Load(j02id);

            BO.p31WorksheetEntryInput c = new BO.p31WorksheetEntryInput();
            c.Addp31Date(BO.Code.Bas.String2Date(d));
            c.j02ID = j02id;
            c.p41ID = recP32.p41ID_Absence;
            c.p34ID = recP32.p34ID;
            c.p32ID = recP32.pid;

            if (recP32.p32Value_Default > 0 || recP32.p32AbsenceBreakFlag != BO.p32AbsenceBreakFlagENUM._None)
            {
                c.Value_Orig = recP32.p32Value_Default.ToString();  //aktivita má explicitně nastavenou default hodnotu nebo se jedná o přestávku
                if (recP32.p32Value_Default == 0)
                {
                    c.Value_Orig = "00:30";
                }
            }
            else
            {
                if (recJ02.c21ID == 0)
                {
                    c.Value_Orig = "8"; //uživatel nemá přiřazený pracovní fond
                }
                else
                {
                    double dblFond = Factory.c21FondCalendarBL.GetSumHours(recJ02.c21ID, recJ02.j02CountryCode, c.p31Date.First(), c.p31Date.First());
                    if (dblFond == 0)
                    {
                        return new BO.Result(true, "Pro tento den je váš pracovní fond nulový. Aktivitu případně vykažte klasickým způsobem.");
                    }
                    c.Value_Orig = dblFond.ToString();
                }
            }
            
            
            c.p31HoursEntryflag = BO.p31HoursEntryFlagENUM.Hodiny;

            c.pid = Factory.p31WorksheetBL.SaveOrigRecord(c, recP34.p33ID, null);

            if (c.pid > 0)
            {
                return new BO.Result(false,null,c.pid);
                
            }
            else
            {
                return new BO.Result(true, Factory.GetFirstNotifyMessage());
            }
           

        }

    }
}
