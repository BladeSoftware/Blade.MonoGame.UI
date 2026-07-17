using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Events;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    [TestClass]
    public class TestButtonSizeStability
    {
        [TestMethod]
        public async Task IdleButton_FinalRectStaysIdentical_AcrossManyConsecutiveFrames()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var button = new Button
            {
                Text = "Test",
                Width = 120,
                Height = 60,
            };
            ui.AddChild(button);

            uiManager.AddUI(ui);

            Rectangle? previous = null;
            for (int i = 0; i < 10; i++)
            {
                await uiManager.PerformLayout();
                Rectangle current = button.GetFinalRect();

                if (previous.HasValue)
                {
                    Assert.AreEqual(previous.Value, current, $"Expected FinalRect to stay stable frame-to-frame, but it changed on frame {i}.");
                }

                previous = current;
            }
        }

        [TestMethod]
        public async Task AutoSizedIdleButton_FinalRectStaysIdentical_AcrossManyConsecutiveFrames()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            // No explicit Width/Height - DesiredSize comes from label1's font measurement,
            // merged up through Border.Content and ButtonTemplate's internal-child chain.
            var button = new Button
            {
                Text = "Test",
            };
            ui.AddChild(button);

            uiManager.AddUI(ui);

            Rectangle? previous = null;
            for (int i = 0; i < 10; i++)
            {
                await uiManager.PerformLayout();
                Rectangle current = button.GetFinalRect();

                if (previous.HasValue)
                {
                    Assert.AreEqual(previous.Value, current, $"Expected auto-sized FinalRect to stay stable frame-to-frame, but it changed on frame {i}: {previous.Value} -> {current}");
                }

                previous = current;
            }
        }

        [TestMethod]
        public async Task HoveredButton_FinalRectStaysIdentical_AcrossManyConsecutiveFramesAfterSettling()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var button = new Button
            {
                Text = "Test",
                Width = 120,
                Height = 60,
            };
            ui.AddChild(button);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            Point buttonCenter = button.GetFinalRect().Center;
            await button.HandleHoverChangedAsync(ui, new UIHoverChangedEvent { Hover = true, X = buttonCenter.X, Y = buttonCenter.Y });

            // Let the hover color transition fully settle.
            await Task.Delay(200);

            Rectangle? previous = null;
            for (int i = 0; i < 10; i++)
            {
                await uiManager.PerformLayout();
                Rectangle current = button.GetFinalRect();

                if (previous.HasValue)
                {
                    Assert.AreEqual(previous.Value, current, $"Expected hovered (settled) FinalRect to stay stable frame-to-frame, but it changed on frame {i}: {previous.Value} -> {current}");
                }

                previous = current;
            }
        }

        [TestMethod]
        public async Task ButtonsInHorizontalStackPanel_FinalRectStaysIdentical_AcrossManyConsecutiveFrames()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var stack = new StackPanel { Orientation = Orientation.Horizontal };
            var button1 = new Button { Text = "One", Width = 100, Height = 40 };
            var button2 = new Button { Text = "Two", Width = 100, Height = 40 };
            var button3 = new Button { Text = "Three", Width = 100, Height = 40 };
            stack.AddChild(button1);
            stack.AddChild(button2);
            stack.AddChild(button3);
            ui.AddChild(stack);

            uiManager.AddUI(ui);

            Rectangle? previous1 = null, previous2 = null, previous3 = null;
            for (int i = 0; i < 10; i++)
            {
                await uiManager.PerformLayout();
                Rectangle current1 = button1.GetFinalRect();
                Rectangle current2 = button2.GetFinalRect();
                Rectangle current3 = button3.GetFinalRect();

                if (previous1.HasValue)
                {
                    Assert.AreEqual(previous1.Value, current1, $"Expected button1's FinalRect to stay stable frame-to-frame, but it changed on frame {i}: {previous1.Value} -> {current1}");
                    Assert.AreEqual(previous2.Value, current2, $"Expected button2's FinalRect to stay stable frame-to-frame, but it changed on frame {i}: {previous2.Value} -> {current2}");
                    Assert.AreEqual(previous3.Value, current3, $"Expected button3's FinalRect to stay stable frame-to-frame, but it changed on frame {i}: {previous3.Value} -> {current3}");
                }

                previous1 = current1;
                previous2 = current2;
                previous3 = current3;
            }
        }
    }
}
