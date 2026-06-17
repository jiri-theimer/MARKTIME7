
namespace UI.Models.p91oper
{
    public class exportViewModel:BaseViewModel
    {
        public string p91ids { get; set; }        
        public string destformat { get; set; }  //isdoc/pohoda
        public bool iszip { get; set; }
        public bool isjointexts { get; set; }
        public string tempsubfolder { get; set; }
        public IEnumerable<BO.p91Invoice> lisP91 { get; set; }
        public List<string> FileNames { get; set; }

        

    }
}
