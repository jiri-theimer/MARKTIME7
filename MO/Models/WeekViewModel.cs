namespace MO.Models
{
    public class WeekDay
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; }
        public bool IsHoliday { get; set; }
        public string HolidayName { get; set; }
        public IEnumerable<BO.p31Worksheet> Entries { get; set; } = new List<BO.p31Worksheet>();
        public double TotalHours { get; set; }

        public int MoneyEntryCount => Entries?.Count(e =>
            e.p33ID == BO.p33IdENUM.PenizeBezDPH || e.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu) ?? 0;

        /// <summary>Úkoly přidělené uživateli s termínem (p56PlanUntil) tento den.</summary>
        public List<BO.p56Task> TasksDue { get; set; } = new List<BO.p56Task>();

        /// <summary>Kalendářové termíny (o22Milestone) tento den - jen pro čtení.</summary>
        public List<BO.o22Milestone> Milestones { get; set; } = new List<BO.o22Milestone>();
    }

    public class WeekViewModel : BaseViewModel
    {
        public DateTime d0 { get; set; }     // kotevní datum (libovolný den v zobrazeném týdnu)
        public DateTime d1 { get; set; }     // pondělí
        public DateTime d2 { get; set; }     // neděle nebo pátek (dle ShowWeekend)

        public bool ShowWeekend { get; set; }

        public List<WeekDay> Days { get; set; } = new List<WeekDay>();

        public double WeekTotalHours => Days.Sum(d => d.TotalHours);
    }
}