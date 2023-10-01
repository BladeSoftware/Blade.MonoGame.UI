namespace Blade.MG.UI.Events
{
    public class UIMouseWheelScrollEvent : UIEvent
    {
        public int X { get; set; }
        public int Y { get; set; }


        public int VerticalScroll { get; set; }
        public int HorizontalScroll { get; set; }

    }
}
