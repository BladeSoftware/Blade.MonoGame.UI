using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeUI.UnitTesting.Tests.Controls
{
    [TestClass]
    public class TestProgressBar
    {
        private static Color SamplePixel(FakeUIManager uiManager, RenderTarget2D renderTarget, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Point point)
        {
            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Black);

            uiManager.Draw(spriteBatch, new GameTime(), renderTarget);

            graphicsDevice.SetRenderTarget(null);

            Color[] pixels = new Color[renderTarget.Width * renderTarget.Height];
            renderTarget.GetData(pixels);

            return pixels[point.Y * renderTarget.Width + point.X];
        }

        [TestMethod]
        public void DeterminateMode_HalfValue_FillsRoughlyHalfTheWidth()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var progressBar = new ProgressBar
            {
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                Width = 200,
                Minimum = 0f,
                Maximum = 100f,
                Value = 50f,
            };
            ui.AddChild(progressBar);

            uiManager.AddUI(ui);
            uiManager.PerformLayout();
            uiManager.PerformLayout();

            var graphicsDevice = FakeGame.Instance.GraphicsDevice;
            using var renderTarget = new RenderTarget2D(graphicsDevice, 400, 300);
            using var spriteBatch = new SpriteBatch(graphicsDevice);

            Rectangle rect = progressBar.GetFinalRect();

            // Well inside the filled (left) half - should be the fill (Primary) color.
            Color filledSample = SamplePixel(uiManager, renderTarget, spriteBatch, graphicsDevice, new Point(rect.Left + 20, rect.Center.Y));

            // Well inside the unfilled (right) half - should be the track (Background) color,
            // not the fill color.
            Color unfilledSample = SamplePixel(uiManager, renderTarget, spriteBatch, graphicsDevice, new Point(rect.Right - 20, rect.Center.Y));

            Assert.AreNotEqual(filledSample, unfilledSample, "Expected the filled and unfilled halves of a 50% ProgressBar to be visibly different colors.");
        }

        [TestMethod]
        public void IndeterminateMode_DoesNotThrow_AndRendersSomething()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var progressBar = new ProgressBar
            {
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                Width = 200,
                IsIndeterminate = true,
            };
            ui.AddChild(progressBar);

            uiManager.AddUI(ui);
            uiManager.PerformLayout();
            uiManager.PerformLayout();

            var graphicsDevice = FakeGame.Instance.GraphicsDevice;
            using var renderTarget = new RenderTarget2D(graphicsDevice, 400, 300);
            using var spriteBatch = new SpriteBatch(graphicsDevice);

            // Just confirming this renders without throwing across a couple of frames -
            // indeterminate mode is time-based and inherently non-deterministic to pixel-sample.
            SamplePixel(uiManager, renderTarget, spriteBatch, graphicsDevice, new Point(1, 1));
            SamplePixel(uiManager, renderTarget, spriteBatch, graphicsDevice, new Point(1, 1));
        }
    }
}
