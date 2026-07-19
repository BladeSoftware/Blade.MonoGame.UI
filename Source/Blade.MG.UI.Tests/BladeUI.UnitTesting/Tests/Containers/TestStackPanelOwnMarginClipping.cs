using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BladeUI.UnitTesting.Tests.Containers
{
    /// <summary>
    /// Covers a bug found in a real usage site (the Examples project's "Dynamic Color" page's
    /// horizontal toolbar row, after the row's oscillation and Apply-button-clipping bugs -
    /// see TestStackPanelScrollbarOscillation.cs - were already fixed): a non-Stretch
    /// (HorizontalAlignment.Left) StackPanel with its own Margin clipped its LAST child, cutting
    /// "Dark Mode" down to "Dark M".
    ///
    /// Root cause: every other control's DesiredSize includes its own Margin+Padding via
    /// UIComponent.MeasureSelf (`desiredWidth += Margin.Value.Left + Margin.Value.Right; ...`),
    /// and ArrangeSelf unconditionally assumes that contract holds
    /// (`desiredWidth = DesiredSize.Width - Margin.Value.Horizontal;`). StackPanel.Measure
    /// overrides DesiredSize with the raw sum of its children's DesiredSizes (each of which
    /// already includes THAT child's own Margin) but never added the StackPanel's OWN
    /// Margin/Padding on top - so ArrangeSelf's unconditional subtraction had nothing to undo,
    /// leaving FinalRect/ActualWidth exactly one Margin-width too small whenever
    /// HorizontalAlignment wasn't Stretch (Stretch instead takes layoutBounds.Width directly,
    /// which is why this only showed up on a Left-aligned toolbar row, not a full-width one).
    /// </summary>
    [TestClass]
    public class TestStackPanelOwnMarginClipping
    {
        [TestMethod]
        public void LeftAlignedRow_WithMargin_FitsAllChildrenWithoutClippingLastChild()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var host = new Panel
            {
                Width = 900,
                Height = 400,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
            };
            ui.AddChild(host);

            var layoutPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Auto,
            };
            host.AddChild(layoutPanel);

            // Mirrors HelpPage_DynamicColor's controlsRow exactly: Left-aligned, its own
            // Margin, both scrollbars hidden, three children each with their own Margin.
            var controlsRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(30, 10, 30, 10),
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Center,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };
            layoutPanel.AddChild(controlsRow);

            var swatch = new Border { Width = 40, Height = 40, Margin = new Thickness(0, 0, 10, 0) };
            var textBox = new TextBox { Text = "#6750A4FF", Width = 140, Margin = new Thickness(0, 0, 20, 0) };
            var checkBox = new CheckBox { Text = "Dark Mode", Margin = new Thickness(0, 0, 20, 0) };

            controlsRow.AddChild(swatch);
            controlsRow.AddChild(textBox);
            controlsRow.AddChild(checkBox);

            uiManager.AddUI(ui);
            uiManager.PerformLayout();
            uiManager.PerformLayout();
            uiManager.PerformLayout();

            var rowRect = controlsRow.GetFinalRect();
            var checkBoxRect = checkBox.GetFinalRect();

            // The last child's right edge must fall within the row's own FinalRect - a StackPanel
            // sized to its own content should never clip the very children it measured.
            Assert.IsTrue(checkBoxRect.Right <= rowRect.Right,
                $"Expected the CheckBox (last child) to fit inside controlsRow's FinalRect, but " +
                $"checkBox.Right={checkBoxRect.Right} > controlsRow.Right={rowRect.Right} - " +
                $"controlsRow FinalRect={rowRect}, DesiredSize={controlsRow.DesiredSize}");

            // The row's own FinalRect.Width must equal its DesiredSize.Width minus its own
            // Margin (the standard ArrangeSelf contract) - i.e. DesiredSize must have folded in
            // this row's own Margin, not just its children's.
            float expectedFinalWidth = controlsRow.DesiredSize.Width - controlsRow.Margin.Value.Horizontal;
            Assert.AreEqual((int)expectedFinalWidth, rowRect.Width,
                $"controlsRow FinalRect.Width should be DesiredSize.Width ({controlsRow.DesiredSize.Width}) minus its own Margin.Horizontal ({controlsRow.Margin.Value.Horizontal}), but was {rowRect.Width}");
        }
    }
}
