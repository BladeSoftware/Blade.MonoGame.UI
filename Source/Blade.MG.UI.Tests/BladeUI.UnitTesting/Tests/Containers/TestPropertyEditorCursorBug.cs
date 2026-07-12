using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Containers
{
    [TestClass]
    public class TestPropertyEditorCursorBug
    {
        // Recursively walks PrivateControls (internal template pieces), Control.Content, and
        // Container.Children - the same three edges SetParentWindow/PropagateAsync use - so
        // nothing in the real tree is missed.
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
        public async Task BlankSpaceClick_InPalette_BelowLastButton_DoesNotFocusEverything()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            // Mirrors MainWindowUI: the ScreenDesignerTab's content isn't added to the UIWindow
            // directly - it's the active page of a TabPanel (mainWindowDockPanel.CenterPanel ->
            // tabPanel -> AddTab(...)), which is a plain Container (not Panel), so unlike Panel
            // it never opts itself out of focus.
            var tabPanel = new TabPanel { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            ui.AddChild(tabPanel);
            uiManager.AddUI(ui);
            uiManager.PerformLayout();

            var outerGrid = new Grid { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            outerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 40) });
            outerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
            tabPanel.AddTab(outerGrid, "ScreenDesigner", setAsActiveTab: true);

            var toolbar = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            outerGrid.AddChild(toolbar, 0, 0);

            var contentGrid = new Grid { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 170) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 320) });
            outerGrid.AddChild(contentGrid, 0, 1);

            // Mirrors BuildPalette: a vertical StackPanel of Buttons, one per palette entry.
            var paletteStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Padding = new Thickness(6),
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Auto,
            };
            var paletteNames = new[] { "Border", "Button", "CheckBox", "ComboBox", "Label", "TextBox" };
            var paletteButtons = new List<Button>();
            foreach (var name in paletteNames)
            {
                var pb = new Button { Text = name, HorizontalAlignment = HorizontalAlignmentType.Stretch, Margin = new Thickness(0, 0, 0, 6) };
                paletteStack.AddChild(pb);
                paletteButtons.Add(pb);
            }
            contentGrid.AddChild(paletteStack, 0, 0);

            var canvasHost = new Panel { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch, IsHitTestVisible = true };
            contentGrid.AddChild(canvasHost, 1, 0);

            var designRoot = new StackPanel
            {
                Name = "Root",
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                IsHitTestVisible = true,
                Padding = new Thickness(8),
            };
            canvasHost.AddChild(designRoot);

            var canvasButton = new Button { Text = "Button", Name = "CanvasButton" };
            designRoot.AddChild(canvasButton);

            // Mirrors BuildInspectorColumn: a 220px TreeView row + a * PropertyEditor row.
            var inspectorColumn = new Grid { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            inspectorColumn.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 220) });
            inspectorColumn.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });

            var hierarchyTree = new TreeView
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                ShowRootNode = true,
                VerticalScrollBarVisible = ScrollBarVisibility.Auto,
            };
            inspectorColumn.AddChild(hierarchyTree, 0, 0);

            var inspector = new PropertyEditor
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };
            inspectorColumn.AddChild(inspector, 0, 1);

            contentGrid.AddChild(inspectorColumn, 2, 0);

            uiManager.PerformLayout();

            // Select the canvas button so the property editor actually builds a full grid of
            // TextBox rows, matching the screenshot (property grid populated when the bug hit).
            inspector.TargetObject = canvasButton;
            uiManager.PerformLayout();

            var propertyTextBoxes = Walk(inspector).OfType<TextBox>().ToList();
            Assert.IsTrue(propertyTextBoxes.Count > 0, "Expected the property editor to have built at least one TextBox row.");

            // "A few pixels below the [last] palette button" - still inside paletteStack's own
            // bounds (its Padding gutter / the empty tail of the StackPanel), but not on top of
            // any actual Button.
            Rectangle lastButtonRect = paletteButtons[^1].GetFinalRect();
            Rectangle paletteRect = paletteStack.GetFinalRect();
            Point blankPoint = new Point(paletteRect.Center.X, lastButtonRect.Bottom + 3);
            Assert.IsTrue(paletteRect.Contains(blankPoint), $"Test setup error - blankPoint {blankPoint} isn't even inside paletteRect {paletteRect}, adjust the layout.");
            foreach (var pb in paletteButtons)
            {
                Assert.IsFalse(pb.GetFinalRect().Contains(blankPoint), $"Test setup error - blankPoint {blankPoint} unexpectedly lands on palette button '{pb.Text.Value}' ({pb.GetFinalRect()}).");
            }

            // Exact replica of the focus-selection logic in UIManager.Mouse.cs's mouse-down
            // handler, so this exercises the real SelectFirst/RaiseFocusChangedEventAsync code
            // path rather than a hand-rolled approximation.
            bool Selector(UIComponent component, UIComponent parent) =>
                component.IsHitTestVisible &&
                component.CanFocus &&
                component.Visible.Value == Visibility.Visible &&
                component.ParentWindow?.Visible?.Value == Visibility.Visible &&
                component.ContainsScreenPoint(blankPoint);

            UIComponent focusComponent = ui.SelectFirst(Selector, true, blankPoint);

            if (focusComponent != null)
            {
                UIWindow focusUIWindow = focusComponent.ParentWindow;
                await focusUIWindow.RaiseFocusChangedEventAsync(focusComponent, focusUIWindow);
            }

            var allFocused = Walk(ui).Where(c => c.HasFocus.Value).ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"focusComponent selected by SelectFirst: {focusComponent?.GetType().Name ?? "<null>"} '{(focusComponent as Control)?.Name}' rect={focusComponent?.GetFinalRect()}");
            sb.AppendLine($"Total components with HasFocus=true after the click: {allFocused.Count}");
            foreach (var c in allFocused)
            {
                sb.AppendLine($"  - {c.GetType().Name} '{(c as Control)?.Name}' rect={c.GetFinalRect()}");
            }

            Assert.IsTrue(allFocused.Count <= 1, $"Expected at most one focused component after clicking blank space, got {allFocused.Count}.\n{sb}");
        }
    }
}
