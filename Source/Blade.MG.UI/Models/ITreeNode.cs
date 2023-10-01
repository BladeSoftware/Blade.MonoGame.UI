namespace Blade.MG.UI.Models
{
    public interface ITreeNode
    {
        public bool IsExpanded { get; set; }
        public bool IsSelected { get; set; }

        public string Text { get; set; }

        public List<ITreeNode> Children { get; set; }

        public IEnumerable<T> TypedNodes<T>() where T : class, ITreeNode
        {
            foreach (var node in Children)
            {
                yield return node as T;
            }
        }

        public T As<T>() where T : class, ITreeNode
        {
            return this as T;
        }

    }
}
