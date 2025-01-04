using Blade.MG.Input;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Blade.MG.UI
{
    public partial class UIManager //: GameEntity
    {

        // Handle Touch Input
        // Despatch events to all windows 
        // TODO: We should probably sort windows by 'z-index' and stop despatching if the event is handled, for now, just iterate in reverse order
        private async Task HandleTouchInputAsync(UIWindow eventLockedWindow, UIComponent eventLockedControl, bool propagateEvents, GameTime gameTime)
        {
            propagateEvents = true;


            // Is touch enabled ?
            if (!InputManager.Touch.IsConnected)
            {
                return;
            }


            // Check for touch events
            if (!InputManager.Touch.TryGetGesture(out var gesture))
            {
                // No gesture
                return;
            }


            // Handle a Tap or Hold / Long Press event
            if (gesture.GestureType == GestureType.Tap || gesture.GestureType == GestureType.Hold)
            {
                Point touchPoint = new Point((int)gesture.Position.X, (int)gesture.Position.Y);

                // Check if the touch event is over a control that can receive focus
                bool selector(UIComponent component, UIComponent parent) =>
                    (
                      component.IsHitTestVisible &&
                      component.CanFocus &&
                      component.Visible.Value == Components.Visibility.Visible &&
                      component.ParentWindow?.Visible?.Value == Components.Visibility.Visible &&
                      component.FinalRect.Contains(touchPoint)
                    );

                UIComponent focusComponent = SelectFirst(selector, true, touchPoint);
                UIWindow focusUIWindow = focusComponent?.ParentWindow;

                if (focusComponent != null)
                {
                    await focusUIWindow.RaiseFocusChangedEventAsync(focusComponent, focusUIWindow);
                }



                if (gesture.GestureType == GestureType.Tap)
                {
                    var uiTapEvent = new UIClickEvent { X = touchPoint.X, Y = touchPoint.Y };
                    await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleTapEventAsync(uiWindow, uiTapEvent); });

                    var uiClickEvent = new UIClickEvent { X = touchPoint.X, Y = touchPoint.Y };
                    await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandlePrimaryClickEventAsync(uiWindow, uiClickEvent); });
                }
                else if (gesture.GestureType == GestureType.Hold)
                {
                    var uiLongPressEvent = new UIClickEvent { X = touchPoint.X, Y = touchPoint.Y };
                    await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleLongPressEventAsync(uiWindow, uiLongPressEvent); });

                    var uiClickEvent = new UIClickEvent { X = touchPoint.X, Y = touchPoint.Y };
                    await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleSecondaryClickEventAsync(uiWindow, uiClickEvent); });
                }

            }




            // Handle Drag gestures
            //int scrollVertical = InputManager.Mouse.ScrollWheelValueDelta;
            //int scrollHorizontal = InputManager.Mouse.HorizontalScrollWheelValueDelta;

            //if (scrollVertical != 0 || scrollHorizontal != 0)
            //{
            //    var uiEvent = new UIMouseWheelScrollEvent
            //    {
            //        X = InputManager.Mouse.X,
            //        Y = InputManager.Mouse.Y,
            //        VerticalScroll = scrollVertical,
            //        HorizontalScroll = scrollHorizontal,
            //        ForcePropogation = false
            //    };

            //    await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleMouseWheelScrollEventAsync(uiWindow, uiEvent); });
            //}

        }

    }
}
