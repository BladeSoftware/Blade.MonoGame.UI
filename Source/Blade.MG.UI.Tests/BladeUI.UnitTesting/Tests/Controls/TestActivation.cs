using Blade.MG.UI.Controls;
using Blade.MG.UI.Events;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    // Regression coverage for the shared Activate/ActivateAsync hook: mouse click, touch tap,
    // gamepad A, and keyboard Enter/Space should all reach a control's real "activate" behavior
    // uniformly (CheckBox's toggle, Button's OnActivate) rather than each device inventing its
    // own way to reach it. These tests drive UIWindow.HandleMouseClickEventAsync directly (mouse
    // click's own path, which calls ActivateAsync internally) to confirm it actually reaches
    // each control's real activation logic.
    [TestClass]
    public class TestActivation
    {
        [TestMethod]
        public async Task CheckBox_SynthesizedClickAtOwnCenter_TogglesChecked()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var checkBox = new CheckBox();

            ui.AddChild(checkBox);
            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            Assert.AreEqual(false, checkBox.IsChecked.Value);

            var center = checkBox.GetFinalRect().Center;
            var clickEvent = new UIClickEvent { X = center.X, Y = center.Y };

            await ui.HandleMouseClickEventAsync(ui, clickEvent);

            Assert.AreEqual(true, checkBox.IsChecked.Value);
        }

        [TestMethod]
        public async Task Button_SynthesizedClickAtOwnCenter_FiresOnActivate()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var button = new Button();
            bool clicked = false;
            button.OnActivate = (sender, e) => clicked = true;

            ui.AddChild(button);
            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            var center = button.GetFinalRect().Center;
            var clickEvent = new UIClickEvent { X = center.X, Y = center.Y };

            await ui.HandleMouseClickEventAsync(ui, clickEvent);

            Assert.IsTrue(clicked);
        }
    }
}
