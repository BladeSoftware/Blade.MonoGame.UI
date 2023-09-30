using System;
using System.Collections.Generic;
using System.Text;
using BladeGame.BladeUI.Components;
using BladeGame.Shared.BladeUI;
using BladeGame.Shared.BladeUI.Components;
using BladeGame.Shared.BladeUI.Events;
using Microsoft.Xna.Framework;

namespace BladeGame.BladeUI.Controls.Templates
{
    public class ButtonTemplate : Control
    {
        private Button button;
        private Label label1;
        private Border border1;

        private Color backgroundNormal = new Color(Color.DarkSlateBlue, 0.55f);
        private Color backgroundHover = new Color(Color.SlateBlue, 0.80f);
        private Color backgroundFocused = new Color(Color.SlateBlue, 0.80f);

        private Color textColorNormal = Color.White;
        private Color textColorHover = Color.White;
        private Color textColorFocused = Color.White;

        private Color borderColorNormal = Color.DarkSlateBlue;
        private Color borderColorHover = Color.White;  // Color.Gray;
        private Color borderColorFocused = Color.MediumSlateBlue;

        private int borderThicknessNormal = 2;
        private int borderThicknessHover = 2;
        private int borderThicknessFocused = 2;


        public ButtonTemplate()
        {
        }

        protected override void InitTemplate()
        {
            button = Parent as Button;

            this.HorizontalAlignment = Parent.HorizontalAlignment;
            this.VerticalAlignment = Parent.VerticalAlignment;
            this.HorizontalContentAlignment = Parent.HorizontalContentAlignment;
            this.VerticalContentAlignment = Parent.VerticalContentAlignment;

            border1 = new Border()
            {
                CornerRadius = 9f,
                Background = backgroundNormal,
                HorizontalAlignment = HorizontalAlignmentType.Center,
                VerticalAlignment = VerticalAlignmentType.Center,
                HorizontalContentAlignment = HorizontalAlignmentType.Stretch,
                VerticalContentAlignment = VerticalAlignmentType.Stretch,
                Margin = new Thickness(0),
                Padding = new Thickness(20, 10)
            };


            label1 = new Label()
            {
                Text = button.Text, // Use the Button Text
                TextColor = button.FontColor,  // Use the Button Foreground Color
                SpriteFont = button.SpriteFont // Use the Button Font
            };

            // Use the Parent Button's ContentAlignment values for the lable text placement
            label1.HorizontalContentAlignment = HorizontalContentAlignment;
            label1.VerticalContentAlignment = VerticalContentAlignment;


            border1.Content = label1;
            Content = border1;

          
        }

        // ---=== Handle State Changes ===---

        public override void HandleFocusChangedEvent(UIFocusChangedEvent uiEvent)
        {
            base.HandleFocusChangedEvent(uiEvent);

            HandleStateChange();
        }

        public override void HandleHoverChanged(UIHoverChangedEvent uiEvent)
        {
            base.HandleHoverChanged(uiEvent);

            HandleStateChange();
        }

        protected override void HandleStateChange()
        {
            // Normal State
            label1.TextColor.Value = textColorNormal;
            border1.Background.Value = backgroundNormal;
            border1.BorderThickness.Value = borderThicknessNormal;
            border1.BorderColor.Value = borderColorNormal;


            // Focused State
            if (Focused.Value)
            {
                label1.TextColor.Value = textColorFocused;
                border1.Background.Value = backgroundFocused;
                border1.BorderThickness.Value = borderThicknessFocused;
                border1.BorderColor.Value = borderColorFocused;
            }


            // Hover State 
            if (MouseHover.Value)
            {
                label1.TextColor.Value = textColorHover;
                border1.Background.Value = backgroundHover;
                border1.BorderThickness.Value = borderThicknessHover;
                border1.BorderColor.Value = borderColorHover;
            }


        }

    }
}
