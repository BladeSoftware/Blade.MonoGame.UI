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
    public class TestBorderThinRect
    {
        private static Color RenderAndSampleCenter(int width, int height)
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

            // Mirrors ButtonTemplate's border1 exactly: CornerRadius 12, BorderThickness 2 (via
            // HandleStateChange), a themed Background.
            var border = new Border
            {
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                CornerRadius = new CornerRadius(12f),
                BorderThickness = new Thickness(2),
                BorderColor = Color.Black,
                Background = Color.Magenta,
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

            uiManager.Draw(spriteBatch, new GameTime(), renderTarget);

            graphicsDevice.SetRenderTarget(null);

            Color[] pixels = new Color[400 * 300];
            renderTarget.GetData(pixels);

            Rectangle rect = border.GetFinalRect();
            Point center = new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
            return pixels[center.Y * 400 + center.X];
        }

        [TestMethod]
        public void Border_NormalHeight_ShowsMagentaFill()
        {
            Color pixel = RenderAndSampleCenter(120, 60);
            Assert.AreEqual(Color.Magenta, pixel, $"Expected Magenta fill at a normal-height Border's center, got {pixel}.");
        }

        [TestMethod]
        public void Border_ThinHeight_MatchingButtonInStackPanel_ShowsMagentaFill()
        {
            // Matches the real-world repro: a Button squished to ~22px tall by a vertical
            // StackPanel, with ButtonTemplate's own CornerRadius=12/BorderThickness=2 border.
            Color pixel = RenderAndSampleCenter(1098, 22);
            Assert.AreEqual(Color.Magenta, pixel, $"Expected Magenta fill at a thin (22px) Border's center, got {pixel} - if this is Lime (host background) or Black, the corner-radius/stencil math is breaking down for a rect this thin.");
        }
    }
}
