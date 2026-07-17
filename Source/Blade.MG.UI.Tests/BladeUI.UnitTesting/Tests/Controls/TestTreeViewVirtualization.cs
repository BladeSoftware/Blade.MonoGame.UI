using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Models;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// TreeView.MeasureNode/ArrangeNode used to recurse into every descendant of a node
    /// regardless of whether that node (or any ancestor) was collapsed - only the collapsed
    /// node's own contribution to layout was skipped, not the walk into its children. A single
    /// collapsed node with thousands of hidden descendants still cost O(descendants) to visit
    /// every frame. Fixed by returning immediately once `collapsed` is true, since a collapsed
    /// node guarantees every descendant is collapsed too - nothing under it needs visiting at
    /// all. GetNodeTemplate/GetExistingNodeTemplate were also switched from an O(children) LINQ
    /// scan (comparing GetHashCode(), not even proper equality) to a Dictionary lookup.
    /// </summary>
    [TestClass]
    public class TestTreeViewVirtualization
    {
        private static TreeNode BuildDeepChain(int depth)
        {
            var root = new TreeNode { Text = "Root", IsExpanded = true };
            var current = root;
            for (int i = 0; i < depth; i++)
            {
                var child = new TreeNode { Text = $"Node {i}", IsExpanded = true };
                current.AddChild(child);
                current = child;
            }
            return root;
        }

        [TestMethod]
        public async Task CollapsingANodeWithManyDescendants_StopsArrangeFromVisitingThem()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            // A long chain (not wide) so depth, not breadth, drives the descendant count -
            // 500 nested nodes under a single collapsible node.
            var hiddenSubtree = BuildDeepChain(500);

            var rootNode = new TreeNode { Text = "Root", IsExpanded = true };
            var collapsibleNode = new TreeNode { Text = "Collapsible", IsExpanded = true };
            collapsibleNode.AddChild(hiddenSubtree);
            rootNode.AddChild(collapsibleNode);

            var treeView = new TreeView
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                RootNode = rootNode,
                ShowRootNode = true,
            };
            ui.AddChild(treeView);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();
            await uiManager.PerformLayout();

            // Expanded: everything is visited (Root + Collapsible + 500-node chain + the
            // synthetic wrapper node = 503 nodes).
            Assert.IsTrue(treeView.LastArrangedNodeCount > 400, $"Test setup error - expected the fully-expanded tree to visit hundreds of nodes, only visited {treeView.LastArrangedNodeCount}.");

            // Now collapse the one node guarding the entire 500-deep chain.
            collapsibleNode.IsExpanded = false;
            await uiManager.PerformLayout();
            await uiManager.PerformLayout();

            Assert.IsTrue(treeView.LastArrangedNodeCount < 10, $"Expected collapsing 'Collapsible' to stop the Arrange walk from descending into its 500-deep hidden chain, but it still visited {treeView.LastArrangedNodeCount} nodes.");
        }
    }
}
