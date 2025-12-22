using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.p31view;

namespace UI.Controllers
{
    public class p31daylineController : BaseController
    {        
        public IActionResult Index(string d,bool export_to_xls)
        {
            var v = new daylineViewModel() { d0 = DateTime.Today, GroupBy = daylineGroupBy.None };
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

            if (export_to_xls)
            {
                v.IsJustXlsExporting = true;
            }
            

            var mqJ02 = new BO.myQueryJ02() {explicit_orderby = "a.j02LastName,a.j02FirstName" };

            dayline_handle_defaults(v, mqJ02);

            v.TheGridQueryButton = new TheGridQueryViewModel() { j72id = Factory.CBL.LoadUserParamInt("dayline-j72id"), paramkey = "dayline-j72id",prefix="p31" };

            v.lisJ02 = Factory.j02UserBL.GetList(mqJ02);
            if (Factory.CurrentUser.j04IsModule_p31)
            {
                v.lisSums = Factory.p31WorksheetBL.GetList_TimelineDays(v.lisJ02.Select(p => p.pid).ToList(), v.d1, v.d2, 0, v.p31statequery.Value,v.TheGridQueryButton.j72id).ToList();
            }
            else
            {
                v.lisSums = new List<BO.p31WorksheetTimelineDay>();
            }
            
            var mq = new BO.myQueryP31() { global_d1 = v.d1, global_d2 = v.d2,MyRecordsDisponible=true, tabquery="time",p31statequery=v.p31statequery.Value,CurrentUser=Factory.CurrentUser };
            
            v.lisC26 = Factory.c26HolidayBL.GetList(new BO.myQueryC26() { global_d1 = v.d1,global_d2 = v.d2 });
            v.lisC24 = Factory.c24DayColorBL.GetList(new BO.myQuery("c24") { IsRecordValid = true });
            v.lisC23 = Factory.c24DayColorBL.GetList_c23(v.d1, v.d2,0);
            v.lisP12 = Factory.p12ApproveUserDayBL.GetList(new BO.myQueryP12() { global_d1 = v.d1, global_d2 = v.d2,isstatus=true });
            //v.lisP61 = Factory.p61ActivityClusterBL.GetList(new BO.myQuery("p61"));

            
            if (v.TheGridQueryButton.j72id > 0)
            {
                v.TheGridQueryButton.j72name = Factory.j72TheGridTemplateBL.LoadName(v.TheGridQueryButton.j72id);
                mq.lisJ73 = Factory.j72TheGridTemplateBL.GetList_j73(v.TheGridQueryButton.j72id, "p31", 0);
            }
            
            
            v.lisP32FUT = Factory.p32ActivityBL.GetList(new BO.myQueryP32() { isabsence = true });
            if (v.lisP32FUT.Count() > 15)
            {
                v.lisP32FUT = v.lisP32FUT.Take(15);
            }
            
            
            if (v.ShowO22 || v.ShowP56)
            {
                var lisX67 = Factory.x67EntityRoleBL.GetList(new BO.myQuery("x67") { explicit_orderby = "x67Ordinary" });
                v.lisX67_p56 = lisX67.Where(p => p.x67Entity == "p56");
                v.lisX67_o22 = lisX67.Where(p => p.x67Entity == "o22");
            }
            if (v.ShowP56)
            {
                v.x67ID_p56 = Factory.CBL.LoadUserParamInt("dayline-x67id_p56", v.lisX67_p56.First().pid);
                v.lisP56 = Factory.p56TaskBL.GetList_Dayline(new BO.myQueryP56() { MyRecordsDisponible = true, x67id = v.x67ID_p56, period_field = "p56PlanUntil", global_d1 = v.d1, global_d2 = v.d2 }, v.x67ID_p56);
            }
            if (v.ShowO22)
            {
                v.x67ID_o22 = Factory.CBL.LoadUserParamInt("dayline-x67id_o22", v.lisX67_o22.First().pid);
                var lisO22 = Factory.o22MilestoneBL.GetList_Dayline(new BO.myQueryO22() { MyRecordsDisponible = true, period_field = "o22PlanUntil", global_d1 = v.d1.AddDays(-20), global_d2 = v.d2.AddDays(20) }, v.x67ID_o22);
                v.lisO22_Udalosti = lisO22.Where(p => p.o21TypeFlag == BO.o21TypeFlagEnum.Udalost);
                v.lisO22_MimoUdalosti = lisO22.Where(p => p.o21TypeFlag != BO.o21TypeFlagEnum.Udalost);

                
            }

            if (v.GroupBy != daylineGroupBy.None)
            {
                v.lisP31 = Factory.p31WorksheetBL.GetList(mq);                
            }
            dayline_format_hours(v);
            

            return View(v);
        }

