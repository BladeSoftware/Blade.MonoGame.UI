using System.Reflection;
using Blade.MG.UI;
using Blade.MG.UI.Caching;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Theming;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// Mirrors HelpPage_DynamicColor.cs's seedTextBox: no Label set, Standard Variant (so
    /// border1's own BorderColor/BorderThickness/Background never change value regardless of
    /// theme/focus - they're constant per-Variant), sitting directly in a plain StackPanel (no
    /// wrapping Border). TextEntryControl.InitTemplate deliberately assigns the template via
    /// Content (not AddInternalChild - see its own comment), so border1 is reached via
    /// ((Control)textBox).Content, not textBox.PrivateControls.
    ///
    /// Covers a race in TextBoxTemplate: border1 (a Border, always cache-enabled) gets its
    /// render cache invalidated correctly by UIWindow.RefreshThemeRecursive's theme-refresh
    /// sweep (walking into Content) - but border1's rebuild is driven by
    /// UIWindow.PreRenderLayout/PreRenderContext.ProcessPendingUpdates, a separate, EARLIER
    /// phase than RenderLayout (where TextBoxTemplate.RenderControl's ApplyThemedValue calls
    /// actually update textBox.TextColor/UnderlineColor from the new theme). Relying on
    /// RenderControl alone to update those values risks a rebuild-before-update ordering bug.
    /// The fix recomputes them in HandleStateChange too, so they're already correct by the time
    /// ANY cache rebuild might consume them, regardless of which phase triggers it.
    /// </summary>
    [TestClass]
    public class TestTextBoxThemeRefreshUncached
    {
        private static UITheme CloneWithDifferentOnSurface(UITheme source, Color newOnSurface)
        {
            var clone = new UITheme();
            foreach (var field in typeof(UITheme).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                field.SetValue(clone, field.GetValue(source));
            }
            clone.OnSurface = newOnSurface;
            return clone;
        }

        [TestMethod]
        public void SetTheme_UpdatesTextColorSynchronously_BeforeAnyDrawCallCanRebuildBorder1Cache()
        {
            var originalTheme = UIManager.DefaultTheme;
            try
            {
                var baseTheme = DefaultThemes.LightTheme();
                var tintedTheme = CloneWithDifferentOnSurface(baseTheme, Color.Lime);

                UIManager.DefaultTheme = baseTheme;

                var uiManager = new FakeUIManager();
                var ui = new EmptyUI();
                uiManager.AddUI(ui);

                var page = new Panel { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
                ui.AddChild(page);

                var layoutPanel = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
                page.AddChild(layoutPanel);

                var controlsRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignmentType.Left, VerticalAlignment = VerticalAlignmentType.Center };
                layoutPanel.AddChild(controlsRow);

                var seedTextBox = new TextBox
                {
                    Text = "#6750A4FF",
                    Width = 140,
                    VerticalAlignment = VerticalAlignmentType.Center,
                };
                controlsRow.AddChild(seedTextBox);

                uiManager.PerformLayout();
                uiManager.PerformLayout();

                var graphicsDevice = FakeGame.Instance.GraphicsDevice;
                using var renderTarget = new RenderTarget2D(graphicsDevice, 400, 300);
                using var spriteBatch = new SpriteBatch(graphicsDevice);

                void Draw()
                {
                    graphicsDevice.SetRenderTarget(renderTarget);
                    graphicsDevice.Clear(Color.Black);
                    uiManager.Draw(spriteBatch, new GameTime(), renderTarget);
                    graphicsDevice.SetRenderTarget(null);
                }

                for (int i = 0; i < 4; i++)
                {
                    Draw();
                }

                UIManager.SetTheme(tintedTheme);

                // No Draw() call in between - HandleStateChange (called synchronously inside
                // SetTheme's theme-refresh sweep) should already have updated textBox.TextColor
                // to the new theme's OnSurface, well before any Draw()/PreRenderLayout call gets
                // a chance to rebuild border1's (now-invalidated) cache.
                Assert.AreEqual(tintedTheme.OnSurface, seedTextBox.TextColor.Value,
                    "Expected textBox.TextColor to already reflect the new theme immediately after UIManager.SetTheme returns, before any Draw() call - otherwise a Draw()/PreRenderLayout call that rebuilds border1's cache in between could bake in the stale color.");
            }
            finally
            {
                UIManager.DefaultTheme = originalTheme;
            }
        }
    }
}
