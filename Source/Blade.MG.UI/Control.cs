using Blade.UI.Components;
using Microsoft.Xna.Framework;

namespace Blade.UI
{
    public class Control : UIComponentDrawable
    {

        private UIComponent content;
        public UIComponent Content
        {
            get
            {
                return content;
            }

            set
            {
                content = value;
                if (content != null)
                {
                    content.Parent = this;
                }

            }
        }


        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);

            if (Content != null && Visible.Value != Visibility.Collapsed)
            {
                Content.Measure(context, ref availableSize, ref parentMinMax);
            }

            MergeChildDesiredSize(context, ref availableSize, Content, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);

            if (Content != null && Visible.Value != Visibility.Collapsed)
            {
                Content.Arrange(context, GetChildBoundingBox(context, Content), layoutBounds);
            }
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            base.RenderControl(context, layoutBounds, parentTransform);

            // Render Content control
            if (Content != null)
            {
                Content.RenderControl(context, Rectangle.Intersect(layoutBounds, FinalContentRect), parentTransform);
            }
        }

    }
}
