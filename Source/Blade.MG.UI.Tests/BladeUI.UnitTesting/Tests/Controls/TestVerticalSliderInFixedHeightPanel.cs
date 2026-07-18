using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// Confirms the fix used in HelpPage_Slider.cs for hosting a vertical Slider (which always
    /// stretches to fill available height - see Slider.Measure): wrap it in a Panel with an
    /// explicit Height instead of nesting it directly as a StackPanel's stacking-axis child.
    /// </summary>
    [TestClass]
    public class TestVerticalSliderInFixedHeightPanel
    {
        [TestMethod]
        public async Task VerticalSlider_WrappedInFixedHeightPanel_FillsThatHeight()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var slider = new Slider { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignmentType.Center };

            var sliderHost = new Panel { Height = 120, HorizontalAlignment = HorizontalAlignmentType.Center };
            sliderHost.AddChild(slider);

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Center,
                VerticalAlignment = VerticalAlignmentType.Center,
                Width = 300,
                Height = 220,
            };
            stack.AddChild(new Label { Text = "Label above" });
            stack.AddChild(sliderHost);
            stack.AddChild(new Label { Text = "Label below" });

            ui.AddChild(stack);
            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            Assert.AreEqual(120, slider.GetFinalRect().Height,
                $"Expected the vertical Slider to fill the 120px fixed-height Panel wrapping it, got {slider.GetFinalRect()}.");
        }
    }
}
