using UI.Views.Shared.Components.myGrid;

namespace UI.Models.p31oper
{
    public class p31KorekceViewModel:BaseViewModel
    {
        public string pids { get; set; }
        public string pids_valid { get; set; }
        public IEnumerable<BO.p31Worksheet> lisP31 { get; set; }

        public myGridInput gridinput { get; set; }

        public int p72ID_AfterTrimming { get; set; }
    }
}
