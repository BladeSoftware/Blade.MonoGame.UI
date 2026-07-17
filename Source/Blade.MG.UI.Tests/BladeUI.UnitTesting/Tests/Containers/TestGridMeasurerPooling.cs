using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Containers
{
    /// <summary>
    /// Grid.Measure used to allocate a brand-new GridMeasurer (plus one GridMeasurerSizing per
    /// column/row) on every single Measure pass, even when the column/row definitions hadn't
    /// changed since the previous frame - pure per-frame garbage for every Grid on screen, every
    /// frame. GridMeasurer.Reset now reuses the existing Measurables array/objects in place when
    /// the count matches (the overwhelmingly common steady-state case), only reallocating when
    /// the column/row count actually changes.
    /// </summary>
    [TestClass]
    public class TestGridMeasurerPooling
    {
        [TestMethod]
        public async Task Grid_WithStableColumnCount_ProducesConsistentWidthsAcrossRepeatedMeasures()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Width = 300,
                Height = 100,
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 2f) });
            ui.AddChild(grid);

            uiManager.AddUI(ui);

            for (int i = 0; i < 5; i++)
            {
                await uiManager.PerformLayout();

                Assert.AreEqual(100f, grid.ColumnDefinitions[0].ActualWidth, 0.01f,
                    $"Expected the 1-weight star column's width to stay correct across repeated Measure passes reusing the pooled GridMeasurer (iteration {i}).");
                Assert.AreEqual(200f, grid.ColumnDefinitions[1].ActualWidth, 0.01f,
                    $"Expected the 2-weight star column's width to stay correct across repeated Measure passes reusing the pooled GridMeasurer (iteration {i}).");
            }
        }

        [TestMethod]
        public async Task Grid_WithAutoColumn_ShrinksWidthAfterChildShrinks_NotStuckAtStalePooledValue()
        {
            // An Auto column's CalcSize must be reset to NaN each Measure pass (see
            // GridMeasurerSizing.Reset) - GridMeasurer.MeasureChild reads the column's *current*
            // CalcSize as its starting point for that frame's max-child-width accumulation. If a
            // pooled GridMeasurerSizing kept a stale CalcSize from the previous frame instead of
            // resetting it, a column whose content shrinks would incorrectly stay pinned at its
            // old (larger) width forever, since the stale value would outweigh the new, smaller
            // desired size in that accumulation.
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Auto) });

            var child = new Label { Width = 200, Height = 20 };
            grid.AddChild(child, 0, 0);
            ui.AddChild(grid);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            Assert.AreEqual(200f, grid.ColumnDefinitions[0].ActualWidth, 0.01f,
                "Test setup error - expected the Auto column to size to the child's initial 200px width.");

            child.Width = 50;
            await uiManager.PerformLayout();

            Assert.AreEqual(50f, grid.ColumnDefinitions[0].ActualWidth, 0.01f,
                "Expected the Auto column to shrink to match the child's new, smaller width - not stay stuck at the previous frame's pooled CalcSize.");
        }

        [TestMethod]
        public async Task Grid_WithColumnCountChangingBetweenFrames_StillProducesCorrectWidths()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Width = 300,
                Height = 100,
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            ui.AddChild(grid);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            Assert.AreEqual(150f, grid.ColumnDefinitions[0].ActualWidth, 0.01f);
            Assert.AreEqual(150f, grid.ColumnDefinitions[1].ActualWidth, 0.01f);

            // Column count changes between Measure passes (e.g. a dynamically-built property
            // grid adding a column) - this must hit GridMeasurer.Reset's reallocation branch
            // rather than reusing a Measurables array sized for the old column count.
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            await uiManager.PerformLayout();

            Assert.AreEqual(100f, grid.ColumnDefinitions[0].ActualWidth, 0.01f,
                "Expected widths to be recalculated correctly for the new 3-column layout after the column count changed between frames.");
            Assert.AreEqual(100f, grid.ColumnDefinitions[1].ActualWidth, 0.01f);
            Assert.AreEqual(100f, grid.ColumnDefinitions[2].ActualWidth, 0.01f);
        }
    }
}
