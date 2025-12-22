using UI.Views.Shared.Components.myGrid;
using UI.Views.Shared.Components.myPeriod;

namespace UI.Models
{
    public class p40PostGenerateViewModel:BaseViewModel
    {
        public string p40ids { get; set; }
        public string oper { get; set; }

        public int j72id { get; set; }
        public myPeriodViewModel periodinput { get; set; } //fixní filtr v horním pruhu

        public IEnumerable<BO.p39WorkSheet_Recurrence_Plan> lisP39 { get;set; }

        public IEnumerable<BO.p40WorkSheet_Recurrence> lisP40 { get; set; }


        public myGridInput gridinput { get; set; }
    }
}
