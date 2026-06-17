using System;

namespace BO
{
    public enum c21ScopeFlagENUM
    {
        Basic = 1,
        Matrix = 2,
        PerTimesheet = 3
    }

    public class c21FondCalendar:BaseBO
    {
        public c21ScopeFlagENUM c21ScopeFlag { get; set; } = c21ScopeFlagENUM.Basic;
        public int x01ID { get; set; }
        public string c21Name { get; set; }
        public bool c21IsTimesheetHours { get; set; }
        public int c21Ordinary { get; set; }
        public double c21Day1_Hours { get; set; }
        public double c21Day2_Hours { get; set; }
        public double c21Day3_Hours { get; set; }
        public double c21Day4_Hours { get; set; }
        public double c21Day5_Hours { get; set; }
        public double c21Day6_Hours { get; set; }
        public double c21Day7_Hours { get; set; }
    }
}
