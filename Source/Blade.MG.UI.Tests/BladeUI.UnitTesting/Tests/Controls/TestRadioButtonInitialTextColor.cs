using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// RadioButton's label was reported invisible until the first hover - RadioButton.textColor
    /// (Controls/RadioButton.cs) is field-initialized to `new Binding&lt;Color&gt;()`, which uses
    /// the parameterless Binding&lt;T&gt; constructor (Value = default(T)) - default(Color) is
    /// (0,0,0,0), fully transparent. label1.TextColor = TextColor (in InitTemplate) aliases the
    /// SAME transparent binding, so the label's text renders invisibly. The only place that ever
    /// assigns a real, visible color is HandleStateChange (`ApplyThemedValue(..., Theme.OnSurface)`),
    /// which only runs in response to a focus or hover change - never on initial construction.
    /// </summary>
    [TestClass]
    public class TestRadioButtonInitialTextColor
    {
        [TestMethod]
        public async Task FreshlyAttachedRadioButton_HasVisibleTextColor_BeforeAnyHoverOrFocus()
        {
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var radioButton = new RadioButton { Text = "Small", GroupName = "Size" };
            ui.AddChild(radioButton);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            Assert.AreNotEqual(0, radioButton.TextColor.Value.A,
                $"Expected the RadioButton's label to have a visible (non-transparent) TextColor immediately after being attached and laid out, not the default(Color) fully-transparent value - got {radioButton.TextColor.Value}.");
        }
    }
}
