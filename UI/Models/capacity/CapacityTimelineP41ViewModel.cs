using BO;

namespace UI.Models.capacity
{
    public class CapacityTimelineP41ViewModel : CapacityTimelineBase
    {
        public BO.myQueryP41 ExternalQuery { get; set; }

        public int j02ID { get; set; }
        public BO.j02User RecJ02 { get; set; }

        public List<CapProject> lisCapProject { get; set; }
        public IEnumerable<CapacityProjectGroupBy> lisData { get; set; }

        public void RefreshState()
        {
            this.RefreshStateBase();

            if (this.j02ID > 0)
            {
                this.InhalePerson();
            }

            this.lisData = _f.r01CapacityBL.GetList_GroupByP41(new BO.myQueryR01() { period_field = "r05Date", j02id = this.j02ID, global_d1 = this.OsaFirstDate, global_d2 = this.OsaLastDate, r02id = this.SelectedR02ID }, this.GroupBy == BO.CapacityGroupByEnum.Day ? this.GroupBy : BO.CapacityGroupByEnum.Month);
            this.lisR01 = _f.r01CapacityBL.GetList(new BO.myQueryR01() { j02id = this.j02ID, global_d1 = this.OsaFirstDate, global_d2 = this.OsaLastDate, r02id = this.SelectedR02ID });

            if (this.ExternalQuery == null)
            {
                this.ExternalQuery = new BO.myQueryP41("le5") { AvailableCapacityD1 = this.OsaFirstDate, AvailableCapacityD2 = this.OsaLastDate };
            }

            if (this.j02ID > 0)
            {
                if (this.lisR04.Count() > 0)
                {
                    this.ExternalQuery.pids = this.lisR04.Select(p => p.p41ID).ToList();
                }
                else
                {
                    this.ExternalQuery.pids = new List<int> { -1 };
                }
            }

            var lisP41 = _f.p41ProjectBL.GetList(this.ExternalQuery);
            if (this.lisR04 == null && lisP41.Count()>0)
            {
                this.lisR04 = _f.p41ProjectBL.GetList_r04(lisP41.Select(p => p.pid).ToList(),null);
            }
            
            this.lisCapProject = new List<CapProject>();

            foreach (var recP41 in lisP41)
            {
                var c = new CapProject() { p41ID = recP41.pid,p41PlanFrom=recP41.p41PlanFrom,p41PlanUntil=recP41.p41PlanUntil };
                if (recP41.p41NameShort != null)
                {
                    c.Project = recP41.p41NameShort;
                }
                else
                {
                    c.Project = recP41.p41Name;
                }
                if (recP41.Client != null)
                {
                    c.Project = $"{BO.Code.Bas.LeftString(recP41.Client,10)} - {BO.Code.Bas.LeftString(c.Project, 15)}";
                }
                else
                {
                    c.Project = BO.Code.Bas.LeftString(recP41.p41Name, 20);

                }

                if (this.lisR04 != null)
                {
                    var qry = this.lisR04.Where(p => p.p41ID == recP41.pid);
                    if (qry.Count() > 0 && recP41.IsPlan_FaZastropovano())
                    {
                        c.r04HoursFa = qry.Sum(p => p.r04HoursFa);
                        c.r04HoursNeFa = qry.Sum(p => p.r04HoursNeFa);
                        c.r04HoursTotal = qry.Sum(p => p.r04HoursTotal);
                        
                    }
                }
                if (c.p41PlanFrom !=null && c.p41PlanUntil != null)
                {
                    c.ObdobiGridColumn = 0;
                    DateTime d1 = (c.p41PlanFrom >= this.OsaFirstDate) ? Convert.ToDateTime(c.p41PlanFrom) : this.OsaFirstDate;
                    DateTime d2 = (c.p41PlanUntil >= this.OsaLastDate) ? this.OsaLastDate : Convert.ToDateTime(c.p41PlanUntil);
                    c.ObdobiGridColumn = 1+(int)(d1 - this.OsaFirstDate).TotalDays;
                    c.ObdobiGridSpan = 1+(int)(d2 - d1).TotalDays;
                   
                    
                    
                }
                this.lisCapProject.Add(c);
            }

            this.Boxes = new List<r01Box>();

            foreach (var c in this.lisR01)
            {
                var box = new r01Box() { j02ID = c.j02ID, p41ID = c.p41ID, HoursFa = c.r01HoursFa, HoursNefa = c.r01HoursNeFa, HoursTotal = c.r01HoursTotal, r01ID = c.pid, Person = c.PersonAsc,Person_Inicialy=c.Person_Inicialy, d1 = c.r01Start, d2 = c.r01End };
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
                if (this.j02ID == 0)
                {
                    box.Title += ": " + c.PersonAsc;
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

        private void InhalePerson()
        {
            if (this.j02ID == 0) return;

            if (this.RecJ02 == null)
            {
                this.RecJ02 = _f.j02UserBL.Load(this.j02ID);
            }


            if (this.lisR04 == null)
            {
                this.lisR04 = _f.p41ProjectBL.GetList_r04(0, this.j02ID);
            }



        }
    }
}