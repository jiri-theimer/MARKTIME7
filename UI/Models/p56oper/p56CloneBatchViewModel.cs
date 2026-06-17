using UI.Models.p31oper;
using UI.Models.Record;

namespace UI.Models.p56oper
{
    public class p56CloneBatchViewModel:BaseViewModel
    {
        public string pids { get; set; }
        public IEnumerable<BO.p56Task> lisSourceP56 { get; set; }
        public List<p56Clone> lisDest { get; set; }
        public BO.p41Project RecP41 { get; set; }
        public ProjectComboViewModel ProjectCombo { get; set; }
    }

    
}
