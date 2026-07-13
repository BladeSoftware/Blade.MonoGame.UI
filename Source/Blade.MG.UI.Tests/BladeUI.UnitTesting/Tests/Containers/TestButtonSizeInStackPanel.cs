using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BladeUI.UnitTesting.Tests.Containers
{
    [TestClass]
    public class TestButtonSizeInStackPanel
    {
        [TestMethod]
        public void Button_AsOnlyChildOfVerticalStackPanel_SizesToContentNotToParent()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var canvasHost = new Panel { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch, IsHitTestVisible = true };
            ui.AddChild(canvasHost);

            var root = new StackPanel
            {
                Name = "Root",
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                IsHitTestVisible = true,
                Padding = new Thickness(8),
            };
            canvasHost.AddChild(root);

            uiManager.AddUI(ui);
            uiManager.PerformLayout();

            var button = new Button { Text = "Button" };
            root.AddChild(button);

            uiManager.PerformLayout();

            // A vertical StackPanel must size each child to its own content height in the stack
            // direction, regardless of the child's VerticalAlignment=Stretch (the framework-wide
            // default) - otherwise a single button in an otherwise-empty design would balloon to
            // fill the whole canvas height instead of sitting as a normal-sized row.
            var buttonRect = button.GetFinalRect();
            var rootContentWidth = root.GetFinalRect().Width - root.Padding.Value.Left - root.Padding.Value.Right;
            Assert.IsTrue(buttonRect.Height is > 0 and < 100, $"Expected a normal button-sized height (well under 100px), got {buttonRect.Height}.");
            Assert.AreEqual(rootContentWidth, buttonRect.Width, $"Expected the button to stretch to the root's full content width (cross-axis stretch), got {buttonRect}.");
        }
    }
}
