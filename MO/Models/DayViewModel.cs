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

        public string DayName => System.Globalization.CultureInfo.CurrentUICulture
            .DateTimeFormat.GetDayName(Date.DayOfWeek);
    }
}