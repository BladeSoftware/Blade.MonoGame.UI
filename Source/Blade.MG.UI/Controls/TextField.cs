using Blade.MG.UI.Controls.Templates;

namespace Blade.MG.UI.Controls
{
    // Lean text entry: just the text and an underline, none of TextBox's border/floating-label/
    // helper-text chrome - for places that only need a compact input, not a full Material-style
    // field (e.g. ComboBox's embedded editable header, or an inline rename box for a label/tree
    // node/tab). Shares its caret/selection/keyboard behavior with TextBox via TextEntryControl.
    public class TextField : TextEntryControl
    {
        public TextField()
        {
            TemplateType = typeof(TextFieldTemplate);
        }
    }
}
