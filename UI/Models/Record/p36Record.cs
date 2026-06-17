
namespace UI.Models.Record
{
    public class p36Record : BaseRecordViewModel
    {
        public BO.p36LockPeriod Rec { get; set; }

        public string ScopePrefix { get; set; }

        public string ComboPerson { get; set; }

        public string ComboTeam { get; set; }

        public List<int> SelectedP34IDs { get; set; }
        public IEnumerable<BO.p34ActivityGroup> lisAllP34 { get; set; }
    }
}