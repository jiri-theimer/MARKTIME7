namespace UI.Models.Tab1
{
    public class p58Tab1: BaseTab1ViewModel
    {
        public BO.p58TaskRecurrence Rec { get; set; }

        public BO.p41Project RecP41 { get; set; }

        public IEnumerable<BO.p59TaskRecurrence_Plan> lisP59 { get; set; }
    }
}
