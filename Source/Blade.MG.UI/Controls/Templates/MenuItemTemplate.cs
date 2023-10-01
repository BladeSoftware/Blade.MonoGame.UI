using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Blade.MG.UI.Models;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls.Templates
{
    public class MenuItemTemplate : Control
    {
        //private MenuItem button;
        private Label label1;

        private Color backgroundNormal = Color.White;
        private Color backgroundHover = Color.Gray;
        private Color backgroundFocused = Color.LightGray;

        private Color textColorNormal = Color.Black;
        private Color textColorHover = Color.Black;
        private Color textColorFocused = Color.Black;

        //private int borderThicknessNormal = 2;
        //private int borderThicknessHover = 2;
        //private int borderThicknessFocused = 2;


        public MenuItemTemplate()
        {
        }

        protected override void InitTemplate()
        {
            IMenuOption menuOption = DataContext as IMenuOption;


            //this.HorizontalAlignment = HorizontalAlignmentType.Stretch;
            //this.VerticalAlignment = VerticalAlignmentType.Top;


            label1 = new Label()
            {
                Text = menuOption?.ToString() ?? "",
                TextColor = Color.Black,
                Height = 34,

                FontName = null,
                FontSize = null,

                //HorizontalAlignment = HorizontalAlignmentType.Left,
                //VerticalAlignment = VerticalAlignmentType.Center,

                Padding = new Thickness(10, 2),
            };


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
            label1.TextColor.Value = textColorNormal;
            Background.Value = backgroundNormal;
            //border1.BorderThickness.Value = borderThicknessNormal;
            //border1.BorderColor.Value = borderColorNormal;


            // Focused State
            if (HasFocus.Value)
            {
                label1.TextColor.Value = textColorFocused;
                Background.Value = backgroundFocused;
                //border1.BorderThickness.Value = borderThicknessFocused;
                //border1.BorderColor.Value = borderColorFocused;
            }


            // Hover State 
            if (MouseHover.Value)
            {
                label1.TextColor.Value = textColorHover;
                Background.Value = backgroundHover;
                //border1.BorderThickness.Value = borderThicknessHover;
                //border1.BorderColor.Value = borderColorHover;
            }


        }

    }
}
