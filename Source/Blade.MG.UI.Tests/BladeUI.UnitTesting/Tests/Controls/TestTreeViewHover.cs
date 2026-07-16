using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Events;
using Blade.MG.UI.Models;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// Investigates the reported bug: hovering the TreeView works, but hovering the expand/
    /// collapse icon button nested inside a TreeNodeTemplate makes the row's hover styling
    /// disappear, and the expand/collapse arrow doesn't visually flip until the mouse leaves
    /// and re-enters the TreeView.
    /// </summary>
    [TestClass]
    public class TestTreeViewHover
    {
        private static (UIWindow ui, FakeUIManager uiManager, TreeView treeView, TreeNode rootNode) BuildTree(bool startExpanded = false)
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            // GameStudio's real ScreenDesignerTab.RefreshHierarchy sets IsExpanded = true on
            // essentially every node it creates - matching that here, since it means label0's
            // Transform is actually rotated 45 degrees (TreeNodeTemplate.RenderControl), unlike
            // this test's previous default (IsExpanded = false, unrotated icon).
            var rootNode = new TreeNode { Text = "Root", IsExpanded = startExpanded };
            rootNode.AddChild(new TreeNode { Text = "Child", IsExpanded = startExpanded });

            var treeView = new TreeView
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                RootNode = rootNode,
                ShowRootNode = true,
            };
            ui.AddChild(treeView);

            uiManager.AddUI(ui);
            uiManager.PerformLayout();
            uiManager.PerformLayout();

            return (ui, uiManager, treeView, rootNode);
        }

        private static void AssertSelectFirstPicksNodeOverButtonAndLabel(UIWindow ui, TreeNodeTemplate nodeTemplate, string label)
        {
            Rectangle buttonRect = nodeTemplate.button1.GetFinalRect();
            Point overButtonIcon = buttonRect.Center;

            Rectangle labelRect = nodeTemplate.label1.GetFinalRect();
            Point overLabel = labelRect.Center;

            bool SelectorAt(UIComponent component, UIComponent parent, Point point) =>
                component.IsHitTestVisible &&
                component.CanHover &&
                component.Visible.Value == Visibility.Visible &&
                component.ParentWindow?.Visible?.Value == Visibility.Visible &&
                component.ContainsScreenPoint(point);

            Assert.IsTrue(buttonRect.Width > 0 && buttonRect.Height > 0, $"[{label}] button1 rect looks degenerate: {buttonRect}");
            Assert.IsTrue(labelRect.Width > 0 && labelRect.Height > 0, $"[{label}] label1 rect looks degenerate: {labelRect}");
            Assert.AreNotEqual(overButtonIcon, overLabel, $"[{label}] button and label centers landed on the exact same point - geometry is degenerate. button rect={buttonRect}, label rect={labelRect}");

            UIComponent selectedOverButton = ui.SelectFirst((c, p) => SelectorAt(c, p, overButtonIcon), true, overButtonIcon);
            UIComponent selectedOverLabel = ui.SelectFirst((c, p) => SelectorAt(c, p, overLabel), true, overLabel);

            Assert.AreSame(nodeTemplate, selectedOverLabel, $"[{label}] Expected the label area to select the TreeNodeTemplate itself, got {selectedOverLabel?.GetType().Name} (rect={selectedOverLabel?.GetFinalRect()}). Node rect={nodeTemplate.GetFinalRect()}, button rect={buttonRect}, label rect={labelRect}.");
            Assert.AreSame(nodeTemplate, selectedOverButton, $"[{label}] Expected the expand-button icon area to ALSO select the TreeNodeTemplate itself (button1/label0 opt out of hover), got {selectedOverButton?.GetType().Name} (rect={selectedOverButton?.GetFinalRect()}). Node rect={nodeTemplate.GetFinalRect()}, button rect={buttonRect}, label rect={labelRect}.");
        }

        [TestMethod]
        public void SelectFirst_OverExpandButtonIcon_StillSelectsTheNodeTemplate()
        {
            var (ui, uiManager, treeView, rootNode) = BuildTree(startExpanded: true);

            var rootTemplate = treeView.Children.OfType<TreeNodeTemplate>().Single(t => t.DataContext == rootNode);

            // RenderControl (where label0's rotation actually gets set) only runs on an actual
            // draw call - PerformLayout alone (Measure/Arrange) never touches Transform.
            var graphicsDevice = FakeGame.Instance.GraphicsDevice;
            using (var renderTarget = new RenderTarget2D(graphicsDevice, 400, 300))
            using (var spriteBatch = new SpriteBatch(graphicsDevice))
            {
                graphicsDevice.SetRenderTarget(renderTarget);
                uiManager.Draw(spriteBatch, new GameTime(), renderTarget);
                graphicsDevice.SetRenderTarget(null);
            }

            Assert.AreNotEqual(0f, rootTemplate.label0.Transform.Rotation.Z, "Test setup error - expected label0 to actually be rotated (matching GameStudio's IsExpanded=true usage), so this test exercises the same geometry as the real bug report.");
            AssertSelectFirstPicksNodeOverButtonAndLabel(ui, rootTemplate, "root node (depth 0)");

            // Expand the root so the child row becomes a real, arranged child too - the child
            // sits at an indented depth (TreeView.ArrangeNode sets nodeTemplate.Padding.Left =
            // NodeIndentPerLevel * depth), which is the layout path the root (depth 0, no
            // indent) never exercises.
            rootNode.IsExpanded = true;
            uiManager.PerformLayout();
            uiManager.PerformLayout();

            var childNode = rootNode.Children.Single();
            var childTemplate = treeView.Children.OfType<TreeNodeTemplate>().Single(t => t.DataContext == childNode);
            AssertSelectFirstPicksNodeOverButtonAndLabel(ui, childTemplate, "child node (depth 1, indented)");
        }

        /// <summary>
        /// GameStudio's real hierarchy tree is height-constrained (220px row in a Grid) and
        /// typically has many sibling/nested nodes, all IsExpanded = true - unlike the earlier
        /// tests here (an unconstrained 800x600 window with 1-2 nodes). Reproduces that more
        /// closely: a small viewport with several expanded siblings, each with their own
        /// children, to check whether hit-testing a MIDDLE row's button still resolves
        /// correctly once virtualization/scrolling is actually in play.
        /// </summary>
        [TestMethod]
        public void SelectFirst_OverExpandButtonIcon_InConstrainedScrollingTree_StillSelectsTheNodeTemplate()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var rootNode = new TreeNode { Text = "Root", IsExpanded = true };
            for (int i = 0; i < 10; i++)
            {
                var child = new TreeNode { Text = $"Node {i}", IsExpanded = true };
                child.AddChild(new TreeNode { Text = $"Node {i}.0" });
                child.AddChild(new TreeNode { Text = $"Node {i}.1" });
                rootNode.AddChild(child);
            }

            // Mirrors ScreenDesignerTab.BuildInspectorColumn exactly: a Grid row constrains the
            // TreeView's height, and the TreeView itself uses pure Stretch/Stretch (no explicit
            // Width/Height) - unlike this test's first attempt (explicit Width=300 conflicting
            // with HorizontalAlignment.Stretch), which triggers ArrangeSelf's Stretch-as-Center
            // fallback (UIComponent.cs:587) and isn't how GameStudio actually uses TreeView.
            var column = new Grid { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            // Extra leading column/row (like the palette column and toolbar row in
            // ScreenDesignerTab) so the TreeView's own Grid cell is offset away from the
            // window's (0,0) origin - the scenario that actually matters for this bug.
            column.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 150) });
            column.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 250) });
            column.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 40) });
            column.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 220) });
            ui.AddChild(column);

            var treeView = new TreeView
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                RootNode = rootNode,
                ShowRootNode = true,
                VerticalScrollBarVisible = ScrollBarVisibility.Auto,
            };
            column.AddChild(treeView, 1, 1);

            uiManager.AddUI(ui);
            uiManager.PerformLayout();
            uiManager.PerformLayout();
            uiManager.PerformLayout();

            // Pick whichever currently-virtualized-in row sits roughly in the middle of the
            // visible set (not the very first/last, edge-adjacent row) - most representative of
            // "just hovering a normal row" rather than one right at the scroll boundary.
            var visibleTemplates = treeView.Children.OfType<TreeNodeTemplate>().ToList();
            Rectangle treeViewRect = treeView.GetFinalRect();
            Assert.IsTrue(visibleTemplates.Count >= 3, $"Test setup error - expected several rows virtualized-in given a 220px-tall viewport with 31 total nodes, got {visibleTemplates.Count}. TreeView rect={treeViewRect}, rows: {string.Join(", ", visibleTemplates.Select(t => t.GetFinalRect().ToString()))}");

            foreach (var t in visibleTemplates)
            {
                Rectangle r = t.GetFinalRect();
                Assert.IsTrue(Rectangle.Intersect(r, treeViewRect) != Rectangle.Empty,
                    $"BUG: row '{((TreeNode)t.DataContext).Text}' rect={r} does not intersect TreeView's own rect={treeViewRect} at all, yet virtualization kept it in Children - it can never be hit-tested/hovered.");
            }

            var middleTemplate = visibleTemplates[visibleTemplates.Count / 2];

            AssertSelectFirstPicksNodeOverButtonAndLabel(ui, middleTemplate, "middle row in a constrained, scrolling tree, offset inside a Grid cell");
        }

        /// <summary>
        /// TreeNodeTemplate is a Border (always cached - Border's constructor sets
        /// EnableCaching = true unconditionally), and its RenderControl override is where
        /// label0's rotation gets flipped based on treeNode.IsExpanded - but RenderControl is
        /// skipped entirely whenever the cached texture is blitted instead (see
        /// RenderChildOrFromCache). ITreeNode.IsExpanded is a plain bool, not a Binding&lt;T&gt;,
        /// so toggling it never raised a Changed event and never invalidated the cache on its
        /// own - the arrow only ever updated as a side effect of some unrelated Binding change
        /// (e.g. hover) happening to invalidate the same cache. Clicking the expand button now
        /// explicitly calls InvalidateCache() (see ToggleExpanded), so this asserts the arrow's
        /// rotation is already correct on the very next render, with no hover event in between.
        /// </summary>
        [TestMethod]
        public void ClickingExpandButton_RotatesArrow_WithoutAnyHoverEventInBetween()
        {
            var (ui, uiManager, treeView, rootNode) = BuildTree();

            var rootTemplate = treeView.Children.OfType<TreeNodeTemplate>().Single(t => t.DataContext == rootNode);

            var graphicsDevice = FakeGame.Instance.GraphicsDevice;
            using var renderTarget = new RenderTarget2D(graphicsDevice, 400, 300);
            using var spriteBatch = new SpriteBatch(graphicsDevice);

            void RenderOnce()
            {
                graphicsDevice.SetRenderTarget(renderTarget);
                graphicsDevice.Clear(Color.Black);
                uiManager.Draw(spriteBatch, new GameTime(), renderTarget);
                graphicsDevice.SetRenderTarget(null);
            }

            // First render populates the row's cache with the (collapsed) arrow orientation.
            RenderOnce();
            Assert.AreEqual(0f, rootTemplate.label0.Transform.Rotation.Z, 0.0001f, "Expected the arrow to start unrotated (collapsed).");

            // Click the expand button - exactly what button1.OnMouseDown does in production.
            rootTemplate.button1.OnMouseDown.Invoke(rootTemplate.button1, new UIMouseDownEvent());
            Assert.IsTrue(rootNode.IsExpanded, "Test setup error - clicking the button should have toggled IsExpanded.");

            // Deliberately no hover enter/leave event here - if the cache weren't explicitly
            // invalidated, RenderControl (and therefore the rotation update) would be skipped,
            // and this assertion would still see the stale, unrotated arrow.
            RenderOnce();
            Assert.AreNotEqual(0f, rootTemplate.label0.Transform.Rotation.Z, "Expected the arrow to have rotated on the very next render after expanding, with no other event forcing a cache refresh.");
        }

        /// <summary>
        /// UIComponent.HandleHoverChangedAsync's recursive descent used to set "MouseHover"
        /// (with no CanHover gate) once per geometrically-contained child it recursed into -
        /// unqualified, so inside e.g. button1's own dispatch (this = button1), that line set
        /// button1.MouseHover = true for every matching descendant it walked into (ButtonBaseTemplate,
        /// label0), completely bypassing button1.CanHover = false. Harmless in practice (nothing
        /// reads Button.MouseHover for rendering) but definitely wrong; fixed by removing the
        /// redundant, unqualified line entirely - each component's own dispatch already sets its
        /// own MouseHover correctly (gated by its own CanHover) at the end of its own call.
        /// </summary>
        [TestMethod]
        public async Task HoverEnter_OverExpandButton_DoesNotSetMouseHoverOnNonHoverableButton()
        {
            var (ui, uiManager, treeView, rootNode) = BuildTree();

            var rootTemplate = treeView.Children.OfType<TreeNodeTemplate>().Single(t => t.DataContext == rootNode);

            Assert.IsFalse(rootTemplate.button1.CanHover, "Test setup error - button1 should opt out of hover (see TreeNodeTemplate.InitTemplate).");
            Assert.IsFalse(rootTemplate.button1.MouseHover.Value, "Test setup error - button1.MouseHover should start false.");

            // Drive the same dispatch RaiseHoverEnterEventAsync would (Hover=true at the button's
            // own position) directly, since RaiseHoverEnterEventAsync itself reads X/Y from the
            // live hardware mouse (InputManager.Mouse.Position), which a headless test can't set.
            Point overButtonIcon = rootTemplate.button1.GetFinalRect().Center;
            await rootTemplate.HandleHoverChangedAsync(ui, new UIHoverChangedEvent { Hover = true, X = overButtonIcon.X, Y = overButtonIcon.Y });

            Assert.IsFalse(rootTemplate.button1.MouseHover.Value, "button1.CanHover is false, so hovering over it should never flip its own MouseHover true, even though it's geometrically under the cursor.");
            Assert.IsTrue(rootTemplate.MouseHover.Value, "The row itself should still correctly be marked as hovered.");
        }
    }
}
