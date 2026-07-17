using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Events;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Containers
{
    /// <summary>
    /// StackPanel.Measure re-measures every child a second time in its own stacking-specific
    /// loop (on top of the generic per-child pass its own base.Measure/ScrollPanel.Measure call
    /// already does), and used to do that second pass against the raw, unreduced availableSize
    /// parameter instead of the same scrollbar-aware size ScrollPanel.Measure computes for
    /// itself (see GetScrollAwareAvailableSize). Since the second (stacking-specific) pass
    /// always runs later and its DesiredSize sticks, a visible vertical scrollbar's reserved
    /// width was silently discarded - and because IsVerticalScrollbarVisible depends on the
    /// previous frame's arranged content bounds, a broad, unrelated invalidation (e.g. a focus
    /// event cascading HasFocus into every descendant of a focused control, not just a hover
    /// flip on one control) could nudge that one-frame-lagged recomputation and make the
    /// available width for a Grid's star columns jump - or even repeatedly ping-pong - as the
    /// discrepancy tipped back and forth.
    /// </summary>
    [TestClass]
    public class TestStackPanelScrollbarWidthConsistency
    {
        [TestMethod]
        public async Task GridInsideAutoScrollingStackPanel_ColumnWidthsStayStable_AcrossAFocusCascade()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Auto,
                Width = 400,
                Height = 200,
            };
            ui.AddChild(stack);

            var grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Top,
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            stack.AddChild(grid);

            ComboBox targetComboBox = null;
            for (int row = 0; row < 15; row++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });
                grid.AddChild(new Label { Text = $"Prop{row}" }, 0, row);

                if (row == 5)
                {
                    targetComboBox = new ComboBox { ItemsSource = new System.Collections.Generic.List<string> { "A", "B" }, SelectedItem = "A" };
                    grid.AddChild(targetComboBox, 1, row);
                }
                else
                {
                    grid.AddChild(new Label { Text = $"value{row}" }, 1, row);
                }
            }

            uiManager.AddUI(ui);
            for (int i = 0; i < 5; i++)
            {
                await uiManager.PerformLayout();
            }

            Assert.IsTrue(stack.IsVerticalScrollbarVisible, "Test setup error - expected content to overflow and the scrollbar to be visible.");

            float settledWidth = grid.ColumnDefinitions[0].ActualWidth;

            // A focus event cascades HasFocus=true into every descendant of the ComboBox - a
            // much broader invalidation than a single control's hover flip - which is exactly
            // the kind of broad invalidation that exposed the double-reduction bug (see the
            // class comment). With the real fix (GetChildBoundingBox no longer trusting the
            // doubly-written FinalContentRect field), the width should never change at all here,
            // not just stop oscillating once destabilized.
            for (int i = 0; i < 10; i++)
            {
                bool focused = (i % 2) == 0;
                await targetComboBox.HandleFocusChangedEventAsync(ui, new UIFocusChangedEvent { Focused = focused });
                await uiManager.PerformLayout();

                Assert.AreEqual(settledWidth, grid.ColumnDefinitions[0].ActualWidth,
                    $"Expected the Grid's column width to stay exactly stable across a focus cascade (iteration {i}, focused={focused}), not jump or ping-pong as the scrollbar-reserved width gets recomputed inconsistently.");
            }
        }
    }
}
