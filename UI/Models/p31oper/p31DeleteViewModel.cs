using UI.Views.Shared.Components.myGrid;

namespace UI.Models.p31oper
{
    public class p31DeleteViewModel:BaseViewModel
    {
        public string pids { get; set; }
        public string pids_valid { get; set; }
        public IEnumerable<BO.p31Worksheet> lisSourceP31 { get; set; }
        public myGridInput gridinput { get; set; }
    }
}
