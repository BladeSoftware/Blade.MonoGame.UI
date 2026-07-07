namespace Blade.MG.UI
{
    public class UIEvent
    {
        public static readonly UIEvent Empty = new UIEvent { Handled = false, ForcePropagation = false };

        public bool Handled { get; set; }
        public bool ForcePropagation { get; set; }
    }
}
