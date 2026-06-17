namespace UI.Models
{
    public class p84CreateViewModel:BaseViewModel
    {
        public IEnumerable<BO.p91Invoice> lisP91 { get; set; }
        public string p91ids { get; set; }

        public List<BO.StringPair> RetErrors { get; set; }
    }
}
