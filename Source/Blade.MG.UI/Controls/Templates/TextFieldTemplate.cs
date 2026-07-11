using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Blade.MG.UI.Services;
using FontStashSharp;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls.Templates
{
    // Minimal visual chrome for TextField: just the text and (optionally) an underline - no
    // border, no floating label, no helper text. See TextBoxTemplate for the full-chrome
    // equivalent; the two share their caret hit-testing and selection/cursor drawing via
    // TextEntryVisuals so they can't drift apart.
    public class TextFieldTemplate : Control, ITextEntryTemplate
    {
        private Label label1;

        private DateTime cursorTime;
        private bool cursorFlashOn = false;

        public TextFieldTemplate()
        {
            IsHitTestVisible = false; // The template itself should not be hit-testable
            CanFocus = false;         // The template itself should not be focusable
        }

        protected override void InitTemplate()
        {
            var textField = ParentAs<TextField>();

            // Themed default is transparent (not Theme.Surface, unlike TextBoxTemplate) - a
            // lean field is meant to sit inline within other UI (e.g. ComboBox's header) without
            // painting its own background box. Still goes through the style-override system so
            // an app can opt into a solid background if it wants one.
            ApplyThemedValue(textField, Background, nameof(TextField.Background), Color.Transparent);

            label1 = new Label()
            {
                // Linked via a getter/setter Binding rather than a plain `Text = textField.Text`
                // snapshot assignment - see TextBoxTemplate's label1 for why (Label.Text is a
                // plain auto-property, so a bare assignment doesn't adopt textField's Binding by
                // reference).
                Text = new Binding<string>(() => textField.Text.Value, v => textField.Text.Value = v),
                Background = Color.Transparent,
                TextColor = new Binding<Color>(() => textField.TextColor.Value, v => textField.TextColor.Value = v),
                HorizontalTextAlignment = new Binding<HorizontalAlignmentType>(() => textField.HorizontalTextAlignment.Value, v => textField.HorizontalTextAlignment.Value = v),
                VerticalTextAlignment = new Binding<VerticalAlignmentType>(() => textField.VerticalTextAlignment.Value, v => textField.VerticalTextAlignment.Value = v),

                FontName = textField.FontName,
                FontSize = textField.FontSize,

                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,

                // Prevent child controls from receiving input events
                IsHitTestVisible = false,
                CanFocus = false
            };

            Content = label1;

            cursorTime = DateTime.Now;
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            base.RenderControl(context, layoutBounds, parentTransform);

            double elapsedMillis = (DateTime.Now - cursorTime).TotalMilliseconds;
            if (elapsedMillis > 500)
            {
                cursorTime = DateTime.Now;
                cursorFlashOn = !cursorFlashOn;
            }

            var textField = ParentAs<TextField>();

            // Recomputed fresh every frame - rather than only reactively in HandleStateChange -
            // so it can never go stale regardless of event-cascade ordering/timing. See
            // TextBoxTemplate.RenderControl's equivalent comment for the full explanation (this
            // is TextField's only focus indicator, so getting it right matters even more here).
            // isFocused/isHovered fold in IsEnabled so a disabled field never shows the
            // interactive-looking hover/focus treatment, only the dimmed Disabled color.
            bool isEnabled = textField.IsEnabled.Value;
            bool isFocused = isEnabled && textField.HasFocus.Value;
            bool isHovered = isEnabled && MouseHover.Value;
            Color indicatorColor = !isEnabled ? Theme.Disabled : (isFocused ? Theme.Primary : (isHovered ? Theme.OnSurface : Theme.OnSurfaceVariant));
            ApplyThemedValue(textField, textField.UnderlineColor, nameof(TextEntryControl.UnderlineColor), indicatorColor);
            ApplyThemedValue(textField, textField.TextColor, nameof(TextField.TextColor), isEnabled ? Theme.OnSurface : Theme.Disabled);

            try
            {
                var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
                context.Renderer.ClipToRect(layoutBounds);

                if (textField.Underline)
                {
                    int thickness = isFocused ? 2 : 1;
                    Rectangle underlineRect = new Rectangle(label1.FinalRect.X, FinalRect.Bottom - thickness, label1.FinalRect.Width, thickness);
                    context.Renderer.FillRect(spriteBatch, underlineRect, textField.UnderlineColor.Value);
                }

                string editText = textField.Text.Value ?? "";
                SpriteFontBase font = FontService.GetFontOrDefault(textField.FontName?.Value, textField.FontSize?.Value);

                TextEntryVisuals.DrawSelectionAndCursor(
                    context, spriteBatch, editText, label1.TextRect, label1.FinalContentRect,
                    textField.SelectionStart, textField.SelectionLength, textField.CursorPosition,
                    cursorFlashOn, isFocused, font,
                    new Color(Theme.Primary, 0.35f), Theme.OnSurface);
            }
            finally
            {
                context.Renderer.EndBatch();
            }
        }

        /// <summary>
        /// Maps an absolute screen X coordinate to the nearest character index in the field's
        /// current text, based on the text's last-rendered position (label1.TextRect). Used by
        /// TextEntryControl for click-to-position-caret and click-drag selection.
        /// </summary>
        public int GetCharacterIndexAtX(float screenX)
        {
            var textField = ParentAs<TextField>();
            string text = textField.Text.Value ?? "";
            SpriteFontBase font = FontService.GetFontOrDefault(textField.FontName?.Value, textField.FontSize?.Value);

            return TextEntryVisuals.GetCharacterIndexAtX(screenX, text, label1.TextRect, font);
        }

        // ---=== Handle State Changes ===---

        public override async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        {
            await base.HandleFocusChangedEventAsync(uiWindow, uiEvent);

            StateHasChanged();
        }

        public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            await base.HandleHoverChangedAsync(uiWindow, uiEvent);

            StateHasChanged();
        }

        protected override void HandleStateChange()
        {
            // Text/underline color and thickness are all recomputed fresh every frame in
            // RenderControl instead of here - see the comment there for why.
        }
    }
}
