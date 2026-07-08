namespace Blade.MG.UI.Events
{
    /// <summary>
    /// Implemented by UI events that carry a screen position, so the generic bounds-checked
    /// dispatch in UIComponent can filter propagation by FinalRect without needing to know
    /// the concrete event type.
    /// </summary>
    public interface IPositionedEvent
    {
        int X { get; }
        int Y { get; }
    }
}
