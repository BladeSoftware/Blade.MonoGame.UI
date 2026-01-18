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

        //private Color backgroundNormal = Color.White;
        //private Color backgroundHover = Color.LightGray;
        //private Color backgroundFocused = Color.White;

        //private Color textColorNormal = Color.Black;
        //private Color textColorHover = Color.Black;
        //private Color textColorFocused = Color.Black;

        //private Color borderColorNormal = Color.DarkSlateBlue;
        //private Color borderColorHover = Color.White;  // Color.Gray;
        //private Color borderColorFocused = Color.MediumSlateBlue;

        //private int borderThicknessNormal = 2;
        //private int borderThicknessHover = 2;
        //private int borderThicknessFocused = 2;

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

            //Background = Color.Transparent;
            //Background = new Binding<Color>(() => GetResourceValue<Color>("Background"));
            //Background = () => GetResourceValue<Color>("Background");
            Background = new Style<Color>("TextBox.Background");

            //Margin = new Thickness(0, 0, 0, 7); // Reserve space for the helper text
            //Padding = new Thickness(0, 7, 0, 0);

            border1 = new Border
            {
                BorderColor = Color.Black,
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
                Text = textBox.Text, // Use the Button Text
                Background = Color.Transparent,
                //TextColor = textBox.FontColor,  // Use the Button Foreground Color
                TextColor = textBox.TextColor,
                FontName = textBox.FontName, // Use the Button Font
                FontSize = textBox.FontSize, // Use the Button Font
                HorizontalTextAlignment = textBox.HorizontalTextAlignment,
                VerticalTextAlignment = textBox.VerticalTextAlignment,

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

            int helperTextHeight = 16;

            if (textBox.Variant == Variant.Standard)
            {
                border1.BorderColor = Color.Transparent;
                border1.Background = Color.Transparent;
                border1.Margin = new Thickness(0, 9, 0, helperTextHeight);
                border1.Padding = new Thickness(10, 5 + 6, 0, 5);

            }
            else if (textBox.Variant == Variant.Filled)
            {
                border1.BorderColor = Color.Transparent;
                border1.Background = Color.LightGray;
                border1.Margin = new Thickness(0, 9, 0, helperTextHeight);
                border1.Padding = new Thickness(10, 5 + 6, 0, 5);
            }
            else if (textBox.Variant == Variant.Outlined)
            {
                border1.BorderColor = Color.Black;
                border1.Background = Color.Transparent;
                border1.Margin = new Thickness(0, 5, 0, helperTextHeight);
                border1.Padding = new Thickness(10, 14, 10, 5);
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
                using var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
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
                        context.Renderer.FillRect(spriteBatch, underlineRect, Color.Black);
                    }
                    else if (textBox.Variant == Variant.Filled)
                    {
                        underlineRect = new Rectangle(border1.FinalRect.X, border1.FinalRect.Bottom - 1, border1.FinalRect.Width, 1);
                        context.Renderer.FillRect(spriteBatch, underlineRect, Color.Black);
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
                    labelSizeColor = Color.LightGray;
                    labelSizeFontSize = textBox.FontSize?.Value ?? FontService.DefaultFontSize;

                    SpriteFontBase labelFont = FontService.GetFontOrDefault(textBox.FontName?.Value, labelSizeFontSize);

                    Rectangle labelBounds = new Rectangle(FinalContentRect.X, label1.FinalContentRect.Y, FinalContentRect.Width, label1.FinalContentRect.Height);
                    labelBounds = labelBounds with { X = labelBounds.X + 8 };

                    context.Renderer.DrawString(spriteBatch, labelBounds, textBox.Label, labelFont, labelSizeColor, HorizontalAlignmentType.Left, VerticalAlignmentType.Center, Rectangle.Intersect(layoutBounds, FinalContentRect));
                }
                else
                {
                    // Text Box has Text, so shrink the label (display Label at the top-left corner of the text box)
                    labelSizeColor = Color.Blue;
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
                    Color helperTextSizeColor = Color.Gray;
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

                // Display the Cursor
                if (cursorFlashOn && textBox.HasFocus.Value)
                {
                    textBox.CursorPosition = textBox.Text.Value.Length;
                    SpriteFontBase font = FontService.GetFontOrDefault(textBox.FontName?.Value, textBox.FontSize?.Value);
                    Vector2 textSize = font.MeasureString(textBox.Text.Value[0..textBox.CursorPosition]);

                    Rectangle cursorRect = new Rectangle((int)(label1.TextRect.Left + textSize.X), (int)label1.TextRect.Top, 2, (int)label1.TextRect.Height);
                    context.Renderer.FillRect(spriteBatch, cursorRect, Color.Black, label1.FinalContentRect);
                }


            }
            finally
            {
                context.Renderer.EndBatch();
            }

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


            // Revert view state to Normal
            // TODO: Only set properties that aren't going to be re-set by the destination view state

            // TypeConverter.UpdateProperty(textBox, nameof(TextBox.TextColor), Color.Black);

            //label1.TextColor = GetResourceValue<Color>("TextColor");
            //Background = textBox.Background; // Default to the Text Box Background Color

            //// Hover State 
            //if (MouseHover)
            //{
            //    label1.TextColor = GetResourceValue<Color>("TextColor.Hover");
            //    Background = GetResourceValue<Color>("Background.Hover");
            //}

            //// Focused State
            //if (HasFocus)
            //{
            //    label1.TextColor = GetResourceValue<Color>("TextColor.Focused");
            //    Background = GetResourceValue<Color>("Background.Focused");
            //}

            switch (this.ViewState)
            {
                case ViewStates.Normal:
                    textBox.TextColor = Color.Black;
                    break;

                case ViewStates.Hover:
                    textBox.TextColor = Color.Violet;
                    break;

                case ViewStates.Focused:
                    textBox.TextColor = Color.DarkRed;
                    break;
            }




            //await base.HandleStateChangeAsync();

            // ---- TESTS ----
            //label1.TextColor = ResourceDict.GetResourceValue<Color>("", "TextColor" + ViewState);
            ////Background = ResourceDict.GetResourceValue<Color>("", "Background" + ViewState());

            //TypeConverter.UpdateProperty(textBox, nameof(TextBox.Text), "Hello, World!");
            //TypeConverter.UpdateProperty(textBox, nameof(TextBox.MaxLength), "123");
            //TypeConverter.UpdateProperty(textBox, nameof(TextBox.FontSize), "68");

            //TypeConverter.UpdateProperty(textBox, nameof(TextBox.Background), Color.Purple.ToString());

            //TypeConverter.UpdateProperty(textBox, nameof(TextBox.Background), Color.Purple);
            //TypeConverter.UpdateProperty(textBox, nameof(TextBox.Background), new Binding<Color>(Color.Purple));


            //Background = new Style<Color>("Background");

            //label1.TextColor = GetResourceViewState<Color>("TextColor");
            //Background = GetResourceViewState<Color>("Background");



            // Very slow
            //Background = (Func<Color>)(() => GetResourceValue<Color>("Background"));
            //Background = new Binding<Color>(() => GetResourceValue<Color>("Background")); 


            //// Normal State
            //label1.TextColor = GetResourceValue<Color>("TextColor");
            //Background = textBox.Background; // Default to the Text Box Background Color

            //// Hover State 
            //if (MouseHover)
            //{
            //    label1.TextColor = GetResourceValue<Color>("TextColor.Hover");
            //    Background = GetResourceValue<Color>("Background.Hover");
            //}

            //// Focused State
            //if (HasFocus)
            //{
            //    label1.TextColor = GetResourceValue<Color>("TextColor.Focused");
            //    Background = GetResourceValue<Color>("Background.Focused");
            //}


        }

    }
}
