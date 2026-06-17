using BL;
using BO;

namespace UI.Models.capacity
{
    

    


    public class CapacityTimelineViewModel : BaseViewModel
    {
        
        public CapacityTimelineViewModel()
        {
            this.d0 = DateTime.Now;
            this.CurYear = this.d0.Year;
            this.CurMonth = this.d0.Month;
            this.GroupBy = BO.CapacityGroupByEnum.Day;
            this.ViewYearColsCount = 2;
            this.ViewMonthColsCount = 4;
        }
        public string prefix { get; set; }      //j02 nebo p41
        public string UserKeyBase { get; set; }
        public bool IsReadOnly { get; set; }
        private BL.Factory _f { get; set; }
        public BO.myQueryJ02 ExternalQueryJ02 { get; set; }
        public BO.myQueryP41 ExternalQueryP41 { get; set; }
        public bool IsUseFaNefa { get; set; }
        public int p41ID { get; set; }
        public int j02ID { get; set; }
        public BO.p41Project RecP41 { get; set; }
        public BO.j02User RecJ02 { get; set; }
        
        public DateTime d0 { get; set; }
        public DateTime d1 { get; set; }
        public DateTime d2 { get; set; }
        public DateTime p41PlanFrom { get; set; }
        public DateTime p41PlanUntil { get; set; }

        public int CurMonth { get; set; }
        public int CurYear { get; set; }
        public DateTime OsaFirstDate { get; set; }
        public DateTime OsaLastDate { get; set; }

        public BO.CapacityGroupByEnum GroupBy { get; set; }
        public int ViewMonthColsCount { get; set; }
        public int ViewYearColsCount { get; set; }
        public List<CapResource> lisCapResource { get; set; }
        public List<CapProject> lisCapProject { get; set; }
        public IEnumerable<CapacityResourceGroupBy> lisData { get; set; }
        public IEnumerable<BO.r01Capacity> lisR01 { get; set; }
        public IEnumerable<BO.r04CapacityResource> lisR04 { get; set; }
        public IEnumerable<BO.r02CapacityVersion> lisR02 { get; set; }

        public List<r01Box> Boxes { get; set; }
        public List<r01Osa> Osax { get; set; }
        public List<r01Osa> Osau { get; set; }

        public int SelectedR02ID { get; set; }
        public BO.r02CapacityVersion RecR02 { get; set; }



        public bool FaZastropovan { get; set; }
        public bool NeFaZastropovan { get; set; }

        public int BoxWidth { get; set; }


        public void SetFactory(BL.Factory f)
        {
            _f = f;
        }
        private void InhaleProject()
        {
            if (this.p41ID == 0) return;

            if (this.RecP41 == null)
            {
                this.RecP41 = _f.p41ProjectBL.Load(this.p41ID);
            }
            
            this.p41PlanFrom = Convert.ToDateTime(this.RecP41.p41PlanFrom);
            this.p41PlanUntil = Convert.ToDateTime(this.RecP41.p41PlanUntil);
            
            if (this.lisR04 == null)
            {
                this.lisR04 = _f.p41ProjectBL.GetList_r04(this.p41ID,0);
            }
            

            this.FaZastropovan = this.RecP41.IsPlan_FaZastropovano();
            this.NeFaZastropovan = this.RecP41.IsPlan_NefaZastropovano();
        }

