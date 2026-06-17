using System;


namespace BO
{
    public class p31WorksheetCalendarDay
    {
        public DateTime p31Date { get; set; }
        public double Hours { get; set; }
        public double Hours_Billable { get; set; }
        public double Hours_NonBillable { get; set; }
        public int Pieces { get; set; }
        public int Moneys { get; set; }

        public bool IsHoliday { get; set; }
        public string HolidayName { get; set; }
    }

    public class p31WorksheetTimelineDay : p31WorksheetCalendarDay
    {
        public int j02ID { get; set; }
        public string Person { get; set; }
        public string p32Color { get; set; }
        public string HoursFormatted { get; set; }
        public string CssStyle { get; set; }
        public string p31DateString { get; }
    }
}