        private void dayline_format_hours(daylineViewModel v)
        {
            v.ShowHHMM = false;
            if (Factory.CurrentUser.j02DefaultHoursFormat == "T") v.ShowHHMM = true;
            int intLastC21ID = 0;double dblLastFond = 0;

            foreach (var recJ02 in v.lisJ02)
            {
                if (recJ02.c21ID > 0)
                {
                    if (recJ02.c21ID != intLastC21ID)
                    {
                        dblLastFond = Factory.c21FondCalendarBL.GetSumHours(recJ02.c21ID, recJ02.j02CountryCode, v.d1, v.d2);                        
                    }
                    intLastC21ID = recJ02.c21ID;
                    recJ02.Rezerva = dblLastFond;
                }
                

                for (DateTime d = v.d1; d <= v.d2; d = d.AddDays(1))
                {
                    var qry = v.lisSums.Where(p => p.j02ID == recJ02.pid && p.p31Date == d);
                    if (qry.Count() > 0)
                    {
                        var rec = qry.First();
                        if (v.ShowHHMM)
                        {
                            rec.HoursFormatted = BO.Code.Time.ShowAsHHMM(rec.Hours.ToString());
                        }
                        else
                        {
                            rec.HoursFormatted = BO.Code.Bas.Number2String(rec.Hours);
                            if (v.IsJustXlsExporting)
                            {
                                rec.HoursFormatted = rec.HoursFormatted.Replace(",", ".");
                            }
                        }
                        if (rec.Hours_Billable > 0 && rec.Hours_NonBillable == 0)
                        {
                            rec.CssStyle = "color:green;";
                        }
                        else
                        {
                            if (rec.Hours_NonBillable > 0 && rec.Hours_Billable == 0) rec.CssStyle = "color:red;";
                        }
                        if (rec.p32Color != null)
                        {
                            rec.CssStyle += $";background-color:{rec.p32Color};";
                        }

                    }
                }

            }
        }
        private void dayline_handle_defaults(daylineViewModel v, BO.myQueryJ02 mqJ02)
        {
            v.ScaleIndex = Factory.CBL.LoadUserParamInt("dayline-scaleindex");
            v.p31statequery = new p31StateQueryViewModel() {UserParamKey = "p31dayline-p31statequery" };
            v.p31statequery.Value = Factory.CBL.LoadUserParamInt(v.p31statequery.UserParamKey);
            



            switch (v.ScaleIndex)
            {
                case 1:
                    v.d1 = BO.Code.Bas.get_first_prev_monday(v.d0);
                    v.d2 = v.d1.AddDays(6);
                    break;
                case 2:
                    v.d1 = BO.Code.Bas.get_first_prev_monday(v.d0);
                    v.d2 = v.d1.AddDays(13);
                    break;
                case 3:
                    v.d1 = BO.Code.Bas.get_first_prev_monday(v.d0);
                    v.d2 = v.d1.AddDays(20);
                    break;
                default:
                    v.d1 = new DateTime(v.d0.Year, v.d0.Month, 1);
                    v.d2 = v.d1.AddMonths(1).AddDays(-1);
                    break;
            }
            

            string strGroupBy = Factory.CBL.LoadUserParam("dayline-groupby", "None");
            v.GroupBy = (daylineGroupBy)Enum.Parse(typeof(daylineGroupBy), strGroupBy);

           
            if (this.Factory.CurrentUser.j04IsModule_p56)
            {
                v.ShowP56 = Factory.CBL.LoadUserParamBool("dayline-showp56", true);
            }
            v.ShowO22 = Factory.CBL.LoadUserParamBool("dayline-showo22", true);

            v.j02IDs = Factory.CBL.LoadUserParam("dayline-j02ids");
            if (v.j02IDs != null)
            {
                var j02ids = BO.Code.Bas.ConvertString2ListInt(v.j02IDs);
                var lis = Factory.j02UserBL.GetList(new BO.myQueryJ02() { pids = j02ids });
                v.SelectedPersons = string.Join(",", lis.Select(p => p.FullnameDesc));
                mqJ02.pids = BO.Code.Bas.ConvertString2ListInt(v.j02IDs);
            }
            v.j07IDs = Factory.CBL.LoadUserParam("dayline-j07ids");
            if (v.j07IDs != null)
            {
                var j07ids = BO.Code.Bas.ConvertString2ListInt(v.j07IDs);
                var lis = Factory.j07PersonPositionBL.GetList(new BO.myQuery("j07") { pids = j07ids });
                v.SelectedPositions = string.Join(",", lis.Select(p => p.j07Name));
                mqJ02.j07ids = j07ids;
            }
            v.j11IDs = Factory.CBL.LoadUserParam("dayline-j11ids");
            if (v.j11IDs != null)
            {
                var j11ids = BO.Code.Bas.ConvertString2ListInt(v.j11IDs);
                var lis = Factory.j11TeamBL.GetList(new BO.myQueryJ11() { pids = j11ids });
                v.SelectedTeams = string.Join(",", lis.Select(p => p.j11Name));
                mqJ02.j11ids = j11ids;
            }
        }



