﻿using Blade.UI.Events;

namespace Blade.UI
{
    /// <summary>
    /// Handle Common Events for UI Components
    /// </summary>
    public abstract class UIComponentEvents : UIComponent
    {
        // ---=== UI Events ===---
        public Action<object, UIClickEvent> OnClick;
        public Func<object, UIClickEvent, Task> OnClickAsync;

        public Action<object, UIClickEvent> OnDoubleClick;
        public Func<object, UIClickEvent, Task> OnDoubleClickAsync;

        public Action<object, UIClickEvent> OnRightClick;
        public Func<object, UIClickEvent, Task> OnRightClickAsync;

        public Action<object, UIMouseDownEvent> OnMouseDown;
        public Func<object, UIMouseDownEvent, Task> OnMouseDownAsync;

        public Action<object, UIMouseUpEvent> OnMouseUp;
        public Func<object, UIMouseUpEvent, Task> OnMouseUpAsync;

        public Action<object, UIMouseWheelScrollEvent> OnMouseWheelScroll;
        public Func<object, UIMouseWheelScrollEvent, Task> OnMouseWheelScrollAsync;

        public Action<object, UIFocusChangedEvent> OnFocusChanged;  // (eventSource, event)
        public Func<object, UIFocusChangedEvent, Task> OnFocusChangedAsync;  // (eventSource, event)

        public Action<UIHoverChangedEvent> OnHoverChanged;
        public Func<UIHoverChangedEvent, Task> OnHoverChangedAsync;


        public override async Task HandleClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            if (this.FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                await base.HandleClickEventAsync(uiWindow, uiEvent);

                OnClick?.Invoke(this, uiEvent);
                await (OnClickAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);

                //if (this.HitTestVisible) uiEvent.Handled = true;
            }
        }

        public override async Task HandleDoubleClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            if (this.FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                await base.HandleDoubleClickEventAsync(uiWindow, uiEvent);
                OnDoubleClick?.Invoke(this, uiEvent);
                await (OnDoubleClickAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);
            }
        }

        public override async Task HandleRightClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            if (this.FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                await base.HandleRightClickEventAsync(uiWindow, uiEvent);
                OnRightClick?.Invoke(this, uiEvent);
                await (OnRightClickAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);
            }
        }

        public override async Task HandleMouseDownEventAsync(UIWindow uiWindow, UIMouseDownEvent uiEvent)
        {
            if (this.FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                await base.HandleMouseDownEventAsync(uiWindow, uiEvent);
                OnMouseDown?.Invoke(this, uiEvent);
                await (OnMouseDownAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);
            }
        }

        public override async Task HandleMouseUpEventAsync(UIWindow uiWindow, UIMouseUpEvent uiEvent)
        {
            if (uiEvent.ForcePropogation || this.FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                await base.HandleMouseUpEventAsync(uiWindow, uiEvent);
                OnMouseUp?.Invoke(this, uiEvent);
                await (OnMouseUpAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);
            }
        }

        public override async Task HandleMouseWheelScrollEventAsync(UIWindow uiWindow, UIMouseWheelScrollEvent uiEvent)
        {
            if (this.FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                await base.HandleMouseWheelScrollEventAsync(uiWindow, uiEvent);
                OnMouseWheelScroll?.Invoke(this, uiEvent);
                await (OnMouseWheelScrollAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);
            }
        }


        public override async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        {
            await base.HandleFocusChangedEventAsync(uiWindow, uiEvent);
            OnFocusChanged?.Invoke(this, uiEvent);
            await (OnFocusChangedAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);

            StateHasChanged();
        }

        public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            if (uiEvent.ForcePropogation || this.FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                await base.HandleHoverChangedAsync(uiWindow, uiEvent);
                OnHoverChanged?.Invoke(uiEvent);
                await (OnHoverChangedAsync?.Invoke(uiEvent) ?? Task.CompletedTask);

                StateHasChanged();
            }
        }

    }
}