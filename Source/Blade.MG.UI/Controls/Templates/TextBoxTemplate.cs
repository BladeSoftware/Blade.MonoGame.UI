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

            // Use the Parent Button's ContentAlignment values for the lable text placement
            label1.HorizontalContentAlignment = HorizontalContentAlignment;
            label1.VerticalContentAlignment = VerticalContentAlignment;


            Content = label1;

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
