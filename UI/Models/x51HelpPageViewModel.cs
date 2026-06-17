namespace UI.Models
{
    public class x51HelpPageViewModel:BaseViewModel
    {
        public BO.x51HelpCore Rec { get; set; }
        public string viewurl { get; set; }
        public string fullurl { get; set; }
        public string pagetitle { get; set; }

        public IEnumerable<BO.x51HelpCore> lisX51 { get; set; }
        public List<myTreeNode> treeNodes { get; set; }

        

    }
}
