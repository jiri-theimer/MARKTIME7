using UI.Models.p31view;

namespace UI.Models
{
    public class p12ApproveViewModel: BaseViewModel
    {
        public int j02ID { get; set; }
        public BO.j02User RecJ02 { get; set; }

        public calendarViewModel LeftPanel { get; set; }

        public string ApproveStatus { get; set; }

        public DateTime d0 { get; set; }
        public DateTime d1 { get; set; }
        public DateTime d2 { get; set; }
        public int jump2row { get; set; }

        public int m0 { get; set; }
        public int y0 { get; set; }
        public bool IsAgendaDescending { get; set; }
        public string JakPocitatPrescas { get; set; }
        public IEnumerable<BO.p12ApproveUserDay> lisP12 { get; set; }
        public IEnumerable<BO.p11Attendance> lisP11 { get; set; }
        public IEnumerable<BO.p32Activity> lisP32FUT { get; set; }
        public IEnumerable<BO.p32Activity> lisP32FUT_OsaX { get; set; }
        public IEnumerable<BO.c24DayColor> lisC24 { get; set; }
        public IEnumerable<BO.c23PersonalDayColor> lisC23 { get; set; }
        public IEnumerable<BO.p31Worksheet> lisP31 { get; set; }
        public IEnumerable<BO.c26Holiday> lisC26 { get; set; }
        public List<BO.p31WorksheetTimelineDay> lisSums { get; set; }
        public IEnumerable<BO.c22FondCalendar_Date> lisC22 { get; set; }


        public string FT(double h)
        {
            if (h == 0) return null;
            if (this.RecJ02.j02DefaultHoursFormat == "T")
            {
                return BO.Code.Time.FormatNumeric(h, true);
            }
            else
            {
                return BO.Code.Time.FormatNumeric(h, false);
            }

        }
    }
}
