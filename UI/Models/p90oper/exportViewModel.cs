namespace UI.Models.p90oper
{
    public class exportViewModel:BaseViewModel
    {
        public string p90ids { get; set; }
        public string destformat { get; set; }  //isdoc/pohoda
        public bool iszip { get; set; }
        public string tempsubfolder { get; set; }
        public IEnumerable<BO.p90Proforma> lisP90 { get; set; }
        public List<string> FileNames { get; set; }
    }
}
