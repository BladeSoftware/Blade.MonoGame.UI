namespace Blade.UI.Models
{
    /// <summary>
    /// Default implementation of ITreeNode
    /// </summary>
    public class TreeNode : ITreeNode
    {
        public bool IsExpanded { get; set; } = false;
        public bool IsSelected { get; set; } = false;

        public string Text { get; set; }

        public object Tag { get; set; }

        public List<ITreeNode> Children { get; set; }

        public T AddChild<T>(T node) where T : TreeNode
        {
            if (Children == null)
            {
                Children = new List<ITreeNode>();
            }

            Children.Add(node);

            return node;
        }

    }
}
