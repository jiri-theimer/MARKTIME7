namespace UI.Models.Tab1
{
    public class p51Tab1:BaseTab1ViewModel
    {
        public p51Tab1()
        {
            this.prefix = "p51";
        }
        public BO.p51PriceList Rec { get; set; }
        public BO.p51PriceListSum RecSum { get; set; }

        public IEnumerable< BO.p52PriceList_Item> lisP52 { get; set; }
    }
}
