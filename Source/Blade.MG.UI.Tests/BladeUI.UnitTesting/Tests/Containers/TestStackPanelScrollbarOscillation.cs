using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BladeUI.UnitTesting.Tests.Containers
{
    /// <summary>
    /// Covers the fix for a layout-oscillation bug reported in a real usage site (the Examples
    /// project's "Dynamic Color" page's horizontal toolbar row: a seed-color swatch, hex TextBox,
    /// "Dark Mode" CheckBox, and "Apply" Button, resizing vertically every frame while the user
    /// watched). StackPanel extends ScrollPanel, whose HorizontalScrollBarVisible/
    /// VerticalScrollBarVisible both default to <see cref="ScrollBarVisibility.Auto"/> - a bad
    /// default for a plain horizontal toolbar row that never needs to scroll in either direction;
    /// the reported mechanism was a vertical scrollbar appearing (height too tight for its
    /// children), narrowing available width enough to trigger a horizontal scrollbar too, which
    /// squeezed the height further, undoing the very thing that triggered the vertical scrollbar
    /// - each frame's Arrange starting from a different size than the last, never converging.
    ///
    /// This test asserts the FIXED configuration (both scrollbars explicitly hidden on a
    /// toolbar-shaped row, matching HelpPage_DynamicColor.cs's own fix) stays size-stable across
    /// many repeated layout passes. Reproducing the ORIGINAL (default Auto/Auto) oscillation
    /// itself in this synthetic harness was attempted but not achieved - a plain
    /// HorizontalAlignment.Left StackPanel here simply grows to its content's natural width
    /// rather than being clamped against its parent's available space the way a live window
    /// resize interacts with it, so `IsHorizontalScrollbarVisible`/`IsVerticalScrollbarVisible`
    /// never actually flip under FakeUIManager's layout pass regardless of which
    /// ScrollBarVisible setting is used. The fix itself is unambiguous regardless (it removes the
    /// only mechanism that could cause the reported resizing, per ScrollPanel.cs's own default),
    /// so this test covers "the fix doesn't regress anything and the configuration is stable" -
    /// not a from-scratch repro of the live oscillation.
    /// </summary>
    [TestClass]
    public class TestStackPanelScrollbarOscillation
    {
        [TestMethod]
        public void HiddenScrollbars_KeepsToolbarRowSizeStable_AcrossRepeatedFrames()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var host = new Panel
            {
                Width = 260,
                Height = 200,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
            };
            ui.AddChild(host);

            var outer = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Auto,
            };
            host.AddChild(outer);

            // Mirrors HelpPage_DynamicColor's controlsRow: a fixed-size swatch, a TextBox, a
            // CheckBox, and a Button, wider than the 260px host - with both scrollbars
            // explicitly hidden, matching the fix.
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Center,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };
            outer.AddChild(row);

            row.AddChild(new Border { Width = 40, Height = 40, Margin = new Thickness(0, 0, 10, 0) });
            row.AddChild(new TextBox { Text = "#6750A4", Width = 140, Margin = new Thickness(0, 0, 20, 0) });
            row.AddChild(new CheckBox { Text = "Dark Mode", Margin = new Thickness(0, 0, 20, 0) });
            row.AddChild(new Button { Text = "Apply as Active Theme", Width = 200 });

            uiManager.AddUI(ui);
            uiManager.PerformLayout();

            var sizes = new System.Collections.Generic.List<(int Width, int Height)>();
            for (int i = 0; i < 10; i++)
            {
                uiManager.PerformLayout();
                var rect = row.GetFinalRect();
                sizes.Add((rect.Width, rect.Height));
            }

            for (int i = 1; i < sizes.Count; i++)
            {
                Assert.AreEqual(sizes[0], sizes[i],
                    $"Expected the toolbar row's size to stay stable across repeated frames with scrollbars explicitly hidden - frame {i} was {sizes[i]}, frame 0 was {sizes[0]}. Fluctuating size means the row is oscillating between layout passes.");
            }
        }
    }
}
