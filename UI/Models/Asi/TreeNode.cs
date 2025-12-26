namespace UI.Models.Asi
{
    public class TreeNode
    {
        public int Id { get; set; }
        public int? IdParent { get; set; }
        public string Name { get; set; }
        public List<TreeNode> Children { get; set; } = new List<TreeNode>();
        public string Prefix { get; set; }
        public string Url { get; set; }
    }
}
