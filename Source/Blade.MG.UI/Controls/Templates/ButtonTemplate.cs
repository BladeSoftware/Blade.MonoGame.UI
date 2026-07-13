using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls.Templates
{
    public class ButtonTemplate : Control
    {
        private Button button;
        private Label label1;
        private Border border1;

        private const int BorderThickness = 2;

        public ButtonTemplate()
        {
        }

        protected override void InitTemplate()
        {
            button = ParentAs<Button>();

            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;

            //this.HorizontalAlignment = button.HorizontalAlignment;
            //this.VerticalAlignment = button.VerticalAlignment;
            //this.HorizontalContentAlignment = button.HorizontalContentAlignment;
            //this.VerticalContentAlignment = button.VerticalContentAlignment;

            // NOTE: do NOT also apply button.Margin here - the button's margin is already
            // applied once by whatever container positions the outer Button itself. Applying it
            // again here shrinks ButtonTemplate's FinalRect inside Button's FinalRect, which
            // was part of the hover-freeze bug.
            //Padding = new Thickness(0);

            //button.Border = new Border()
            border1 = new Border()
            {
                CornerRadius = new CornerRadius(12f),
                Elevation = 1,
                Background = Theme.PrimaryContainer,
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
            // Set directly rather than relying solely on the base FinalRect-matching
            // propagation chain (Button -> ButtonTemplate -> border1 -> label1, each with its
            // own FinalRect check) to eventually set it - any one mismatch along that chain
            // silently drops the update, which is what caused hover to only "take" some of the
            // time.
            MouseHover = uiEvent.Hover;

            await base.HandleHoverChangedAsync(uiWindow, uiEvent);

            StateHasChanged();
        }

        protected override void HandleStateChange()
        {
            // Normal State
            label1.TextColor.Value = Theme.OnPrimaryContainer;
            border1.Background.Value = Theme.PrimaryContainer;
            border1.BorderThickness.Value = BorderThickness;
            border1.BorderColor.Value = Theme.Outline;
            border1.Elevation.Value = 1;

            // Focused State
            if (HasFocus.Value)
            {
                border1.BorderColor.Value = Theme.Tertiary;
                border1.Elevation.Value = 2;
            }

            // Hover State
            if (MouseHover.Value)
            {
                label1.TextColor.Value = Theme.OnSecondaryContainer;
                border1.Background.Value = Theme.SecondaryContainer;
                border1.BorderThickness.Value = BorderThickness;
                border1.BorderColor.Value = Theme.Tertiary;
                border1.Elevation.Value = 2;
            }
        }

    }
}
