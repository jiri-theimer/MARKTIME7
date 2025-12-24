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

            foreach(var c in roots)
            {
                if (c.Children.Count() > 0)
                {
                    //c.Name = $"{c.Name} ({c.Children.Count()})";
                    c.Name = $"{c.Name} ({CountDescendants(c)})";
                    foreach (var d in c.Children)
                    {
                        if (d.Children.Count() > 0)
                        {
                            d.Name = $"{d.Name} ({CountDescendants(d)})";
                        }
                    }
                }
                
            }

            return roots;
        }

        public static int CountDescendants(TreeNode node)
        {
            return node.Children.Sum(c => 1 + CountDescendants(c));
        }
    }
}
