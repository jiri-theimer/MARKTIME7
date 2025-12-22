using UI.Models.Asi;

namespace UI.Code
{
    public static class basTree
    {
        public static List<TreeNode> BuildTree(List<TreeNode> flatList)
        {
            var lookup = flatList.ToDictionary(x => x.Id);
            List<TreeNode> roots = new List<TreeNode>();

            foreach (var node in flatList)
            {
                if (node.IdParent.HasValue && lookup.ContainsKey(node.IdParent.Value))
                {
                    lookup[node.IdParent.Value].Children.Add(node);
                }
                else
                {
                    roots.Add(node);
                }
            }

            return roots;
        }
    }
}
