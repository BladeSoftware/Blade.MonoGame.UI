namespace Blade.UI.Events
{
    public class UIMouseMoveEvent : UIEvent
    {
        public int LastX { get; set; }
        public int LastY { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public int DeltaX { get; set; }
        public int DeltaY { get; set; }

    }
}
