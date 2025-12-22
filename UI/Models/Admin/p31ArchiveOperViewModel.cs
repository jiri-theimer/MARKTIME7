namespace UI.Models.Admin
{
    public class p31ArchiveOperViewModel:BaseViewModel
    {
        public string oper { get; set; }
        public int SelectedYear { get; set; }

        public IEnumerable<BO.p31TotalByYear> lisStat { get; set; }
    }
}
