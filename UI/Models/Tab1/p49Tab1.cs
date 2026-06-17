namespace UI.Models.Tab1
{
    public class p49Tab1: BaseTab1ViewModel
    {
        public p49Tab1()
        {
            this.prefix = "p49";
        }
        public BO.p49FinancialPlan Rec { get; set; }
        public BO.p41Project RecP41 { get; set; }
    }
}
