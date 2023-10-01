namespace Blade.MG.UI.Events
{
    public class UIHoverChangedEvent : UIEvent
    {
        public int X { get; set; }
        public int Y { get; set; }

        public bool Hover { get; set; }
    }
}
