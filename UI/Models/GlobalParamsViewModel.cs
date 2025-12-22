namespace UI.Models
{
    public class GlobalParamsViewModel:BaseViewModel
    {
        public List<BO.GlobalParam> lisParams { get; set; }
        public string prefix { get; set; }
        public int pid { get; set; }
    }
}
