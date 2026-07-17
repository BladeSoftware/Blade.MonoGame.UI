using Blade.MG.Input;
using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Events;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    [TestClass]
    public class TestSlider
    {
        private static (FakeUIManager uiManager, EmptyUI ui, Slider slider) BuildSlider()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var slider = new Slider
            {
                HorizontalAlignment = HorizontalAlignmentType.Left,
                Width = 200,
                Minimum = 0f,
                Maximum = 100f,
                Value = 0f,
            };
            ui.AddChild(slider);

            uiManager.AddUI(ui);
            uiManager.PerformLayout();
            uiManager.PerformLayout();

            return (uiManager, ui, slider);
        }

        [TestMethod]
        public void DraggingToTheRightEdge_SetsValueToMaximum()
        {
            var (uiManager, ui, slider) = BuildSlider();

            int changedCount = 0;
            slider.OnValueChanged = v => changedCount++;

            var rect = slider.GetFinalRect();

            // rect.Right itself is one past the last pixel Rectangle.Contains accepts - use the
            // last valid pixel instead so ContainsScreenPoint (and therefore the drag) actually
            // triggers.
            var uiEvent = new UIMouseDownEvent { X = rect.Right - 1, Y = rect.Center.Y, PrimaryButton = new Blade.MG.Input.Button(ButtonState.Pressed, ButtonState.Released) };
            slider.HandleMouseDownEventAsync(ui, uiEvent).GetAwaiter().GetResult();

            Assert.AreEqual(100f, slider.Value.Value, 0.01f, "Expected dragging to the right edge to set Value to Maximum.");
            Assert.IsTrue(changedCount > 0, "Expected OnValueChanged to fire.");
        }

        [TestMethod]
        public void DraggingToTheLeftEdge_SetsValueToMinimum()
        {
            var (uiManager, ui, slider) = BuildSlider();
            slider.Value.Value = 50f;

            var rect = slider.GetFinalRect();

            var uiEvent = new UIMouseDownEvent { X = rect.Left, Y = rect.Center.Y, PrimaryButton = new Blade.MG.Input.Button(ButtonState.Pressed, ButtonState.Released) };
            slider.HandleMouseDownEventAsync(ui, uiEvent).GetAwaiter().GetResult();

            Assert.AreEqual(0f, slider.Value.Value, 0.01f, "Expected dragging to the left edge to set Value to Minimum.");
        }

        [TestMethod]
        public async Task ArrowKeys_NudgeValueWhileFocused()
        {
            var (uiManager, ui, slider) = BuildSlider();
            slider.HasFocus.Value = true;
            slider.Value.Value = 50f;

            await slider.HandleKeyPressAsync(ui, new UIKeyEvent { Key = Keys.Right });
            Assert.IsTrue(slider.Value.Value > 50f, $"Expected Right arrow to increase Value above 50, got {slider.Value.Value}.");

            float afterRight = slider.Value.Value;

            await slider.HandleKeyPressAsync(ui, new UIKeyEvent { Key = Keys.Left });
            Assert.IsTrue(slider.Value.Value < afterRight, $"Expected Left arrow to decrease Value below {afterRight}, got {slider.Value.Value}.");
        }

        [TestMethod]
        public async Task HomeAndEndKeys_JumpToMinAndMax()
        {
            var (uiManager, ui, slider) = BuildSlider();
            slider.HasFocus.Value = true;
            slider.Value.Value = 50f;

            await slider.HandleKeyPressAsync(ui, new UIKeyEvent { Key = Keys.End });
            Assert.AreEqual(100f, slider.Value.Value, 0.01f);

            await slider.HandleKeyPressAsync(ui, new UIKeyEvent { Key = Keys.Home });
            Assert.AreEqual(0f, slider.Value.Value, 0.01f);
        }
    }
}
