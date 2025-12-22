using UI.Views.Shared.Components.myGrid;
using UI.Views.Shared.Components.myPeriod;

namespace UI.Models.p31oper
{
    public class p31ExcludeBillingViewModel: BaseViewModel
    {
        public string prefix { get; set; }
        public string pids { get; set; }
        public bool IsOnlyHours { get; set; }
        public IEnumerable<BO.p31Worksheet> lisP31 { get; set; }

        public BO.p41Project RecP41 { get; set; }

        public int p31ExcludeBillingFlag { get; set; }

        
        public string Filter { get; set; }
        public myGridInput gridinput { get; set; }

        public myPeriodViewModel periodinput { get; set; } 
    }
}
