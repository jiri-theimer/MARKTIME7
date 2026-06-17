namespace UI.Models.Tab1
{
    public class o51Tab1: BaseTab1ViewModel
    {
        public o51Tab1()
        {
            this.prefix = "o51";
        }
        public BO.o51Tag Rec { get; set; }
        public BO.o51TagSum RecSum { get; set; }

    }
}
