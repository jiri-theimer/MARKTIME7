

namespace UI.Models
{
    public class iCalGenViewModel:BaseViewModel
    {
        public string PersonNameFormat { get; set; }
        public bool IsMe { get; set; }
        public bool IsP31 { get; set; }
        public bool IsO22 { get; set; }
        public bool IsP56 { get; set; }
        public int j02ID { get; set; }
        public string ComboJ02ID { get; set; }
        public int j11ID { get; set; }
        public string ComboJ11ID { get; set; }
        public string p32IDs { get; set; }
        public IEnumerable<BO.p61ActivityCluster> lisP61 { get; set; }
        public int p61ID { get; set; }
        public string p32Names { get; set; }

        public int p41ID { get; set; }
        public string Project { get; set; }

        public IEnumerable<BO.x67EntityRole> lisX67_o22 { get; set; }
        public IEnumerable<BO.x67EntityRole> lisX67_p56 { get; set; }
        public int x67ID_o22 { get; set; }
        public int x67ID_p56 { get; set; }

        public DateTime? d1 { get; set; }
        public DateTime? d2 { get; set; }

        public string FinalUrl { get; set; }
    }
}
