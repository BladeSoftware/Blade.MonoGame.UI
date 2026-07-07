using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blade.MG.UI.Controls
{
    public class Button : TemplatedControl
    {
        public Binding<string> Text;

        public Binding<HorizontalAlignmentType> HorizontalTextAlignment;
        public Binding<VerticalAlignmentType> VerticalTextAlignment;

        public Binding<string> FontName { get; set; }
        public Binding<float> FontSize { get; set; }
        public Binding<Color> TextColor { get; set; }


        public Button()
        {
            TemplateType = typeof(ButtonTemplate);

            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;

            HorizontalTextAlignment = HorizontalAlignmentType.Center;
            VerticalTextAlignment = VerticalAlignmentType.Center;

            Text = null;
            TextColor = UIManager.DefaultTheme.OnSurface;

            IsHitTestVisible = true;
            IsTabStop = true;
        }


        //public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        //{
        //    base.Measure(context, ref availableSize, ref parentMinMax);
        //}

        //public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        //{
        //    base.Arrange(context, layoutBounds, parentLayoutBounds);
        //}


        //// --- Back buffer fields ---
        //private RenderTarget2D backBuffer;
        //private bool backBufferDirty = true;
        //private Rectangle lastRenderRect;
        //private Transform lastRenderTransform;

        //public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        //{
        //    //    base.RenderControl(context, layoutBounds, parentTransform);


        //    if (Visible.Value != Visibility.Visible)
        //        return;

        //    var graphicsDevice = context.Game.GraphicsDevice;
        //    var renderer = context.Renderer;

        //    backBufferDirty = true; // For testing, always mark as dirty

        //    // If back buffer is dirty or size/layout changed, redraw
        //    if (backBufferDirty || backBuffer == null ||
        //        backBuffer.Width != layoutBounds.Width || backBuffer.Height != layoutBounds.Height)
        //    {
        //        // Dispose old buffer if needed
        //        backBuffer?.Dispose();

        //        // Create new RenderTarget2D
        //        backBuffer = new RenderTarget2D(
        //            graphicsDevice,
        //            Math.Max(1, layoutBounds.Width),
        //            Math.Max(1, layoutBounds.Height),
        //            false,
        //            graphicsDevice.PresentationParameters.BackBufferFormat,
        //            DepthFormat.None,
        //            0,
        //            RenderTargetUsage.PreserveContents);


        //        // Save the current render target
        //        var previousRenderTargets = graphicsDevice.GetRenderTargets();

        //        // Set render target
        //        graphicsDevice.SetRenderTarget(backBuffer);
        //        //graphicsDevice.Clear(Color.Transparent);

        //        // Render children to back buffer
        //        base.RenderControl(context, new Rectangle(0, 0, layoutBounds.Width, layoutBounds.Height), parentTransform);

        //        // Reset render target
        //        //graphicsDevice.SetRenderTarget(null);
        //        graphicsDevice.SetRenderTargets(previousRenderTargets);

        //        // Save last render info
        //        lastRenderRect = layoutBounds;
        //        lastRenderTransform = parentTransform;
        //        backBufferDirty = false;
        //    }

        //    // Draw the back buffer to the screen
        //    using (var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform))
        //    {
        //        renderer.PushState();
        //        renderer.ClipToRect(layoutBounds);
        //        spriteBatch.Draw(backBuffer, layoutBounds, Color.White);
        //        renderer.PopState();
        //        context.Renderer.EndBatch();
        //    }

        //}

        // ---=== UI Events ===---
        // ...

    }
}
