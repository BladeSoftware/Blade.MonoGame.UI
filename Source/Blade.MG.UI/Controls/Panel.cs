using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    public class Panel : Container
    {
        public Panel()
        {
            Background = Color.Transparent;
            IsHitTestVisible = true;
            CanHover = true;
            CanFocus = false;
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            // Track this panel's own content bounds as a genuine viewport-clip ancestor (see
            // UIContext.AncestorClipBounds) so a descendant's drop shadow can be clipped to
            // the panel's true edge, not just wherever ordinary content narrowing lands. Uses
            // FinalContentRect (not FinalRect) because children are positioned within the
            // Padding-adjusted content area - clipping to the unpadded outer FinalRect would
            // let a shadow bleed into this panel's own Padding gutter, which reads as escaping
            // the panel entirely when that gutter sits flush against a same-colored sibling.
            var previousAncestorClip = context.AncestorClipBounds;
            context.AncestorClipBounds = Rectangle.Intersect(previousAncestorClip, FinalContentRect);
            
            // Render any child controls
            base.RenderControl(context, Rectangle.Intersect(layoutBounds, FinalRect), parentTransform);

            context.AncestorClipBounds = previousAncestorClip;
        }
    }
}
