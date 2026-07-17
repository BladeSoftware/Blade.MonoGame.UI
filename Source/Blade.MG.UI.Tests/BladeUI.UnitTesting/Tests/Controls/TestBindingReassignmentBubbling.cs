using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// Margin/Padding/Visible/HasFocus/MouseHover (UIComponent), Background (UIComponentDrawable)
    /// and BorderColor/BorderThickness/CornerRadius/Elevation (Border) used to be plain
    /// auto-properties (`{ get; set; }`). Assigning a raw value to one - e.g.
    /// `border.Background = Color.Red;`, relying on Binding&lt;T&gt;'s implicit T->Binding&lt;T&gt;
    /// conversion - replaced the property's backing Binding&lt;T&gt; instance wholesale, silently
    /// orphaning whatever Changed subscription EnsureBindingsWired had already wired up on the
    /// original instance (used for render-cache invalidation / layout dirty-flagging bubbling -
    /// see BubbleInvalidation). Now routed through SetField, which recognizes an implicitly-cast
    /// Binding&lt;T&gt; and copies its Getter/Setter onto the EXISTING field instead of replacing it -
    /// the field's own object identity (and therefore its Changed subscribers) never changes.
    ///
    /// This test proves the subscription survives a raw-value reassignment by observing its real
    /// effect: MeasureRecomputeCount/ArrangeRecomputeCount only increment when isLayoutDirty (set
    /// via BubbleInvalidation, which only fires if Changed correctly fired) is true - if the
    /// reassignment's Changed event were being silently dropped, a later frame would incorrectly
    /// skip recomputation entirely.
    /// </summary>
    [TestClass]
    public class TestBindingReassignmentBubbling
    {
        [TestMethod]
        public async Task ReassigningBackgroundWithRawValue_StillBubblesAndForcesRecompute()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var border = new Border { Width = 100, Height = 50 };
            ui.AddChild(border);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();
            await uiManager.PerformLayout(); // settle into the "skipping" steady state

            int measureCountBefore = border.MeasureRecomputeCount;
            int arrangeCountBefore = border.ArrangeRecomputeCount;

            // Raw-value reassignment via the implicit T->Binding<T> conversion - exactly the
            // pattern that used to silently orphan the Changed subscription.
            border.Background = Color.Red;

            await uiManager.PerformLayout();

            Assert.AreEqual(Color.Red, border.Background.Value, "Expected the reassigned value to actually take effect.");
            Assert.AreEqual(measureCountBefore + 1, border.MeasureRecomputeCount,
                "Expected reassigning Background with a raw value to still bubble via Changed and force a Measure recompute, not silently go stale.");
            Assert.AreEqual(arrangeCountBefore + 1, border.ArrangeRecomputeCount,
                "Expected reassigning Background with a raw value to still bubble via Changed and force an Arrange recompute, not silently go stale.");

            // A second raw-value reassignment - proves the subscription keeps working
            // indefinitely, not just for the first rebind after construction. The preceding
            // block already proved a single PerformLayout fully bubbles+recomputes+settles a
            // raw reassignment, so this extra "settle again" call is expected to be a no-op
            // (zero additional recomputes) rather than needing a frame of its own.
            int measureCountAfterFirst = border.MeasureRecomputeCount;
            await uiManager.PerformLayout(); // confirm already settled - should be a no-op

            Assert.AreEqual(measureCountAfterFirst, border.MeasureRecomputeCount,
                "Expected the system to already be settled after the first reassignment's PerformLayout, with no extra recompute needed.");

            border.Background = Color.Blue;
            await uiManager.PerformLayout();

            Assert.AreEqual(Color.Blue, border.Background.Value);
            Assert.AreEqual(measureCountAfterFirst + 1, border.MeasureRecomputeCount,
                "Expected a second raw-value reassignment to still bubble correctly and force exactly one more recompute.");
        }
    }
}
