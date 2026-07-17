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
    public class TestTooltip
    {
        // Popup.ShowAt/Close resolve UIManager via Game.Services (the same convention
        // ModalBase.Show/ShowAsync already use) - FakeGame.Instance is a singleton shared across
        // the whole test run, so each test must (re-)register its own FakeUIManager rather than
        // relying on one left over from a previous test, and AddService throws if a UIManager is
        // already registered.
        private static void RegisterUIManager(UIManager uiManager)
        {
            if (FakeGame.Instance.Services.GetService(typeof(UIManager)) != null)
            {
                FakeGame.Instance.Services.RemoveService(typeof(UIManager));
            }

            FakeGame.Instance.Services.AddService(typeof(UIManager), uiManager);
        }

        [TestMethod]
        public async Task Attach_ShowsAfterDelay_AndHidesImmediatelyOnHoverLeave()
        {
            var uiManager = new FakeUIManager();
            RegisterUIManager(uiManager);
            var ui = new EmptyUI();

            var button = new Button
            {
                Text = "Hover me",
                Width = 120,
                Height = 40,
            };
            ui.AddChild(button);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            Tooltip.Attach(button, FakeGame.Instance, "Tooltip text", delaySeconds: 0.05f);

            Assert.IsNull(uiManager.Find<Tooltip>(), "Expected no Tooltip to be shown before any hover.");

            // UIManager.RunInputHandler (the real caller of HandleHoverChangedAsync) only awaits
            // a handler synchronously up to its first genuine yield point - if it doesn't
            // complete immediately (e.g. because of this Tooltip's Task.Delay), it lets the task
            // keep running in the background rather than blocking that frame's Update(). Awaiting
            // the call directly here would defeat the point of the delay, so mirror that
            // fire-and-continue behavior instead of awaiting straight through it.
            Point buttonCenter = button.GetFinalRect().Center;
            Task hoverTask = button.HandleHoverChangedAsync(ui, new UIHoverChangedEvent { Hover = true, X = buttonCenter.X, Y = buttonCenter.Y });

            // Immediately after hover starts, the delay hasn't elapsed yet.
            Assert.IsNull(uiManager.Find<Tooltip>(), "Expected the Tooltip to not appear immediately - it should wait for the hover delay.");

            await Task.Delay(200);
            await hoverTask;

            Assert.IsNotNull(uiManager.Find<Tooltip>(), "Expected the Tooltip to appear after the hover delay elapsed.");

            await button.HandleHoverChangedAsync(ui, new UIHoverChangedEvent { Hover = false, X = buttonCenter.X, Y = buttonCenter.Y, ForcePropagation = true });

            // UIManager.Remove(instance) only enqueues the removal (same as Add queues via
            // HandleTaskQueue) - it's applied on the next frame's Update/PerformLayout, exactly
            // like it would be for a real game running at 60fps, so pump one frame before
            // checking it actually disappeared.
            await uiManager.PerformLayout();

            Assert.IsNull(uiManager.Find<Tooltip>(), "Expected the Tooltip to close once hover ends.");
        }

        [TestMethod]
        public async Task Attach_HoveringBrieflyThenLeaving_NeverShowsTooltip()
        {
            var uiManager = new FakeUIManager();
            RegisterUIManager(uiManager);
            var ui = new EmptyUI();

            var button = new Button
            {
                Text = "Hover me",
                Width = 120,
                Height = 40,
            };
            ui.AddChild(button);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            Tooltip.Attach(button, FakeGame.Instance, "Tooltip text", delaySeconds: 0.3f);

            Point buttonCenter = button.GetFinalRect().Center;
            Task hoverTask = button.HandleHoverChangedAsync(ui, new UIHoverChangedEvent { Hover = true, X = buttonCenter.X, Y = buttonCenter.Y });

            // Leave well before the 0.3s delay elapses - the pending show should be cancelled.
            await Task.Delay(50);
            await button.HandleHoverChangedAsync(ui, new UIHoverChangedEvent { Hover = false, X = buttonCenter.X, Y = buttonCenter.Y, ForcePropagation = true });

            await Task.Delay(400);
            await hoverTask;

            Assert.IsNull(uiManager.Find<Tooltip>(), "Expected a brief hover (shorter than the delay) to never show the Tooltip at all.");
        }
    }
}
