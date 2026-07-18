using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Events;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// Reproduces a reported bug: hovering the same tooltip-attached button repeatedly showed an
    /// extra empty tooltip box each time, and the text was always blank. Root cause: AddWindow
    /// (UIManager.cs) calls Initialize()/LoadContent() unconditionally on every Add() - correct
    /// for Initialize (Context/Renderer/SpriteBatch legitimately need recreating after a prior
    /// Close() disposed them), but Popup.LoadContent()/Tooltip.LoadContent() had no guard against
    /// being called again on a reshow (Close then ShowAt), so every reshow created a BRAND NEW
    /// contentBorder/label and added it as an ADDITIONAL child, never removing the previous one -
    /// and since Tooltip.Attach called SetText() BEFORE ShowAt() (which is what triggers that
    /// re-creation), the just-written text was always immediately discarded by a fresh, blank
    /// Label.
    /// </summary>
    [TestClass]
    public class TestTooltipRepeatedHover
    {
        private static void RegisterUIManager(UIManager uiManager)
        {
            if (FakeGame.Instance.Services.GetService(typeof(UIManager)) != null)
            {
                FakeGame.Instance.Services.RemoveService(typeof(UIManager));
            }

            FakeGame.Instance.Services.AddService(typeof(UIManager), uiManager);
        }

        [TestMethod]
        public async Task HoveringRepeatedly_ReusesTheSameContentBorder_AndAlwaysShowsCorrectText()
        {
            var uiManager = new FakeUIManager();
            RegisterUIManager(uiManager);
            var ui = new EmptyUI();

            var button = new Button { Text = "Hover me", Width = 120, Height = 40 };
            ui.AddChild(button);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            Tooltip.Attach(button, FakeGame.Instance, "Tooltip text", delaySeconds: 0.05f);

            Point buttonCenter = button.GetFinalRect().Center;

            for (int i = 0; i < 3; i++)
            {
                Task hoverTask = button.HandleHoverChangedAsync(ui, new UIHoverChangedEvent { Hover = true, X = buttonCenter.X, Y = buttonCenter.Y });
                await Task.Delay(150);
                await hoverTask;
                await uiManager.PerformLayout();

                var shownTooltips = uiManager.GetWindows.OfType<Tooltip>().ToList();
                Assert.AreEqual(1, shownTooltips.Count, $"Expected exactly one Tooltip window (cycle {i}).");

                var tooltip = shownTooltips[0];
                Assert.AreEqual(1, tooltip.Children.Count,
                    $"Expected the Tooltip to still have exactly one contentBorder child (cycle {i}), not an extra one piled up from this reshow.");

                var label = (Label)tooltip.Content;
                Assert.AreEqual("Tooltip text", label?.Text?.Value,
                    $"Expected the Tooltip's label to show the correct text (cycle {i}), not be blank.");

                await button.HandleHoverChangedAsync(ui, new UIHoverChangedEvent { Hover = false, X = buttonCenter.X, Y = buttonCenter.Y, ForcePropagation = true });
                await uiManager.PerformLayout();
            }

            int tooltipWindowCount = uiManager.GetWindows.Count(w => w is Tooltip);
            Assert.AreEqual(0, tooltipWindowCount,
                $"Expected no Tooltip windows to remain after the hover ended each time, but found {tooltipWindowCount}.");
        }
    }
}
