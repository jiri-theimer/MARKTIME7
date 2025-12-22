
using UI.Views.Shared.Components.myGrid;

namespace UI.Models.Record
{
    public class j11Record: BaseRecordViewModel
    {
        public BO.j11Team Rec { get; set; }
        public string j02IDs { get; set; }

        public int SelectedJ02ID { get; set; }
        public string SelectedPerson { get; set; }
        public int SelectedJ07ID { get; set; }
        public string SelectedPosition { get; set; }

        public myGridInput gridinput { get; set; }
    }
}
