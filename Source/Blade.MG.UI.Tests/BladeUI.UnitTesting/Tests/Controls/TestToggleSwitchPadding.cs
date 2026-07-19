using System.Linq;
using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Controls.Templates;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// ToggleSwitch has the same Padding self-cancellation bug fixed in Button/CheckBox (see
    /// TestButtonPadding.cs / TestCheckBoxPadding.cs) - ToggleSwitchTemplate is the self-painting
    /// leaf (track/thumb drawn at FinalRect-relative coordinates), so the fix reads
    /// toggle.Padding directly onto ToggleSwitchTemplate's own Padding.
    /// </summary>
    [TestClass]
    public class TestToggleSwitchPadding
    {
        private static Rectangle MeasurePill(FakeUIManager uiManager, EmptyUI ui, ToggleSwitch toggle)
        {
            toggle.HorizontalAlignment = HorizontalAlignmentType.Left;
            toggle.VerticalAlignment = VerticalAlignmentType.Top;

            var host = new Panel { Width = 900, Height = 200, HorizontalAlignment = HorizontalAlignmentType.Left, VerticalAlignment = VerticalAlignmentType.Top };
            host.AddChild(toggle);
            ui.AddChild(host);

            uiManager.PerformLayout();
            uiManager.PerformLayout();

            var template = (ToggleSwitchTemplate)toggle.PrivateControls.First();
            return template.GetFinalRect();
        }

        [TestMethod]
        public void Padding_GrowsTheRenderedTemplate_NotJustTheInvisibleOuterBox()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();
            uiManager.AddUI(ui);

            var plainRect = MeasurePill(uiManager, ui, new ToggleSwitch { Text = "Dark Mode" });
            var paddedRect = MeasurePill(uiManager, ui, new ToggleSwitch { Text = "Dark Mode", Padding = new Thickness(20, 10) });

            Assert.IsTrue(paddedRect.Width > plainRect.Width + 30,
                $"Expected the padded ToggleSwitch's rendered template to be noticeably wider than the plain one's, but plain={plainRect.Width}, padded={paddedRect.Width}.");
            Assert.IsTrue(paddedRect.Height > plainRect.Height + 10,
                $"Expected the padded ToggleSwitch's rendered template to be noticeably taller than the plain one's, but plain={plainRect.Height}, padded={paddedRect.Height}.");
        }
    }
}
