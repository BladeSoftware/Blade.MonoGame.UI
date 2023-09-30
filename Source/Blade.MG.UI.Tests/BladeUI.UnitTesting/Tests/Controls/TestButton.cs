using Blade.UI.Components;
using Blade.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Containers
{
    [TestClass]
    public class TestButton
    {

        public TestButton()
        {
            // Create Resources to be shared by all unit tests in this class
        }


        [DataTestMethod()]
        [DataRow(0, 0, 0, 0, 0, 0, 0, 0)]
        [DataRow(3, 5, 7, 11, 0, 0, 0, 0)]
        [DataRow(0, 0, 0, 0, 13, 17, 19, 21)]
        [DataRow(3, 5, 7, 11, 13, 17, 19, 21)]
        public async Task TestButton_WithMarginAndPadding(int marginLeft, int marginRight, int marginTop, int marginBottom, int paddingLeft, int paddingRight, int paddingTop, int paddingBottom)
        {
            var uiManager = new FakeUIManager();

            var ui = new EmptyUI();
            //ui.Initialize(FakeGame.Instance);

            var button = new Button
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom),
                Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom)
            };

            ui.AddChild(button);

            uiManager.AddUI(ui, FakeGame.Instance);

            // Do UI Layout
            await uiManager.PerformLayout();


            // Check Results
            var layoutRect = ui.GetFinalContentRect();

            var finalRect = button.GetFinalRect();
            var finalContentRect = button.GetFinalContentRect();

            // The empty UI should expand to fill the full layout space
            Rectangle expectedFinalRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom);
            Assert.AreEqual(finalRect, expectedFinalRect);

            Rectangle expectedFinalContentRect = TestHelper.ShrinkRect(layoutRect, marginLeft, marginTop, marginRight, marginBottom, paddingLeft, paddingTop, paddingRight, paddingBottom);
            Assert.AreEqual(finalContentRect, expectedFinalContentRect);

        }

    }
}
