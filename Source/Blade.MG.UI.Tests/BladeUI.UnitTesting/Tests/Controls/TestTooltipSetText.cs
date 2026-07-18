using Blade.MG.UI.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BladeUI.UnitTesting.Tests.Controls
{
    /// <summary>
    /// Tooltip.SetText used to throw a NullReferenceException on its very first call -
    /// Tooltip.LoadContent constructed its internal Label without ever setting Text, and
    /// Label's own constructor does `Text = null;` (a plain null assignment to the reference-
    /// type Binding&lt;string&gt; property - no implicit conversion needed for a null literal -
    /// see TestLabelTextNullability), leaving label.Text genuinely null rather than a
    /// null-valued Binding. SetText's `label.Text.Value = text;` then dereferenced that null.
    /// Called directly here (bypassing Tooltip.Attach's async hover-delay dispatch, which was
    /// observed to silently swallow this exact exception rather than surface it to the test).
    /// </summary>
    [TestClass]
    public class TestTooltipSetText
    {
        [TestMethod]
        public void SetText_AfterLoadContent_DoesNotThrow_AndUpdatesLabel()
        {
            var tooltip = new Tooltip();
            tooltip.LoadContent();

            tooltip.SetText("Hello");

            var label = (Label)tooltip.Content;
            Assert.AreEqual("Hello", label.Text.Value);
        }
    }
}
