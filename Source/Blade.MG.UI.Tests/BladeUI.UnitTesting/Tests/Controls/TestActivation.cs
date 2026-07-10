using Blade.MG.UI.Controls;
using Blade.MG.UI.Events;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    // Regression coverage for the fix where keyboard Enter/Space, touch tap, and gamepad A
    // synthesize a click at the focused/tapped control's own center point and dispatch it
    // through HandleMouseClickEventAsync (previously HandlePrimaryClickEventAsync, which only
    // MenuItem overrode - see UIManager.Keyboard.cs, UIManager.Touch.cs, UIManager.GamePad.cs).
    // These tests exercise that same dispatch path - through UIWindow.HandleMouseClickEventAsync
    // at a synthesized point, not a direct method call on the control - to confirm it actually
    // reaches each control's real click logic.
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
        public async Task Button_SynthesizedClickAtOwnCenter_FiresOnPrimaryClick()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var button = new Button();
            bool clicked = false;
            button.OnPrimaryClick = (sender, e) => clicked = true;

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
