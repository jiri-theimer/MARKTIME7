

namespace UI.Models
{
    public class HelpViewModel:BaseViewModel
    {
        public string Header { get; set; }
        public string BodyHtml{ get; set; }


        public List<string> lisFiles { get; set; }
        public string SelectedFile { get; set; }
        public string SelectedHtml { get; set; }
        public int SelectedId { get; set; }
        public List<UI.Models.Asi.TreeNode> treeNodes { get; set; }

    }
}
