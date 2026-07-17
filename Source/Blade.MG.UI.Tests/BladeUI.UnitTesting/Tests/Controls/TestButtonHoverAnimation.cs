using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Events;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// ButtonTemplate.HandleStateChange now eases Background/TextColor/BorderColor toward their
    /// resolved target via ApplyThemedValueAnimated (PropertyAnimationManager) instead of
    /// snapping instantly on the very next frame - this guards that the transition is actually
    /// gradual (not a one-frame jump dressed up as an animation call) and that it still reaches
    /// the exact themed target once the transition finishes.
    /// </summary>
    [TestClass]
    public class TestButtonHoverAnimation
    {
        private static IEnumerable<UIComponent> Walk(UIComponent root)
        {
            if (root == null) yield break;
            yield return root;
            foreach (var child in root.PrivateControls)
            {
                foreach (var d in Walk(child)) yield return d;
            }
            if (root is Control control && control.Content != null)
            {
                foreach (var d in Walk(control.Content)) yield return d;
            }
            if (root is Container container)
            {
                foreach (var child in container.Children)
                {
                    foreach (var d in Walk(child)) yield return d;
                }
            }
        }

        [TestMethod]
        public async Task HoveringButton_EasesBackgroundOverTime_RatherThanSnapping()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var button = new Button
            {
                Text = "Test",
                Width = 120,
                Height = 60,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
            };
            ui.AddChild(button);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            var border1 = Walk(button).OfType<Border>().Single();
            Color normalColor = border1.Background.Value;

            Point buttonCenter = button.GetFinalRect().Center;
            await button.HandleHoverChangedAsync(ui, new UIHoverChangedEvent { Hover = true, X = buttonCenter.X, Y = buttonCenter.Y });

            // Tick immediately - the transition has barely started, so the rendered color should
            // still be at (or very near) the pre-hover color, not the fully-hovered target.
            await uiManager.PerformLayout();
            Color justAfterHoverColor = border1.Background.Value;

            Color hoveredTarget = UIManager.DefaultTheme.SecondaryContainer;
            Assert.AreNotEqual(hoveredTarget, justAfterHoverColor, "Expected the hover transition to still be easing immediately after the state change, not already at the target.");

            // Let the ~100ms transition finish, then pump one more frame so
            // PropertyAnimationManager.Update() applies the settled value.
            await Task.Delay(150);
            await uiManager.PerformLayout();

            Assert.AreEqual(hoveredTarget, border1.Background.Value, "Expected the hover transition to reach the exact themed hover color once it finishes.");
            Assert.AreNotEqual(normalColor, border1.Background.Value, "Sanity check: the hovered color should actually differ from the normal color.");
        }
    }
}
