

namespace BO.Code
{
    public enum RecurrenceTypeENUM
    {
        Day = 1,
        Week = 2,
        Month = 3,
        Quarter = 4,
        Year = 5,
        Weeks2=6
    }

    public static class Recurrence
    {
        public static int GetPocetCyklu(DateTime d1,DateTime d2,RecurrenceTypeENUM recurrencetype)
        {           
            DateTime dtmp = d1;
            int units = 0;
            
            while (dtmp < d2)
            {
                units++;

                switch (recurrencetype)
                {
                    case RecurrenceTypeENUM.Day:
                        dtmp = dtmp.AddDays(1);
                        break;
                    case RecurrenceTypeENUM.Week:
                        dtmp = dtmp.AddDays(7);
                        break;
                    case RecurrenceTypeENUM.Month:
                        dtmp = dtmp.AddMonths(1);
                        break;
                    case RecurrenceTypeENUM.Quarter:
                        dtmp = dtmp.AddMonths(3);
                        break;
                    case RecurrenceTypeENUM.Year:
                        dtmp = dtmp.AddYears(1);
                        break;
                    case RecurrenceTypeENUM.Weeks2:
                        dtmp = dtmp.AddDays(14);
                        break;
                }
            }

            return units;
        }

        public static List<DateTime> GetCykly(DateTime d1, DateTime d2, RecurrenceTypeENUM recurrencetype)
        {
            DateTime dtmp = d1;
            int units = 0;
            var ret = new List<DateTime>();
            ret.Add(d1);

            while (dtmp < d2)
            {
                units++;

                switch (recurrencetype)
                {
                    case RecurrenceTypeENUM.Day:
                        dtmp = dtmp.AddDays(1);
                        break;
                    case RecurrenceTypeENUM.Week:
                        dtmp = dtmp.AddDays(7);
                        break;
                    case RecurrenceTypeENUM.Month:
                        dtmp = dtmp.AddMonths(1);
                        break;
                    case RecurrenceTypeENUM.Quarter:
                        dtmp = dtmp.AddMonths(3);
                        break;
                    case RecurrenceTypeENUM.Year:
                        dtmp = dtmp.AddYears(1);
                        break;
                }
                ret.Add(dtmp);
            }

            return ret;
        }
    }
}
