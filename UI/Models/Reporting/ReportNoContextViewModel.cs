
using UI.Views.Shared.Components.myPeriod;

namespace UI.Models
{
    public class ReportNoContextViewModel : BaseViewModel
    {
        public string caller_prefix { get; set; }
        public string caller_pids { get; set; }
        public BO.x31Report RecX31 { get; set; }
        public string ReportFileName { get; set; }
        public string ReportExportName { get; set; }
        public int SelectedX31ID { get; set; }
        public string SelectedReport { get; set; }
        public string Translate { get; set; }

        public bool NoFramework { get; set; }

        public bool IsPeriodFilter { get; set; }
        public DateTime d1 { get; set; }
        public DateTime d2 { get; set; }

        public bool IsConfirmNeeded { get; set; }
        public bool IsConfirmed { get; set; }

        public myPeriodViewModel PeriodFilter { get; set; }

        public int SelectedJ72ID { get; set; }

        public IEnumerable<BO.j72TheGridTemplate> lisJ72 { get; set; }

        public int LangIndex { get; set; }

        public string GeneratedOutputFileName { get; set; }
        
        public string CsvDelimiter { get; set; }
        public bool CsvUvozovky { get; set; } = true;

    }
}
