using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Blade.MG.UI.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        public int Thickness { get; set; } = 6;  // Default thickness

        public Action<DragContext> OnDragStart { get; set; }
        public Action<DragContext> OnDragging { get; set; }
        public Action<DragContext> OnDragEnd { get; set; }

        private bool isDragging = false;
        private Point dragStart;
        private int initialOffset;

        public SplitterBar(SplitterOrientation orientation)
        {
            Orientation = orientation;
            IsHitTestVisible = true;
            Background = Color.Gray;
        }

        private DragContext GetDragContext(Point currentPoint)
        {
            var dragContext = new DragContext
            {
                DragStart = dragStart,
                CurrentPoint = currentPoint,
                //Delta = new Point(currentPoint.X - dragStart.X, currentPoint.Y - dragStart.Y)
            };

            // Splitter bars can only be dragged in one orientation
            dragContext.Delta = Orientation == SplitterOrientation.Horizontal
                ? new Point(currentPoint.X - dragStart.X, 0)
                : new Point(0, currentPoint.Y - dragStart.Y);

            return dragContext;
        }

        public override async Task HandleMouseDownEventAsync(UIWindow uiWindow, UIMouseDownEvent uiEvent)
        {
            if (uiEvent.Handled) return;

            if (uiEvent.PrimaryButton.Pressed && FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                LockEventsToControl(uiWindow, this);

                isDragging = true;
                dragStart = new Point(uiEvent.X, uiEvent.Y);

                initialOffset = Orientation == SplitterOrientation.Horizontal ? (int)Left : (int)Top;

                OnDragStart?.Invoke(GetDragContext(dragStart));

                uiEvent.Handled = true;

                StateHasChanged();

                return;
            }

            await base.HandleMouseDownEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleMouseUpEventAsync(UIWindow uiWindow, UIMouseUpEvent uiEvent)
        {
            if (uiEvent.Handled) return;

            if (isDragging)
            {
                UnlockEventsFromControl(uiWindow, this);

                isDragging = false;

                OnDragEnd?.Invoke(GetDragContext(new Point(uiEvent.X, uiEvent.Y)));

                uiEvent.Handled = true;

                StateHasChanged();

                return;
            }

            await base.HandleMouseUpEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleMouseMoveEventAsync(UIWindow uiWindow, UIMouseMoveEvent uiEvent)
        {
            if (isDragging && OnDragging != null)
            {
                OnDragging?.Invoke(GetDragContext(new Point(uiEvent.X, uiEvent.Y)));

                //OnDragging(initialOffset + delta);
            }

            await base.HandleMouseMoveEventAsync(uiWindow, uiEvent);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
                return;

            base.RenderControl(context, layoutBounds, parentTransform);

            try
            {
                using var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);

                Color barColor = isDragging ? Color.DarkGray : Background.Value;
                context.Renderer.FillRect(spriteBatch, FinalRect, barColor, layoutBounds);

                // Optionally, draw a grip indicator
                if (Thickness >= 6)
                {
                    int gripSize = 3;
                    int gripLength = 20;

                    Color gripColor = Color.LightGray;
                    if (Orientation == SplitterOrientation.Horizontal)
                    {
                        int y = FinalRect.Center.Y - gripSize;
                        int xStart = FinalRect.Left + 4;
                        int xEnd = FinalRect.Right - 4;
                        context.Renderer.FillRect(spriteBatch, new Rectangle(xStart, y, xEnd - xStart, gripSize * 2), gripColor, layoutBounds);
                    }
                    else
                    {
                        int x = FinalRect.Center.X - gripSize;
                        int yStart = FinalRect.Top + 4;
                        int yEnd = FinalRect.Bottom - 4;
                        context.Renderer.FillRect(spriteBatch, new Rectangle(x, yStart, gripSize * 2, yEnd - yStart), gripColor, layoutBounds);
                    }
                }
            }
            finally
            {
                context.Renderer.EndBatch();
            }
        }
    }
}