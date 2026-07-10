using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
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

        // --- Touch Events ---
        [JsonIgnore][XmlIgnore] public Action<object, UIClickEvent> OnTap;
        [JsonIgnore][XmlIgnore] public Func<object, UIClickEvent, Task> OnTapAsync;

        [JsonIgnore][XmlIgnore] public Action<object, UIClickEvent> OnLongPress;
        [JsonIgnore][XmlIgnore] public Func<object, UIClickEvent, Task> OnLongPressAsync;


        // --- Mouse Events ---
        [JsonIgnore][XmlIgnore] public Action<object, UIClickEvent> OnMouseDoubleClick;
        [JsonIgnore][XmlIgnore] public Func<object, UIClickEvent, Task> OnMouseDoubleClickAsync;

        [JsonIgnore][XmlIgnore] public Action<object, UIClickEvent> OnMouseRightClick;
        [JsonIgnore][XmlIgnore] public Func<object, UIClickEvent, Task> OnMouseRightClickAsync;

        [JsonIgnore][XmlIgnore] public Action<object, UIMouseDownEvent> OnMouseDown;
        [JsonIgnore][XmlIgnore] public Func<object, UIMouseDownEvent, Task> OnMouseDownAsync;

        [JsonIgnore][XmlIgnore] public Action<object, UIMouseUpEvent> OnMouseUp;
        [JsonIgnore][XmlIgnore] public Func<object, UIMouseUpEvent, Task> OnMouseUpAsync;

        [JsonIgnore][XmlIgnore] public Action<object, UIMouseWheelScrollEvent> OnMouseWheelScroll;
        [JsonIgnore][XmlIgnore] public Func<object, UIMouseWheelScrollEvent, Task> OnMouseWheelScrollAsync;


        // --- Virtual Events ---
        [JsonIgnore][XmlIgnore] public Action<object, UIClickEvent> OnActivate;

        [JsonIgnore][XmlIgnore] public Func<object, UIClickEvent, Task> OnActivateAsync;

        [JsonIgnore][XmlIgnore] public Action<object, UIClickEvent> OnMultiClick;
        [JsonIgnore][XmlIgnore] public Func<object, UIClickEvent, Task> OnMultiClickAsync;

        [JsonIgnore][XmlIgnore] public Action<object, UIClickEvent> OnSecondaryClick;
        [JsonIgnore][XmlIgnore] public Func<object, UIClickEvent, Task> OnSecondaryClickAsync;


        // --- State Change Events ---
        [JsonIgnore][XmlIgnore] public Action<object, UIFocusChangedEvent> OnFocusChanged;  // (eventSource, event)
        [JsonIgnore][XmlIgnore] public Func<object, UIFocusChangedEvent, Task> OnFocusChangedAsync;  // (eventSource, event)

        [JsonIgnore][XmlIgnore] public Action<UIHoverChangedEvent> OnHoverChanged;
        [JsonIgnore][XmlIgnore] public Func<UIHoverChangedEvent, Task> OnHoverChangedAsync;



        public override async Task HandleTapEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            if (ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y)) && Visible.Value == Visibility.Visible)
            {
                await base.HandleTapEventAsync(uiWindow, uiEvent);

                OnTap?.Invoke(this, uiEvent);
                await (OnTapAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);

                // ActivateAsync is NOT called here - UIManager.Touch.cs follows every Tap with
                // a HandleMouseClickEventAsync dispatch at the same point, and
                // HandleMouseClickEventAsync (below) already calls ActivateAsync. Calling it
                // here too would invoke it twice per tap.

                //if (this.HitTestVisible) uiEvent.Handled = true;
            }
        }

        // Single, input-agnostic hook every "this control was activated" input path funnels
        // through: a real mouse primary-button click (HandleMouseClickEventAsync below), a
        // touch tap promoted to a click (UIManager.Touch.cs), a gamepad A press, and a keyboard
        // Enter/Space press (the latter two call ActivateAsync directly on the focused
        // component - see UIManager.GamePad.cs / UIManager.Keyboard.cs - bypassing hit-testing
        // entirely since the target is already known). Controls with real "activate" behavior
        // (CheckBox's toggle, ComboBox's open/close, ListView's select-by-position, etc.)
        // override Activate/ActivateAsync instead of any device-specific handler, so that
        // behavior is reached uniformly regardless of input device. Override Activate for
        // synchronous logic, ActivateAsync for logic that needs to await something -
        // ActivateAsync always invokes the (possibly overridden) Activate first via virtual
        // dispatch, so overriding just one of the pair is enough; call base.ActivateAsync/
        // base.Activate to preserve OnActivate/OnActivateAsync firing for app code that only
        // wants a callback.
        public virtual void Activate(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            OnActivate?.Invoke(this, uiEvent);
        }

        public virtual async Task ActivateAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            Activate(uiWindow, uiEvent);
            await (OnActivateAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);
        }

        public override async Task HandleLongPressEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            if (ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y)) && Visible.Value == Visibility.Visible)
            {
                await base.HandleLongPressEventAsync(uiWindow, uiEvent);

                OnLongPress?.Invoke(this, uiEvent);
                await (OnLongPressAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);

                OnSecondaryClick?.Invoke(this, uiEvent);
                await (OnSecondaryClickAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);

                //if (this.HitTestVisible) uiEvent.Handled = true;
            }
        }

        public override async Task HandleMouseClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            if (ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y)) && Visible.Value == Visibility.Visible)
            {
                await base.HandleMouseClickEventAsync(uiWindow, uiEvent);

                await ActivateAsync(uiWindow, uiEvent);

                //if (this.HitTestVisible) uiEvent.Handled = true;
            }
        }

        public override async Task HandleMouseDoubleClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            if (ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y)) && Visible.Value == Visibility.Visible)
            {
                await base.HandleMouseDoubleClickEventAsync(uiWindow, uiEvent);

                OnMouseDoubleClick?.Invoke(this, uiEvent);
                await (OnMouseDoubleClickAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);

                OnMultiClick?.Invoke(this, uiEvent);
                await (OnMultiClickAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);
            }
        }

        public override async Task HandleMouseRightClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            if (ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y)) && Visible.Value == Visibility.Visible)
            {
                await base.HandleMouseRightClickEventAsync(uiWindow, uiEvent);

                OnMouseRightClick?.Invoke(this, uiEvent);
                await (OnMouseRightClickAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);

                OnSecondaryClick?.Invoke(this, uiEvent);
                await (OnSecondaryClickAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);
            }
        }

        public override async Task HandleMouseDownEventAsync(UIWindow uiWindow, UIMouseDownEvent uiEvent)
        {
            if (ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y)) && Visible.Value == Visibility.Visible)
            {
                await base.HandleMouseDownEventAsync(uiWindow, uiEvent);
                OnMouseDown?.Invoke(this, uiEvent);
                await (OnMouseDownAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);
            }
        }

        public override async Task HandleMouseUpEventAsync(UIWindow uiWindow, UIMouseUpEvent uiEvent)
        {
            if (uiEvent.ForcePropagation || (ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y)) && Visible.Value == Visibility.Visible))
            {
                await base.HandleMouseUpEventAsync(uiWindow, uiEvent);
                OnMouseUp?.Invoke(this, uiEvent);
                await (OnMouseUpAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);
            }
        }

        public override async Task HandleMouseWheelScrollEventAsync(UIWindow uiWindow, UIMouseWheelScrollEvent uiEvent)
        {
            if (ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y)) && Visible.Value == Visibility.Visible)
            {
                await base.HandleMouseWheelScrollEventAsync(uiWindow, uiEvent);
                OnMouseWheelScroll?.Invoke(this, uiEvent);
                await (OnMouseWheelScrollAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);
            }
        }


        public override async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        {
            //if (uiEvent.ForcePropagation || ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y)))
            //{
            await base.HandleFocusChangedEventAsync(uiWindow, uiEvent);
            OnFocusChanged?.Invoke(this, uiEvent);
            await (OnFocusChangedAsync?.Invoke(this, uiEvent) ?? Task.CompletedTask);

            StateHasChanged();
            //}
        }

        public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            if (uiEvent.ForcePropagation || ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y)))
            {
                await base.HandleHoverChangedAsync(uiWindow, uiEvent);
                OnHoverChanged?.Invoke(uiEvent);
                await (OnHoverChangedAsync?.Invoke(uiEvent) ?? Task.CompletedTask);

                StateHasChanged();
            }
        }

    }
}
