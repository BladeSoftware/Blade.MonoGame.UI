using Microsoft.Xna.Framework.Input;

namespace Blade.MG.UI.Events
{
    public class UIMouseUpEvent : UIEvent
    {
        public int X { get; set; }
        public int Y { get; set; }

        public ButtonState LeftButton { get; set; }
        public ButtonState MiddleButton { get; set; }
        public ButtonState RightButton { get; set; }

    }
}
