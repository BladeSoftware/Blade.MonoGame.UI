using Microsoft.Xna.Framework.Input;

namespace Blade.UI.Events
{
    public class UIKeyEvent : UIEvent
    {
        public Keys Key;
        public string KeyChar;

        public bool Shift;
        public bool LeftShift;
        public bool RightShift;

        public bool Ctrl;
        public bool LeftCtrl;
        public bool RightCtrl;

        public bool Alt;
        public bool LeftAlt;
        public bool RightAlt;

        public bool CapsLock;
        public bool NumLock;

        public UIKeyEvent()
        {

        }

        public UIKeyEvent(KeyboardState keyboardState)
        {
            // Update Key State
            LeftShift = keyboardState.IsKeyDown(Keys.LeftShift);
            RightShift = keyboardState.IsKeyDown(Keys.RightShift);
            Shift = LeftShift || RightShift;

            LeftCtrl = keyboardState.IsKeyDown(Keys.LeftControl);
            RightCtrl = keyboardState.IsKeyDown(Keys.RightControl);
            Ctrl = LeftCtrl || RightCtrl;

            LeftAlt = keyboardState.IsKeyDown(Keys.LeftAlt);
            RightAlt = keyboardState.IsKeyDown(Keys.RightAlt);
            Alt = LeftAlt || RightAlt;

            CapsLock = keyboardState.CapsLock;
            NumLock = keyboardState.NumLock;

        }
    }
}
