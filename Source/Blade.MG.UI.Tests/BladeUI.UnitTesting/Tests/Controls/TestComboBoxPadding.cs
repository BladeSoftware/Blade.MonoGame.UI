using System.Linq;
using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Controls.Templates;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// ComboBox has the same Padding self-cancellation bug as Button/CheckBox/ToggleSwitch (see
    /// TestButtonPadding.cs), except RootBorder (the actual rendered chrome) already had a
    /// hardcoded 4px baseline Padding rather than zero - so the fix adds combo.Padding on top of
    /// that existing baseline instead of replacing it outright, to avoid changing the default
    /// (unpadded) appearance.
    /// </summary>
    [TestClass]
    public class TestComboBoxPadding
    {
        [TestMethod]
        public void Padding_GrowsRootBorder_OnTopOfExistingBaseline()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var host = new Panel { Width = 900, Height = 200, HorizontalAlignment = HorizontalAlignmentType.Left, VerticalAlignment = VerticalAlignmentType.Top };
            ui.AddChild(host);

            var plainCombo = new ComboBox { Width = 200, Height = 40 };
            var paddedCombo = new ComboBox { Width = 200, Height = 40, Padding = new Thickness(20, 10) };

            host.AddChild(plainCombo);
            host.AddChild(paddedCombo);

            uiManager.AddUI(ui);
            uiManager.PerformLayout();
            uiManager.PerformLayout();

            var plainRootBorder = ((ComboBoxTemplate)plainCombo.PrivateControls.First()).RootBorder;
            var paddedRootBorder = ((ComboBoxTemplate)paddedCombo.PrivateControls.First()).RootBorder;

            // Default (no combo.Padding set) should be unchanged from the existing 4px baseline.
            Assert.AreEqual(new Thickness(4).Left, plainRootBorder.Padding.Value.Left,
                "Expected the default RootBorder Padding baseline (4px) to be unaffected when combo.Padding isn't set.");

            // Padding = new Thickness(20, 10) should add on top of that 4px baseline.
            Assert.AreEqual(24, paddedRootBorder.Padding.Value.Left, "Expected RootBorder.Padding.Left to be the 4px baseline plus combo.Padding.Left (20).");
            Assert.AreEqual(14, paddedRootBorder.Padding.Value.Top, "Expected RootBorder.Padding.Top to be the 4px baseline plus combo.Padding.Top (10).");
        }
    }
}
