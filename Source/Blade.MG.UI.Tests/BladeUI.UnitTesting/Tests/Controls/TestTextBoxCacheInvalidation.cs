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
    /// Reproduces the bug where a TextBox's rendered glyphs never updated as the user typed.
    /// TextBoxTemplate's border1 (Border enables render caching by default - see Border's own
    /// constructor) wraps label1, whose Text is a getter/setter relay onto TextBox.Text (see
    /// TextBoxTemplate's own comments) rather than the same Binding&lt;string&gt; instance -
    /// mutating TextBox.Text.Value directly (exactly what every keystroke does, via
    /// TextEntryControl.AddChar/HandleBackspace/etc.) never fired label1.Text's own Changed
    /// event, so BubbleInvalidation never invalidated border1's cache and the cached bitmap -
    /// including the old glyphs - kept getting reused until some unrelated change (e.g. a
    /// focus/hover-driven BorderColor change) invalidated it instead.
    /// </summary>
    [TestClass]
    public class TestTextBoxCacheInvalidation
    {
        [TestMethod]
        public void TextBox_TextValueChanges_RenderedGlyphsUpdate()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var host = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };
            ui.AddChild(host);

            var textBox = new TextBox
            {
                Width = 300,
                Height = 40,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                Text = "A",
                Label = null,
                HelperText = null,
            };
            host.AddChild(textBox);

            uiManager.AddUI(ui);
            uiManager.PerformLayout();
            uiManager.PerformLayout();

            var graphicsDevice = FakeGame.Instance.GraphicsDevice;
            using var renderTarget = new RenderTarget2D(graphicsDevice, 400, 300);
            using var spriteBatch = new SpriteBatch(graphicsDevice);

            long SampleTextBoxChecksum()
            {
                graphicsDevice.SetRenderTarget(renderTarget);
                graphicsDevice.Clear(Color.Black);

                uiManager.Draw(spriteBatch, new GameTime(), renderTarget);

                graphicsDevice.SetRenderTarget(null);

                Color[] pixels = new Color[400 * 300];
                renderTarget.GetData(pixels);

                Rectangle rect = textBox.GetFinalRect();
                long checksum = 0;

                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    for (int x = rect.Left; x < rect.Right; x++)
                    {
                        Color c = pixels[y * 400 + x];
                        checksum += c.R + (c.G * 31) + (c.B * 97);
                    }
                }

                return checksum;
            }

            // TextBoxTemplate.InitTemplate's border1 starts with BorderThickness = 1, then its
            // own first RenderControl pass settles it to 0 (Standard/Filled variants never show
            // a border stroke) - a genuine one-time state change that invalidates border1's
            // cache independently of anything to do with Text. Draw a few times up front so that
            // transient settles and the rendered pixels stop changing on their own, leaving
            // Text.Value as the only variable that changes from here on.
            long stableChecksum = 0;
            for (int i = 0; i < 4; i++)
            {
                stableChecksum = SampleTextBoxChecksum();
            }

            long repeatChecksum = SampleTextBoxChecksum();
            Assert.AreEqual(stableChecksum, repeatChecksum, "Expected the rendered TextBox pixels to be stable frame-over-frame once warmed up with no changes in between - if they still differ, something unrelated to Text is causing per-frame churn that this test can't distinguish from the bug it's meant to catch.");

            // Mutate Text.Value directly - exactly what TextEntryControl.AddChar does for every
            // keystroke - rather than reassigning the whole Text binding.
            textBox.Text.Value = "WWWWWWWWWW";

            long afterTextChangeChecksum = SampleTextBoxChecksum();

            Assert.AreNotEqual(stableChecksum, afterTextChangeChecksum, "Expected the rendered TextBox pixels to change after Text.Value changed - equal checksums mean border1's cached texture went stale instead of being invalidated by the typed change.");
        }
    }
}
