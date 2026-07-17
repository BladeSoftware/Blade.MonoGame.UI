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

        // Routed through ApplyThemedValue (checked against `button`, not this template, since
        // button is the control an application actually holds a reference to and would call
        // SetStyleOverride on) instead of direct .Value assignment - a plain assignment here
        // silently discarded any SetStyleOverride the application had set, since the next
        // hover/focus change would just snap straight back to the theme value with no way to
        // tell an override had been requested. TextColor/Background use Button's own real
        // property names as the override key (nameof); BorderColor/BorderThickness/Elevation
        // have no equivalent on Button itself (they're internal to this template's border1), so
        // they use plain string keys instead.
        // Color properties ease toward their target via ApplyThemedValueAnimated (backed by
        // PropertyAnimationManager, ticked once per frame from UIManager.Update - see
        // TextBoxTemplate's label transitions for the same pattern) instead of snapping
        // instantly, so hover/focus reads as a deliberate transition rather than a flicker.
        // BorderThickness (Thickness) and Elevation (int) have no AnimateTo overload, so those
        // still snap via the plain ApplyThemedValue - a 1px/1-unit jump isn't worth animating.
        protected override void HandleStateChange()
        {
            // Normal State
            ApplyThemedValueAnimated(button, label1.TextColor, nameof(Button.TextColor), Theme.OnPrimaryContainer);
            ApplyThemedValueAnimated(button, border1.Background, nameof(Button.Background), Theme.PrimaryContainer);
            ApplyThemedValue(button, border1.BorderThickness, "BorderThickness", BorderThickness);
            ApplyThemedValueAnimated(button, border1.BorderColor, "BorderColor", Theme.Outline);
            ApplyThemedValue(button, border1.Elevation, "Elevation", 1);

            // Focused State
            if (HasFocus.Value)
            {
                ApplyThemedValueAnimated(button, border1.BorderColor, "BorderColor", Theme.Tertiary);
                ApplyThemedValue(button, border1.Elevation, "Elevation", 2);
            }

            // Hover State
            if (MouseHover.Value)
            {
                ApplyThemedValueAnimated(button, label1.TextColor, nameof(Button.TextColor), Theme.OnSecondaryContainer);
                ApplyThemedValueAnimated(button, border1.Background, nameof(Button.Background), Theme.SecondaryContainer);
                ApplyThemedValue(button, border1.BorderThickness, "BorderThickness", BorderThickness);
                ApplyThemedValueAnimated(button, border1.BorderColor, "BorderColor", Theme.Tertiary);
                ApplyThemedValue(button, border1.Elevation, "Elevation", 2);
            }
        }

    }
}
