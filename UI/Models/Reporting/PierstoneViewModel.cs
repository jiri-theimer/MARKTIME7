using UI.Views.Shared.Components.myPeriod;

namespace UI.Models.Reporting
{
    public class PierstoneViewModel:BaseViewModel
    {
        public string prefix { get; set; }
        public string pids { get; set; }
        public List<int> lisPIDs { get; set; }
        public p31StateQueryViewModel p31statequery { get; set; }   //filtrování podle stavu aktivit v horním pruhu
        public IEnumerable<BO.p31Worksheet> lisP31 { get; set; }

        public myPeriodViewModel PeriodFilter { get; set; }

        public string TempFileName { get; set; }
        public string PageOrientation { get; set; }
        public string ReportLanguaage { get; set; } //cs/eng

        public string DataScope { get; set; }   //time/all
    }
}
