namespace MO.Models
{
    public class CalendarViewModel : BaseViewModel
    {
        public DateTime d0 { get; set; }      // kotevní datum (libovolný den v zobrazeném měsíci)
        public DateTime d1 { get; set; }      // první den mřížky (pondělí)
        public DateTime d2 { get; set; }      // poslední den mřížky (neděle nebo pátek)
        public int Year => d0.Year;
        public int Month => d0.Month;

        public bool ShowWeekend { get; set; }

        public IEnumerable<BO.p31Worksheet> lisP31 { get; set; } = new List<BO.p31Worksheet>();
        public List<BO.p31WorksheetTimelineDay> lisSums { get; set; } = new List<BO.p31WorksheetTimelineDay>();
        public IEnumerable<BO.c26Holiday> lisC26 { get; set; } = new List<BO.c26Holiday>();

        /// <summary>Úkoly přidělené uživateli s termínem (p56PlanUntil) v zobrazeném období.</summary>
        public IEnumerable<BO.p56Task> lisTasks { get; set; } = new List<BO.p56Task>();

        public int GetTasksDueCount(DateTime d)
        {
            return lisTasks?.Count(t => t.p56PlanUntil?.Date == d.Date) ?? 0;
        }

        /// <summary>Kalendářové termíny (o22Milestone) v zobrazeném období - jen pro čtení.</summary>
        public IEnumerable<BO.o22Milestone> lisMilestones { get; set; } = new List<BO.o22Milestone>();

        public int GetMilestonesCount(DateTime d)
        {
            return lisMilestones?.Count(m => m.o22PlanFrom?.Date == d.Date) ?? 0;
        }

        // Pomocné: rychlý lookup hodin a stavů pro den
        public double GetHours(DateTime d)
        {
            var sum = lisSums?.FirstOrDefault(s => s.p31Date.Date == d.Date);
            return sum != null ? sum.Hours : 0;
        }

        public int GetEntryCount(DateTime d)
        {
            return lisP31?.Count(p => p.p31Date.Date == d.Date) ?? 0;
        }

        public int GetMoneyEntryCount(DateTime d)
        {
            return lisP31?.Count(p => p.p31Date.Date == d.Date &&
                (p.p33ID == BO.p33IdENUM.PenizeBezDPH || p.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu)) ?? 0;
        }

        public bool IsHoliday(DateTime d)
        {
            return lisC26?.Any(h => h.c26Date.Date == d.Date) == true;
        }

        public string GetHolidayName(DateTime d)
        {
            return lisC26?.FirstOrDefault(h => h.c26Date.Date == d.Date)?.c26Name;
        }
    }
}