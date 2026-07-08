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

        public MenuItemTemplate()
        {
        }

        protected override void InitTemplate()
        {
            IMenuOption menuOption = DataContext as IMenuOption;


            //this.HorizontalAlignment = HorizontalAlignmentType.Stretch;
            //this.VerticalAlignment = VerticalAlignmentType.Top;


            var menuItem = ParentAs<MenuItem>();

            label1 = new Label()
            {
                Text = menuOption?.ToString() ?? "",

                // Link to the MenuItem's own TextColor binding so a developer can override it
                // directly (menuItem.TextColor = ...) as well as via SetStyleOverride.
                TextColor = menuItem.TextColor,
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
            var menuItem = ParentAs<MenuItem>();

            // Normal State
            ApplyThemedValue(menuItem, label1.TextColor, nameof(MenuItem.TextColor), Theme.OnSurface);
            ApplyThemedValue(menuItem, Background, nameof(MenuItem.Background), Theme.Surface);

            // Hover State
            if (MouseHover.Value)
            {
                ApplyThemedValue(menuItem, label1.TextColor, nameof(MenuItem.TextColor), Theme.OnSecondaryContainer);
                ApplyThemedValue(menuItem, Background, nameof(MenuItem.Background), Theme.SecondaryContainer);
            }

            // Focused State
            if (HasFocus.Value)
            {
                ApplyThemedValue(menuItem, label1.TextColor, nameof(MenuItem.TextColor), Theme.OnPrimaryContainer);
                ApplyThemedValue(menuItem, Background, nameof(MenuItem.Background), Theme.PrimaryContainer);
            }
        }

    }
}
