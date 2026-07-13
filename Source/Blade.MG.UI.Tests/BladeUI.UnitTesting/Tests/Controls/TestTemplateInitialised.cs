using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Controls.Templates;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Tests.Controls
{
    [TestClass]
    public class TestTemplateInitialised
    {
        [TestMethod]
        public async Task TextBox_InternalTemplate_GetsTemplateInitialised_EvenThoughItNeverCallsBase()
        {
            // TextBoxTemplate.InitTemplate() never calls base.InitTemplate() (unlike
            // TextEntryControl, which deliberately skips it but used to remember to set
            // TemplateInitialised=true itself as a workaround - TextBoxTemplate simply never did).
            // TemplateInitialised must now be guaranteed by the Parent setter itself, regardless
            // of what any InitTemplate override chain does, so this has to come out true anyway.
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var textBox = new TextBox();
            ui.AddChild(textBox);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            var template = (TextBoxTemplate)textBox.Content;
            Assert.IsTrue(template.TemplateInitialised, "TextBoxTemplate never became TemplateInitialised=true, even though its InitTemplate ran and built its Border/Label chrome - a later Parent reassignment would incorrectly rebuild it from scratch.");
        }

        [TestMethod]
        public async Task ControlAddedToAlreadyInitialisedParent_GetsItsOwnTemplateInitialised()
        {
            // Mirrors the real repro: a StackPanel that's already been laid out (and is itself
            // TemplateInitialised=true) later gets a fresh Button added to it - the Button's own
            // Parent-setter-triggered InitTemplate must still run and flag itself correctly.
            var uiManager = new FakeUIManager();
            var ui = new EmptyUI();

            var root = new StackPanel { HorizontalAlignment = HorizontalAlignmentType.Stretch, VerticalAlignment = VerticalAlignmentType.Stretch };
            ui.AddChild(root);

            uiManager.AddUI(ui);
            await uiManager.PerformLayout();

            Assert.IsTrue(root.TemplateInitialised, "Test setup error - expected the root to already be initialised.");

            var button = new Button { Text = "Button" };
            root.AddChild(button);

            Assert.IsTrue(button.TemplateInitialised, "Button's own InitTemplate should have run as soon as its Parent was set.");
        }
    }
}
