using Blade.MG.UI;
using Blade.MG.UI.Caching;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// Reproduces the reported "RadioButton label invisible until hover" bug directly against
    /// the real cached-render pipeline (a Border with EnableCaching, matching the Examples
    /// project's Section component) rather than just checking layout/binding state, since the
    /// symptom is specifically about what gets drawn into the cache texture.
    /// </summary>
    [TestClass]
    public class TestRadioButtonFirstRenderPixels
    {
        [TestMethod]
        public void FirstCacheRender_ActuallyDrawsTheLabelText()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var border = new Border
            {
                Width = 200,
                Height = 60,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                Background = Color.White,
            };

            var radioButton = new RadioButton
            {
                Text = "Small",
                Width = 180,
                Height = 40,
                HorizontalAlignment = HorizontalAlignmentType.Left,
            };
            border.Content = radioButton;

            ui.AddChild(border);
            uiManager.AddUI(ui);
            uiManager.PerformLayout();

            Console.WriteLine($"border.FinalRect={border.GetFinalRect()}");
            Console.WriteLine($"radioButton.FinalRect={radioButton.GetFinalRect()}");
            Console.WriteLine($"radioButton.DesiredSize={radioButton.DesiredSize}");

            using var spriteBatch = new SpriteBatch(FakeGame.Instance.GraphicsDevice);
            using var renderTarget = new RenderTarget2D(FakeGame.Instance.GraphicsDevice, 800, 600);
            var gameTime = new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16));

            uiManager.Draw(spriteBatch, gameTime, renderTarget);

            var cachedTexture = ((ICacheable)border).CachedTexture;
            Assert.IsNotNull(cachedTexture, "Test setup error - expected the Border to have populated its cache texture on the first Draw.");

            Color[] pixels = new Color[cachedTexture.Width * cachedTexture.Height];
            cachedTexture.GetData(pixels);

            int nonWhiteCount = pixels.Count(p => p != Color.White && p.A > 0);
            Console.WriteLine($"nonWhiteCount={nonWhiteCount} out of {pixels.Length}");

            var distinctColors = pixels.Where(p => p.A > 0).GroupBy(p => p).OrderByDescending(g => g.Count()).Take(10);
            foreach (var g in distinctColors)
            {
                Console.WriteLine($"  color={g.Key} count={g.Count()}");
            }

            Color textColor = UIManager.DefaultTheme.OnSurface;
            bool anyTextPixelDrawn = pixels.Any(p => p.A > 200 && Math.Abs(p.R - textColor.R) < 10 && Math.Abs(p.G - textColor.G) < 10 && Math.Abs(p.B - textColor.B) < 10);

            Assert.IsTrue(anyTextPixelDrawn, "Expected the label's text to actually be drawn (in the theme's OnSurface color) into the Border's cache texture on the very first render, before any hover/focus event.");
        }

        [TestMethod]
        public void FirstCacheRender_WithPlainLabel_ActuallyDrawsText()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var border = new Border
            {
                Width = 200,
                Height = 60,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                Background = Color.White,
            };

            var label = new Label
            {
                Text = "Small",
                TextColor = UIManager.DefaultTheme.OnSurface,
                Width = 180,
                Height = 40,
                HorizontalAlignment = HorizontalAlignmentType.Left,
            };
            border.Content = label;

            ui.AddChild(border);
            uiManager.AddUI(ui);
            uiManager.PerformLayout();

            using var spriteBatch = new SpriteBatch(FakeGame.Instance.GraphicsDevice);
            using var renderTarget = new RenderTarget2D(FakeGame.Instance.GraphicsDevice, 800, 600);
            var gameTime = new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16));

            uiManager.Draw(spriteBatch, gameTime, renderTarget);

            var cachedTexture = ((ICacheable)border).CachedTexture;
            Color[] pixels = new Color[cachedTexture.Width * cachedTexture.Height];
            cachedTexture.GetData(pixels);

            Color textColor = UIManager.DefaultTheme.OnSurface;
            bool anyTextPixelDrawn = pixels.Any(p => p.A > 200 && Math.Abs(p.R - textColor.R) < 10 && Math.Abs(p.G - textColor.G) < 10 && Math.Abs(p.B - textColor.B) < 10);

            Assert.IsTrue(anyTextPixelDrawn, "Expected a PLAIN Label's text (not inside a RadioButton) to render on the very first cache pass.");
        }

        [TestMethod]
        public void FirstCacheRender_WithLabelNestedInsidePlainControl_ActuallyDrawsText()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var border = new Border
            {
                Width = 200,
                Height = 60,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                Background = Color.White,
            };

            var wrapperControl = new Control
            {
                Width = 180,
                Height = 40,
                HorizontalAlignment = HorizontalAlignmentType.Left,
            };

            var label = new Label
            {
                Text = "Small",
                TextColor = UIManager.DefaultTheme.OnSurface,
                HorizontalAlignment = HorizontalAlignmentType.Left,
            };
            wrapperControl.Content = label;
            border.Content = wrapperControl;

            ui.AddChild(border);
            uiManager.AddUI(ui);
            uiManager.PerformLayout();

            using var spriteBatch = new SpriteBatch(FakeGame.Instance.GraphicsDevice);
            using var renderTarget = new RenderTarget2D(FakeGame.Instance.GraphicsDevice, 800, 600);
            var gameTime = new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16));

            uiManager.Draw(spriteBatch, gameTime, renderTarget);

            var cachedTexture = ((ICacheable)border).CachedTexture;
            Color[] pixels = new Color[cachedTexture.Width * cachedTexture.Height];
            cachedTexture.GetData(pixels);

            Color textColor = UIManager.DefaultTheme.OnSurface;
            bool anyTextPixelDrawn = pixels.Any(p => p.A > 200 && Math.Abs(p.R - textColor.R) < 10 && Math.Abs(p.G - textColor.G) < 10 && Math.Abs(p.B - textColor.B) < 10);

            Assert.IsTrue(anyTextPixelDrawn, "Expected a Label's text nested one extra level deep (Border -> plain Control -> Label) to still render on the very first cache pass.");
        }
    }
}
