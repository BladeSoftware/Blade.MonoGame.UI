using System.Linq;
using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Controls.Templates;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// Covers a bug found in a real usage site (the Dynamic Color Examples page's "Apply as
    /// Active Theme" button, reported as cramped with no way to add breathing room): setting
    /// Button.Padding had NO visible effect on the rendered pill no matter what value was used.
    ///
    /// Root cause: ButtonTemplate.cs hardcoded border1 (the actual rendered pill, a Border whose
    /// FinalRect is what's drawn) to Padding=0, so Button's own Padding never reached it. Setting
    /// Padding directly on Button itself doesn't work either - Button's Padding inflates Button's
    /// own DesiredSize (via UIComponent.MergeChildDesiredSize, same contract every control gets)
    /// AND deflates the FinalContentRect handed down to its internal template by the exact same
    /// amount (UIComponent.ArrangeSelf: FinalContentRect = FinalRect inset by this control's own
    /// Padding) - both happen at the same Button/ButtonTemplate hop, so they cancel out and
    /// border1 (further down, filling whatever box it's given via HorizontalAlignment.Stretch)
    /// never sees the extra space. border1 itself doesn't have this problem: what's actually
    /// rendered is border1's own FinalRect (inflated correctly by its own Padding, same as any
    /// auto-sized control wrapping smaller content), not a FinalContentRect one hop further down -
    /// only label1 (border1's Content) sees the deflated box, which is exactly the desired
    /// "breathing room around the label" effect.
    ///
    /// Fix: ButtonTemplate.cs now binds border1.Padding to button.Padding.Value instead of a
    /// hardcoded zero.
    /// </summary>
    [TestClass]
    public class TestButtonPadding
    {
        private static Border GetPillBorder(Button button)
        {
            var template = (ButtonTemplate)button.PrivateControls.First();
            return (Border)template.Content;
        }

        private static Rectangle MeasureButtonPill(FakeUIManager uiManager, EmptyUI ui, Button button)
        {
            // Top/Left-aligned (not the framework's default Stretch) so each button sizes to its
            // own natural content, unaffected by any sibling's size or the host's - isolates
            // Padding's effect on the rendered pill from unrelated cross-axis stretch behavior.
            button.HorizontalAlignment = HorizontalAlignmentType.Left;
            button.VerticalAlignment = VerticalAlignmentType.Top;

            var host = new Panel { Width = 900, Height = 200, HorizontalAlignment = HorizontalAlignmentType.Left, VerticalAlignment = VerticalAlignmentType.Top };
            host.AddChild(button);
            ui.AddChild(host);

            uiManager.PerformLayout();
            uiManager.PerformLayout();

            return GetPillBorder(button).GetFinalRect();
        }

        [TestMethod]
        public void Padding_GrowsTheRenderedPill_NotJustTheInvisibleOuterBox()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();
            uiManager.AddUI(ui);

            var plainPill = MeasureButtonPill(uiManager, ui, new Button { Text = "Apply as Active Theme" });
            var paddedPill = MeasureButtonPill(uiManager, ui, new Button { Text = "Apply as Active Theme", Padding = new Thickness(20, 10) });

            Assert.IsTrue(paddedPill.Width > plainPill.Width + 30,
                $"Expected the padded button's rendered pill to be noticeably wider than the plain one's (roughly +40px for 20px left/right Padding), but plain={plainPill.Width}, padded={paddedPill.Width}.");
            Assert.IsTrue(paddedPill.Height > plainPill.Height + 10,
                $"Expected the padded button's rendered pill to be noticeably taller than the plain one's (roughly +20px for 10px top/bottom Padding), but plain={plainPill.Height}, padded={paddedPill.Height}.");
        }
    }
}
