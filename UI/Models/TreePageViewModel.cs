using UI.Views.Shared.Components.myPeriod;

namespace UI.Models
{
    public class TreePageViewModel: BaseViewModel
    {
        private string _entity { get; set; }
        public string prefix { get; set; }
        public int pid { get; set; }
        public string TabName { get; set; }
        public string rez { get; set; }

        public List<UI.Models.Asi.TreeNode> lisTreeNodes { get; set; }

        public int pid_loaded { get; set; }     //pid načtený z user parametrů
        public List<NavTab> NavTabs;

        public string DefaultNavTabUrl { get; set; }
        public string DefaultNavName { get; set; }
        public string DefTab { get; set; }  //výchozí záložka


        public myPeriodViewModel periodinput { get; set; } //fixní filtr v horním pruhu
        public RecordBinQueryViewModel recordbinquery { get; set; } //filtrování podle archivu
        public p31StateQueryViewModel p31statequery { get; set; }   //filtrování podle stavu aktivit v horním pruhu
        public p31TabQueryViewModel p31tabquery { get; set; }   //filtrování podle formátu aktivit v horním pruhu
        public TheGridQueryViewModel TheGridQueryButton { get; set; }   //pojmenovaný filtr

        public string entity
        {
            get
            {
                if (_entity == null)
                {
                    _entity = BO.Code.Entity.GetEntity(this.prefix);
                }
                return _entity;
            }
        }
    }
}
