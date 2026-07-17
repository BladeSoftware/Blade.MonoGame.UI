using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Events;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// ButtonTemplate.HandleStateChange used to assign border1.Background/label1.TextColor/
    /// border1.BorderColor/border1.BorderThickness/border1.Elevation directly (e.g.
    /// "border1.Background.Value = Theme.SecondaryContainer;") instead of routing through
    /// ApplyThemedValue - so Button.SetStyleOverride(...) was silently discarded the next time
    /// hover/focus changed state, since the direct assignment always won regardless of any
    /// override. Fixed by routing every state-dependent assignment through ApplyThemedValue
    /// (checked against `button`, since that's the object an application actually holds a
    /// reference to and calls SetStyleOverride on).
    ///
    /// Asserts against the internal template's live Binding values rather than rendered pixels -
    /// ButtonTemplate's border1 has a non-zero CornerRadius/Elevation, which engages Border's
    /// stencil-based rounded-corner mask and drop shadow, and pixel-sampling that combination
    /// under FakeGame's headless GraphicsDevice proved unreliable independent of this bug (the
    /// underlying Binding value was already confirmed correct while the sampled pixel wasn't) -
    /// the Binding value is what the fix actually changes and is what future template rewrites
    /// would break, so it's the more precise thing to assert on here.
    /// </summary>
    [TestClass]
    public class TestButtonStyleOverride
    {
        private static IEnumerable<UIComponent> Walk(UIComponent root)
        {
            if (root == null) yield break;
            yield return root;
            foreach (var child in root.PrivateControls)
            {
                foreach (var d in Walk(child)) yield return d;
            }
            if (root is Control control && control.Content != null)
            {
                foreach (var d in Walk(control.Content)) yield return d;
            }
            if (root is Container container)
            {
                foreach (var child in container.Children)
                {
                    foreach (var d in Walk(child)) yield return d;
                }
            }
        }

        [TestMethod]
        public async Task SetStyleOverride_OnBackground_SurvivesHoverStateChange()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var button = new Button
            {
                Text = "Test",
                Width = 120,
                Height = 60,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
            };
            ui.AddChild(button);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();
            await uiManager.PerformLayout();

            var border1 = Walk(button).OfType<Border>().Single();

            // Baseline, unhovered: whatever the theme's normal Background is - not our override
            // color, and not the theme's hover color either.
            Assert.AreNotEqual(Color.Red, border1.Background.Value, "Expected the un-overridden normal state to not already be red.");

            button.SetStyleOverride(nameof(Button.Background), Color.Red);

            // Hover the button - this is exactly the state transition (Theme.PrimaryContainer ->
            // Theme.SecondaryContainer) that a direct-assignment bug would silently snap back to,
            // discarding the override.
            Point buttonCenter = button.GetFinalRect().Center;
            await button.HandleHoverChangedAsync(ui, new UIHoverChangedEvent { Hover = true, X = buttonCenter.X, Y = buttonCenter.Y });

            // ButtonTemplate now eases Background/TextColor/BorderColor toward their resolved
            // target via ApplyThemedValueAnimated (PropertyAnimationManager) instead of snapping
            // instantly - the resolved target is still the override color, but border1.Background
            // .Value only catches up once PropertyAnimationManager.Update() ticks (normally once
            // per UIManager.Update/frame), so let the ~100ms transition finish and pump one frame
            // before reading it back, rather than asserting on it mid-flight.
            await Task.Delay(150);
            await uiManager.PerformLayout();

            Assert.AreEqual(Color.Red, border1.Background.Value, "Expected the SetStyleOverride background to survive the hover state change.");

            // Un-hover: the override should still stick, not just happen to match one state.
            await button.HandleHoverChangedAsync(ui, new UIHoverChangedEvent { Hover = false, X = buttonCenter.X, Y = buttonCenter.Y, ForcePropagation = true });

            await Task.Delay(150);
            await uiManager.PerformLayout();

            Assert.AreEqual(Color.Red, border1.Background.Value, "Expected the SetStyleOverride background to survive returning to the normal (unhovered) state too.");
        }
    }
}
