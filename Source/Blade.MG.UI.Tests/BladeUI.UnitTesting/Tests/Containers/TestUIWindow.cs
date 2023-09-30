using Blade.UI;
using Blade.UI.Components;
using Blade.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Containers
{
    [TestClass]
    public class TestUIWindow
    {
        //private MockGame mockGame;

        public TestUIWindow()
        {
            // Create Resources to be shared by all unit tests in this class
        }


        [TestMethod]
        public async Task TestUIWindow_NoChildren()
        {
            FakeUIManager uiManager = new FakeUIManager();

            var ui = new EmptyUI();
            uiManager.AddUI(ui, FakeGame.Instance);

            // Do UI Layout
            await uiManager.PerformLayout();


            // Check Results
            var layoutRect = FakeGame.Instance.LayoutRectangle;

            var finalRect = ui.GetFinalRect();
            var finalContentRect = ui.GetFinalContentRect();

            // The empty UI should expand to fill the full layout space
            Assert.AreEqual(finalRect, layoutRect);
            Assert.AreEqual(finalContentRect, layoutRect);
        }


        [DataTestMethod()]
        [DataRow(0, 0, 0, 0, 0, 0, 0, 0)]
        [DataRow(3, 5, 7, 11, 0, 0, 0, 0)]
        [DataRow(0, 0, 0, 0, 13, 17, 19, 21)]
        [DataRow(3, 5, 7, 11, 13, 17, 19, 21)]
        public async Task TestUIWindow_NoChildren_WithMarginAndPadding(int marginLeft, int marginRight, int marginTop, int marginBottom, int paddingLeft, int paddingRight, int paddingTop, int paddingBottom)
        {
            var uiManager = new FakeUIManager();

            var ui = new EmptyUI();
            ui.Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom);
            ui.Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom);
            uiManager.AddUI(ui, FakeGame.Instance);

            // Do UI Layout
            await uiManager.PerformLayout();


            // Check Results
            var layoutRect = FakeGame.Instance.LayoutRectangle;

            var finalRect = ui.GetFinalRect();
            var finalContentRect = ui.GetFinalContentRect();


            // The empty UI should expand to fill the full layout space
            Rectangle expectedFinalRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom);
            Assert.AreEqual(finalRect, expectedFinalRect);

            Rectangle expectedFinalContentRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom, paddingLeft, paddingTop, paddingRight, paddingBottom);
            Assert.AreEqual(finalContentRect, expectedFinalContentRect);
        }


        [DataTestMethod()]
        // No Margin or Padding on Parent or Child
        [DataRow(0, 0, 0, 0, 0, 0, 0, 0,  // Parent
                 0, 0, 0, 0, 0, 0, 0, 0)] // Child
        // No Margin or Padding on Parent, Child has Margin and Padding
        [DataRow(0, 0, 0, 0, 0, 0, 0, 0,
                 3, 5, 7, 11, 13, 17, 19, 21)]
        // Parent has Margin and Padding, No Margin or Padding on Child
        [DataRow(3, 5, 7, 11, 13, 17, 19, 21,
                 3, 5, 7, 11, 13, 17, 19, 21)]
        public async Task TestUIWindow_WithPanelChild_WithMarginAndPadding(int marginLeft, int marginRight, int marginTop, int marginBottom, int paddingLeft, int paddingRight, int paddingTop, int paddingBottom,
                                                                     int childMarginLeft, int childMarginRight, int childMarginTop, int childMarginBottom, int childPaddingLeft, int childPaddingRight, int childPaddingTop, int childPaddingBottom
                                                                    )
        {
            var uiManager = new FakeUIManager();

            var ui = new EmptyUI();
            ui.Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom);
            ui.Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom);

            var panel = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Margin = new Thickness(childMarginLeft, childMarginTop, childMarginRight, childMarginBottom),
                Padding = new Thickness(childPaddingLeft, childPaddingTop, childPaddingRight, childPaddingBottom)
            };

            ui.AddChild(panel);

            uiManager.AddUI(ui, FakeGame.Instance);

            // Do UI Layout
            await uiManager.PerformLayout();


            // Check Results for Parent window
            var layoutRect = FakeGame.Instance.LayoutRectangle;

            var finalRect = ui.GetFinalRect();
            var finalContentRect = ui.GetFinalContentRect();

            // The empty UI should expand to fill the full layout space
            Rectangle expectedFinalRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom);
            Assert.AreEqual(finalRect, expectedFinalRect);

            Rectangle expectedFinalContentRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom, paddingLeft, paddingTop, paddingRight, paddingBottom);
            Assert.AreEqual(finalContentRect, expectedFinalContentRect);


            // Check Results for child Panel
            finalRect = panel.GetFinalRect();
            finalContentRect = panel.GetFinalContentRect();

            // The panel should fill the Parent UI's finalContentRect with space for the Panel's margin
            var expectedChildFinalRect = TestHelper.ShrinkRect(expectedFinalContentRect, childMarginLeft, childMarginTop, childMarginRight, childMarginBottom);
            Assert.AreEqual(finalRect, expectedChildFinalRect);

            // The panel's content rect should fill the Parent UI's finalContentRect with space for the Panel's margin and padding
            var expectedChildFinalContentRect = TestHelper.ShrinkRect(expectedFinalContentRect, childMarginLeft, childMarginTop, childMarginRight, childMarginBottom, childPaddingLeft, childPaddingTop, childPaddingRight, childPaddingBottom);
            Assert.AreEqual(finalContentRect, expectedChildFinalContentRect);

        }

    }
}
