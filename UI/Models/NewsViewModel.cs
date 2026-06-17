namespace UI.Models
{
    public class NewsViewModel : BaseViewModel
    {
        public int RowsCount { get; set; }
        public IEnumerable<BO.x52Blog> lisX52 { get; set; }
        
    }
}
