namespace MO.Models
{
    public class DayViewModel : BaseViewModel
    {
        public DateTime Date { get; set; }
        public IEnumerable<BO.p31Worksheet> Entries { get; set; } = new List<BO.p31Worksheet>();
        public double TotalHours { get; set; }
        public bool IsHoliday { get; set; }
        public string HolidayName { get; set; }

        public IEnumerable<BO.p34ActivityGroup> SesitList { get; set; } = new List<BO.p34ActivityGroup>();

        /// <summary>Úkoly přidělené uživateli s termínem (p56PlanUntil) tento den.</summary>
        public List<BO.p56Task> TasksDue { get; set; } = new List<BO.p56Task>();

        /// <summary>Kalendářové termíny (o22Milestone) tento den - jen pro čtení.</summary>
        public List<BO.o22Milestone> Milestones { get; set; } = new List<BO.o22Milestone>();

        public string DayName => System.Globalization.CultureInfo.CurrentUICulture
            .DateTimeFormat.GetDayName(Date.DayOfWeek);
    }
}