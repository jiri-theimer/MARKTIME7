using UI.Views.Shared.Components.myGrid;

namespace UI.Models.Record
{
    public class NajitTextViewModel:BaseViewModel
    {
        public string field { get; set; }
        public string prefix { get; set; }        

        public myGridInput gridinput { get; set; }
    }
}
