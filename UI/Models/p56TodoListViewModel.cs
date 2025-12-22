

namespace UI.Models
{
    public class p56TodoListViewModel:BaseViewModel
    {
        public string RecordEntity { get; set; }
        public int RecordPid { get; set; }

        public List<p56Repeater> lisP56 { get; set; }
        public IEnumerable<BO.p57TaskType> lisP57 { get; set; }
        public IEnumerable<BO.x67EntityRole> lisX67 { get; set; }
        public int SelectedP57ID { get; set; }
        public int SelectedX67ID { get; set; }
        public ProjectComboViewModel ProjectCombo { get; set; }

        public string p55Name { get; set; }

        public int SelectedP55ID { get; set; }
        public string SelectedP55Name { get; set; }

        public string TodoListTemplate { get; set; }

        
    }



    public class p56Repeater : BO.p56Task
    {
        public string Assign_j02IDs { get; set; }
        public string Assign_Persons { get; set; }
        public string Assign_j11IDs { get; set; }
        public string Assign_j11Names { get; set; }
       
        public bool IsTempDeleted { get; set; }
        public string TempGuid { get; set; }
        public string CssTempDisplay
        {
            get
            {
                if (this.IsTempDeleted)
                {
                    return "display:none;";
                }
                else
                {
                    return "display:table-row;";
                }
            }
        }
    }
}
