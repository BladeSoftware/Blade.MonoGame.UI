using Blade.MG.Input;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Blade.MG.UI
{
    public partial class UIManager //: GameEntity
    {

        private static int MouseDoubleClickDelay = 300; // In Milliseconds
        private static DateTime MouseDoubleClickTime = DateTime.MinValue;

        // Handle Mouse Input
        // Despatch events to all windows 
        // TODO: We should probably sort windows by 'z-index' and stop despatching if the event is handled
        // For now, just iterate in reverse order
        private async Task HandleMouseInputAsync(UIWindow eventLockedWindow, UIComponent eventLockedControl, bool propagateEvents, GameTime gameTime)
        {
            propagateEvents = true;

            if (eventLockedWindow != null)
            {
                // Check if the mouse has moved off of a control it was previously hovering over
                UIComponent component = eventLockedWindow.hover.Where(p => p == eventLockedWindow.EventLockedControl).FirstOrDefault();
                if (component == null || !component.FinalRect.Contains(InputManager.MouseState.Position))
                {
                    eventLockedWindow.hover.Remove(component);
                    await eventLockedWindow.RaiseHoverLeaveEventAsync(component, eventLockedWindow);
                }

            }
            else
            {
                // Check if the mouse is hovering over a control
                bool selector(UIComponent component, UIComponent parent) => (component.HitTestVisible && component.FinalRect.Contains(InputManager.MouseState.Position));
                UIComponent selected = SelectFirst(selector, true, InputManager.MouseState.Position);
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

            // Select first hover component
            UIComponent hoverComponent = HoverIterator.FirstOrDefault();
            UIWindow hoverUIWindow = hoverComponent?.ParentWindow;


            // Check if user is pressing any mouse buttons
            if ((InputManager.MouseState.LeftButton == ButtonState.Pressed && InputManager.LastMouseState.LeftButton != ButtonState.Pressed) ||
                (InputManager.MouseState.MiddleButton == ButtonState.Pressed && InputManager.LastMouseState.MiddleButton != ButtonState.Pressed) ||
                (InputManager.MouseState.RightButton == ButtonState.Pressed && InputManager.LastMouseState.RightButton != ButtonState.Pressed))
            {
                var uiEvent = new UIMouseDownEvent
                {
                    X = InputManager.MouseState.X,
                    Y = InputManager.MouseState.Y,

                    LeftButton = InputManager.MouseState.LeftButton,
                    MiddleButton = InputManager.MouseState.MiddleButton,
                    RightButton = InputManager.MouseState.RightButton
                };

                await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleMouseDownEventAsync(uiWindow, uiEvent); });

                if (hoverComponent != null)
                {
                    uiEvent.Handled = false;
                    await hoverUIWindow.RaiseFocusChangedEventAsync(hoverComponent, hoverUIWindow);
                }
            }

            // Check if user has released the left mouse button - Handle this separately as we also fire Click / Double Click events on the Left Button
            if (InputManager.MouseState.LeftButton != ButtonState.Pressed && InputManager.LastMouseState.LeftButton == ButtonState.Pressed)
            {
                var uiEvent = new UIMouseUpEvent
                {
                    X = InputManager.MouseState.X,
                    Y = InputManager.MouseState.Y,

                    LeftButton = InputManager.MouseState.LeftButton,
                    MiddleButton = InputManager.MouseState.MiddleButton,
                    RightButton = InputManager.MouseState.RightButton,

                    ForcePropogation = eventLockedWindow != null
                };
                await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleMouseUpEventAsync(uiWindow, uiEvent); });

                var uiClickEvent = new UIClickEvent { X = InputManager.MouseState.X, Y = InputManager.MouseState.Y };
                await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleClickEventAsync(uiWindow, uiClickEvent); });

                if (MouseDoubleClickTime != DateTime.MinValue && (DateTime.Now - MouseDoubleClickTime).TotalMilliseconds < MouseDoubleClickDelay)
                {
                    uiClickEvent = new UIClickEvent { X = InputManager.MouseState.X, Y = InputManager.MouseState.Y };
                    await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleDoubleClickEventAsync(uiWindow, uiClickEvent); });

                    MouseDoubleClickTime = DateTime.MinValue;
                }
                else
                {
                    MouseDoubleClickTime = DateTime.Now;
                }
            }

            // Check if user has released the right mouse button - Handle this separately as we also fire Click / Double Click events on the Right Button
            if (InputManager.MouseState.RightButton != ButtonState.Pressed && InputManager.LastMouseState.RightButton == ButtonState.Pressed)
            {
                var uiEvent = new UIMouseUpEvent
                {
                    X = InputManager.MouseState.X,
                    Y = InputManager.MouseState.Y,

                    LeftButton = InputManager.MouseState.LeftButton,
                    MiddleButton = InputManager.MouseState.MiddleButton,
                    RightButton = InputManager.MouseState.RightButton,

                    ForcePropogation = eventLockedWindow != null
                };
                await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleMouseUpEventAsync(uiWindow, uiEvent); });

                var uiClickEvent = new UIClickEvent { X = InputManager.MouseState.X, Y = InputManager.MouseState.Y };
                await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleRightClickEventAsync(uiWindow, uiClickEvent); });
            }

            // Check if user has released the middle mouse buttons
            if (InputManager.MouseState.MiddleButton != ButtonState.Pressed && InputManager.LastMouseState.MiddleButton == ButtonState.Pressed)
            {
                var uiEvent = new UIMouseUpEvent
                {
                    X = InputManager.MouseState.X,
                    Y = InputManager.MouseState.Y,

                    LeftButton = InputManager.MouseState.LeftButton,
                    MiddleButton = InputManager.MouseState.MiddleButton,
                    RightButton = InputManager.MouseState.RightButton,

                    ForcePropogation = eventLockedWindow != null
                };
                await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleMouseUpEventAsync(uiWindow, uiEvent); });
            }

            // Handle Mouse Movement
            if (InputManager.LastMouseState.X != InputManager.MouseState.X || InputManager.LastMouseState.Y != InputManager.MouseState.Y)
            {
                var uiEvent = new UIMouseMoveEvent
                {
                    X = InputManager.MouseState.X,
                    Y = InputManager.MouseState.Y,
                    LastX = InputManager.LastMouseState.X,
                    LastY = InputManager.LastMouseState.Y,
                    DeltaX = InputManager.MouseState.X - InputManager.LastMouseState.X,
                    DeltaY = InputManager.MouseState.Y - InputManager.LastMouseState.Y
                };

                await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.RaiseMouseMoveEventAsync(uiEvent, uiWindow); });

            }

            // Handle Mouse Scroll
            int scrollVertical = InputManager.MouseState.ScrollWheelValue - InputManager.LastMouseState.ScrollWheelValue;
            int scrollHorizontal = InputManager.MouseState.HorizontalScrollWheelValue - InputManager.LastMouseState.HorizontalScrollWheelValue;

            if (scrollVertical != 0 || scrollHorizontal != 0)
            {
                var uiEvent = new UIMouseWheelScrollEvent
                {
                    X = InputManager.MouseState.X,
                    Y = InputManager.MouseState.Y,
                    VerticalScroll = scrollVertical,
                    HorizontalScroll = scrollHorizontal,
                    ForcePropogation = false
                };

                await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleMouseWheelScrollEventAsync(uiWindow, uiEvent); });
            }

        }


    }
}
