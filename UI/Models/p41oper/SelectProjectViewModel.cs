

namespace UI.Models.p41oper
{
    public class SelectProjectViewModel:BaseViewModel
    {
        public string source_prefix { get; set; }
        public int source_pid { get; set; }

        public IEnumerable<BO.p41Project> lisP41 { get; set; }
        public int SelectedPid { get; set; }
        public int leindex { get; set; }
    }
}
