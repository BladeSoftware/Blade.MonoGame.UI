using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// PropertyEditor's search box used to filter by diffing searchBox.Text once per frame in
    /// Arrange, because Binding&lt;T&gt; had no change notification. Now that Binding&lt;T&gt;
    /// raises Changed (see TestCacheInvalidation), PropertyEditor subscribes to
    /// searchBox.Text.Changed directly instead. This test types into the search box and checks
    /// the filter applied WITHOUT ever calling PerformLayout/Arrange afterward - proving the
    /// refresh happens synchronously off the Changed event, not on the next frame's Arrange.
    /// </summary>
    [TestClass]
    public class TestPropertyEditorSearch
    {
        private class SearchTestTarget
        {
            [DesignerProperty]
            public string Alpha { get; set; } = "a-value";
            [DesignerProperty]
            public string Beta { get; set; } = "b-value";
        }

        [TestMethod]
        public void TypingInSearchBox_FiltersGrid_WithoutAnArrangePass()
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

            editor.TargetObject = new SearchTestTarget();
            uiManager.PerformLayout();

            var innerStackPanel = editor.Children.OfType<StackPanel>().Single(sp => sp.Name == "PropertyEditor_StackPanel");
            var grid = innerStackPanel.Children.OfType<Grid>().Single(g => g.Name == "PropertyEditor_Grid");
            var searchBox = innerStackPanel.Children.OfType<TextBox>().Single();

            System.Func<System.Collections.Generic.List<string>> currentLabels = () =>
                grid.Children.OfType<Label>().Select(l => l.Text.Value).ToList();

            var before = currentLabels();
            CollectionAssert.AreEquivalent(new[] { "Alpha", "Beta" }, before, "Expected both properties listed before any filtering.");

            // Simulate typing "alp" - real keystrokes ultimately do Text.Value = ... too (see
            // TextEntryControl's Insert/Remove methods), so mutating .Value directly here
            // exercises the same Changed-event path without needing to synthesize key events.
            searchBox.Text.Value = "alp";

            // Deliberately no uiManager.PerformLayout()/editor.Arrange() call here - if the
            // filter still relied on per-frame Arrange diffing, this assertion would see the
            // stale, unfiltered grid.
            var afterTyping = currentLabels();
            CollectionAssert.AreEquivalent(new[] { "Alpha" }, afterTyping, "Expected the grid to already be filtered to just 'Alpha' immediately after Text.Value changed, with no Arrange pass in between.");

            // Clearing the filter should restore both rows, same way.
            searchBox.Text.Value = "";
            var afterClearing = currentLabels();
            CollectionAssert.AreEquivalent(new[] { "Alpha", "Beta" }, afterClearing, "Expected clearing the search text to restore both properties.");
        }
    }
}
