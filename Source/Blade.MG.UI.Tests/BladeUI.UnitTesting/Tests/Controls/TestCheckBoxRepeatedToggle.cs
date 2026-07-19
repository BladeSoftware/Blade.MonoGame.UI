using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Events;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// Reproduces the bug where a CheckBox's rendered check mark only ever updated on the very
    /// first click (which also gains focus) and never again on subsequent clicks while it
    /// stayed focused. Root cause: CheckBox.IsChecked (CheckBox.cs) was a plain
    /// <c>{ get; set; }</c> auto-property, not SetField-backed like every other Binding&lt;T&gt;
    /// property in this library - CheckBox.ActivateAsync toggles it via wholesale reassignment
    /// (<c>IsChecked = IsChecked?.Value switch {...}</c>), which goes through Binding&lt;T&gt;'s
    /// implicit bool?->Binding&lt;bool?&gt; cast and constructs a BRAND NEW instance every single
    /// toggle. UIComponent.EnsureBindingsWired only ever wires a control's bindings' Changed
    /// events ONCE per instance (guarded by a bindingsWired flag, never re-scanned), so every
    /// post-construction reassignment silently discarded whatever Changed subscription had been
    /// wired, leaving the new instance's Changed event with zero subscribers - nothing ever
    /// bubbled a cache invalidation from IsChecked changing on its own. The first click after
    /// gaining focus appeared to work only because HasFocus (a real SetField-backed Binding)
    /// changed on that same click and bubbled independently, incidentally forcing a redraw that
    /// happened to also show the just-toggled check mark; every later click, with focus already
    /// held, toggled the underlying value but never forced anything to repaint it.
    /// </summary>
    [TestClass]
    public class TestCheckBoxRepeatedToggle
    {
        [TestMethod]
        public async System.Threading.Tasks.Task RepeatedClicksWhileFocused_EachToggleTheRenderedCheckMark()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var host = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };
            ui.AddChild(host);

            // A cached ancestor Border, mirroring how a CheckBox is typically hosted inside a
            // Section/panel in real usage (Border always enables render caching - see Border's
            // own constructor) - without this wrapper, IsChecked's own render would still run
            // every frame regardless (nothing above it to skip), masking the bug.
            var border = new Border
            {
                Width = 200,
                Height = 60,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                CornerRadius = new CornerRadius(0f),
                BorderThickness = new Thickness(0),
                Background = Color.Transparent,
                EnableCaching = true,
            };
            host.AddChild(border);

            var checkbox = new CheckBox
            {
                Width = 180,
                Height = 40,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                Text = "Check me",
            };
            border.Content = checkbox;

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

                Color[] pixels = new Color[renderTarget.Width * renderTarget.Height];
                renderTarget.GetData(pixels);

                Rectangle rect = border.GetFinalRect();
                long checksum = 0;
                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    for (int x = rect.Left; x < rect.Right; x++)
                    {
                        Color c = pixels[y * renderTarget.Width + x];
                        checksum += c.R + (c.G * 31) + (c.B * 97);
                    }
                }
                return checksum;
            }

            long uncheckedChecksum = SampleChecksum();

            // First click: gains focus AND toggles IsChecked (false -> true) - matches the
            // user's exact report ("the first time I click it, it focuses the checkbox and
            // toggles the check mark").
            checkbox.HasFocus.Value = true;
            await checkbox.ActivateAsync(ui, new UIClickEvent());

            long afterFirstClickChecksum = SampleChecksum();
            Assert.AreNotEqual(uncheckedChecksum, afterFirstClickChecksum,
                "Expected the check mark to render after the first click (which also gains focus).");

            // Second click: focus is unchanged (already focused) - only IsChecked toggles again
            // (true -> false). This is the click the user reported as not refreshing.
            await checkbox.ActivateAsync(ui, new UIClickEvent());

            long afterSecondClickChecksum = SampleChecksum();
            Assert.AreNotEqual(afterFirstClickChecksum, afterSecondClickChecksum,
                "Expected the check mark to render again after a second click while already focused - equal checksums mean IsChecked's own change never forced a redraw, matching the reported bug where only the first (focus-gaining) click visibly refreshes the check mark.");
        }

        /// <summary>
        /// Reproduces the NullReferenceException introduced by the SetField-backed IsChecked fix
        /// above, hit immediately when opening the Examples project's CheckBox page. That page
        /// constructs a tristate CheckBox with <c>IsChecked = null</c> (expressing the
        /// indeterminate state) - a literal `null` assigned directly to a Binding&lt;bool?&gt;
        /// property is directly reference-compatible with Binding&lt;T&gt; itself (a class), so
        /// no implicit T-&gt;Binding&lt;T&gt; conversion runs and SetField's `value` parameter
        /// arrives as an actual null reference, not a Binding wrapping a null bool? - dereferencing
        /// `value.IsImplicitCast` then threw. The OLD plain-auto-property IsChecked silently
        /// tolerated this (just stored a null property reference, papered over by every read site
        /// already using `checkbox.IsChecked?.Value`) - SetField needs to handle it explicitly
        /// instead of newly crashing on a case the framework already has established behavior for.
        /// </summary>
        [TestMethod]
        public void AssigningIsCheckedNull_ForTristateIndeterminateState_DoesNotThrow()
        {
            CheckBox checkbox = null;

            void Construct()
            {
                checkbox = new CheckBox
                {
                    Text = "Tristate",
                    Tristate = true,
                    IsChecked = null,
                };
            }

            Construct();

            Assert.IsNotNull(checkbox.IsChecked,
                "Expected IsChecked to remain a real Binding<bool?> instance (never a null property reference itself) even after assigning a literal null - only the wrapped Value should become null.");
            Assert.IsNull(checkbox.IsChecked.Value,
                "Expected IsChecked.Value to be null (the tristate indeterminate state) after assigning IsChecked = null.");
        }
    }
}