        public void RefreshState()
        {
            this.GroupBy = (CapacityGroupByEnum)_f.CBL.LoadUserParamInt($"{this.UserKeyBase}-GroupBy", 1);
            this.ViewMonthColsCount = _f.CBL.LoadUserParamInt($"{this.UserKeyBase}-ViewMonthColsCount", 4);
            this.ViewYearColsCount = _f.CBL.LoadUserParamInt($"{this.UserKeyBase}-ViewYearColsCount", 1);
            this.SelectedR02ID = _f.CBL.LoadUserParamInt($"{this.UserKeyBase}-r02ID");

            this.CurYear = _f.CBL.LoadUserParamInt($"{this.UserKeyBase}-CurYear", DateTime.Now.Year);
            this.CurMonth = _f.CBL.LoadUserParamInt($"{this.UserKeyBase}-CurMonth", DateTime.Now.Month);
            if (this.CurYear < DateTime.Now.Year - 2)
            {
                this.CurYear = DateTime.Now.Year; this.CurMonth = DateTime.Now.Month;
            }

            this.IsUseFaNefa = _f.Lic.x01IsCapacityFaNefa;
            if (this.p41ID > 0)
            {
                this.InhaleProject();
            }

            this.d0 = new DateTime(this.CurYear, this.CurMonth, 1);
            this.Osax = new List<r01Osa>();

            int intIndex = 1;
            switch (this.GroupBy)
            {
                case CapacityGroupByEnum.Day:
                    this.BoxWidth = 40;
                    this.d1 = new DateTime(this.d0.Year, this.d0.Month, 1);
                    this.d2 = this.d1.AddMonths(1).AddDays(-1);
                    for (DateTime d = this.d1; d <= this.d2; d = d.AddDays(1))
                    {
                        var osa = new r01Osa() { d1 = d, d2 = d, Label = d.ToString("dd") + "<br>" + BO.Code.Bas.DayOfWeekString(d).Substring(0, 2), Index = intIndex };
                        if (d <= this.p41PlanUntil && d >= this.p41PlanFrom)
                        {
                            osa.Label = osa.Label + "<div class='inplan'></div>";
                        }
                        this.Osax.Add(osa);
                        intIndex += 1;
                    }
                    break;
                case CapacityGroupByEnum.Week:
                    this.d1 = new DateTime(this.d0.Year, this.d0.Month, 1);
                    this.d2 = this.d0.AddMonths(3);

                    break;
                case CapacityGroupByEnum.Month:
                    this.BoxWidth = 9;
                    this.d1 = this.d0;
                    this.d2 = this.d0.AddMonths(this.ViewMonthColsCount - 1);


                    for (DateTime d = this.d1; d <= new DateTime(this.d2.Year, this.d2.Month, 1).AddMonths(1).AddDays(-1); d = d.AddDays(1))
                    {
                        var osa = new r01Osa() { d1 = d, d2 = d, Index = intIndex };
                        if (d <= this.p41PlanUntil && d >= this.p41PlanFrom)
                        {
                            osa.Label = "<div class='inplan'></div>";
                        }
                        this.Osax.Add(osa);
                        intIndex += 1;
                    }

                    break;
                case CapacityGroupByEnum.Year:
                    this.BoxWidth = 2;
                    this.d1 = new DateTime(this.d0.Year, 1, 1);
                    this.d2 = this.d1.AddYears(this.ViewYearColsCount - 1);

                    for (DateTime d = this.d1; d <= new DateTime(this.d2.Year, this.d2.Month, 1).AddYears(1).AddDays(-1); d = d.AddDays(1))
                    {
                        var osa = new r01Osa() { d1 = d, d2 = d, Index = intIndex };
                        if (d <= this.p41PlanUntil && d >= this.p41PlanFrom)
                        {
                            osa.Label = "<div class='inplan'></div>";
                        }
                        this.Osax.Add(osa);
                        intIndex += 1;
                    }
                    break;
            }

            this.OsaFirstDate = this.Osax.First().d1;
            this.OsaLastDate = this.Osax.Last().d1;

            
            this.lisR02 = _f.r02CapacityVersionBL.GetList(new myQuery("r02"));
            if (this.SelectedR02ID==0 && lisR02.Count() > 0)
            {
                this.SelectedR02ID = lisR02.First().pid;
            }
            this.lisData = _f.r01CapacityBL.GetList_GroupByJ02(new myQueryR01() {period_field="r05Date", p41id = this.p41ID, global_d1 = this.OsaFirstDate, global_d2 = this.OsaLastDate,r02id=this.SelectedR02ID }, this.GroupBy == CapacityGroupByEnum.Day ? this.GroupBy : CapacityGroupByEnum.Month);
            this.lisR01 = _f.r01CapacityBL.GetList(new myQueryR01() { p41id = this.p41ID, global_d1 = this.OsaFirstDate, global_d2 = this.OsaLastDate,r02id=this.SelectedR02ID });

            if (this.ExternalQueryJ02 == null)
            {
                this.ExternalQueryJ02 =new BO.myQueryJ02() { explicit_orderby = "a.j02LastName" };
            }

            
            if (this.p41ID > 0)
            {
                if (this.lisR04.Count() > 0)
                {
                    this.ExternalQueryJ02.pids = this.lisR04.Select(p => p.j02ID).ToList();
                }
                else
                {
                    this.ExternalQueryJ02.pids = new List<int> { -1 };
                }                
            }

            var lisJ02 = _f.j02UserBL.GetList(this.ExternalQueryJ02);
            this.lisCapResource = new List<CapResource>();
            int intLastC21ID = 0; double dblLastFond = 0;

            foreach (var recJ02 in lisJ02)
            {
                if (recJ02.c21ID > 0)
                {
                    if (recJ02.c21ID != intLastC21ID)
                    {
                        dblLastFond = _f.c21FondCalendarBL.GetSumHours(recJ02.c21ID, recJ02.j02CountryCode, this.OsaFirstDate, this.OsaLastDate);
                    }
                    intLastC21ID = recJ02.c21ID;
                    recJ02.Rezerva = dblLastFond;
                }

                var c = new CapResource() { j02ID = recJ02.pid, Person = BO.Code.Bas.OM2(recJ02.FullnameDesc, 15), FondHours = recJ02.Rezerva };

                if (this.lisR04 != null)
                {
                    var qry = this.lisR04.Where(p => p.j02ID == recJ02.pid);
                    if (qry.Count() > 0)
                    {
                        c.r04HoursFa = qry.First().r04HoursFa;
                        c.r04HoursNeFa = qry.First().r04HoursNeFa;
                        c.r04HoursTotal = qry.First().r04HoursTotal;
                        c.r04Text = qry.First().r04Text;
                    }
                }
                this.lisCapResource.Add(c);
            }


            this.Boxes = new List<r01Box>();

            foreach (var c in this.lisR01)
            {
                var box = new r01Box() { j02ID = c.j02ID, p41ID = c.p41ID, HoursFa = c.r01HoursFa, HoursNefa = c.r01HoursNeFa, HoursTotal = c.r01HoursTotal, r01ID = c.pid,Project=BO.Code.Bas.OM2(c.Project,15),d1=c.r01Start,d2=c.r01End };
                if (c.r01Color == null)
                {
                    //box.Color = "#CFF4FC";
                    box.Color = "khaki";
                }
                else
                {
                    box.Color = c.r01Color;
                }
                box.Title = c.r01HoursTotal.ToString() + "h  ";
                if (c.r01Start.Day == c.r01End.Day && c.r01Start.Month == c.r01End.Month)
                {
                    box.Title += " " + c.r01Start.ToString("dd.MM. ddd");
                }
                else
                {
                    box.Title += " " + c.r01DaysPlan.ToString() + "d: ";
                    if (c.r01Start.Year != c.r01End.Year)
                    {
                        box.Title += " " + c.r01Start.ToString("dd.MM.yyyy") + " - " + c.r01End.ToString("dd.MM.yyyy");
                    }
                    else
                    {
                        box.Title += " " + c.r01Start.ToString("dd.MM. ddd") + " - " + c.r01End.ToString("dd.MM.ddd");
                    }
                }
                if (c.r01Text != null)
                {
                    box.Title += " ..." + c.r01Text;
                }
                if (this.p41ID == 0)
                {
                    box.Title += ": " +BO.Code.Bas.OM2(c.Client,15) +" - "+ c.Project;
                }
                switch (this.GroupBy)
                {
                    case CapacityGroupByEnum.Day:
                        box.ColStart = c.r01Start.Day;
                        if (c.r01Start.Month != this.d1.Month)
                        {
                            box.ColStart = 1;
                        }
                        box.ColSpan = c.r01End.Day - box.ColStart + 1;

                        if (c.r01End.Month != this.OsaLastDate.Month)
                        {
                            box.ColSpan = this.Osax.Count() - box.ColStart + 1;
                        }
                        break;
                    case CapacityGroupByEnum.Month:
                    case CapacityGroupByEnum.Year:

                        box.ColStart = getdays(this.d1, c.r01Start);
                        if (c.r01Start > this.d1)
                        {
                            box.ColSpan = getdays(c.r01Start, c.r01End);
                        }
                        else
                        {
                            box.ColSpan = getdays(this.d1, c.r01End);
                        }

                        if (c.r01Start <= this.d1)
                        {
                            box.ColStart = 1;
                        }
                        if (c.r01End >= this.OsaLastDate)
                        {
                            box.ColSpan = this.Osax.Count() - box.ColStart + 1;
                        }

                        break;
                }

                box.DaysPlan = c.r01DaysPlan;

                box.ColStart += 1;
                this.Boxes.Add(box);

            }

        }

        private int getdays(DateTime d1, DateTime d2)
        {
            return (d2 - d1).Days + 1;
        }

    }


}