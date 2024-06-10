using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Blade.MG.UI.Controls.Templates
{
    public class ButtonTemplate : Control
    {
        private Button button;
        private Label label1;
        private Border border1;

        //private Color backgroundNormal = new Color(Color.DarkSlateBlue, 0.95f);
        //private Color backgroundHover = new Color(Color.MediumSlateBlue, 0.95f);
        //private Color backgroundFocused = new Color(Color.SlateBlue, 0.95f);

        //private Color textColorNormal = Color.White;
        //private Color textColorHover = Color.White;
        //private Color textColorFocused = Color.White;

        //private Color borderColorNormal = Color.MediumSlateBlue;
        //private Color borderColorHover = Color.White;  // Color.Gray;
        //private Color borderColorFocused = Color.LightSlateGray;

        private int borderThicknessNormal = 2;
        private int borderThicknessHover = 2;
        private int borderThicknessFocused = 2;


        public ButtonTemplate()
        {
        }

        protected override void InitTemplate()
        {
            button = Parent as Button;

            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;

            //this.HorizontalAlignment = button.HorizontalAlignment;
            //this.VerticalAlignment = button.VerticalAlignment;
            //this.HorizontalContentAlignment = button.HorizontalContentAlignment;
            //this.VerticalContentAlignment = button.VerticalContentAlignment;

            Margin = button.Margin;
            //Padding = new Thickness(0);

            //button.Border = new Border()
            border1 = new Border()
            {
                CornerRadius = 9f,
                Background = Theme.Outline, //backgroundNormal,
                //HorizontalAlignment = this.HorizontalAlignment, //HorizontalAlignmentType.Center,
                //VerticalAlignment = this.VerticalAlignment, //VerticalAlignmentType.Center,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                //HorizontalContentAlignment = HorizontalAlignmentType.Stretch,
                //VerticalContentAlignment = VerticalAlignmentType.Stretch,
                Margin = new Thickness(0),
                Padding = new Thickness(0)
            };


            label1 = new Label()
            {
                Text = button.Text, // Use the Button Text
                TextColor = button.TextColor ?? Color.Black,  // Use the Button Foreground Color
                FontName = button.FontName, // Use the Button Font
                FontSize = button.FontSize, // Use the Button Font
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                HorizontalTextAlignment = button.HorizontalTextAlignment,
                VerticalTextAlignment = button.VerticalTextAlignment,
                Padding = new Thickness(2),
                Margin = new Thickness(0)
            };

            border1.Content = label1;
            Content = border1;

        }

        // ---=== Handle State Changes ===---

        public override async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        {
            await base.HandleFocusChangedEventAsync(uiWindow, uiEvent);

            StateHasChanged();
        }

        public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            if (uiEvent.Hover == false || FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                await base.HandleHoverChangedAsync(uiWindow, uiEvent);
            }

            StateHasChanged();
        }

        protected override void HandleStateChange()
        {
            //await base.HandleStateChangeAsync();

            // Normal State
            label1.TextColor.Value = Theme.OnPrimaryContainer; //textColorNormal;
            border1.Background.Value = Theme.PrimaryContainer; //backgroundNormal;
            border1.BorderThickness.Value = borderThicknessNormal;
            border1.BorderColor.Value = Theme.Outline; //borderColorNormal;


            // Focused State
            if (HasFocus.Value)
            {
                //label1.TextColor.Value =  textColorFocused;
                //border1.Background.Value = backgroundFocused;
                //border1.BorderThickness.Value = borderThicknessFocused;
                //border1.BorderColor.Value = borderColorFocused;
                border1.BorderColor.Value = Theme.Tertiary;
            }


            // Hover State 
            if (MouseHover.Value)
            {
                label1.TextColor.Value = Theme.OnSecondaryContainer; // textColorHover;
                border1.Background.Value = Theme.SecondaryContainer; //backgroundHover;
                border1.BorderThickness.Value = borderThicknessHover;
                border1.BorderColor.Value = Theme.Tertiary; //borderColorHover;
            }


        }

    }
}
