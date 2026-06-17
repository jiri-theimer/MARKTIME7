

namespace UI.Models.Record
{
    public class p42Record:BaseRecordViewModel
    {
        public BO.p42ProjectType Rec { get; set; }

        public List<p43Repeater> lisP43 { get; set; }
        
        public IEnumerable<BO.p34ActivityGroup> lisAllP34 { get; set; }

        
        public string ComboB01Name { get; set; }
        public string ComboX38Name { get; set; }
        public string ComboP61Name { get; set; }
        public CreateAssignViewModel creates { get; set; }

    }

    public class p43Repeater : BO.p43ProjectType_Workload
    {
        public bool IsSelected { get; set; }
        public string p34Name { get; set; }
    }
}
