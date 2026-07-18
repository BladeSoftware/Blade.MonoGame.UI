using Blade.MG.UI.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace BladeUI.UnitTesting.Tests.Core
{
    [TestClass]
    public class TestLabelTextNullability
    {
        [TestMethod]
        public void ConstructingLabel_WithoutSettingText_LeavesTextPropertyNull()
        {
            var label = new Label { TextColor = Color.White };

            Assert.IsNull(label.Text, "Label's constructor does `Text = null;` - a plain null assignment to the Binding<string> property (no implicit conversion needed for null), not a null-valued Binding<string>. Confirms callers must set Text explicitly.");
        }
    }
}
