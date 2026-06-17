using BO;

namespace UI.Models.capacity
{
    public class CapacityTimelineJ02ViewModel:CapacityTimelineBase
    {
        public BO.myQueryJ02 ExternalQuery { get; set; }
        public int p41ID { get; set; }       
        public BO.p41Project RecP41 { get; set; }

        public List<CapResource> lisCapResource { get; set; }
        public IEnumerable<CapacityResourceGroupBy> lisData { get; set; }

        public DateTime p41PlanFrom { get; set; }
        public DateTime p41PlanUntil { get; set; }

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
            this.RefreshStateBase();

            if (this.p41ID > 0)
            {
                this.InhaleProject();
            }

            this.lisData = _f.r01CapacityBL.GetList_GroupByJ02(new BO.myQueryR01() { period_field = "r05Date", p41id = this.p41ID, global_d1 = this.OsaFirstDate, global_d2 = this.OsaLastDate, r02id = this.SelectedR02ID }, this.GroupBy == BO.CapacityGroupByEnum.Day ? this.GroupBy : BO.CapacityGroupByEnum.Month);
            this.lisR01 = _f.r01CapacityBL.GetList(new BO.myQueryR01() { p41id = this.p41ID, global_d1 = this.OsaFirstDate, global_d2 = this.OsaLastDate, r02id = this.SelectedR02ID });

            if (this.ExternalQuery == null)
            {
                this.ExternalQuery = new BO.myQueryJ02() { explicit_orderby = "a.j02LastName" };
            }

            if (this.p41ID > 0)
            {
                if (this.lisR04.Count() > 0)
                {
                    this.ExternalQuery.pids = this.lisR04.Select(p => p.j02ID).ToList();
                }
                else
                {
                    this.ExternalQuery.pids = new List<int> { -1 };
                }
            }

            var lisJ02 = _f.j02UserBL.GetList(this.ExternalQuery);
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
                var box = new r01Box() { j02ID = c.j02ID, p41ID = c.p41ID, HoursFa = c.r01HoursFa, HoursNefa = c.r01HoursNeFa, HoursTotal = c.r01HoursTotal, r01ID = c.pid, Project = BO.Code.Bas.OM2(c.Project, 15), d1 = c.r01Start, d2 = c.r01End };
                if (c.r01Color == null)
                {                    
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
                    box.Title += ": " + BO.Code.Bas.OM2(c.Client, 15) + " - " + c.Project;
                }
                switch (this.GroupBy)
                {
                    case BO.CapacityGroupByEnum.Day:
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
                    case BO.CapacityGroupByEnum.Month:
                    case BO.CapacityGroupByEnum.Year:

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
    }
}
