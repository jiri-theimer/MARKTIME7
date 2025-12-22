namespace UI.Models.Tab1
{
    public class p84Tab1: BaseTab1ViewModel
    {
        public p84Tab1()
        {
            this.prefix = "p84";
        }
        public BO.p84Upominka Rec { get; set; }
        public BO.p91Invoice RecP91 { get; set; }
        public BO.p83UpominkaType RecP83 { get; set; }

    }
}
