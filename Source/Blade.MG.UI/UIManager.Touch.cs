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

            // Drain every gesture queued this frame, not just the first - a fast tap followed
            // by the start of a drag (or several drag samples) in the same frame would
            // otherwise silently lose all but the first.
            while (InputManager.Touch.TryGetGesture(out var gesture))
            {
                await HandleGestureAsync(eventLockedWindow, gesture);
            }
        }

        private async Task HandleGestureAsync(UIWindow eventLockedWindow, GestureSample gesture)
        {
            // Handle a Tap, DoubleTap or Hold / Long Press event
            if (gesture.GestureType == GestureType.Tap || gesture.GestureType == GestureType.DoubleTap || gesture.GestureType == GestureType.Hold)
            {
                Point touchPoint = new Point((int)gesture.Position.X, (int)gesture.Position.Y);

                // Check if the touch event is over a control that can receive focus
                bool selector(UIComponent component, UIComponent parent) =>
                    (
                      component.IsHitTestVisible &&
                      component.CanFocus &&
                      component.Visible.Value == Components.Visibility.Visible &&
                      component.ParentWindow?.Visible?.Value == Components.Visibility.Visible &&
                      component.ContainsScreenPoint(touchPoint)
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
                    await DispatchEventAsync(eventLockedWindow, touchPoint, async (uiWindow) => { await uiWindow.HandleTapEventAsync(uiWindow, uiTapEvent); });

                    // Dispatched through HandleMouseClickEventAsync (not
                    // HandlePrimaryClickEventAsync) so a tap reaches each control's real click
                    // logic (CheckBox toggling, ComboBox/ListView/TabPanel selection, etc.),
                    // not just the generic OnPrimaryClick delegate - see
                    // UIManager.GamePad.cs's A-button handling for the same fix.
                    var uiClickEvent = new UIClickEvent { X = touchPoint.X, Y = touchPoint.Y };
                    await DispatchEventAsync(eventLockedWindow, touchPoint, async (uiWindow) => { await uiWindow.HandleMouseClickEventAsync(uiWindow, uiClickEvent); });
                }
                else if (gesture.GestureType == GestureType.DoubleTap)
                {
                    // MonoGame only raises DoubleTap for the pair as a whole (no separate Tap
                    // gestures precede it), so synthesize the "first click" side of it too -
                    // mirroring how a real mouse double-click still fires an ordinary click
                    // (HandleMouseClickEventAsync) before HandleMouseDoubleClickEventAsync, in
                    // UIManager.Mouse.cs - so anything hooked to a plain click/tap still runs.
                    var uiTapEvent = new UIClickEvent { X = touchPoint.X, Y = touchPoint.Y };
                    await DispatchEventAsync(eventLockedWindow, touchPoint, async (uiWindow) => { await uiWindow.HandleTapEventAsync(uiWindow, uiTapEvent); });

                    var uiClickEvent = new UIClickEvent { X = touchPoint.X, Y = touchPoint.Y };
                    await DispatchEventAsync(eventLockedWindow, touchPoint, async (uiWindow) => { await uiWindow.HandleMouseClickEventAsync(uiWindow, uiClickEvent); });

                    var uiDoubleClickEvent = new UIClickEvent { X = touchPoint.X, Y = touchPoint.Y };
                    await DispatchEventAsync(eventLockedWindow, touchPoint, async (uiWindow) => { await uiWindow.HandleMouseDoubleClickEventAsync(uiWindow, uiDoubleClickEvent); });
                }
                else if (gesture.GestureType == GestureType.Hold)
                {
                    var uiLongPressEvent = new UIClickEvent { X = touchPoint.X, Y = touchPoint.Y };
                    await DispatchEventAsync(eventLockedWindow, touchPoint, async (uiWindow) => { await uiWindow.HandleLongPressEventAsync(uiWindow, uiLongPressEvent); });

                    var uiClickEvent = new UIClickEvent { X = touchPoint.X, Y = touchPoint.Y };
                    await DispatchEventAsync(eventLockedWindow, touchPoint, async (uiWindow) => { await uiWindow.HandleSecondaryClickEventAsync(uiWindow, uiClickEvent); });
                }

                return;
            }

            // Handle drag-to-scroll: translate the per-sample drag delta into a synthesized
            // wheel-scroll event and dispatch it through the existing
            // HandleMouseWheelScrollEventAsync path, reusing ScrollBar's own ScrollOffset
            // clamping unchanged rather than adding a new scroll API.
            //
            // Sign/scale: ScrollBar.HandleMouseWheelScrollEventAsync does
            // `ScrollOffset -= VerticalScroll / 2` and `ScrollOffset += HorizontalScroll / 2`.
            // For "natural" touch scrolling (content sticks to the finger), dragging up/left
            // should increase ScrollOffset (reveal what's below/to the right); the *2 cancels
            // out that handler's /2 so a drag tracks the finger 1:1 in pixels instead of at
            // half speed.
            if (gesture.GestureType == GestureType.VerticalDrag)
            {
                Point touchPoint = new Point((int)gesture.Position.X, (int)gesture.Position.Y);

                var uiEvent = new UIMouseWheelScrollEvent
                {
                    X = touchPoint.X,
                    Y = touchPoint.Y,
                    VerticalScroll = (int)(gesture.Delta.Y * 2),
                };

                await DispatchEventAsync(eventLockedWindow, touchPoint, async (uiWindow) => { await uiWindow.HandleMouseWheelScrollEventAsync(uiWindow, uiEvent); });
            }
            else if (gesture.GestureType == GestureType.HorizontalDrag)
            {
                Point touchPoint = new Point((int)gesture.Position.X, (int)gesture.Position.Y);

                var uiEvent = new UIMouseWheelScrollEvent
                {
                    X = touchPoint.X,
                    Y = touchPoint.Y,
                    HorizontalScroll = (int)(-gesture.Delta.X * 2),
                };

                await DispatchEventAsync(eventLockedWindow, touchPoint, async (uiWindow) => { await uiWindow.HandleMouseWheelScrollEventAsync(uiWindow, uiEvent); });
            }
        }

    }
}
