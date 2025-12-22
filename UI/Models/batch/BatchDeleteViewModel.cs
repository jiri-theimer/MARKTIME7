using UI.Views.Shared.Components.myGrid;

namespace UI.Models.batch
{
    public class BatchDeleteViewModel : BaseViewModel
    {
        public int j72id { get; set; }
        public string entity { get; set; }
        public string prefix { get; set; }
        public string pids { get; set; }
        public string pids_valid { get; set; }
        public myGridInput gridinput { get; set; }
        public string oper { get; set; }    //delete nebo archive nebo restore
    }
}
