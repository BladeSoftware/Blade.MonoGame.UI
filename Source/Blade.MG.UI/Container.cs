﻿using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI
{
    public class Container : UIComponentDrawable
    {
        public List<UIComponent> Children { get; private set; } = new List<UIComponent>();

        public Container()
        {

        }

        public virtual void AddChild(UIComponent item, UIComponent parent = null, object dataContext = null)
        {
            item.DataContext = dataContext ?? DataContext ?? parent?.DataContext;

            item.Parent = parent ?? this;
            Children.Add(item);
        }

        public bool RemoveChild(UIComponent item)
        {
            return Children.Remove(item);
        }

        public int RemoveAllChildren()
        {
            return Children.RemoveAll(p => true);
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
                    child.Arrange(context, GetChildBoundingBox(context, child), FinalContentRect);
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
            //foreach (var child in CollectionsMarshal.AsSpan<UIComponent>((List<UIComponent>)Children))
            foreach (var child in Children)
            {
                child.RenderControl(context, Rectangle.Intersect(layoutBounds, FinalContentRect), Transform.Combine(parentTransform, child.Transform, child));
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
