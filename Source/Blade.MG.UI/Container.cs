using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI
{
    public class Container : UIComponentDrawable
    {
        private List<UIComponent> children = new List<UIComponent>();

        // List<T>.AsReadOnly() allocates a brand-new ReadOnlyCollection<T> wrapper every call -
        // Children used to call it on every single access, including from Measure/Arrange/
        // RenderControl (every frame) and often multiple times per call (e.g. Count then an
        // indexer read in the same loop). ReadOnlyCollection<T> just wraps the underlying list
        // by reference (a live view, not a snapshot), so it's safe to allocate this wrapper
        // exactly once and reuse it forever - Add/Remove/Clear on `children` below are all
        // immediately visible through it with no invalidation needed.
        private readonly IReadOnlyList<UIComponent> childrenReadOnly;
        public IReadOnlyList<UIComponent> Children { get => childrenReadOnly; }

        public Container()
        {
            childrenReadOnly = children.AsReadOnly();
        }

        public virtual void AddChild(UIComponent item, UIComponent parent = null, object dataContext = null)
        {
            // Prefer, in order: an explicit dataContext argument (the caller is deliberately
            // binding this child to a specific data item, e.g. a list/tree/tab item template),
            // then the item's own already-set DataContext (some controls - TabPanel, ListView -
            // store real state there and must not have it silently wiped just by being attached
            // to a parent), then fall back to inheriting from this container/its own parent.
            item.DataContext = dataContext ?? item.DataContext ?? DataContext ?? parent?.DataContext;

            item.Parent = parent ?? this;
            children.Add(item);
            BubbleInvalidation();
        }

        public bool RemoveChild(UIComponent item)
        {
            if (!children.Remove(item))
            {
                return false;
            }

            item.Parent = null;
            BubbleInvalidation();
            return true;
        }

        public void RemoveAllChildren()
        {
            foreach (var child in children)
            {
                child.Parent = null;
            }

            children.Clear();
            BubbleInvalidation();
        }

        public int IndexOfChild(UIComponent item)
        {
            return children.IndexOf(item);
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);

            MergeChildDesiredSize(context, ref availableSize, Children, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);
            

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    //if (this is IItemTemplate && Parent != null)
                    //{
                    //    child.Arrange(context, Parent.GetChildBoundingBox(context, child), FinalContentRect);
                    //}
                    //else
                    //{
                    child.Arrange(context, GetChildBoundingBox(context, child), FinalContentRect);
                    //}
                }
            }
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            base.RenderControl(context, layoutBounds, parentTransform);

            // Render Child controls
            foreach (var child in Children)
            {
                RenderChildOrFromCache(child, context, Rectangle.Intersect(layoutBounds, FinalContentRect), Transform.Combine(parentTransform, child.Transform, child));
            }
        }

        public override void Dispose()
        {
            foreach (var child in Children)
            {
                child.Dispose();
            }

            base.Dispose();
        }

    }
}
