using Blade.MG.UI;
using Blade.MG.UI.Caching;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// Reproduces the reported "indeterminate ProgressBar doesn't move" bug. ProgressBar's sweep
    /// position is computed from DateTime.UtcNow every RenderControl call (Controls/ProgressBar.cs)
    /// - it never touches a Binding&lt;T&gt;, so nothing ever fires Changed for it, so it never
    /// bubbles a cache invalidation (see BubbleInvalidation). A cached ancestor Border (every
    /// Border is cached - see its constructor) only re-renders when its CacheStateHash changes or
    /// something bubbles an invalidation - neither ever happens here, so the sweep gets captured
    /// once and frozen in the cache texture forever.
    /// </summary>
    [TestClass]
    public class TestIndeterminateProgressBarInCachedBorder
    {
        [TestMethod]
        public void InsideCachedBorder_SweepPositionFreezesAfterFirstRender()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var border = new Border
            {
                Width = 200,
                Height = 20,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                Background = Color.White,
            };

            var progressBar = new ProgressBar
            {
                IsIndeterminate = true,
                Width = 180,
            };
            border.Content = progressBar;

            ui.AddChild(border);
            uiManager.AddUI(ui);
            uiManager.PerformLayout();

            using var spriteBatch = new SpriteBatch(FakeGame.Instance.GraphicsDevice);
            using var renderTarget = new RenderTarget2D(FakeGame.Instance.GraphicsDevice, 800, 600);
            var gameTime = new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16));

            uiManager.Draw(spriteBatch, gameTime, renderTarget);
            var cachedTexture = ((ICacheable)border).CachedTexture;
            Color[] firstFramePixels = new Color[cachedTexture.Width * cachedTexture.Height];
            cachedTexture.GetData(firstFramePixels);

            // Let enough wall-clock time pass for the sweep to have visibly moved (cycle is 1.5s).
            Thread.Sleep(400);
            uiManager.PerformLayout();
            uiManager.Draw(spriteBatch, gameTime, renderTarget);
            Color[] secondFramePixels = new Color[cachedTexture.Width * cachedTexture.Height];
            cachedTexture.GetData(secondFramePixels);

            CollectionAssert.AreEqual(firstFramePixels, secondFramePixels,
                "Test setup error (or bug already fixed) - expected the sweep to be frozen (identical pixels) across frames when hosted inside a cached Border.");
        }
    }
}
