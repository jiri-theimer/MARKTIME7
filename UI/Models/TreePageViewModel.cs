namespace UI.Models
{
    public class TreePageViewModel: BaseViewModel
    {
        private string _entity { get; set; }
        public string prefix { get; set; }
        public int pid { get; set; }
        public string TabName { get; set; }
        public string rez { get; set; }

        public int pid_loaded { get; set; }     //pid načtený z user parametrů
        public List<NavTab> NavTabs;

        public string DefaultNavTabUrl { get; set; }
        public string DefaultNavName { get; set; }
        public string DefTab { get; set; }  //výchozí záložka


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
