using System.Reflection;
using System.Threading;
using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Theming;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// Reproduces the reported bug: switching the active theme (UIManager.SetTheme) doesn't
    /// refresh an unfocused TextBox's floating label color - it stays stuck showing the OLD
    /// theme's color until the box is focused (or blurred) again.
    ///
    /// Root cause: TextBoxTemplate.HandleStateChange (called by UIWindow.RefreshThemeRecursive's
    /// theme-refresh sweep, via StateHasChanged, on every single control) only re-applies
    /// Background - the floating label's color/position/font-size and the underline color are
    /// all deliberately computed fresh in RenderControl instead (see that method's own comment
    /// on why - they depend on isFocused/isHovered branching). But TextBoxTemplate can sit under
    /// a cached ancestor Border (any Border - EnableCaching defaults to true, see Border's own
    /// constructor - not just the Examples project's specific "Section" wrapper), and
    /// RenderControl is skipped entirely whenever that ancestor's cache stays valid (see
    /// UIComponentDrawable.RenderChildOrFromCache). StateHasChanged() itself never forces that
    /// cache invalid - it only calls HandleStateChange(), and invalidation is otherwise purely a
    /// side effect of some Binding&lt;T&gt;'s Changed event firing (see UIComponent.
    /// BubbleInvalidation/OnOwnBindingChanged). Since HandleStateChange only touches Background,
    /// if this TextBox's Background color happens not to differ between the two themes (or
    /// nothing else in the same cached subtree changes either), nothing invalidates the cache,
    /// RenderControl never re-runs, and the label's color/underline are never recomputed from
    /// the new theme - only a real focus change (which flips the HasFocus Binding, a property
    /// that DOES reliably fire Changed/BubbleInvalidation on its own) forces it.
    /// </summary>
    [TestClass]
    public class TestTextBoxThemeRefresh
    {
        /// <summary>
        /// Shallow-copies every public field from <paramref name="source"/> into a new
        /// UITheme, then overwrites OnSurfaceVariant - isolates the floating label's unfocused
        /// color as the ONLY themed value that differs between the two themes used in this
        /// test, so a full-subtree redraw can't be triggered by some other, unrelated themed
        /// value (like Background) changing instead - which is exactly what happened when this
        /// test was first written comparing LightTheme() vs DarkTheme() outright: Background
        /// (Theme.Surface) differs hugely between those two, and TextBoxTemplate.
        /// HandleStateChange DOES re-apply Background reliably (see its own override) - that
        /// alone was enough to invalidate the cache and mask the real bug, which is specifically
        /// about values ONLY recomputed in RenderControl (label color/underline), not
        /// HandleStateChange.
        /// </summary>
        private static UITheme CloneWithDifferentOnSurfaceVariant(UITheme source, Color newOnSurfaceVariant)
        {
            var clone = new UITheme();
            foreach (var field in typeof(UITheme).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                field.SetValue(clone, field.GetValue(source));
            }
            clone.OnSurfaceVariant = newOnSurfaceVariant;
            return clone;
        }

        [TestMethod]
        public void SetTheme_RefreshesUnfocusedTextBoxLabelColor_WithoutRefocusing()
        {
            var originalTheme = UIManager.DefaultTheme;
            try
            {
                var baseTheme = DefaultThemes.LightTheme();
                var tintedTheme = CloneWithDifferentOnSurfaceVariant(baseTheme, Color.Lime);

                UIManager.DefaultTheme = baseTheme;

                var uiManager = new FakeUIManager();
                var ui = new EmptyUI();

                // A Border wrapping the TextBox - EnableCaching defaults to true (Border's own
                // constructor), matching any real app's "put a TextBox inside a Border/Section"
                // usage, not just the Examples project's specific wrapper.
                var wrapper = new Border
                {
                    HorizontalAlignment = HorizontalAlignmentType.Left,
                    VerticalAlignment = VerticalAlignmentType.Top,
                    Width = 320,
                    Height = 80,
                };
                ui.AddChild(wrapper);

                var textBox = new TextBox
                {
                    Width = 300,
                    Height = 60,
                    HorizontalAlignment = HorizontalAlignmentType.Left,
                    VerticalAlignment = VerticalAlignmentType.Top,
                    Label = "Some Label",
                    Text = "",
                    HelperText = null,
                    ShrinkLabel = true, // Keep the label in its shrunk (unfocused-colored) position
                };
                wrapper.Content = textBox;

                uiManager.AddUI(ui);
                uiManager.PerformLayout();
                uiManager.PerformLayout();

                var graphicsDevice = FakeGame.Instance.GraphicsDevice;
                using var renderTarget = new RenderTarget2D(graphicsDevice, 400, 300);
                using var spriteBatch = new SpriteBatch(graphicsDevice);

                long SampleChecksum()
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

                // Warm up until the first-render transients (border thickness settling, etc. -
                // see TestTextBoxCacheInvalidation.cs's own comment on this) stop changing the
                // checksum on their own.
                long lightThemeChecksum = 0;
                for (int i = 0; i < 4; i++)
                {
                    lightThemeChecksum = SampleChecksum();
                }

                // Switch to a theme that differs ONLY in OnSurfaceVariant (the unfocused
                // floating label's color) - WITHOUT focusing or blurring the TextBox in between.
                UIManager.SetTheme(tintedTheme);

                // label1's color eases via PropertyAnimationManager (see TextBoxTemplate.
                // RenderControl) rather than snapping - AnimateTo is only actually CALLED the
                // NEXT time RenderControl runs (registering the new target), and the value only
                // progresses toward it via PropertyAnimationManager.Update, ticked once per real
                // frame from PerformLayout (wall-clock-time based, not GameTime-delta - see
                // TestToggleSwitch.cs's own comment on this exact pitfall). So: prime (sample
                // once to register the animation target, still shows the OLD color), sleep (let
                // real time elapse past the 80ms duration), pump (PerformLayout ticks the
                // animation), then sample again for the real result.
                SampleChecksum();
                Thread.Sleep(150);
                uiManager.PerformLayout();

                long afterThemeSwitchChecksum = SampleChecksum();

                Assert.AreNotEqual(lightThemeChecksum, afterThemeSwitchChecksum,
                    "Expected the TextBox's rendered label to change color after UIManager.SetTheme - " +
                    "equal checksums mean the ancestor Border's cached bitmap went stale but was never " +
                    "invalidated, leaving the floating label frozen at the old theme's color until a focus change forces it.");
            }
            finally
            {
                UIManager.DefaultTheme = originalTheme;
            }
        }
    }
}
