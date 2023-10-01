using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace BladeUI.UnitTesting.Tests.Containers
{
    [TestClass]
    public class TestPanel
    {

        //private MockGame mockGame;

        public TestPanel()
        {
            // Create Resources to be shared by all unit tests in this class
            //mockGame = new MockGame();
        }

        //[ClassCleanup]
        public void CleanUp()
        {
            //mockGame?.Dispose();
        }


        [TestMethod]
        public void TestPanel_NoChildren()
        {
            var uiManager = new FakeUIManager();

            var ui = new EmptyUI();

            var panel = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch
            };

            ui.AddChild(panel);

            uiManager.AddUI(ui, FakeGame.Instance);

            // Do UI Layout
            uiManager.PerformLayout();


            // Check Results
            var layoutRect = ui.GetFinalContentRect();

            var finalRect = ui.GetFinalRect();
            var finalContentRect = ui.GetFinalContentRect();

            // The empty UI should expand to fill the full layout space
            Rectangle expectedFinalRect = layoutRect;
            Assert.AreEqual(finalRect, expectedFinalRect);

            Rectangle expectedFinalContentRect = layoutRect;
            Assert.AreEqual(finalContentRect, expectedFinalContentRect);
        }

        [DataTestMethod()]
        [DataRow(0, 0, 0, 0, 0, 0, 0, 0)]
        [DataRow(3, 5, 7, 11, 0, 0, 0, 0)]
        [DataRow(0, 0, 0, 0, 13, 17, 19, 21)]
        [DataRow(3, 5, 7, 11, 13, 17, 19, 21)]
        public void TestPanel_NoChildren_WithMarginAndPadding(int marginLeft, int marginRight, int marginTop, int marginBottom, int paddingLeft, int paddingRight, int paddingTop, int paddingBottom)
        {
            var uiManager = new FakeUIManager();

            var ui = new EmptyUI();

            var panel = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom),
                Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom)
            };

            ui.AddChild(panel);

            uiManager.AddUI(ui, FakeGame.Instance);

            // Do UI Layout
            uiManager.PerformLayout();


            // Check Results
            var layoutRect = ui.GetFinalContentRect();

            var finalRect = panel.GetFinalRect();
            var finalContentRect = panel.GetFinalContentRect();

            // The empty UI should expand to fill the full layout space
            Rectangle expectedFinalRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom);
            Assert.AreEqual(finalRect, expectedFinalRect);

            Rectangle expectedFinalContentRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom, paddingLeft, paddingTop, paddingRight, paddingBottom);
            Assert.AreEqual(finalContentRect, expectedFinalContentRect);

        }

        [DataTestMethod()]
        [DataRow(0, 0, 0, 0, 0, 0, 0, 0)]
        [DataRow(3, 5, 7, 11, 0, 0, 0, 0)]
        [DataRow(0, 0, 0, 0, 13, 17, 19, 21)]
        [DataRow(3, 5, 7, 11, 13, 17, 19, 21)]
        public void TestPanel_WithPanelChild_WithMarginAndPadding(int marginLeft, int marginRight, int marginTop, int marginBottom, int paddingLeft, int paddingRight, int paddingTop, int paddingBottom)
        {
            var uiManager = new FakeUIManager();

            var ui = new EmptyUI();
            ui.Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom);
            ui.Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom);

            var panel = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom),
                Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom)
            };

            ui.AddChild(panel);

            var panel2 = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom),
                Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom)
            };

            panel.AddChild(panel2);


            uiManager.AddUI(ui, FakeGame.Instance);

            // Do UI Layout
            uiManager.PerformLayout();


            // Check Results
            var layoutRect = panel.GetFinalContentRect();

            var finalRect = panel2.GetFinalRect();
            var finalContentRect = panel2.GetFinalContentRect();

            // The empty UI should expand to fill the full layout space
            Rectangle expectedFinalRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom);
            Assert.AreEqual(finalRect, expectedFinalRect);

            Rectangle expectedFinalContentRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom, paddingLeft, paddingTop, paddingRight, paddingBottom);
            Assert.AreEqual(finalContentRect, expectedFinalContentRect);

        }

    }
}
