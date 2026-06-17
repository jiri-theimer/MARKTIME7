using BO;

namespace UI.Models.capacity
{
    public class r01Osa
    {
        public DateTime d1 { get; set; }
        public DateTime d2 { get; set; }
        public string Label { get; set; }

        public bool InPlan { get; set; }
        public int Index { get; set; }
    }

    public class r01Box
    {
        public int ColStart { get; set; }
        public int ColSpan { get; set; }

        public string Title { get; set; }
        public int r01ID { get; set; }
        public int j02ID { get; set; }
        public int p41ID { get; set; }
        public string Project { get; set; }
        public string Person { get; set; }
        public string Person_Inicialy { get; set; }
        public double HoursFa { get; set; }
        public double HoursNefa { get; set; }
        public double HoursTotal { get; set; }
        public int DaysPlan { get; set; }
        public string Color { get; set; }
        public DateTime d1 { get; set; }
        public DateTime d2 { get; set; }

    }

    public class CapResource
    {
        public int j02ID { get; set; }
        public string Person { get; set; }
        public double FondHours { get; set; }
        public double r04HoursFa { get; set; }
        public double r04HoursNeFa { get; set; }
        public double r04HoursTotal { get; set; }
        public string r04Text { get; set; }
    }

    public class CapProject
    {
        public int p41ID { get; set; }
        public string Project { get; set; }
        public DateTime? p41PlanFrom { get; set; }
        public DateTime? p41PlanUntil { get; set; }
        public double r04HoursFa { get; set; }
        public double r04HoursNeFa { get; set; }
        public double r04HoursTotal { get; set; }
        public int ObdobiGridColumn { get; set; }
        public int ObdobiGridSpan { get; set; }

    }

    public class CapacityTimelineBase:BaseViewModel
    {
        public CapacityTimelineBase()
        {
            this.d0 = DateTime.Now;
            this.CurYear = this.d0.Year;
            this.CurMonth = this.d0.Month;
            this.GroupBy = BO.CapacityGroupByEnum.Day;
            this.ViewYearColsCount = 2;
            this.ViewMonthColsCount = 4;
        }
        public bool IsReadOnly { get; set; }
        internal BL.Factory _f { get; set; }

        public DateTime d0 { get; set; }
        public DateTime d1 { get; set; }
        public DateTime d2 { get; set; }
        public bool IsUseFaNefa { get; set; }
        public string UserKeyBase { get; set; }
        public int CurMonth { get; set; }
        public int CurYear { get; set; }
        public DateTime OsaFirstDate { get; set; }
        public DateTime OsaLastDate { get; set; }

        public BO.CapacityGroupByEnum GroupBy { get; set; }
        public int ViewMonthColsCount { get; set; }
        public int ViewYearColsCount { get; set; }
        
        
        
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


        internal void RefreshStateBase()
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
                        //if (d <= this.p41PlanUntil && d >= this.p41PlanFrom)
                        //{
                        //    osa.Label = osa.Label + "<div class='inplan'></div>";
                        //}
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
                        //if (d <= this.p41PlanUntil && d >= this.p41PlanFrom)
                        //{
                        //    osa.Label = "<div class='inplan'></div>";
                        //}
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
                        //if (d <= this.p41PlanUntil && d >= this.p41PlanFrom)
                        //{
                        //    osa.Label = "<div class='inplan'></div>";
                        //}
                        this.Osax.Add(osa);
                        intIndex += 1;
                    }
                    break;
            }

            this.OsaFirstDate = this.Osax.First().d1;
            this.OsaLastDate = this.Osax.Last().d1;


            this.lisR02 = _f.r02CapacityVersionBL.GetList(new myQuery("r02"));
            if (this.SelectedR02ID == 0 && lisR02.Count() > 0)
            {
                this.SelectedR02ID = lisR02.First().pid;
            }
            

            

        }

        internal int getdays(DateTime d1, DateTime d2)
        {
            return (d2 - d1).Days + 1;
        }

    }
}
