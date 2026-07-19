using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Events;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Threading;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// PropertyEditor's per-property name label (PropertyLabelCell, a private Label subclass)
    /// should show a subtle highlight on hover, and stay highlighted for as long as its own
    /// value editor holds focus - see PropertyEditor.cs's own comment on PropertyLabelCell.
    /// </summary>
    [TestClass]
    public class TestPropertyEditorLabelHighlight
    {
        private class HighlightTestTarget
        {
            [DesignerProperty]
            public string Alpha { get; set; } = "a-value";
        }

        private static void PumpAnimations(FakeUIManager uiManager)
        {
            // ApplyThemedValueAnimated eases toward its target over ~100ms (see
            // UIComponentDrawable.DefaultStateTransitionDuration) rather than snapping instantly
            // - PerformLayout calls PropertyAnimationManager.Update() (see UIManager.Update),
            // which is what actually progresses it, so a couple of real-time pumps are needed
            // before checking the settled value, matching the pattern already established for
            // TextBoxTemplate's own label animation tests this session.
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(30);
                uiManager.PerformLayout();
            }
        }

        [TestMethod]
        public void Label_HighlightsOnHover_AndWhileItsEditorHasFocus()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var editor = new PropertyEditor
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };
            ui.AddChild(editor);

            uiManager.AddUI(ui);
            uiManager.PerformLayout();

            editor.TargetObject = new HighlightTestTarget();
            uiManager.PerformLayout();

            var innerStackPanel = editor.Children.OfType<StackPanel>().Single(sp => sp.Name == "PropertyEditor_StackPanel");
            var grid = innerStackPanel.Children.OfType<Grid>().Single(g => g.Name == "PropertyEditor_Grid");

            var label = grid.Children.OfType<Label>().Single();
            var valueEditor = grid.Children.OfType<TextBox>().Single();

            PumpAnimations(uiManager);
            Assert.AreEqual(Color.Transparent, label.Background.Value, "Expected no highlight before any hover/focus.");

            // Hover on - ForcePropagation bypasses the real hit-test geometry check, matching
            // the pattern used by other hover-simulating tests in this suite.
            label.HandleHoverChangedAsync(ui, new UIHoverChangedEvent { Hover = true, ForcePropagation = true }).GetAwaiter().GetResult();
            PumpAnimations(uiManager);
            Assert.AreNotEqual(Color.Transparent, label.Background.Value, "Expected a highlight while hovering the label.");

            // Hover off - highlight should clear again.
            label.HandleHoverChangedAsync(ui, new UIHoverChangedEvent { Hover = false, ForcePropagation = true }).GetAwaiter().GetResult();
            PumpAnimations(uiManager);
            Assert.AreEqual(Color.Transparent, label.Background.Value, "Expected the highlight to clear once the mouse leaves the label.");

            // The editor gaining focus (e.g. the user clicked into the text field to edit it)
            // should highlight the label too, even with the mouse nowhere near it.
            valueEditor.HasFocus.Value = true;
            PumpAnimations(uiManager);
            Assert.AreNotEqual(Color.Transparent, label.Background.Value, "Expected the label to stay highlighted while its value editor has focus.");

            valueEditor.HasFocus.Value = false;
            PumpAnimations(uiManager);
            Assert.AreEqual(Color.Transparent, label.Background.Value, "Expected the highlight to clear once the editor loses focus.");
        }
    }
}
