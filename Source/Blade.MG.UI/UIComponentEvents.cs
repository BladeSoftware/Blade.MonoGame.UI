using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI
{
    /// <summary>
    /// Handle Common Events for UI Components
    /// </summary>
    public abstract class UIComponentEvents : UIComponent
    {
        // ---=== UI Events ===---
        [JsonIgnore][XmlIgnore] public Action<object, UIClickEvent> OnClick;

        [JsonIgnore][XmlIgnore] public Func<object, UIClickEvent, Task> OnClickAsync;

        [JsonIgnore][XmlIgnore] public Action<object, UIClickEvent> OnDoubleClick;
        [JsonIgnore][XmlIgnore] public Func<object, UIClickEvent, Task> OnDoubleClickAsync;

        [JsonIgnore][XmlIgnore] public Action<object, UIClickEvent> OnRightClick;
        [JsonIgnore][XmlIgnore] public Func<object, UIClickEvent, Task> OnRightClickAsync;

        [JsonIgnore][XmlIgnore] public Action<object, UIMouseDownEvent> OnMouseDown;
        [JsonIgnore][XmlIgnore] public Func<object, UIMouseDownEvent, Task> OnMouseDownAsync;

        [JsonIgnore][XmlIgnore] public Action<object, UIMouseUpEvent> OnMouseUp;
        [JsonIgnore][XmlIgnore] public Func<object, UIMouseUpEvent, Task> OnMouseUpAsync;

        [JsonIgnore][XmlIgnore] public Action<object, UIMouseWheelScrollEvent> OnMouseWheelScroll;
        [JsonIgnore][XmlIgnore] public Func<object, UIMouseWheelScrollEvent, Task> OnMouseWheelScrollAsync;

        [JsonIgnore][XmlIgnore] public Action<object, UIFocusChangedEvent> OnFocusChanged;  // (eventSource, event)
        [JsonIgnore][XmlIgnore] public Func<object, UIFocusChangedEvent, Task> OnFocusChangedAsync;  // (eventSource, event)

        [JsonIgnore][XmlIgnore] public Action<UIHoverChangedEvent> OnHoverChanged;
        [JsonIgnore][XmlIgnore] public Func<UIHoverChangedEvent, Task> OnHoverChangedAsync;


        public override async Task HandleClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            if (FinalRect.Contains(uiEvent.X, uiEvent.Y) && Visible.Value == Visibility.Visible)
            {
                await base.HandleClickEventAsync(uiWindow, uiEvent);

                OnClick?.Invoke(this, uiEvent);
                await (OnClickAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);

                //if (this.HitTestVisible) uiEvent.Handled = true;
            }
        }

        public override async Task HandleDoubleClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            if (FinalRect.Contains(uiEvent.X, uiEvent.Y) && Visible.Value == Visibility.Visible)
            {
                await base.HandleDoubleClickEventAsync(uiWindow, uiEvent);
                OnDoubleClick?.Invoke(this, uiEvent);
                await (OnDoubleClickAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);
            }
        }

        public override async Task HandleRightClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            if (FinalRect.Contains(uiEvent.X, uiEvent.Y) && Visible.Value == Visibility.Visible)
            {
                await base.HandleRightClickEventAsync(uiWindow, uiEvent);
                OnRightClick?.Invoke(this, uiEvent);
                await (OnRightClickAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);
            }
        }

        public override async Task HandleMouseDownEventAsync(UIWindow uiWindow, UIMouseDownEvent uiEvent)
        {
            if (FinalRect.Contains(uiEvent.X, uiEvent.Y) && Visible.Value == Visibility.Visible)
            {
                await base.HandleMouseDownEventAsync(uiWindow, uiEvent);
                OnMouseDown?.Invoke(this, uiEvent);
                await (OnMouseDownAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);
            }
        }

        public override async Task HandleMouseUpEventAsync(UIWindow uiWindow, UIMouseUpEvent uiEvent)
        {
            if (uiEvent.ForcePropogation || (FinalRect.Contains(uiEvent.X, uiEvent.Y) && Visible.Value == Visibility.Visible))
            {
                await base.HandleMouseUpEventAsync(uiWindow, uiEvent);
                OnMouseUp?.Invoke(this, uiEvent);
                await (OnMouseUpAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);
            }
        }

        public override async Task HandleMouseWheelScrollEventAsync(UIWindow uiWindow, UIMouseWheelScrollEvent uiEvent)
        {
            if (FinalRect.Contains(uiEvent.X, uiEvent.Y) && Visible.Value == Visibility.Visible)
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
            if (uiEvent.ForcePropogation || FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                await base.HandleHoverChangedAsync(uiWindow, uiEvent);
                OnHoverChanged?.Invoke(uiEvent);
                await (OnHoverChangedAsync?.Invoke(uiEvent) ?? Task.CompletedTask);

                StateHasChanged();
            }
        }

    }
}
