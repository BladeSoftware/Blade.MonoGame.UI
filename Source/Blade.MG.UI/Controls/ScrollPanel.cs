using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Blade.MG.UI.Controls
{
    public class ScrollPanel : Panel
    {
        private bool isHorizontallyScrollable = false;
        private bool isVerticallyScrollable = false;

        public bool IsHorizontalScrollbarVisible => (horizontalScrollBarVisible == ScrollBarVisibility.Always) || (horizontalScrollBarVisible == ScrollBarVisibility.Auto && isHorizontallyScrollable);
        public bool IsVerticalScrollbarVisible => (verticalScrollBarVisible == ScrollBarVisibility.Always) || (verticalScrollBarVisible == ScrollBarVisibility.Auto && isVerticallyScrollable);


        private ScrollBarVisibility horizontalScrollBarVisible = ScrollBarVisibility.Auto;
        public ScrollBarVisibility HorizontalScrollBarVisible
        {
            get => horizontalScrollBarVisible;
            set
            {
                horizontalScrollBarVisible = value;

                if (VerticalScrollBar != null)
                {
                    VerticalScrollBar.Margin.Value = new Thickness(0, 0, 0, IsHorizontalScrollbarVisible ? 20 : 0);
                }
            }
        }

        private ScrollBarVisibility verticalScrollBarVisible = ScrollBarVisibility.Auto;

        public ScrollBarVisibility VerticalScrollBarVisible
        {
            get => verticalScrollBarVisible;
            set
            {
                verticalScrollBarVisible = value;

                if (HorizontalScrollBar != null)
                {
                    HorizontalScrollBar.Margin.Value = new Thickness(0, 0, IsVerticalScrollbarVisible ? 20 : 0, 0);
                }

            }
        }

        public ScrollBar HorizontalScrollBar { get; set; }
        public ScrollBar VerticalScrollBar { get; set; }

        private Visibility BoolToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

        public int HorizontalScrollOffset => HorizontalScrollBar?.ScrollOfset ?? 0;
        public int VerticalScrollOffset => VerticalScrollBar?.ScrollOfset ?? 0;


        public ScrollPanel()
        {
            IsHitTestVisible = false;
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            HorizontalScrollBar = new ScrollBar { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, IsVerticalScrollbarVisible ? 20 : 0, 0), Visible = BoolToVisibility(IsHorizontalScrollbarVisible), VerticalAlignment = VerticalAlignmentType.Bottom };
            VerticalScrollBar = new ScrollBar { Orientation = Orientation.Vertical, Margin = new Thickness(0, 0, 0, IsHorizontalScrollbarVisible ? 20 : 0), Visible = BoolToVisibility(IsVerticalScrollbarVisible), HorizontalAlignment = HorizontalAlignmentType.Right };

            //HorizontalScrollBar.Parent = this;
            //VerticalScrollBar.Parent = this;

            //internalChildren.Add(HorizontalScrollBar);
            //internalChildren.Add(VerticalScrollBar);

            AddInternalChild(HorizontalScrollBar);
            AddInternalChild(VerticalScrollBar);

        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            //base.Measure(context, ref availableSize, ref parentMinMax);

            //if (string.Equals(Name, "ProjectExplorerTree")) { }
            //if (string.Equals(Name, "AnimationCellStackPanel")) { }

            IsWidthVirtual = true;
            IsHeightVirtual = true;

            var contentAvailableSize = availableSize with
            {
                Width = availableSize.Width - (IsVerticalScrollbarVisible ? (int)VerticalScrollBar.Width.ToPixels() : 0),
                Height = availableSize.Height - (IsHorizontalScrollbarVisible ? (int)HorizontalScrollBar.Height.ToPixels() : 0)
            };

            base.Measure(context, ref contentAvailableSize, ref parentMinMax);


            HorizontalScrollBar.Measure(context, ref availableSize, ref parentMinMax);
            VerticalScrollBar.Measure(context, ref availableSize, ref parentMinMax);

            if (IsHorizontalScrollbarVisible)
            {
                AddChildDesiredSizeHorizontal(context, ref availableSize, ref parentMinMax, VerticalScrollBar);
            }

            if (IsVerticalScrollbarVisible)
            {
                AddChildDesiredSizeVertical(context, ref availableSize, ref parentMinMax, HorizontalScrollBar);
            }

        }

        /// <summary>
        /// Layout Children
        /// </summary>
        /// <param name="layoutBounds">Size of Parent Container</param>
        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);

            int verticalScrollBarWidth = IsVerticalScrollbarVisible ? (int)VerticalScrollBar.Width.ToPixels() : 0;
            int horizontalScrollBarHeight = IsHorizontalScrollbarVisible ? (int)HorizontalScrollBar.Height.ToPixels() : 0;

            // base.Arrange(context, layoutBounds, contentLayoutBounds);

            if (IsHorizontalScrollbarVisible) HorizontalScrollBar.Arrange(context, FinalRect, parentLayoutBounds);
            if (IsVerticalScrollbarVisible) VerticalScrollBar.Arrange(context, FinalRect, parentLayoutBounds);


            int contentWidth = FinalRect.Width - Padding.Value.Horizontal - verticalScrollBarWidth;
            int contentHeight = FinalRect.Height - Padding.Value.Vertical - horizontalScrollBarHeight;

            if (contentWidth < 0)
            {
                contentWidth = 0;
            }
            if (contentHeight < 0)
            {
                contentHeight = 0;
            }

            FinalContentRect = new Rectangle(FinalRect.Left + Padding.Value.Left, FinalRect.Top + Padding.Value.Top, contentWidth, contentHeight);


            // --=== Re-calculate the Scrollbar Max Values ===--
            // Find the largest size required by the child controls
            //Rectangle childBounds = Children.FirstOrDefault()?.FinalRect ?? Rectangle.Empty;
            Rectangle childBounds = Rectangle.Empty;
            //Rectangle childBounds = Children.FirstOrDefault()?.FinalContentRect ?? Rectangle.Empty;

            //foreach (var child in CollectionsMarshal.AsSpan(Children))
            foreach (var child in Children)
            {
                var childFinalRect = child.FinalRect with
                {
                    X = child.FinalRect.X - child.Margin.Value.Left,
                    Y = child.FinalRect.Y - child.Margin.Value.Top,
                    Width = child.FinalRect.Width + child.Margin.Value.Horizontal,
                    Height = child.FinalRect.Height + child.Margin.Value.Vertical
                };

                //var childFinalRect = child.FinalRect;

                if (childBounds == Rectangle.Empty)
                {
                    childBounds = childFinalRect;
                }
                else
                {
                    childBounds = Rectangle.Union(childBounds, childFinalRect);
                    //childBounds = Rectangle.Union(childBounds, child.FinalRect);
                }
            }

            // Substract the available parent area, as we don't need to scroll if everything fits in the available space
            int w = childBounds.Width - FinalContentRect.Width;// + verticalScrollBarWidth;
            int h = childBounds.Height - FinalContentRect.Height;// + horizontalScrollBarHeight;

            HorizontalScrollBar.MaxValue = w;
            VerticalScrollBar.MaxValue = h;

            isHorizontallyScrollable = (FinalContentRect.Width < childBounds.Width);
            isVerticallyScrollable = (FinalContentRect.Height < childBounds.Height);

            HorizontalScrollBar.Visible = BoolToVisibility(IsHorizontalScrollbarVisible);
            VerticalScrollBar.Visible = BoolToVisibility(IsVerticalScrollbarVisible);

            //HorizontalScrollBar.MaxValue = w < -verticalScrollBarWidth ? w : (w + verticalScrollBarWidth);
            //VerticalScrollBar.MaxValue = h < -horizontalScrollBarHeight ? h : (h + horizontalScrollBarHeight);

        }

        /// <summary>
        /// Calculate Childs layout rectangle
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public override Rectangle GetChildBoundingBox(UIContext context, UIComponent child)
        {
            var rect = base.GetChildBoundingBox(context, child);

            if (child.Visible.Value == Visibility.Collapsed)
            {
                return new Rectangle(rect.Left, rect.Top, 0, 0);
            }

            rect = rect with
            {
                X = rect.X - HorizontalScrollBar.ScrollOfset,
                Y = rect.Y - VerticalScrollBar.ScrollOfset,

                Width = rect.Width - (IsVerticalScrollbarVisible ? (int)VerticalScrollBar.Width.ToPixels() : 0),
                Height = rect.Height - (IsHorizontalScrollbarVisible ? (int)HorizontalScrollBar.Height.ToPixels() : 0)
            };

            return rect;
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            base.RenderControl(context, Rectangle.Intersect(layoutBounds, FinalRect), parentTransform);

            // If both scrollbars are visible, then there's a small rectangle gap where they meet.
            // This fills that gap
            if (IsHorizontalScrollbarVisible && IsVerticalScrollbarVisible)
            {
                try
                {
                    using var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);

                    // context.Renderer.FillRect(new Rectangle(finalRect.Left, finalRect.Bottom - HorizontalScrollBar.BarThickness, finalRect.Width, HorizontalScrollBar.BarThickness), HorizontalScrollBar.Background);
                    context.Renderer.FillRect(spriteBatch, new Rectangle(FinalRect.Right - VerticalScrollBar.BarThickness, FinalRect.Bottom - HorizontalScrollBar.BarThickness, VerticalScrollBar.BarThickness, HorizontalScrollBar.BarThickness), HorizontalScrollBar.Background.Value, Rectangle.Intersect(layoutBounds, FinalRect));

                }
                finally
                {
                    context.Renderer.EndBatch();
                }

            }

            if (IsHorizontalScrollbarVisible) HorizontalScrollBar.RenderControl(context, Rectangle.Intersect(layoutBounds, FinalRect), parentTransform);
            if (IsVerticalScrollbarVisible) VerticalScrollBar.RenderControl(context, Rectangle.Intersect(layoutBounds, FinalRect), parentTransform);
        }


        // ---=== UI Events ===---

        public override async Task HandleMouseDownEventAsync(UIWindow uiWindow, UIMouseDownEvent uiEvent)
        {
            if (IsHorizontalScrollbarVisible && HorizontalScrollBar != null) await HorizontalScrollBar.HandleMouseDownEventAsync(uiWindow, uiEvent);
            if (uiEvent.Handled) return;

            if (IsVerticalScrollbarVisible && VerticalScrollBar != null) await VerticalScrollBar.HandleMouseDownEventAsync(uiWindow, uiEvent);
            if (uiEvent.Handled) return;

            await base.HandleMouseDownEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleMouseUpEventAsync(UIWindow uiWindow, UIMouseUpEvent uiEvent)
        {
            if (IsHorizontalScrollbarVisible && HorizontalScrollBar != null) await HorizontalScrollBar.HandleMouseUpEventAsync(uiWindow, uiEvent);
            if (uiEvent.Handled) return;

            if (IsVerticalScrollbarVisible && VerticalScrollBar != null) await VerticalScrollBar.HandleMouseUpEventAsync(uiWindow, uiEvent);
            if (uiEvent.Handled) return;

            await base.HandleMouseUpEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleMouseMoveEventAsync(UIWindow uiWindow, UIMouseMoveEvent uiEvent)
        {
            if (IsHorizontalScrollbarVisible && HorizontalScrollBar != null) await HorizontalScrollBar.HandleMouseMoveEventAsync(uiWindow, uiEvent);
            if (uiEvent.Handled) return;

            if (IsVerticalScrollbarVisible && VerticalScrollBar != null) await VerticalScrollBar.HandleMouseMoveEventAsync(uiWindow, uiEvent);
            if (uiEvent.Handled) return;

            await base.HandleMouseMoveEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleMouseWheelScrollEventAsync(UIWindow uiWindow, UIMouseWheelScrollEvent uiEvent)
        {
            await base.HandleMouseWheelScrollEventAsync(uiWindow, uiEvent);

            if (uiEvent.Handled) return;

            if (IsHorizontalScrollbarVisible && HorizontalScrollBar != null) await HorizontalScrollBar.HandleMouseWheelScrollEventAsync(uiWindow, uiEvent);
            if (IsVerticalScrollbarVisible && VerticalScrollBar != null) await VerticalScrollBar.HandleMouseWheelScrollEventAsync(uiWindow, uiEvent);

        }

        public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            await base.HandleHoverChangedAsync(uiWindow, uiEvent);
        }

    }
}
