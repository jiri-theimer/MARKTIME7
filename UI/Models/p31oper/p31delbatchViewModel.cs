using UI.Views.Shared.Components.myGrid;

namespace UI.Models.p31oper
{
    public class p31delbatchViewModel:BaseViewModel
    {
        
        public string pids { get; set; }
        public string pids_valid { get; set; }
        public int j72id { get; set; }
        public string oper { get; set; }
        public myGridInput gridinput { get; set; }
        
    }
}
