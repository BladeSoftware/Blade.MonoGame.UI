using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Events;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    [TestClass]
    public class TestRadioButton
    {
        [TestMethod]
        public async Task CheckingOneRadioButton_UnchecksOthersInTheSameGroup()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var stack = new StackPanel { Orientation = Orientation.Vertical };
            ui.AddChild(stack);

            var optionA = new RadioButton { Text = "A", GroupName = "options" };
            var optionB = new RadioButton { Text = "B", GroupName = "options" };
            var optionC = new RadioButton { Text = "C", GroupName = "options" };
            stack.AddChild(optionA);
            stack.AddChild(optionB);
            stack.AddChild(optionC);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            await optionA.ActivateAsync(ui, new UIClickEvent());
            Assert.IsTrue(optionA.IsChecked.Value, "Expected A to become checked.");
            Assert.IsFalse(optionB.IsChecked.Value);
            Assert.IsFalse(optionC.IsChecked.Value);

            await optionB.ActivateAsync(ui, new UIClickEvent());
            Assert.IsFalse(optionA.IsChecked.Value, "Expected checking B to uncheck A.");
            Assert.IsTrue(optionB.IsChecked.Value);
            Assert.IsFalse(optionC.IsChecked.Value);
        }

        [TestMethod]
        public async Task DifferentGroups_DoNotAffectEachOther()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var stack = new StackPanel { Orientation = Orientation.Vertical };
            ui.AddChild(stack);

            var groupOneOptionA = new RadioButton { Text = "1A", GroupName = "group1" };
            var groupTwoOptionA = new RadioButton { Text = "2A", GroupName = "group2" };
            stack.AddChild(groupOneOptionA);
            stack.AddChild(groupTwoOptionA);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            await groupOneOptionA.ActivateAsync(ui, new UIClickEvent());
            await groupTwoOptionA.ActivateAsync(ui, new UIClickEvent());

            Assert.IsTrue(groupOneOptionA.IsChecked.Value, "Expected a different group's selection to be unaffected.");
            Assert.IsTrue(groupTwoOptionA.IsChecked.Value);
        }

        [TestMethod]
        public async Task ClickingAnAlreadyCheckedRadioButton_StaysChecked()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var option = new RadioButton { Text = "A", GroupName = "options" };
            ui.AddChild(option);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            int changedCount = 0;
            option.OnValueChanged = _ => changedCount++;

            await option.ActivateAsync(ui, new UIClickEvent());
            await option.ActivateAsync(ui, new UIClickEvent());

            Assert.IsTrue(option.IsChecked.Value);
            Assert.AreEqual(1, changedCount, "Expected OnValueChanged to fire only once - clicking an already-checked radio button is a no-op, unlike a CheckBox toggle.");
        }
    }
}
