using Blade.MG.Input;

namespace Blade.MG.UI.Events
{
    public class UIMouseDownEvent : UIEvent
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Button PrimaryButton { get; set; }
        public Button MiddleButton { get; set; }
        public Button SecondaryButton { get; set; }

    }
}
