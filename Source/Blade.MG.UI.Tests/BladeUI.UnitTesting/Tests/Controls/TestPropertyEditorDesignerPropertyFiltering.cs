using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// PropertyEditor used to show every public read/write property it found via reflection, with
    /// no way to curate the set - now only properties tagged with [DesignerProperty] appear,
    /// decoupled from whether the property happens to be Binding&lt;T&gt;-typed or a plain CLR
    /// property. This proves the filter itself, independent of the search-box filtering already
    /// covered by TestPropertyEditorSearch.
    /// </summary>
    [TestClass]
    public class TestPropertyEditorDesignerPropertyFiltering
    {
        private class FilterTestTarget
        {
            [DesignerProperty]
            public string Shown { get; set; } = "shown-value";

            // No [DesignerProperty] - even though it's public, readable, and writable, it must
            // not appear. Before this change, every such property showed up unconditionally.
            public string Hidden { get; set; } = "hidden-value";
        }

        [TestMethod]
        public void RefreshProperties_OnlyShowsPropertiesTaggedWithDesignerPropertyAttribute()
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

            editor.TargetObject = new FilterTestTarget();
            uiManager.PerformLayout();

            var innerStackPanel = editor.Children.OfType<StackPanel>().Single(sp => sp.Name == "PropertyEditor_StackPanel");
            var grid = innerStackPanel.Children.OfType<Grid>().Single(g => g.Name == "PropertyEditor_Grid");
            var labels = grid.Children.OfType<Label>().Select(l => l.Text.Value).ToList();

            CollectionAssert.AreEquivalent(new[] { "Shown" }, labels,
                "Expected only the [DesignerProperty]-tagged property to appear, not the untagged one.");
        }
    }
}
