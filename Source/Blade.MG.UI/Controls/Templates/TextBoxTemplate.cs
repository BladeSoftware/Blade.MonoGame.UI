using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Blade.MG.UI.Models;
using Blade.MG.UI.Services;
using FontStashSharp;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls.Templates
{
    public class TextBoxTemplate : Control
    {
        private Border border1;
        private Label label1;

        private DateTime cursorTime;
        private bool cursorFlashOn = false;

        public TextBoxTemplate()
        {
            IsHitTestVisible = false;  // The template itself should not be hit-testable
            CanFocus = false;           // The template itself should not be focusable
        }

        protected override void InitTemplate()
        {
            var textBox = ParentAs<TextBox>();

            // Link to the TextBox's own Background binding so a developer can override it
            // directly (textBox.Background = ...) as well as via SetStyleOverride.
            Background = textBox.Background;
            ApplyThemedValue(textBox, Background, nameof(TextBox.Background), Theme.Surface);

            //Margin = new Thickness(0, 0, 0, 7); // Reserve space for the helper text
            //Padding = new Thickness(0, 7, 0, 0);

            border1 = new Border
            {
                // Link to the TextBox's own BorderColor binding so a developer can override it
                // directly (textBox.BorderColor = ...) as well as via SetStyleOverride.
                BorderColor = textBox.BorderColor,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(0),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,

                // Prevent child controls from receiving input events
                IsHitTestVisible = false,
                CanFocus = false
            };

            label1 = new Label()
            {
                // Linked via a getter/setter Binding rather than a plain `Text = textBox.Text`
                // snapshot assignment. Label.Text is a plain auto-property, so that bare
                // assignment *looks* like it adopts textBox's own Binding<string> by
                // reference (making every future read/write of either side visible through
                // both) - but in practice label1's text never reflected what was typed or
                // programmatically selected. Building the Binding from a getter/setter pair
                // instead means every access reads/writes straight through to textBox.Text.Value
                // itself, live, with nothing to snapshot or fall out of sync - so it holds up
                // regardless of what was breaking the reference-sharing assumption above.
                Text = new Binding<string>(() => textBox.Text.Value, v => textBox.Text.Value = v),
                Background = Color.Transparent,
                TextColor = new Binding<Color>(() => textBox.TextColor.Value, v => textBox.TextColor.Value = v),
                HorizontalTextAlignment = new Binding<HorizontalAlignmentType>(() => textBox.HorizontalTextAlignment.Value, v => textBox.HorizontalTextAlignment.Value = v),
                VerticalTextAlignment = new Binding<VerticalAlignmentType>(() => textBox.VerticalTextAlignment.Value, v => textBox.VerticalTextAlignment.Value = v),

                // FontName/FontSize are left as plain snapshot assignments (not linked): unlike
                // the properties above, TextBox allows these to be a literal null Binding
                // (meaning "use the default font"), and LabelTemplate already reads them via
                // null-conditional access (label.FontName?.Value) - a live getter closure can't
                // return null through a non-nullable Binding<T> the same way, and neither
                // FontName nor FontSize changes after a TextBox is constructed in any current
                // usage, so there's no staleness risk here to link against.
                FontName = textBox.FontName,
                FontSize = textBox.FontSize,

                // Prevent child controls from receiving input events
                IsHitTestVisible = false,
                CanFocus = false
            };


            border1.Content = label1;

            Content = border1;

            cursorTime = DateTime.Now;
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            var textBox = ParentAs<TextBox>();

            // These margins/paddings exist purely to reserve room for the floating label
            // (above/inside the box) and the helper text (below it) - both drawn by
            // RenderControl below. A TextBox with neither set (e.g. ComboBox's embedded
            // editable header, which uses TextBox purely as a compact input with no Material
            // label/helper chrome) has nothing to draw there, so reserving the space anyway
            // just inflates the control well past what its content needs - which is exactly
            // what was blowing out ComboBox's fixed-height header.
            bool hasLabel = !string.IsNullOrEmpty(textBox.Label);
            bool hasHelperText = !string.IsNullOrEmpty(textBox.HelperText);

            int helperTextHeight = hasHelperText ? 16 : 0;

            if (textBox.Variant == Variant.Standard)
            {
                ApplyThemedValue(textBox, border1.BorderColor, nameof(TextBox.BorderColor), Color.Transparent);
                border1.Background = Color.Transparent;
                border1.CornerRadius = new CornerRadius(0);
                border1.Margin = new Thickness(0, hasLabel ? 9 : 0, 0, helperTextHeight);
                // With no label, match DisplayLabel/label1's own implicit zero vertical
                // padding exactly (rather than merely shrinking it) - a fixed-height host like
                // ComboBox's 40px header budgets for a plain Label's zero-padding footprint,
                // and even the "compact" 5px top/bottom this used to fall back to was still
                // enough to push the box past that budget and get clipped.
                border1.Padding = hasLabel ? new Thickness(10, 11, 0, 5) : new Thickness(10, 0, 0, 0);

            }
            else if (textBox.Variant == Variant.Filled)
            {
                ApplyThemedValue(textBox, border1.BorderColor, nameof(TextBox.BorderColor), Color.Transparent);
                border1.Background = Theme.SurfaceVariant;
                border1.CornerRadius = new CornerRadius(8, 8, 0, 0); // Rounded top corners, flat above the underline
                border1.Margin = new Thickness(0, hasLabel ? 9 : 0, 0, helperTextHeight);
                border1.Padding = hasLabel ? new Thickness(10, 11, 0, 5) : new Thickness(10, 0, 0, 0);
            }
            else if (textBox.Variant == Variant.Outlined)
            {
                ApplyThemedValue(textBox, border1.BorderColor, nameof(TextBox.BorderColor), Theme.Outline);
                border1.Background = Color.Transparent;
                border1.CornerRadius = new CornerRadius(8);
                border1.Margin = new Thickness(0, hasLabel ? 5 : 0, 0, helperTextHeight);
                border1.Padding = hasLabel ? new Thickness(10, 14, 10, 5) : new Thickness(10, 0, 10, 0);
            }

            base.Measure(context, ref availableSize, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);
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

            var textBox = ParentAs<TextBox>();

            try
            {
                var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
                context.Renderer.ClipToRect(layoutBounds);


                // If the Variant = Filled, then fill the textbox including the top margin
                if (textBox.Variant == Variant.Filled)
                {
                    Rectangle filledRect = border1.FinalRect with { Y = border1.FinalRect.Top - border1.Margin.Value.Top, Height = border1.Margin.Value.Top };
                    context.Renderer.FillRect(spriteBatch, filledRect, border1.Background.Value);
                }


                // Display the underline
                if (textBox.Underline)
                {
                    Rectangle underlineRect;

                    if (textBox.Variant == Variant.Standard)
                    {
                        underlineRect = new Rectangle(label1.FinalRect.X, border1.FinalRect.Bottom - 1, border1.FinalRect.Width, 1);
                        context.Renderer.FillRect(spriteBatch, underlineRect, Theme.Outline);
                    }
                    else if (textBox.Variant == Variant.Filled)
                    {
                        underlineRect = new Rectangle(border1.FinalRect.X, border1.FinalRect.Bottom - 1, border1.FinalRect.Width, 1);
                        context.Renderer.FillRect(spriteBatch, underlineRect, Theme.Outline);
                    }

                }


                // Display the Label - Either in the text box or above it
                Color labelSizeColor;
                float labelSizeFontSize;

                // If the text box has focus OR contains text, then shrink the label
                bool shrinkLabel = textBox.HasFocus.Value;
                if (!string.IsNullOrEmpty(textBox.Text?.Value))
                {
                    shrinkLabel = true;
                }

                if (!shrinkLabel)
                {
                    // Text Box Text is Empty, so display Label in the text box
                    labelSizeColor = Theme.OnSurfaceVariant;
                    labelSizeFontSize = textBox.FontSize?.Value ?? FontService.DefaultFontSize;

                    SpriteFontBase labelFont = FontService.GetFontOrDefault(textBox.FontName?.Value, labelSizeFontSize);

                    Rectangle labelBounds = new Rectangle(FinalContentRect.X, label1.FinalContentRect.Y, FinalContentRect.Width, label1.FinalContentRect.Height);
                    labelBounds = labelBounds with { X = labelBounds.X + 8 };

                    context.Renderer.DrawString(spriteBatch, labelBounds, textBox.Label, labelFont, labelSizeColor, HorizontalAlignmentType.Left, VerticalAlignmentType.Center, Rectangle.Intersect(layoutBounds, FinalContentRect));
                }
                else
                {
                    // Text Box has Text, so shrink the label (display Label at the top-left corner of the text box)
                    labelSizeColor = Theme.Primary;
                    labelSizeFontSize = 15;

                    SpriteFontBase labelFont = FontService.GetFontOrDefault(textBox.FontName?.Value, labelSizeFontSize);
                    Vector2 labelSize = labelFont.MeasureString(textBox.Label);

                    Rectangle labelBounds;

                    if (textBox.Variant == Variant.Outlined)
                    {
                        labelBounds = new Rectangle(FinalContentRect.Left + 5, border1.FinalRect.Top - 7, (int)labelSize.X + 10, (int)labelSize.Y);
                        context.Renderer.FillRect(spriteBatch, labelBounds, textBox.Background.Value);
                    }
                    else
                    {
                        labelBounds = new Rectangle(FinalContentRect.Left + 5, textBox.FinalRect.Top + 2, (int)labelSize.X + 10, (int)labelSize.Y);
                    }

                    labelBounds = labelBounds with { X = labelBounds.X + 5 };

                    context.Renderer.DrawString(spriteBatch, labelBounds, textBox.Label, labelFont, labelSizeColor, HorizontalAlignmentType.Left, VerticalAlignmentType.Top, layoutBounds);
                }


                // Display the helper text
                if (!string.IsNullOrEmpty(textBox.HelperText))
                {

                    // Text Box has Text, so display Label at the top-left corner of the text box
                    Color helperTextSizeColor = Theme.OnSurfaceVariant;
                    float helperTextSizeFontSize = 14;

                    SpriteFontBase helperTextSizeFont = FontService.GetFontOrDefault(textBox.FontName?.Value, helperTextSizeFontSize);
                    Vector2 helperTextSize = helperTextSizeFont.MeasureString(textBox.HelperText);

                    Rectangle helperTextBounds;

                    helperTextBounds = new Rectangle(FinalContentRect.Left + 2, border1.FinalRect.Bottom + 3, (int)helperTextSize.X + 10, (int)helperTextSize.Y);

                    if (textBox.Variant == Variant.Standard)
                    {
                        helperTextBounds = helperTextBounds with { X = helperTextBounds.X + 8 };
                    }

                    context.Renderer.DrawString(spriteBatch, helperTextBounds, textBox.HelperText, helperTextSizeFont, helperTextSizeColor, HorizontalAlignmentType.Left, VerticalAlignmentType.Top, textBox.FinalRect);
                }

                // Display the Selection highlight and the Cursor
                {
                    string editText = textBox.Text.Value ?? "";
                    SpriteFontBase font = FontService.GetFontOrDefault(textBox.FontName?.Value, textBox.FontSize?.Value);

                    if (textBox.SelectionLength > 0)
                    {
                        int selectionStart = Math.Clamp(textBox.SelectionStart, 0, editText.Length);
                        int selectionEnd = Math.Clamp(selectionStart + textBox.SelectionLength, 0, editText.Length);

                        float startX = font.MeasureString(editText[0..selectionStart]).X;
                        float endX = font.MeasureString(editText[0..selectionEnd]).X;

                        Rectangle selectionRect = new Rectangle((int)(label1.TextRect.Left + startX), (int)label1.TextRect.Top, (int)(endX - startX), (int)label1.TextRect.Height);
                        context.Renderer.FillRect(spriteBatch, selectionRect, new Color(Theme.Primary, 0.35f), label1.FinalContentRect);
                    }

                    if (cursorFlashOn && textBox.HasFocus.Value)
                    {
                        int cursorPosition = Math.Clamp(textBox.CursorPosition, 0, editText.Length);
                        Vector2 textSize = font.MeasureString(editText[0..cursorPosition]);

                        Rectangle cursorRect = new Rectangle((int)(label1.TextRect.Left + textSize.X), (int)label1.TextRect.Top, 2, (int)label1.TextRect.Height);
                        context.Renderer.FillRect(spriteBatch, cursorRect, Theme.OnSurface, label1.FinalContentRect);
                    }
                }


            }
            finally
            {
                context.Renderer.EndBatch();
            }

        }

        /// <summary>
        /// Maps an absolute screen X coordinate to the nearest character index in the text
        /// box's current text, based on the text's last-rendered position (label1.TextRect).
        /// Used by TextBox for click-to-position-caret and click-drag selection.
        /// </summary>
        public int GetCharacterIndexAtX(float screenX)
        {
            var textBox = ParentAs<TextBox>();
            string text = textBox.Text.Value ?? "";

            if (text.Length == 0)
            {
                return 0;
            }

            float relativeX = screenX - label1.TextRect.Left;
            if (relativeX <= 0)
            {
                return 0;
            }

            SpriteFontBase font = FontService.GetFontOrDefault(textBox.FontName?.Value, textBox.FontSize?.Value);

            float previousWidth = 0f;
            for (int i = 1; i <= text.Length; i++)
            {
                float width = font.MeasureString(text[0..i]).X;
                float charCenter = (previousWidth + width) / 2f;

                if (relativeX < charCenter)
                {
                    return i - 1;
                }

                previousWidth = width;
            }

            return text.Length;
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


        //public T GetResourceViewState<T>(string property)
        //{
        //    string resourceKey = ResourceKey;
        //    return GetResourceValue<T>($"{property}{ViewState()}");
        //}


        protected override void HandleStateChange()
        {
            var textBox = ParentAs<TextBox>();

            // Text color and background are theme-driven and developer-overridable.
            // Border color is re-applied every Measure() pass since it depends on Variant,
            // so it isn't set reactively here.
            ApplyThemedValue(textBox, textBox.TextColor, nameof(TextBox.TextColor), Theme.OnSurface);
            ApplyThemedValue(textBox, Background, nameof(TextBox.Background), Theme.Surface);
        }

    }
}
