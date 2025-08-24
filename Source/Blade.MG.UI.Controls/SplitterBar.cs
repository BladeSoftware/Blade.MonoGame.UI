using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    public enum SplitterOrientation
    {
        Horizontal,
        Vertical
    }

    public class SplitterBar : Control
    {
        public SplitterOrientation Orientation { get; set; }
        public int Thickness { get; set; } = 6;
        public Action<int> OnDrag { get; set; }

        private bool isDragging = false;
        private Point dragStart;
        private int initialOffset;

        public SplitterBar(SplitterOrientation orientation)
        {
            Orientation = orientation;
            IsHitTestVisible = true;
            Background = Color.Gray;
        }

        public override async Task HandleMouseDownEventAsync(UIWindow uiWindow, UIMouseDownEvent uiEvent)
        {
            isDragging = true;
            dragStart = new Point(uiEvent.X, uiEvent.Y);
            initialOffset = Orientation == SplitterOrientation.Horizontal ? (int)Left : (int)Top;
            await base.HandleMouseDownEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleMouseUpEventAsync(UIWindow uiWindow, UIMouseUpEvent uiEvent)
        {
            isDragging = false;
            await base.HandleMouseUpEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleMouseMoveEventAsync(UIWindow uiWindow, UIMouseMoveEvent uiEvent)
        {
            if (isDragging && OnDrag != null)
            {
                int delta = Orientation == SplitterOrientation.Horizontal
                    ? uiEvent.X - dragStart.X
                    : uiEvent.Y - dragStart.Y;
                OnDrag(initialOffset + delta);
            }
            await base.HandleMouseMoveEventAsync(uiWindow, uiEvent);
        }
    }
}