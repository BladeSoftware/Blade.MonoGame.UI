namespace Blade.MG.UI.Controls.Templates
{
    // Implemented by every visual template that backs a TextEntryControl (TextBoxTemplate's
    // full chrome, TextFieldTemplate's minimal underline-only chrome) so TextEntryControl's
    // shared mouse-to-caret handling (click-to-position, drag-select) can map a screen X
    // coordinate to a character index without knowing which concrete template it's hosting.
    public interface ITextEntryTemplate
    {
        int GetCharacterIndexAtX(float screenX);
    }
}
