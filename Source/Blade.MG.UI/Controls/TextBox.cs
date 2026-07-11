using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    // Full Material-style text entry: border, floating label, helper text. For a lean field with
    // just an underline and no other chrome (e.g. ComboBox's embedded editable header, or an
    // inline rename box), use TextField instead - both share the same caret/selection/keyboard
    // behavior via TextEntryControl.
    public class TextBox : TextEntryControl
    {
        private Binding<Color> borderColor = new Binding<Color>();
        public Binding<Color> BorderColor { get => borderColor; set => SetField(ref borderColor, value); }

        public Variant Variant { get; set; }
        public string Label { get; set; }
        public string HelperText { get; set; }
        public bool ShrinkLabel { get; set; } // Label stays Shrunk and doesn't fill the textbox if the text is empty

        public TextBox()
        {
            TemplateType = typeof(TextBoxTemplate);

            Variant = Variant.Standard;
            Label = null;
            HelperText = null;

            ShrinkLabel = false;
        }
    }
}
