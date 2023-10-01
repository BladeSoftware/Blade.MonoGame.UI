using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace BladeUI.UnitTesting.Tests.Containers
{
    [TestClass]
    public class TestScrollPanel
    {

        public TestScrollPanel()
        {
            // Create Resources to be shared by all unit tests in this class
        }

        //[ClassCleanup]
        //public void CleanUp()
        //{
        //}


        [DataTestMethod()]
        [DataRow(0, 0, 0, 0, 0, 0, 0, 0)]
        [DataRow(3, 5, 7, 11, 0, 0, 0, 0)]
        [DataRow(0, 0, 0, 0, 13, 17, 19, 21)]
        [DataRow(3, 5, 7, 11, 13, 17, 19, 21)]
        public void TestScrollPanel_NoChildren_WithMarginAndPadding(int marginLeft, int marginRight, int marginTop, int marginBottom, int paddingLeft, int paddingRight, int paddingTop, int paddingBottom)
        {
            var uiManager = new FakeUIManager();

            var ui = new EmptyUI();

            var scrollPanel = new ScrollPanel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom),
                Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom)
            };

            ui.AddChild(scrollPanel);

            uiManager.AddUI(ui, FakeGame.Instance);

            // Do UI Layout
            uiManager.PerformLayout();


            // Check Results
            var layoutRect = ui.GetFinalContentRect();

            var finalRect = scrollPanel.GetFinalRect();
            var finalContentRect = scrollPanel.GetFinalContentRect();

            // The empty UI should expand to fill the full layout space
            Rectangle expectedFinalRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom);
            Assert.AreEqual(finalRect, expectedFinalRect);

            Rectangle expectedFinalContentRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom, paddingLeft, paddingTop, paddingRight, paddingBottom);

            expectedFinalContentRect = TestHelper.ShrinkRect(expectedFinalContentRect, 0, 0, scrollPanel.HorizontalScrollBarVisible ? (int)scrollPanel.HorizontalScrollBar.Height.ToPixels() : 0, scrollPanel.VerticalScrollBarVisible ? (int)scrollPanel.VerticalScrollBar.Width.ToPixels(finalContentRect.Width) : 0);


            Assert.AreEqual(finalContentRect, expectedFinalContentRect);

        }

    }
}
