using UI.Views.Shared.Components.myGrid;
using UI.Views.Shared.Components.myPeriod;

namespace UI.Models.Imap
{
    public class MyInboxViewModel:BaseViewModel
    {
        public BO.j40MailAccount RecJ40 { get; set; }
        public IEnumerable<BO.j40MailAccount> lisJ40 { get; set; }
        public int SelectedJ40ID { get; set; }
        public string SelectedFolder { get; set; }
        public List<string> Folders { get; set; }

        public int TakeLastTop { get; set; }

        public string TakeSeenFlag { get; set; }
        public string TakeFlaggedFlag { get; set; }     //all/true/false
        public string NahazovatZpravamPriznak { get; set; } //seen/deleted

        public myGridInput gridinput { get; set; }

        public myPeriodViewModel periodinput { get; set; }
        public RecordBinQueryViewModel recordbinquery { get; set; } //filtrování podle archivu

        public int MaVazbu { get; set; }
    }
}
