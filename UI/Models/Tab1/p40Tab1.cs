namespace UI.Models.Tab1
{
    public class p40Tab1:BaseTab1ViewModel
    {
        public p40Tab1()
        {
            this.prefix = "p40";
        }
        public BO.p40WorkSheet_Recurrence Rec { get; set; }
        public BO.p41Project RecP41 { get; set; }
        public IEnumerable<BO.p39WorkSheet_Recurrence_Plan> lisP39 { get; set; }

        public int PlusMinusp39Mesice { get; set; }
    }
}
