using Blade.MG.Input;
using Blade.MG.Input.Keyboards;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Blade.MG.UI
{
    public partial class UIManager //: GameEntity
    {
        private Keys repeatKey = Keys.None;
        private DateTime repeatKeyPressed = DateTime.MaxValue;
        private bool repeatKeyFirst = true;
        private UIKeyEvent repeatKeyEvent;

        private async Task HandleKeyboardInputAsync(UIWindow eventLockedWindow, UIComponent eventLockedControl, bool propagateEvents, GameTime gameTime)
        {
            // Handle Keyboard Input
            if (!propagateEvents)
            {
                return;
            }

            // Handle Key Press / Release
            Keys[] lastPressedKeys = InputManager.Keyboard.LastKeyboardState.GetPressedKeys();
            for (int i = 0; i < lastPressedKeys.Count(); i++)
            {
                Keys key = lastPressedKeys[i];

                // If the Key was Down on the last check and now it's Up, then the user has released it
                if (!InputManager.Keyboard.IsKeyDown(key))
                {
                    var uiEvent = new UIKeyEvent(InputManager.Keyboard.KeyboardState) { Key = key, KeyChar = KeyboardMapping.Keyboard.GetChar(key, InputManager.Keyboard.KeyboardState) };

                    await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleKeyUpAsync(uiWindow, uiEvent); });

                    if (key == repeatKey)
                    {
                        repeatKey = Keys.None;
                    }
                }

            }

            Keys[] pressedKeys = InputManager.Keyboard.KeyboardState.GetPressedKeys();
            for (int i = 0; i < pressedKeys.Count(); i++)
            {
                Keys key = pressedKeys[i];
                //System.Diagnostics.Debug.WriteLine("Key : " + key.ToString());

                // If the Key was Up on the last check and now it's Down, then the user has pressed it
                if (!InputManager.Keyboard.LastKeyboardState.IsKeyDown(key))
                {

                    var uiEvent = new UIKeyEvent(InputManager.Keyboard.KeyboardState) { Key = key, KeyChar = KeyboardMapping.Keyboard.GetChar(key, InputManager.Keyboard.KeyboardState) };
                    await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleKeyDownAsync(uiWindow, uiEvent); });

                    repeatKey = key;
                    repeatKeyFirst = true;
                    repeatKeyPressed = DateTime.Now;
                    repeatKeyEvent = uiEvent;

                    uiEvent.Handled = false;

                    await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleKeyPressAsync(uiWindow, uiEvent); });

                    //// Handle Special Keys
                    //if (key == Keys.Tab && !uiEvent.Handled)
                    //{
                    //    HandleTabNext();  // TODO: Have a focused window and stay within that ??
                    //}

                }

            }

            // Handle Repeated KeyPress event if user holds the key down
            if (repeatKey != Keys.None)
            {
                if (repeatKeyFirst)
                {
                    if ((DateTime.Now - repeatKeyPressed).TotalMilliseconds > 500)
                    {
                        repeatKeyEvent.Handled = false;
                        await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleKeyPressAsync(uiWindow, repeatKeyEvent); });
                        repeatKeyFirst = false;
                    }
                }
                else
                {
                    if ((DateTime.Now - repeatKeyPressed).TotalMilliseconds > 200)
                    {
                        repeatKeyEvent.Handled = false;
                        await DispatchEventAsync(eventLockedWindow, async (uiWindow) => { await uiWindow.HandleKeyPressAsync(uiWindow, repeatKeyEvent); });
                    }
                }
            }
        }

    }
}
