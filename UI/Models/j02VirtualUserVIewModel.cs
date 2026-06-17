namespace UI.Models
{
    public class j02VirtualUserVIewModel:BaseViewModel
    {
        public int pid { get; set; }
        public int j02VirtualParentID { get; set; }
        public BO.j02User RecParent { get; set; }
        public BO.j02User Rec { get; set; }
    }
}
