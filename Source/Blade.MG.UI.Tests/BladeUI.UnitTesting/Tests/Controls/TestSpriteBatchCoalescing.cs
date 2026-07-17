using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Renderer;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// UIRenderer.BeginBatch used to default to SpriteSortMode.Immediate, meaning every single
    /// SpriteBatch.Draw call inside one Begin/End pair became its own separate GPU draw call.
    /// Border's stencil-based rounded-corner mask (DrawSmoothCornerMask) draws one 1x1 sprite
    /// per masked pixel in each corner - for a Border with a real corner radius, that's dozens
    /// to low hundreds of individual GPU draw calls for what's visually a single rounded
    /// rectangle. Switched the default to SpriteSortMode.Deferred, which preserves the exact
    /// same submission order (so no visual difference - confirmed by the rest of this test
    /// suite's pixel-sampling tests still passing) but lets SpriteBatch coalesce consecutive
    /// same-texture draws into far fewer actual GPU draw calls. This measures that directly via
    /// GraphicsDevice.Metrics.DrawCount rather than just trusting the code change.
    /// </summary>
    [TestClass]
    public class TestSpriteBatchCoalescing
    {
        private static int RenderRoundedBorderAndCountDrawCalls()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var host = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Background = Color.Lime,
            };
            ui.AddChild(host);

            // A generous corner radius on a reasonably large Border maximizes the number of
            // individual per-pixel stencil-mask draws (DrawSmoothCornerMask in Border.cs) that
            // Deferred mode has the opportunity to coalesce.
            var border = new Border
            {
                Width = 200,
                Height = 150,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                CornerRadius = new CornerRadius(30f),
                BorderThickness = new Thickness(3),
                BorderColor = Color.Black,
                Background = Color.Magenta,
                Elevation = 4,
            };
            host.AddChild(border);

            uiManager.AddUI(ui);
            uiManager.PerformLayout();
            uiManager.PerformLayout();

            var graphicsDevice = FakeGame.Instance.GraphicsDevice;
            using var renderTarget = new RenderTarget2D(graphicsDevice, 400, 300);
            using var spriteBatch = new SpriteBatch(graphicsDevice);

            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Black);

            long drawCountBefore = graphicsDevice.Metrics.DrawCount;

            uiManager.Draw(spriteBatch, new GameTime(), renderTarget);

            long drawCountAfter = graphicsDevice.Metrics.DrawCount;

            graphicsDevice.SetRenderTarget(null);

            return (int)(drawCountAfter - drawCountBefore);
        }

        [TestMethod]
        public void RoundedBorderWithElevationAndBorderStroke_RendersWithFarFewerDrawCallsThanImmediateMode()
        {
            int deferredDrawCalls = RenderRoundedBorderAndCountDrawCalls();

            Assert.IsTrue(deferredDrawCalls > 0, "Test setup error - expected at least some draw calls to have happened.");

            // A 30px-radius corner mask alone would be on the order of 100+ individual 1x1
            // draws in Immediate mode (roughly radius^2 - pi*radius^2/4 masked pixels per
            // corner, times 4 corners), on top of the shadow/background/border-stroke draws.
            // Deferred mode should coalesce nearly all of the same-texture ones together.
            Assert.IsTrue(deferredDrawCalls < 30, $"Expected Deferred mode to coalesce the corner-mask's many per-pixel draws into a small number of actual GPU draw calls, got {deferredDrawCalls}.");
        }
    }
}
