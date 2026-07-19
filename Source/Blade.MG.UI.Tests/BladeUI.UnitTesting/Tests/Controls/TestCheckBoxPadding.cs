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
    /// CheckBox has the same Padding self-cancellation bug fixed in Button (see
    /// TestButtonPadding.cs) - CheckBox.Padding inflates CheckBox's own DesiredSize but
    /// deflates the FinalContentRect handed to CheckBoxTemplate by the same amount at the same
    /// hop, so CheckBoxTemplate (which stretches to fill whatever box it's given) never actually
    /// grows. Unlike Button, there's no separate nested Border here - CheckBoxTemplate itself is
    /// the self-painting leaf (the checkbox square is drawn at FinalRect-relative coordinates,
    /// and Control's base RenderControl fills Background using FinalRect too), so the fix reads
    /// checkbox.Padding directly onto CheckBoxTemplate's own Padding instead.
    /// </summary>
    [TestClass]
    public class TestCheckBoxPadding
    {
        private static Rectangle MeasurePill(FakeUIManager uiManager, EmptyUI ui, CheckBox checkbox)
        {
            checkbox.HorizontalAlignment = HorizontalAlignmentType.Left;
            checkbox.VerticalAlignment = VerticalAlignmentType.Top;

            var host = new Panel { Width = 900, Height = 200, HorizontalAlignment = HorizontalAlignmentType.Left, VerticalAlignment = VerticalAlignmentType.Top };
            host.AddChild(checkbox);
            ui.AddChild(host);

            uiManager.PerformLayout();
            uiManager.PerformLayout();

            var template = (CheckBoxTemplate)checkbox.PrivateControls.First();
            return template.GetFinalRect();
        }

        [TestMethod]
        public void Padding_GrowsTheRenderedTemplate_NotJustTheInvisibleOuterBox()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();
            uiManager.AddUI(ui);

            var plainRect = MeasurePill(uiManager, ui, new CheckBox { Text = "Dark Mode" });
            var paddedRect = MeasurePill(uiManager, ui, new CheckBox { Text = "Dark Mode", Padding = new Thickness(20, 10) });

            Assert.IsTrue(paddedRect.Width > plainRect.Width + 30,
                $"Expected the padded CheckBox's rendered template to be noticeably wider than the plain one's, but plain={plainRect.Width}, padded={paddedRect.Width}.");
            Assert.IsTrue(paddedRect.Height > plainRect.Height + 10,
                $"Expected the padded CheckBox's rendered template to be noticeably taller than the plain one's, but plain={plainRect.Height}, padded={paddedRect.Height}.");
        }
    }
}
