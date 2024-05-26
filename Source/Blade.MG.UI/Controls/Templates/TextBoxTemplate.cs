using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls.Templates
{
    public class TextBoxTemplate : Control
    {
        private TextBox textBox;
        private Label label1;
        //private Border border1;

        private Color backgroundNormal = Color.White;
        private Color backgroundHover = Color.LightGray;
        private Color backgroundFocused = Color.White;

        private Color textColorNormal = Color.Black;
        private Color textColorHover = Color.Black;
        private Color textColorFocused = Color.Black;

        //private Color borderColorNormal = Color.DarkSlateBlue;
        //private Color borderColorHover = Color.White;  // Color.Gray;
        //private Color borderColorFocused = Color.MediumSlateBlue;

        //private int borderThicknessNormal = 2;
        //private int borderThicknessHover = 2;
        //private int borderThicknessFocused = 2;


        public TextBoxTemplate()
        {
        }

        protected override void InitTemplate()
        {
            textBox = Parent as TextBox;

            HorizontalAlignment = Parent.HorizontalAlignment;
            VerticalAlignment = Parent.VerticalAlignment;
            HorizontalContentAlignment = Parent.HorizontalContentAlignment;
            VerticalContentAlignment = Parent.VerticalContentAlignment;

            label1 = new Label()
            {
                Text = textBox.Text, // Use the Button Text
                //TextColor = textBox.FontColor,  // Use the Button Foreground Color
                FontName = textBox.FontName, // Use the Button Font
                FontSize = textBox.FontSize // Use the Button Font
            };

            // Use the Parent Button's ContentAlignment values for the label text placement
            label1.HorizontalAlignment = HorizontalContentAlignment;
            label1.VerticalAlignment = VerticalContentAlignment;
            label1.HorizontalContentAlignment = HorizontalContentAlignment;
            label1.VerticalContentAlignment = VerticalContentAlignment;


            Content = label1;

        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            base.RenderControl(context, layoutBounds, parentTransform);

            var textBox = ParentAs<TextBox>();

            try
            {
                using var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
                context.Renderer.ClipToRect(layoutBounds);

                //Rectangle boxRect = textBox.FinalContentRect;
                Rectangle boxRect = label1.FinalContentRect;

                //context.Renderer.FillRoundedRect(spriteBatch, boxRect, boxRect.Width / 4, Color.LightGray);
                context.Renderer.DrawRoundedRect(spriteBatch, boxRect, 0, Color.Black, 2f);

                //if (checkbox.IsChecked?.Value == null)
                //{
                //    // Indeterminate
                //    var insideRect = boxRect;
                //    insideRect.Inflate(-3f, -3f);

                //    context.Renderer.FillRoundedRect(spriteBatch, insideRect, 4, new Color(Color.Black, 1f));
                //}
                //else if (checkbox.IsChecked?.Value == true)
                //{
                //    // Checked
                //    if (img != null)
                //    {
                //        spriteBatch.Draw(img, boxRect, Color.Red);
                //    }
                //}
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

        protected override void HandleStateChange()
        {
            //await base.HandleStateChangeAsync();

            // Normal State
            label1.TextColor.Value = textColorNormal;
            Background.Value = backgroundNormal;

            // Hover State 
            if (MouseHover.Value)
            {
                label1.TextColor.Value = textColorHover;
                Background.Value = backgroundHover;
            }

            // Focused State
            if (HasFocus.Value)
            {
                label1.TextColor.Value = textColorFocused;
                Background.Value = backgroundFocused;
            }


        }

    }
}
