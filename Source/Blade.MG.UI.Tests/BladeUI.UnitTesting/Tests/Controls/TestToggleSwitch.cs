using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Events;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    [TestClass]
    public class TestToggleSwitch
    {
        private static (FakeUIManager uiManager, Border border, ToggleSwitch toggle, RenderTarget2D renderTarget, SpriteBatch spriteBatch, EmptyUI ui) BuildToggleInCachedBorder()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var host = new Panel
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };
            ui.AddChild(host);

            // A cached ancestor Border, mirroring how a ToggleSwitch is typically hosted inside
            // a Section/panel in real usage (Border always enables render caching - see Border's
            // own constructor) - the same setup TestCheckBoxRepeatedToggle uses, since without
            // it every render would happen unconditionally regardless, masking the class of bug
            // this is meant to catch.
            var border = new Border
            {
                Width = 200,
                Height = 60,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                CornerRadius = new CornerRadius(0f),
                BorderThickness = new Thickness(0),
                Background = Color.Transparent,
                EnableCaching = true,
            };
            host.AddChild(border);

            var toggle = new ToggleSwitch
            {
                Width = 180,
                Height = 40,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                Text = "Enable feature",
            };
            border.Content = toggle;

            uiManager.AddUI(ui);
            uiManager.PerformLayout();
            uiManager.PerformLayout();

            var graphicsDevice = FakeGame.Instance.GraphicsDevice;
            var renderTarget = new RenderTarget2D(graphicsDevice, 400, 300);
            var spriteBatch = new SpriteBatch(graphicsDevice);

            return (uiManager, border, toggle, renderTarget, spriteBatch, ui);
        }

        private static long SampleChecksum(FakeUIManager uiManager, Border border, RenderTarget2D renderTarget, SpriteBatch spriteBatch)
        {
            var graphicsDevice = FakeGame.Instance.GraphicsDevice;

            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Black);

            uiManager.Draw(spriteBatch, new GameTime(), renderTarget);

            graphicsDevice.SetRenderTarget(null);

            Color[] pixels = new Color[renderTarget.Width * renderTarget.Height];
            renderTarget.GetData(pixels);

            Rectangle rect = border.GetFinalRect();
            long checksum = 0;
            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    Color c = pixels[y * renderTarget.Width + x];
                    checksum += c.R + (c.G * 31) + (c.B * 97);
                }
            }
            return checksum;
        }

        [TestMethod]
        public async Task RepeatedClicksWhileFocused_EachToggleTheRenderedThumb()
        {
            var (uiManager, border, toggle, renderTarget, spriteBatch, ui) = BuildToggleInCachedBorder();
            using (renderTarget)
            using (spriteBatch)
            {
                long offChecksum = SampleChecksum(uiManager, border, renderTarget, spriteBatch);

                // First click: gains focus AND toggles On. PropertyAnimationManager.AnimateTo
                // (called from ToggleSwitchTemplate.RenderControl) is only actually registered
                // the next time RenderControl runs - so a priming SampleChecksum call right after
                // the click is needed before sleeping, otherwise the sleep elapses before the
                // animation has even started (PropertyAnimation<T>.Progress is wall-clock-time
                // based, see PropertyAnimation.cs - a PerformLayout() with ~0ms of real time
                // elapsed since AnimateTo was first called ticks the animation at ~0% progress,
                // rendering identically to Off).
                toggle.HasFocus.Value = true;
                await toggle.ActivateAsync(ui, new UIClickEvent());
                SampleChecksum(uiManager, border, renderTarget, spriteBatch); // primes the animation
                Thread.Sleep(20);
                uiManager.PerformLayout();

                long afterFirstClickChecksum = SampleChecksum(uiManager, border, renderTarget, spriteBatch);
                Assert.AreNotEqual(offChecksum, afterFirstClickChecksum,
                    "Expected the thumb to render after the first click (which also gains focus).");

                // Second click: focus is unchanged (already focused) - only IsOn toggles again.
                await toggle.ActivateAsync(ui, new UIClickEvent());
                SampleChecksum(uiManager, border, renderTarget, spriteBatch); // primes the animation
                Thread.Sleep(20);
                uiManager.PerformLayout();

                long afterSecondClickChecksum = SampleChecksum(uiManager, border, renderTarget, spriteBatch);
                Assert.AreNotEqual(afterFirstClickChecksum, afterSecondClickChecksum,
                    "Expected the thumb to render again after a second click while already focused - equal checksums mean IsOn's own change never forced a redraw.");
            }
        }

        [TestMethod]
        public void ActivateAsync_TogglesIsOn()
        {
            var toggle = new ToggleSwitch();
            Assert.IsFalse(toggle.IsOn.Value);

            var ui = new EmptyUI();
            ui.AddChild(toggle);

            toggle.ActivateAsync(ui, new UIClickEvent()).GetAwaiter().GetResult();
            Assert.IsTrue(toggle.IsOn.Value);

            toggle.ActivateAsync(ui, new UIClickEvent()).GetAwaiter().GetResult();
            Assert.IsFalse(toggle.IsOn.Value);
        }

        [TestMethod]
        public void ActivateAsync_WhileDisabled_DoesNotToggle()
        {
            var toggle = new ToggleSwitch { IsEnabled = false };
            var ui = new EmptyUI();
            ui.AddChild(toggle);

            toggle.ActivateAsync(ui, new UIClickEvent()).GetAwaiter().GetResult();

            Assert.IsFalse(toggle.IsOn.Value, "Expected a disabled ToggleSwitch to ignore activation.");
        }

        [TestMethod]
        public async Task SlideAnimation_RendersMultipleDistinctIntermediateFrames_NotJustAJump()
        {
            var (uiManager, border, toggle, renderTarget, spriteBatch, ui) = BuildToggleInCachedBorder();
            using (renderTarget)
            using (spriteBatch)
            {
                // Settle the initial Off state and let the first-render snap happen.
                for (int i = 0; i < 4; i++)
                {
                    SampleChecksum(uiManager, border, renderTarget, spriteBatch);
                }

                toggle.HasFocus.Value = true;
                await toggle.ActivateAsync(ui, new UIClickEvent());

                // No further explicit invalidation between these samples - only
                // uiManager.PerformLayout() (which calls PropertyAnimationManager.Update() every
                // time, per UIManager.Update) advances the animation clock. If the thumb's own
                // Changed->BubbleInvalidation wiring is working, each of these should see the
                // cached ancestor Border's cache invalidated again as thumbProgress eases toward
                // its On target, producing several DISTINCT consecutive checksums rather than a
                // single jump straight from Off to On.
                long previousChecksum = SampleChecksum(uiManager, border, renderTarget, spriteBatch);
                int distinctTransitions = 0;
                for (int i = 0; i < 8; i++)
                {
                    Thread.Sleep(20);
                    uiManager.PerformLayout();
                    long checksum = SampleChecksum(uiManager, border, renderTarget, spriteBatch);

                    if (checksum != previousChecksum)
                    {
                        distinctTransitions++;
                    }

                    previousChecksum = checksum;
                }

                Assert.IsTrue(distinctTransitions >= 3,
                    $"Expected the thumb's slide to render at least 3 distinct intermediate frames over ~160ms (a 150ms animation), but only saw {distinctTransitions} - too few distinct frames means the thumb is jumping between Off/On instead of sliding smoothly.");
            }
        }
    }
}
