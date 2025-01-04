using Blade.MG.Input;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI
{
    public partial class UIManager //: GameEntity
    {

        private static int MouseDoubleClickDelay = 300; // In Milliseconds
        private static DateTime MouseDoubleClickTime = DateTime.MinValue;

        // Handle Mouse Input
        // Despatch events to all windows 
        // TODO: We should probably sort windows by 'z-index' and stop despatching if the event is handled, for now, just iterate in reverse order
        private async Task HandleMouseInputAsync(UIWindow eventLockedWindow, UIComponent eventLockedControl, bool propagateEvents, GameTime gameTime)
        {
            propagateEvents = true;

            if (eventLockedWindow != null)
            {
                // Check if the mouse has moved off of a control it was previously hovering over
                UIComponent component = eventLockedWindow.hover.Where(p => p == eventLockedWindow.EventLockedControl).FirstOrDefault();
                if (component == null ||
                    component.Visible.Value != Components.Visibility.Visible ||
                    component.ParentWindow?.Visible?.Value != Components.Visibility.Visible ||
                    !component.FinalRect.Contains(InputManager.Mouse.Position))
                {
                    eventLockedWindow.hover.Remove(component);
                    await eventLockedWindow.RaiseHoverLeaveEventAsync(component, eventLockedWindow);
                }

            }
            else
            {
                // Check if the mouse is hovering over a control
                bool selector(UIComponent component, UIComponent parent) =>
                    (
                      component.IsHitTestVisible &&
                      component.CanHover &&
                      component.Visible.Value == Components.Visibility.Visible &&
                      component.ParentWindow?.Visible?.Value == Components.Visibility.Visible &&
                      component.FinalRect.Contains(InputManager.Mouse.Position)
                    );

                UIComponent selected = SelectFirst(selector, true, InputManager.Mouse.Position);
                UIWindow selectedUIWindow = selected?.ParentWindow;

                // Check if the mouse has moved off of a control is was previously hovering over
                foreach (UIComponent component in HoverIterator)
                {
                    UIWindow uiWindow = component.ParentWindow;

                    if (eventLockedControl != null && eventLockedControl != component) { continue; }

                    if (selected == null || component != selected)
                    {
                        uiWindow.hover.Remove(component);
                        await uiWindow.RaiseHoverLeaveEventAsync(component, uiWindow);
                    }
                }

                if (selected != null && selectedUIWindow?.hover != null && !selectedUIWindow.hover.Contains(selected))
                {
                    await selectedUIWindow.RaiseHoverEnterEventAsync(selected, selectedUIWindow);
                    if (selected.MouseHover.Value)
                    {
                        selectedUIWindow.hover.Add(selected);
                    }
                }

            }


            // Check if user is pressing any mouse buttons
            if (InputManager.Mouse.PrimaryButton.Pressed ||
                InputManager.Mouse.MiddleButton.Pressed ||
                InputManager.Mouse.SecondaryButton.Pressed)
            {
                var uiEvent = new UIMouseDownEvent
                {
                    X = InputManager.Mouse.X,
                    Y = InputManager.Mouse.Y,

                    PrimaryButton = InputManager.Mouse.PrimaryButton,
                    MiddleButton = InputManager.Mouse.MiddleButton,
                    SecondaryButton = InputManager.Mouse.SecondaryButton
                };

                await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleMouseDownEventAsync(uiWindow, uiEvent); });

                // Select first hover component
                //UIComponent hoverComponent = HoverIterator.FirstOrDefault();
                //UIWindow hoverUIWindow = hoverComponent?.ParentWindow;

                //if (hoverComponent != null)
                //{
                //    uiEvent.Handled = false;
                //    await hoverUIWindow.RaiseFocusChangedEventAsync(hoverComponent, hoverUIWindow);
                //}

                // Check if the mouse is over a control that can receive focus
                bool selector(UIComponent component, UIComponent parent) =>
                    (
                      component.IsHitTestVisible &&
                      component.CanFocus &&
                      component.Visible.Value == Components.Visibility.Visible &&
                      component.ParentWindow?.Visible?.Value == Components.Visibility.Visible &&
                      component.FinalRect.Contains(InputManager.Mouse.Position)
                    );

                UIComponent focusComponent = SelectFirst(selector, true, InputManager.Mouse.Position);
                UIWindow focusUIWindow = focusComponent?.ParentWindow;

                if (focusComponent != null)
                {
                    uiEvent.Handled = false;
                    await focusUIWindow.RaiseFocusChangedEventAsync(focusComponent, focusUIWindow);
                }
            }

            // Check if user has released the left mouse button - Handle this separately as we also fire Click / Double Click events on the Left Button
            if (InputManager.Mouse.PrimaryButton.Released)
            {
                var uiEvent = new UIMouseUpEvent
                {
                    X = InputManager.Mouse.X,
                    Y = InputManager.Mouse.Y,

                    PrimaryButton = InputManager.Mouse.PrimaryButton,
                    MiddleButton = InputManager.Mouse.MiddleButton,
                    SecondaryButton = InputManager.Mouse.SecondaryButton,

                    ForcePropogation = eventLockedWindow != null
                };
                await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleMouseUpEventAsync(uiWindow, uiEvent); });

                var uiClickEvent = new UIClickEvent { X = InputManager.Mouse.X, Y = InputManager.Mouse.Y };
                await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleMouseClickEventAsync(uiWindow, uiClickEvent); });

                if (MouseDoubleClickTime != DateTime.MinValue && (DateTime.Now - MouseDoubleClickTime).TotalMilliseconds < MouseDoubleClickDelay)
                {
                    uiClickEvent = new UIClickEvent { X = InputManager.Mouse.X, Y = InputManager.Mouse.Y };
                    await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleMouseDoubleClickEventAsync(uiWindow, uiClickEvent); });

                    MouseDoubleClickTime = DateTime.MinValue;
                }
                else
                {
                    MouseDoubleClickTime = DateTime.Now;
                }
            }

            // Check if user has released the right mouse button - Handle this separately as we also fire Click / Double Click events on the Right Button
            if (InputManager.Mouse.SecondaryButton.Released)
            {
                var uiEvent = new UIMouseUpEvent
                {
                    X = InputManager.Mouse.X,
                    Y = InputManager.Mouse.Y,

                    PrimaryButton = InputManager.Mouse.PrimaryButton,
                    MiddleButton = InputManager.Mouse.MiddleButton,
                    SecondaryButton = InputManager.Mouse.SecondaryButton,

                    ForcePropogation = eventLockedWindow != null
                };
                await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleMouseUpEventAsync(uiWindow, uiEvent); });

                var uiClickEvent = new UIClickEvent { X = InputManager.Mouse.X, Y = InputManager.Mouse.Y };
                await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleMouseRightClickEventAsync(uiWindow, uiClickEvent); });
            }

            // Check if user has released the middle mouse buttons
            if (InputManager.Mouse.MiddleButton.Released)
            {
                var uiEvent = new UIMouseUpEvent
                {
                    X = InputManager.Mouse.X,
                    Y = InputManager.Mouse.Y,

                    PrimaryButton = InputManager.Mouse.PrimaryButton,
                    MiddleButton = InputManager.Mouse.MiddleButton,
                    SecondaryButton = InputManager.Mouse.SecondaryButton,

                    ForcePropogation = eventLockedWindow != null
                };
                await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleMouseUpEventAsync(uiWindow, uiEvent); });
            }

            // Handle Mouse Movement
            if (InputManager.Mouse.HasPointerMoved)
            {
                var uiEvent = new UIMouseMoveEvent
                {
                    X = InputManager.Mouse.X,
                    Y = InputManager.Mouse.Y,
                    LastX = InputManager.Mouse.PreviousMouseState.X,
                    LastY = InputManager.Mouse.PreviousMouseState.Y,
                    DeltaX = InputManager.Mouse.PositionXDelta,
                    DeltaY = InputManager.Mouse.PositionYDelta
                };

                await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleMouseMoveEventAsync(uiWindow, uiEvent); });

            }

            // Handle Mouse Scroll
            int scrollVertical = InputManager.Mouse.ScrollWheelValueDelta;
            int scrollHorizontal = InputManager.Mouse.HorizontalScrollWheelValueDelta;

            if (scrollVertical != 0 || scrollHorizontal != 0)
            {
                var uiEvent = new UIMouseWheelScrollEvent
                {
                    X = InputManager.Mouse.X,
                    Y = InputManager.Mouse.Y,
                    VerticalScroll = scrollVertical,
                    HorizontalScroll = scrollHorizontal,
                    ForcePropogation = false
                };

                await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleMouseWheelScrollEventAsync(uiWindow, uiEvent); });
            }

        }

        internal async Task RaiseClickEventAsync(UIWindow uiWindow, UIComponent component)
        {
            UIComponentEvents ctrl = component as UIComponentEvents;
            if (ctrl != null)
            {
                var uiEvent = new UIClickEvent { X = InputManager.Mouse.X, Y = InputManager.Mouse.Y };
                await ctrl.HandleMouseClickEventAsync(uiWindow, uiEvent);
            }
        }

        //internal async Task RaiseMouseMoveEventAsync(UIMouseMoveEvent uiEvent, UIWindow uiWindow)
        //{
        //    await uiWindow.HandleMouseMoveEventAsync(uiWindow, uiEvent);
        //}


    }
}
