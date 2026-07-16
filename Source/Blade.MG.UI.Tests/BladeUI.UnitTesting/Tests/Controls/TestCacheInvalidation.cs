using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// Reproduces the bug where a cached ancestor (e.g. a Border with EnableCaching) never
    /// notices a change to a nested child's Binding&lt;T&gt; property, because CacheStateHash
    /// only covers the cached control's own bindings. BubbleInvalidation (UIComponent.cs)
    /// fixes this by having every control's bindings bubble a cache-invalidate call up the
    /// Parent chain to every ICacheable ancestor.
    /// </summary>
    [TestClass]
    public class TestCacheInvalidation
    {
        [TestMethod]
        public void CachedBorder_ChildBackgroundChanges_CacheIsInvalidated()
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

            var border = new Border
            {
                Width = 120,
                Height = 60,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                CornerRadius = new CornerRadius(0f),
                BorderThickness = new Thickness(0),
                Background = Color.Transparent,
                EnableCaching = true,
            };
            host.AddChild(border);

            var child = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Background = Color.Blue,
            };
            border.Content = child;

            uiManager.AddUI(ui);
            uiManager.PerformLayout();
            uiManager.PerformLayout();

            var graphicsDevice = FakeGame.Instance.GraphicsDevice;
            using var renderTarget = new RenderTarget2D(graphicsDevice, 400, 300);
            using var spriteBatch = new SpriteBatch(graphicsDevice);

            Color SampleBorderCenter()
            {
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

            // First draw populates the Border's render-target cache with the child's Blue background.
            Color firstSample = SampleBorderCenter();
            Assert.AreEqual(Color.Blue, firstSample, $"Expected Blue on the first (cache-populating) draw, got {firstSample}.");

            // Mutate the CHILD's binding, not the Border's own - CacheStateHash never sees this.
            child.Background.Value = Color.Red;

            Color secondSample = SampleBorderCenter();
            Assert.AreEqual(Color.Red, secondSample, $"Expected Red after the child's Background changed - got {secondSample}, which means the cached Border texture went stale instead of being invalidated by the child's change.");
        }

        /// <summary>
        /// Same bug as above, but with a Label as the cached ancestor's child instead of a
        /// Panel. Label.Measure (Label.cs) never calls base.Measure, so the bindings-wiring
        /// hook originally lived only in Measure would silently never wire a Label's own
        /// bindings - Background/TextColor changes on a Label (e.g. ListViewItemTemplate's
        /// hover highlight) would never bubble to invalidate a cached ancestor. Fixed by also
        /// wiring bindings from the Parent setter (see UIComponent.cs), which every attached
        /// control goes through regardless of whether its Measure override calls base.
        /// </summary>
        [TestMethod]
        public void CachedBorder_ChildLabelBackgroundChanges_CacheIsInvalidated()
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

            var border = new Border
            {
                Width = 120,
                Height = 60,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                CornerRadius = new CornerRadius(0f),
                BorderThickness = new Thickness(0),
                Background = Color.Transparent,
                EnableCaching = true,
            };
            host.AddChild(border);

            var label = new Label
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Text = string.Empty,
                Background = Color.Blue,
            };
            border.Content = label;

            uiManager.AddUI(ui);
            uiManager.PerformLayout();
            uiManager.PerformLayout();

            var graphicsDevice = FakeGame.Instance.GraphicsDevice;
            using var renderTarget = new RenderTarget2D(graphicsDevice, 400, 300);
            using var spriteBatch = new SpriteBatch(graphicsDevice);

            Color SampleBorderCenter()
            {
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

            Color firstSample = SampleBorderCenter();
            Assert.AreEqual(Color.Blue, firstSample, $"Expected Blue on the first (cache-populating) draw, got {firstSample}.");

            label.Background.Value = Color.Red;

            Color secondSample = SampleBorderCenter();
            Assert.AreEqual(Color.Red, secondSample, $"Expected Red after the Label child's Background changed - got {secondSample}, which means the cached Border texture went stale because Label.Measure never wires bindings (it doesn't call base.Measure).");
        }
    }
}
