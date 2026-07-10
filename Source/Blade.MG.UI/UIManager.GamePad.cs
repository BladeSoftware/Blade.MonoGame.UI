using Blade.MG.Input;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Blade.MG.UI
{
    public partial class UIManager //: GameEntity
    {
        private enum GamePadNavDirection
        {
            None,
            Next,
            Previous
        }

        private GamePadNavDirection repeatNavDirection = GamePadNavDirection.None;
        private DateTime repeatNavPressed = DateTime.MaxValue;
        private bool repeatNavFirst = true;

        private async Task HandleGamePadInputAsync(UIWindow eventLockedWindow, UIComponent eventLockedControl, bool propagateEvents, GameTime gameTime)
        {
            if (!propagateEvents)
            {
                return;
            }

            var gamePad = InputManager.GamePad(PlayerIndex.One);

            if (!gamePad.IsConnected)
            {
                repeatNavDirection = GamePadNavDirection.None;
                return;
            }

            // --- Focus navigation: D-Pad (or left stick, treated as a virtual D-Pad) moves
            // focus through IsTabStop controls in TabIndex order, same as Tab/Shift+Tab on a
            // keyboard (see UIWindow.HandleTabNext/HandleTabPrevious).
            bool nextHeld = gamePad.DPad.Down.IsDown || gamePad.DPad.Right.IsDown ||
                gamePad.IsButtonDown(Buttons.LeftThumbstickDown) || gamePad.IsButtonDown(Buttons.LeftThumbstickRight);

            bool previousHeld = gamePad.DPad.Up.IsDown || gamePad.DPad.Left.IsDown ||
                gamePad.IsButtonDown(Buttons.LeftThumbstickUp) || gamePad.IsButtonDown(Buttons.LeftThumbstickLeft);

            GamePadNavDirection navDirection = nextHeld ? GamePadNavDirection.Next : (previousHeld ? GamePadNavDirection.Previous : GamePadNavDirection.None);

            if (navDirection == GamePadNavDirection.None)
            {
                repeatNavDirection = GamePadNavDirection.None;
            }
            else if (navDirection != repeatNavDirection)
            {
                // Newly pressed (or switched) direction - navigate immediately and start the repeat timer.
                repeatNavDirection = navDirection;
                repeatNavFirst = true;
                repeatNavPressed = DateTime.Now;

                await NavigateFocusAsync(eventLockedWindow, navDirection);
            }
            else
            {
                // Direction held - auto-repeat, mirroring UIManager.Keyboard.cs's
                // 500ms-initial/200ms-repeat cadence.
                double thresholdMs = repeatNavFirst ? 500 : 200;

                if ((DateTime.Now - repeatNavPressed).TotalMilliseconds > thresholdMs)
                {
                    repeatNavFirst = false;
                    repeatNavPressed = DateTime.Now;

                    await NavigateFocusAsync(eventLockedWindow, navDirection);
                }
            }

            // --- Activate: A activates whichever control currently has focus. Calls
            // ActivateAsync directly on that control - no hit-test dispatch needed, since the
            // target is already known (there's no "outside" concept for a gamepad button, unlike
            // a real click). This also sidesteps a modal-dispatch bug the old fake-click
            // synthesis had: while a modal popup is open (e.g. an open ComboBox dropdown),
            // top-level DispatchEventAsync routes exclusively to the modal, so a synthesized
            // click at the *main window's* focused-control coordinates used to be misrouted to
            // the popup and misread as an outside-click-closes. Still passes FinalRect.Center as
            // the event's position so any override that inspects it (e.g. ComboBox's
            // dropdown-button-vs-header check) behaves the same as a real click would - see
            // UIManager.Keyboard.cs's Enter/Space handling for the same technique.
            if (gamePad.IsButtonPressed(Buttons.A))
            {
                UIComponent focused = GetFocusedComponent(eventLockedWindow);
                if (focused is UIComponentEvents focusedEvents)
                {
                    Point center = focused.FinalRect.Center;
                    var uiActivateEvent = new UIClickEvent { X = center.X, Y = center.Y };

                    await focusedEvents.ActivateAsync(focused.ParentWindow, uiActivateEvent);
                }
            }

            // --- Cancel/Back: B is synthesized as an Escape key press and dispatched through
            // the normal keyboard path, so it gets the same behavior Escape already has (and
            // will automatically pick up any future Escape handling too) - see
            // ModalBase.HandleKeyPressAsync and ComboBox.HandleKeyPressAsync.
            if (gamePad.IsButtonPressed(Buttons.B))
            {
                var uiEscapeEvent = new UIKeyEvent { Key = Keys.Escape };

                await DispatchEventAsync(eventLockedWindow, null, async (uiWindow) => { await uiWindow.HandleKeyPressAsync(uiWindow, uiEscapeEvent); });
            }
        }

        private async Task NavigateFocusAsync(UIWindow eventLockedWindow, GamePadNavDirection direction)
        {
            if (direction == GamePadNavDirection.Next)
            {
                await DispatchEventAsync(eventLockedWindow, null, async (uiWindow) => { await uiWindow.HandleTabNext(); });
            }
            else if (direction == GamePadNavDirection.Previous)
            {
                await DispatchEventAsync(eventLockedWindow, null, async (uiWindow) => { await uiWindow.HandleTabPrevious(); });
            }
        }

        // Same modal-priority ordering DispatchEventAsync uses elsewhere: a topmost modal
        // window owns focus/input exclusively, otherwise search all windows in reverse z-order.
        // Shared by gamepad A (above) and keyboard Enter/Space (UIManager.Keyboard.cs).
        private UIComponent GetFocusedComponent(UIWindow eventLockedWindow)
        {
            if (eventLockedWindow != null)
            {
                return eventLockedWindow.FocusedComponent;
            }

            for (int i = uiWindows.Count - 1; i >= 0; i--)
            {
                if (uiWindows.Values[i].IsModal)
                {
                    return uiWindows.Values[i].FocusedComponent;
                }
            }

            for (int i = uiWindows.Count - 1; i >= 0; i--)
            {
                if (uiWindows.Values[i].FocusedComponent != null)
                {
                    return uiWindows.Values[i].FocusedComponent;
                }
            }

            return null;
        }

    }
}
