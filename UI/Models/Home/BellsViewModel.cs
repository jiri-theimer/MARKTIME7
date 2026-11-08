namespace UI.Models.Home
{
    public class BellsViewModel : UI.Models.BaseViewModel
    {
        public string ViewMode { get; set; }      //"" = aktivní, "hidden" = skryté, "showall" = vše
        public List<BO.j97BellsLog> Items { get; set; }
        public int ActiveCount { get; set; }
    }
}
