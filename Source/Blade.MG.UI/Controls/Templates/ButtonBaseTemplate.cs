using Blade.MG.UI.Components;
using Blade.MG.UI.Events;

namespace Blade.MG.UI.Controls.Templates
{
    public class ButtonBaseTemplate : Control
    {
        private Button button;

        public ButtonBaseTemplate()
        {
        }

        protected override void InitTemplate()
        {
            button = Parent as Button;

            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;

            Margin = button.Margin;
            //Padding = new Thickness(0);

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

        }

    }
}
