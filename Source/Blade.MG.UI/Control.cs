using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI
{
    public class Control : UIComponentDrawable
    {

        private UIComponent content;
        public UIComponent Content
        {
            get => content;

            set
            {
                if (content != null && content != value)
                {
                    // Detach the outgoing child so it stops bubbling invalidation into a
                    // parent it's no longer part of (see BubbleInvalidation).
                    content.Parent = null;
                }

                content = value;
                if (content != null)
                {
                    content.Parent = this;
                }

                // Replacing Content changes this control's rendered output even if no
                // Binding<T>.Value changed (e.g. swapping in an entirely different child).
                BubbleInvalidation();
            }
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);

            if (Content != null && Visible.Value != Visibility.Collapsed)
            {
                //Content.Measure(context, ref availableSize, ref parentMinMax);
                MergeChildDesiredSize(context, ref availableSize, Content, ref parentMinMax);
            }

        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);

            if (Content != null && Visible.Value != Visibility.Collapsed)
            {
                //if (this is IItemTemplate && Parent != null)
                //{
                //    Content.Arrange(context, Parent.GetChildBoundingBox(context, Content), layoutBounds);
                //}
                //else
                //{
                Content.Arrange(context, GetChildBoundingBox(context, Content), layoutBounds);
                //}
            }
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (string.Equals(Name, "BTN-TEST1"))
            {
            }

            if (string.Equals(Name, "Root", StringComparison.InvariantCultureIgnoreCase))
            {
            }

            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            base.RenderControl(context, layoutBounds, parentTransform);

            // Render Content control
            if (Content != null)
            {
                RenderChildOrFromCache(Content, context, Rectangle.Intersect(layoutBounds, FinalContentRect), Transform.Combine(parentTransform, Content.Transform, Content));
            }
        }

    }
}
