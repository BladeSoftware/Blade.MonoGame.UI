using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI.Controls.Templates
{
    public class ListViewItemTemplate : Control, IItemTemplate
    {
        public Label label1;

        private Color backgroundNormal = new Color(Color.DarkSlateBlue, 0.55f);
        private Color backgroundHover = new Color(Color.SlateBlue, 0.80f);
        private Color backgroundFocused = new Color(Color.SlateBlue, 0.80f);

        private Color textColorNormal = Color.White;
        private Color textColorHover = Color.White;
        private Color textColorFocused = Color.White;


        private Color borderColorNormal = Color.Orange;

        private int borderThicknessNormal = 2;

        [JsonIgnore]
        [XmlIgnore]
        private Binding<SpriteFont> SpriteFont { get; set; }

        public ListViewItemTemplate()
        {
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            IsHitTestVisible = true;

            string item = DataContext?.ToString() ?? "null";

            //this.HorizontalAlignment = Parent.HorizontalContentAlignment;
            //this.VerticalAlignment = Parent.VerticalContentAlignment;

            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;
            //HorizontalContentAlignment = HorizontalAlignmentType.Left; //Parent.HorizontalContentAlignment;
            //VerticalContentAlignment = VerticalAlignmentType.Center; //Parent.VerticalContentAlignment;

            label1 = new Label()
            {
                Height = "40px",
                Text = item,
                TextColor = Color.Black,
                Background = Color.Transparent,
                //Width = 100,
                //Height = 100,
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(8, 0, 0, 0),

                //SpriteFont = button.SpriteFont // Use the Button Font
            };

            // Use the Parent Button's ContentAlignment values for the lable text placement
            //label1.HorizontalContentAlignment = HorizontalContentAlignment;
            //label1.VerticalContentAlignment = VerticalContentAlignment;


            Content = label1;


        }

        // ---=== Handle State Changes ===---

        public override async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        {
            HasFocus = uiEvent.Focused;

            await base.HandleFocusChangedEventAsync(uiWindow, uiEvent);
            StateHasChanged();
        }

        public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            MouseHover = uiEvent.Hover;

            await base.HandleHoverChangedAsync(uiWindow, uiEvent);
            StateHasChanged();
        }

        protected override void HandleStateChange()
        {
            //await base.HandleStateChangeAsync();

            // Normal State
            ((UIComponentDrawable)Content).Background = Color.Transparent;
            //((Grid)Content).Background = Color.Transparent;

            //label1.TextColor.Value = textColorNormal;
            //border1.Background.Value = backgroundNormal;
            //border1.BorderThickness.Value = borderThicknessNormal;
            //border1.BorderColor.Value = borderColorNormal;


            //// Focused State
            //if (Focused.Value)
            //{
            //    label1.TextColor.Value = textColorFocused;
            //    border1.Background.Value = backgroundFocused;
            //    border1.BorderThickness.Value = borderThicknessFocused;
            //    border1.BorderColor.Value = borderColorFocused;
            //}


            // Hover State 
            if (MouseHover.Value)
            {
                //label1.TextColor.Value = textColorHover;
                //border1.Background.Value = backgroundHover;
                //border1.BorderThickness.Value = borderThicknessHover;
                //border1.BorderColor.Value = borderColorHover;

                ((UIComponentDrawable)Content).Background = Color.LightBlue;
                //((Grid)Content).Background = Color.LightBlue;
            }
        }

    }
}
