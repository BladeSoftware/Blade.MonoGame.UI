using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// ListView.Arrange used to Measure/Arrange every item in DataContext every frame just to
    /// find out which ones actually land in the viewport, and look up existing item templates
    /// via an O(children) LINQ scan comparing GetHashCode() (not even proper equality) - both
    /// scale with total item count, not visible item count. For a large list this meant a
    /// per-frame cost proportional to the whole dataset rather than the viewport, which won't
    /// sustain high framerates. Fixed by estimating the visible index range directly from
    /// ScrollOffset and a running-average row height, and by keying template lookup off a
    /// Dictionary&lt;object, UIComponent&gt; instead of scanning Children.
    /// </summary>
    [TestClass]
    public class TestListViewVirtualization
    {
        private static List<string> BuildItems(int count) =>
            Enumerable.Range(0, count).Select(i => $"Item {i}").ToList();

        [TestMethod]
        public async Task LargeList_OnlyCreatesTemplatesForVisibleItems()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var items = BuildItems(2000);

            var listView = new ListView
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                Height = 200, // Small viewport relative to 2000 items.
                DataContext = items,
            };
            ui.AddChild(listView);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();
            await uiManager.PerformLayout();

            // A 200px-tall viewport with ~32px rows should need on the order of ten real
            // children, not 2000 - the whole point of virtualization.
            Assert.IsTrue(listView.Children.Count < 50, $"Expected only a small, viewport-sized set of real children, got {listView.Children.Count} (out of {items.Count} total items) - virtualization isn't working.");
            Assert.IsTrue(listView.Children.Count > 0, "Expected at least some children to be created for a non-empty, visible list.");

            // The items actually realized should start from the top of the list (no scrolling
            // has happened yet).
            var firstItem = listView.Children.OfType<UIComponent>().Select(c => c.DataContext as string).Where(s => s != null).OrderBy(s => int.Parse(s.Split(' ')[1])).First();
            Assert.AreEqual("Item 0", firstItem, "Expected the topmost realized item to be the first item in the list before any scrolling.");
        }

        [TestMethod]
        public async Task LargeList_ScrollingToMiddle_RealizesItemsNearThatIndex()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var items = BuildItems(2000);

            var listView = new ListView
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                Height = 200,
                DataContext = items,
            };
            ui.AddChild(listView);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();
            await uiManager.PerformLayout();

            // Scroll roughly to the middle of the list.
            listView.VerticalScrollBar.ScrollOffset = listView.VerticalScrollBar.MaxValue / 2;
            await uiManager.PerformLayout();
            await uiManager.PerformLayout();

            Assert.IsTrue(listView.Children.Count < 50, $"Expected the realized-child count to stay small after scrolling, got {listView.Children.Count}.");

            var realizedIndices = listView.Children
                .Select(c => c.DataContext as string)
                .Where(s => s != null)
                .Select(s => int.Parse(s.Split(' ')[1]))
                .OrderBy(i => i)
                .ToList();

            Assert.IsTrue(realizedIndices.Count > 0, "Expected at least some items to be realized after scrolling.");

            // Roughly the middle of the dataset should now be realized - not still item 0, and
            // not off the end either.
            Assert.IsTrue(realizedIndices.Min() > 200, $"Expected realized items to have scrolled well past the start of the list, lowest realized index was {realizedIndices.Min()}.");
            Assert.IsTrue(realizedIndices.Max() < 1800, $"Expected realized items to still be within the list, highest realized index was {realizedIndices.Max()}.");
        }

        /// <summary>
        /// The resulting Children set alone doesn't prove virtualization is actually limiting
        /// per-frame work - the pre-existing intersect-based Add/Remove check already produced a
        /// small, correct Children set even when every item was Measured/Arranged first to find
        /// out which ones qualified. This asserts on LastArrangedItemCount directly (how many
        /// items Arrange actually touched this pass), which is what the fix changes: it should
        /// scale with the viewport, not with a 10,000-item dataset.
        /// </summary>
        [TestMethod]
        public async Task HugeList_ArrangePassOnlyTouchesViewportSizedItemCount()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var items = BuildItems(10000);

            var listView = new ListView
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                Height = 200,
                DataContext = items,
            };
            ui.AddChild(listView);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();
            await uiManager.PerformLayout();

            Assert.IsTrue(listView.LastArrangedItemCount > 0, "Expected at least some items to have been arranged.");
            Assert.IsTrue(listView.LastArrangedItemCount < 50, $"Expected the last Arrange pass to have touched only a viewport-sized number of items, got {listView.LastArrangedItemCount} out of {items.Count} total - virtualization isn't limiting per-frame work.");
        }
    }
}