        //zobrazení ZOOM okna pro vybranou osobu a den
        public IActionResult Zoom(int j02id,string d,int m,int y,int p28id,int p41id,int p32id,bool? p32isbillable,int p70id,bool? iswip,bool? isapproved_and_wait4invoice,string d1,string d2)
        {
            var v = new daylineZoomViewModel() { j02ID=j02id,p28ID=p28id,p41ID=p41id,p32ID=p32id,p32IsBillable= p32isbillable,IsWip=iswip,p70ID=p70id, IsApproved_And_Wait4Invoice=isapproved_and_wait4invoice };
            if (m>0 && y > 0)
            {
                v.SelectedDate1 = new DateTime(y, m, 1);
                v.SelectedDate2 = v.SelectedDate1.AddMonths(1).AddDays(-1);
            }
            else
            {
                if (d1 !=null && d2 != null)
                {
                    v.SelectedDate1 = BO.Code.Bas.String2Date(d1);
                    v.SelectedDate2 = BO.Code.Bas.String2Date(d2);
                }
                else
                {
                    v.SelectedDate1 = BO.Code.Bas.String2Date(d);
                    v.SelectedDate2 = v.SelectedDate1;
                }
                
            }

            if (v.j02ID==0 || v.SelectedDate1.Year<2000)
            {
                return this.StopPage(true, "Na vstupu chybí osoba nebo datum.");
            }
            v.RecJ02 = Factory.j02UserBL.Load(v.j02ID);

            string strMyQueryInline = "MyRecordsDisponible|bool|1|j02id|int|" + v.j02ID.ToString()+ "|global_d1|date|" + BO.Code.Bas.ObjectDate2String(v.SelectedDate1,"dd.MM.yyyy") + "|global_d2|date|" + BO.Code.Bas.ObjectDate2String(v.SelectedDate2, "dd.MM.yyyy");
            if (v.p28ID > 0)
            {
                strMyQueryInline += "|p28id|int|" + v.p28ID.ToString();
            }
            if (v.p41ID > 0)
            {
                strMyQueryInline += "|p41id|int|" + v.p41ID.ToString();
            }
            if (v.p32ID > 0)
            {
                strMyQueryInline += "|p32id|int|" + v.p32ID.ToString();
            }
            if (v.p70ID > 0)
            {
                strMyQueryInline += "|p70id|int|" + v.p70ID.ToString();
            }
            if (v.p32IsBillable != null)
            {
                strMyQueryInline += "|p32isbillable|bool|" + BO.Code.Bas.GB(Convert.ToBoolean(v.p32IsBillable));
            }
            if (v.IsWip != null)
            {
                strMyQueryInline += "|iswip|bool|" + BO.Code.Bas.GB(Convert.ToBoolean(v.IsWip));
            }
            if (v.IsApproved_And_Wait4Invoice != null)
            {
                strMyQueryInline += "|isapproved_and_wait4invoice|bool|" + BO.Code.Bas.GB(Convert.ToBoolean(v.IsApproved_And_Wait4Invoice));
            }

            v.gridinput = new Views.Shared.Components.myGrid.myGridInput() { entity = "p31Worksheet", master_entity = "inform", myqueryinline = strMyQueryInline,oncmclick= "local_cm(event)" }; //grid má vlastní zdroj kontextového menu
            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load("p31", null, 0, strMyQueryInline);
            
            
            return View(v);
        }

    }
}
