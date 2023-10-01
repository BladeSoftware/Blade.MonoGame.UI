namespace Blade.MG.UI
{
    public class UIEvent
    {
        public static readonly UIEvent Empty = new UIEvent { Handled = false, ForcePropogation = false };

        public bool Handled { get; set; }
        public bool ForcePropogation { get; set; }
    }
}
