using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Models;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// UIComponent's Measure/Arrange dirty-flagging skip (see TestLayoutDirtyFlagging) is safe
    /// for TreeNodeTemplate (a Border subclass, so it funnels through the same shared
    /// MeasureSelf/ArrangeSelf choke points) because TreeView routes every per-node state change
    /// that matters - indentation - through a proper Binding&lt;T&gt; assignment (Padding, updated
    /// once per Arrange in ArrangeNode) that bubbles via BubbleInvalidation, not a direct field
    /// poke. Note TreeNodeTemplate.label1.Text is actually a one-time snapshot of the node's Text
    /// taken in InitTemplate (`Text = treeNode?.Text ?? "null"`) - it never re-syncs afterward,
    /// so a later change to the underlying ITreeNode.Text does not update the rendered label at
    /// all; that's a pre-existing limitation unrelated to this skip mechanism, not something to
    /// guard against here.
    /// </summary>
    [TestClass]
    public class TestTreeViewLayoutDirtyFlagging
    {
        private static IEnumerable<UIComponent> Walk(UIComponent root)
        {
            if (root == null) yield break;
            yield return root;
            foreach (var child in root.PrivateControls)
            {
                foreach (var d in Walk(child)) yield return d;
            }
            if (root is Control control && control.Content != null)
            {
                foreach (var d in Walk(control.Content)) yield return d;
            }
            if (root is Container container)
            {
                foreach (var child in container.Children)
                {
                    foreach (var d in Walk(child)) yield return d;
                }
            }
        }

        [TestMethod]
        public async Task DifferentSiblingNodes_KeepDistinctDesiredSizes_AcrossManyFramesOfSkipping()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var rootNode = new TreeNode { Text = "Root", IsExpanded = true };
            var shortNode = new TreeNode { Text = "A", IsExpanded = true };
            var longNode = new TreeNode { Text = "A much longer node label than the other one", IsExpanded = true };
            rootNode.AddChild(shortNode);
            rootNode.AddChild(longNode);

            var treeView = new TreeView
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                RootNode = rootNode,
                ShowRootNode = true,
            };
            ui.AddChild(treeView);

            uiManager.AddUI(ui);
            for (int i = 0; i < 5; i++)
            {
                await uiManager.PerformLayout();
            }

            var labels = Walk(treeView).OfType<TreeNodeTemplate>().Select(t => t.label1).ToList();
            var shortLabel = labels.Single(l => l.Text.Value == "A");
            var longLabel = labels.Single(l => l.Text.Value == "A much longer node label than the other one");

            // Each node's own DesiredSize must still reflect its own (creation-time) text after
            // several frames of skipping - not accidentally shared/overwritten by a sibling
            // visited in the same Arrange pass.
            Assert.IsTrue(longLabel.DesiredSize.Width > shortLabel.DesiredSize.Width,
                $"Expected the longer node's label to measure wider than the short one across repeated frames, got short={shortLabel.DesiredSize.Width}, long={longLabel.DesiredSize.Width}.");
        }

        [TestMethod]
        public async Task RepeatedlyCollapsingAndExpanding_KeepsVisitedNodeCountCorrectEachFrame()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var rootNode = new TreeNode { Text = "Root", IsExpanded = true };
            var collapsibleNode = new TreeNode { Text = "Collapsible", IsExpanded = true };
            var grandchild1 = new TreeNode { Text = "Child 1", IsExpanded = true };
            var grandchild2 = new TreeNode { Text = "Child 2", IsExpanded = true };
            collapsibleNode.AddChild(grandchild1);
            collapsibleNode.AddChild(grandchild2);
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

            // Expanded: Root + Collapsible + 2 grandchildren = 4 nodes visited.
            Assert.AreEqual(4, treeView.LastArrangedNodeCount, "Test setup error - expected all 4 nodes visited while fully expanded.");

            for (int i = 0; i < 3; i++)
            {
                collapsibleNode.IsExpanded = false;
                await uiManager.PerformLayout();
                await uiManager.PerformLayout(); // extra settle frame between toggles

                Assert.AreEqual(2, treeView.LastArrangedNodeCount, $"Expected only Root + Collapsible to be visited once collapsed (iteration {i}), but the skip left stale data from before.");

                collapsibleNode.IsExpanded = true;
                await uiManager.PerformLayout();
                await uiManager.PerformLayout();

                Assert.AreEqual(4, treeView.LastArrangedNodeCount, $"Expected all 4 nodes visited again after re-expanding (iteration {i}), but the skip left stale data from before.");
            }
        }
    }
}
