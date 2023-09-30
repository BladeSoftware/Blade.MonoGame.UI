using Blade.UI.Components;
using Blade.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace BladeUI.UnitTesting.Tests.Containers
{
    [TestClass]
    public class TestStackPanel
    {

        public TestStackPanel()
        {
            // Create Resources to be shared by all unit tests in this class
        }


        [DataTestMethod()]
        [DataRow(0, 0, 0, 0, 0, 0, 0, 0)]
        [DataRow(3, 5, 7, 11, 0, 0, 0, 0)]
        [DataRow(0, 0, 0, 0, 13, 17, 19, 21)]
        [DataRow(3, 5, 7, 11, 13, 17, 19, 21)]
        public void TestStackPanel_NoChildren_WithMarginAndPadding(int marginLeft, int marginRight, int marginTop, int marginBottom, int paddingLeft, int paddingRight, int paddingTop, int paddingBottom)
        {
            var uiManager = new FakeUIManager();

            var ui = new EmptyUI();

            var stackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom),
                Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom),
                Orientation = Orientation.Horizontal
            };

            ui.AddChild(stackPanel);

            uiManager.AddUI(ui, FakeGame.Instance);

            // Do UI Layout
            uiManager.PerformLayout();


            // Check Results
            var layoutRect = ui.GetFinalContentRect();

            var finalRect = stackPanel.GetFinalRect();
            var finalContentRect = stackPanel.GetFinalContentRect();

            // The empty UI should expand to fill the full layout space
            Rectangle expectedFinalRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom);
            Assert.AreEqual(finalRect, expectedFinalRect);

            Rectangle expectedFinalContentRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom, paddingLeft, paddingTop, paddingRight, paddingBottom);

            expectedFinalContentRect = TestHelper.ShrinkRect(expectedFinalContentRect, 0, 0, stackPanel.HorizontalScrollBarVisible ? (int)stackPanel.HorizontalScrollBar.Height.ToPixels(finalContentRect.Height) : 0, stackPanel.VerticalScrollBarVisible ? (int)stackPanel.VerticalScrollBar.Width.ToPixels(finalContentRect.Width) : 0);

            Assert.AreEqual(finalContentRect, expectedFinalContentRect);

        }

        [DataTestMethod()]
        //[DataRow(0, 0, 0, 0, 0, 0, 0, 0)]
        //[DataRow(1, 1, 1, 1, 1, 1, 1, 1)]
        //[DataRow(3, 5, 7, 11, 0, 0, 0, 0)]
        //[DataRow(0, 0, 0, 0, 13, 17, 19, 21)]
        //[DataRow(3, 5, 7, 11, 13, 17, 19, 21)]
        [DataRow(10, 10, 10, 10, 10, 10, 10, 10)]
        public void TestStackPanel_WithPanelChildren_WithMarginAndPadding(int marginLeft, int marginRight, int marginTop, int marginBottom, int paddingLeft, int paddingRight, int paddingTop, int paddingBottom)
        {
            var uiManager = new FakeUIManager();

            var ui = new EmptyUI();
            ui.Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom);
            ui.Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom);

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom),
                Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom)
            };

            ui.AddChild(stackPanel);

            var panelChild1 = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom),
                Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom),
                Width = 100,
                Height = 100
            };

            stackPanel.AddChild(panelChild1);

            var panelChild2 = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom),
                Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom),
                Width = 100,
                Height = 100
            };

            stackPanel.AddChild(panelChild2);

            var panelChild3 = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom),
                Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom)
            };

            stackPanel.AddChild(panelChild3);


            uiManager.AddUI(ui, FakeGame.Instance);

            // Do UI Layout
            uiManager.PerformLayout();


            // Check Results
            var layoutRect = stackPanel.GetFinalContentRect();

            // Check Child 1
            var finalRect = panelChild1.GetFinalRect();
            var finalContentRect = panelChild1.GetFinalContentRect();

            // The empty UI should expand to fill the full layout space
            //Rectangle expectedFinalRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom);
            Rectangle expectedFinalRect_Child1 = new Rectangle(
                layoutRect.Left + panelChild1.Margin.Value.Left,
                layoutRect.Top + panelChild1.Margin.Value.Top,
                (int)(panelChild1.Width.ToPixels(finalContentRect.Width)),
                (int)(panelChild1.Height.ToPixels(finalContentRect.Height))
                );
            Assert.AreEqual(finalRect, expectedFinalRect_Child1);


            //Rectangle expectedFinalContentRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom, paddingLeft, paddingTop, paddingRight, paddingBottom);
            Rectangle expectedFinalContentRect_Child1 = new Rectangle(
                layoutRect.Left + panelChild1.Margin.Value.Left + panelChild1.Padding.Value.Left,
                layoutRect.Top + panelChild1.Margin.Value.Top + panelChild1.Padding.Value.Top,
                (int)(panelChild1.Width.ToPixels(finalContentRect.Width) - panelChild1.Padding.Value.Left - panelChild1.Padding.Value.Right),
                (int)(panelChild1.Height.ToPixels(finalContentRect.Height) - panelChild1.Padding.Value.Top - panelChild1.Padding.Value.Bottom)
            );
            Assert.AreEqual(finalContentRect, expectedFinalContentRect_Child1);


            // Check Child 2
            finalRect = panelChild2.GetFinalRect();
            finalContentRect = panelChild2.GetFinalContentRect();

            // The empty UI should expand to fill the full layout space
            //Rectangle expectedFinalRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom);
            Rectangle expectedFinalRect_Child2 = new Rectangle(
                expectedFinalRect_Child1.Right + panelChild1.Margin.Value.Right + panelChild2.Margin.Value.Left,
                layoutRect.Top + panelChild2.Margin.Value.Top,
                (int)(panelChild2.Width.ToPixels(finalContentRect.Width)),
                (int)(panelChild2.Height.ToPixels(finalContentRect.Height))
                );
            Assert.AreEqual(finalRect, expectedFinalRect_Child2);


            //Rectangle expectedFinalContentRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom, paddingLeft, paddingTop, paddingRight, paddingBottom);
            Rectangle expectedFinalContentRect_Child2 = new Rectangle(
                expectedFinalRect_Child1.Right + panelChild1.Margin.Value.Right + panelChild2.Margin.Value.Left + panelChild2.Padding.Value.Left,
                layoutRect.Top + panelChild2.Margin.Value.Top + panelChild2.Padding.Value.Top,
                (int)(panelChild2.Width.ToPixels(finalContentRect.Width) - panelChild2.Padding.Value.Left - panelChild2.Padding.Value.Right),
                (int)(panelChild2.Height.ToPixels(finalContentRect.Height) - panelChild2.Padding.Value.Top - panelChild2.Padding.Value.Bottom)
            );
            Assert.AreEqual(finalContentRect, expectedFinalContentRect_Child2);

        }

    }
}
