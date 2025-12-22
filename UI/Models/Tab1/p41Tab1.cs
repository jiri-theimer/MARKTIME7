

namespace UI.Models.Tab1
{
    public class p41Tab1: BaseTab1ViewModel
    {
        public BO.p41Project Rec { get; set; }
        //public BO.p28Contact RecClient { get; set; }
        public BO.p41ProjectSum RecSum { get; set; }

       
        public IEnumerable<BO.p26ProjectContact> lisP26 { get; set; }


       

    }
}
