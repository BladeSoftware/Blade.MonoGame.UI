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
    /// MeasureSelf/ArrangeSelf (the shared choke points behind Border/Label/Control/etc. - see
    /// the "Layout dirty-flagging" comment block in UIComponent.cs) used to unconditionally
    /// recompute DesiredSize/FinalRect every single frame for every control, regardless of
    /// whether anything relevant had actually changed - expensive for a mostly-static HUD tree
    /// running at a very high frame rate. Fixed by skipping recomputation when neither this
    /// control's own inputs (Width/Height/Margin/Padding/etc., snapshot-compared every call)
    /// nor anything in its subtree (tracked via the existing BubbleInvalidation walk) changed
    /// since the last pass.
    ///
    /// These tests assert directly on MeasureRecomputeCount/ArrangeRecomputeCount (instrumentation
    /// that only increments on an actual recompute, never on a skip) rather than only on the
    /// resulting FinalRect/DesiredSize - the whole point is proving the skip actually happens,
    /// not just that the (necessarily identical either way) output is correct.
    /// </summary>
    [TestClass]
    public class TestLayoutDirtyFlagging
    {
        [TestMethod]
        public async Task StaticTree_SecondFrame_SkipsRecomputation()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var label = new Label { Text = "Hello", Width = 100, Height = 30 };
            var border = new Border { Content = label, Width = 120, Height = 50 };
            ui.AddChild(border);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            int borderMeasureCountAfterFirstFrame = border.MeasureRecomputeCount;
            int borderArrangeCountAfterFirstFrame = border.ArrangeRecomputeCount;
            int labelMeasureCountAfterFirstFrame = label.MeasureRecomputeCount;
            int labelArrangeCountAfterFirstFrame = label.ArrangeRecomputeCount;

            Assert.AreNotEqual(0, borderMeasureCountAfterFirstFrame, "Expected the first frame to actually compute layout.");
            Assert.AreNotEqual(0, labelMeasureCountAfterFirstFrame, "Expected the first frame to actually compute layout.");

            // Nothing changed - a second frame should skip recomputation entirely for both.
            await uiManager.PerformLayout();

            Assert.AreEqual(borderMeasureCountAfterFirstFrame, border.MeasureRecomputeCount, "Expected Border's Measure to be skipped on an unchanged second frame.");
            Assert.AreEqual(borderArrangeCountAfterFirstFrame, border.ArrangeRecomputeCount, "Expected Border's Arrange to be skipped on an unchanged second frame.");
            Assert.AreEqual(labelMeasureCountAfterFirstFrame, label.MeasureRecomputeCount, "Expected Label's Measure to be skipped on an unchanged second frame.");
            Assert.AreEqual(labelArrangeCountAfterFirstFrame, label.ArrangeRecomputeCount, "Expected Label's Arrange to be skipped on an unchanged second frame.");
        }

        [TestMethod]
        public async Task ChangingNestedLabelText_RecomputesLabelAndBubblesToAncestorBorder()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var label = new Label { Text = "Hello", Width = 100, Height = 30 };
            var border = new Border { Content = label, Width = 120, Height = 50 };
            ui.AddChild(border);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();
            await uiManager.PerformLayout(); // settle into the "skipping" steady state

            int borderMeasureCountBefore = border.MeasureRecomputeCount;
            int labelMeasureCountBefore = label.MeasureRecomputeCount;

            // Text is a Binding<string> - changing it fires Binding.Changed, which bubbles via
            // BubbleInvalidation all the way up through label -> border -> ui, marking every
            // ancestor's isLayoutDirty even though border's own Width/Height/Margin/etc. never
            // changed - this is exactly the "a descendant's own property changed" case that a
            // pure "did my own inputs change" comparison (with no bubble signal) would miss.
            label.Text.Value = "A much longer piece of text than before";

            await uiManager.PerformLayout();

            Assert.AreEqual(labelMeasureCountBefore + 1, label.MeasureRecomputeCount, "Expected the Label's own Measure to recompute after its Text changed.");
            Assert.AreEqual(borderMeasureCountBefore + 1, border.MeasureRecomputeCount, "Expected the ancestor Border's Measure to also recompute, since the Label's DesiredSize may have changed.");
        }

        [TestMethod]
        public async Task ChangingWidthDirectly_StillRecomputes_EvenThoughItIsNotABinding()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var border = new Border { Width = 120, Height = 50 };
            ui.AddChild(border);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();
            await uiManager.PerformLayout(); // settle into the "skipping" steady state

            int measureCountBefore = border.MeasureRecomputeCount;
            int arrangeCountBefore = border.ArrangeRecomputeCount;

            // Width/Height are plain Length properties, not Binding<T> - they never fire
            // Binding.Changed and so never bubble on their own. The skip-check must catch this
            // itself via its own snapshot comparison, or a control whose size is set directly
            // (rather than through a data-bound property) would silently freeze in place.
            border.Width = 300;

            await uiManager.PerformLayout();

            Assert.AreEqual(measureCountBefore + 1, border.MeasureRecomputeCount, "Expected changing Width directly (not through a Binding) to still force a Measure recompute.");
            Assert.AreEqual(arrangeCountBefore + 1, border.ArrangeRecomputeCount, "Expected changing Width directly (not through a Binding) to still force an Arrange recompute.");
            Assert.AreEqual(300, border.GetFinalRect().Width, "Expected the new Width to have actually taken effect, not been skipped as stale.");
        }

        [TestMethod]
        public async Task ChangingTransformDirectly_StillRecomputesArrange_EvenThoughItIsNotABinding()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var border = new Border { Width = 120, Height = 50 };
            ui.AddChild(border);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();
            await uiManager.PerformLayout(); // settle into the "skipping" steady state

            int arrangeCountBefore = border.ArrangeRecomputeCount;

            // Transform is a plain struct property, not Binding<T> - a game rotating/scaling a
            // HUD element frame-by-frame (e.g. a spinning icon) mutates it directly. If the
            // skip-check didn't separately compare it, EffectiveTransform would freeze at
            // whichever value happened to be current when the control last recomputed.
            border.Transform = border.Transform with { Rotation = new Vector3(0, 0, 1f) };

            await uiManager.PerformLayout();

            Assert.AreEqual(arrangeCountBefore + 1, border.ArrangeRecomputeCount, "Expected changing Transform directly (not through a Binding) to still force an Arrange recompute.");
        }
    }
}
