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

            //try
            //{
            //    //context.Renderer.BeginBatch(transform: Transform.Combine(parentTransform, Transform));
            //    context.Renderer.BeginBatch(transform: parentTransform);

            //    if (BackgroundTexture != null)
            //    {
            //        // Draw a rectangle using the 1x1 texture we created
            //        context.Renderer.FillRect(finalRect, BackgroundTexture, Background.Value, layoutBounds); // TODO: Implement Scaling Options ? Uniform / UniformToFit / Repeat? etc.
            //    }
            //    else if (this.Background.Value != Color.Transparent)
            //    {
            //        //context.Renderer.FillRect(finalRect, Matrix.Identity, Color.LightCoral, layoutBounds);
            //        //context.Renderer.FillRect(finalRect, parentTransform, Background, layoutBounds);
            //        context.Renderer.FillRect(finalRect, Background.Value, layoutBounds);
            //    }
            //}
            //finally
            //{
            //    context.Renderer.EndBatch();
            //}

            // Render any child controls
            base.RenderControl(context, Rectangle.Intersect(layoutBounds, FinalRect), parentTransform);

        }
    }
}
