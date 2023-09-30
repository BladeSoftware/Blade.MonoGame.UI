﻿using Blade.UI.Components;
using Blade.UI.Events;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;

namespace Blade.UI.Controls
{
    public class ScrollPanel : Panel
    {
        private bool horizontalScrollBarVisible = true;
        public bool HorizontalScrollBarVisible
        {
            get => horizontalScrollBarVisible;
            set
            {
                if (value != horizontalScrollBarVisible && VerticalScrollBar != null)
                {
                    VerticalScrollBar.Margin.Value = new Thickness(0, 0, 0, value ? 20 : 0);
                }

                horizontalScrollBarVisible = value;
            }
        }

        private bool verticalScrollBarVisible = true;

        public bool VerticalScrollBarVisible
        {
            get => verticalScrollBarVisible;
            set
            {
                if (value != verticalScrollBarVisible && HorizontalScrollBar != null)
                {
                    HorizontalScrollBar.Margin.Value = new Thickness(0, 0, value ? 20 : 0, 0);
                }

                verticalScrollBarVisible = value;
            }
        }

        public ScrollBar HorizontalScrollBar { get; set; }
        public ScrollBar VerticalScrollBar { get; set; }

        private Visibility BoolToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

        public int HorizontalScrollOffset => HorizontalScrollBar?.ScrollOfset ?? 0;
        public int VerticalScrollOffset => VerticalScrollBar?.ScrollOfset ?? 0;


        public ScrollPanel()
        {
            HitTestVisible = false;
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            HorizontalScrollBar = new ScrollBar { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, verticalScrollBarVisible ? 20 : 0, 0), Visible = BoolToVisibility(HorizontalScrollBarVisible), VerticalAlignment = VerticalAlignmentType.Bottom };
            VerticalScrollBar = new ScrollBar { Orientation = Orientation.Vertical, Margin = new Thickness(0, 0, 0, horizontalScrollBarVisible ? 20 : 0), Visible = BoolToVisibility(VerticalScrollBarVisible), HorizontalAlignment = HorizontalAlignmentType.Right };

            HorizontalScrollBar.Parent = this;
            VerticalScrollBar.Parent = this;

            privateControls.Add(HorizontalScrollBar);
            privateControls.Add(VerticalScrollBar);

        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);

            HorizontalScrollBar.Measure(context, ref availableSize, ref parentMinMax);
            VerticalScrollBar.Measure(context, ref availableSize, ref parentMinMax);

            if (HorizontalScrollBarVisible)
            {
                AddChildDesiredSizeHorizontal(context, ref availableSize, ref parentMinMax, VerticalScrollBar);
            }

            if (VerticalScrollBarVisible)
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
            // Reduce the parent layout area by the size of the scroll bars
            var parentLayoutContentBounds = parentLayoutBounds with
            {
                Width = layoutBounds.Width - (VerticalScrollBarVisible ? (int)VerticalScrollBar.Width.ToPixels() : 0),
                Height = layoutBounds.Height - (HorizontalScrollBarVisible ? (int)HorizontalScrollBar.Height.ToPixels() : 0)
            };

            base.Arrange(context, layoutBounds, parentLayoutContentBounds);

            //base.Arrange(context, layoutBounds, parentLayoutBounds);

            if (HorizontalScrollBarVisible) HorizontalScrollBar.Arrange(context, FinalRect, parentLayoutBounds);
            if (VerticalScrollBarVisible) VerticalScrollBar.Arrange(context, FinalRect, parentLayoutBounds);

            int contentWidth = FinalRect.Width - Padding.Value.Left - Padding.Value.Right;
            int contentHeight = FinalRect.Height - Padding.Value.Top - Padding.Value.Bottom;

            if (VerticalScrollBarVisible) contentWidth -= (int)VerticalScrollBar.Width.ToPixels();
            if (HorizontalScrollBarVisible) contentHeight -= (int)HorizontalScrollBar.Height.ToPixels();

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
            Rectangle childBounds = Children.FirstOrDefault()?.FinalRect ?? Rectangle.Empty;

            //foreach (var child in Children)
            foreach (var child in CollectionsMarshal.AsSpan<UIComponent>((List<UIComponent>)Children))
            {
                childBounds = Rectangle.Union(childBounds, child.FinalRect);
            }

            // Substract the available parent area, as we don't need to scroll if everything fits in the available space
            int w = childBounds.Width - FinalContentRect.Width;
            int h = childBounds.Height - FinalContentRect.Height;

            HorizontalScrollBar.MaxValue = w;
            VerticalScrollBar.MaxValue = h;

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

                Width = rect.Width - (VerticalScrollBarVisible ? (int)VerticalScrollBar.Width.ToPixels() : 0),
                Height = rect.Height - (HorizontalScrollBarVisible ? (int)HorizontalScrollBar.Height.ToPixels() : 0)
            };

            return rect;
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (this.Visible.Value != Visibility.Visible)
            {
                return;
            }

            base.RenderControl(context, Rectangle.Intersect(layoutBounds, FinalRect), parentTransform);

            // If both scrollbars are visible, then there's a small rectangle gap where they meet.
            // This fills that gap
            if (HorizontalScrollBarVisible && VerticalScrollBarVisible)
            {
                try
                {
                    context.Renderer.BeginBatch(transform: parentTransform);

                    // context.Renderer.FillRect(new Rectangle(finalRect.Left, finalRect.Bottom - HorizontalScrollBar.BarThickness, finalRect.Width, HorizontalScrollBar.BarThickness), HorizontalScrollBar.Background);
                    context.Renderer.FillRect(new Rectangle(FinalRect.Right - VerticalScrollBar.BarThickness, FinalRect.Bottom - HorizontalScrollBar.BarThickness, VerticalScrollBar.BarThickness, HorizontalScrollBar.BarThickness), HorizontalScrollBar.Background.Value);

                }
                finally
                {
                    context.Renderer.EndBatch();
                }

            }

            if (HorizontalScrollBarVisible) HorizontalScrollBar.RenderControl(context, FinalRect, parentTransform);
            if (VerticalScrollBarVisible) VerticalScrollBar.RenderControl(context, FinalRect, parentTransform);
        }


        // ---=== UI Events ===---

        public override async Task HandleMouseDownEventAsync(UIWindow uiWindow, UIMouseDownEvent uiEvent)
        {
            if (HorizontalScrollBarVisible && HorizontalScrollBar != null) await HorizontalScrollBar.HandleMouseDownEventAsync(uiWindow, uiEvent);
            if (uiEvent.Handled) return;

            if (VerticalScrollBarVisible && VerticalScrollBar != null) await VerticalScrollBar.HandleMouseDownEventAsync(uiWindow, uiEvent);
            if (uiEvent.Handled) return;

            await base.HandleMouseDownEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleMouseUpEventAsync(UIWindow uiWindow, UIMouseUpEvent uiEvent)
        {
            if (HorizontalScrollBarVisible && HorizontalScrollBar != null) await HorizontalScrollBar.HandleMouseUpEventAsync(uiWindow, uiEvent);
            if (uiEvent.Handled) return;

            if (VerticalScrollBarVisible && VerticalScrollBar != null) await VerticalScrollBar.HandleMouseUpEventAsync(uiWindow, uiEvent);
            if (uiEvent.Handled) return;

            await base.HandleMouseUpEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleMouseMoveEventAsync(UIWindow uiWindow, UIMouseMoveEvent uiEvent)
        {
            if (HorizontalScrollBarVisible && HorizontalScrollBar != null) await HorizontalScrollBar.HandleMouseMoveEventAsync(uiWindow, uiEvent);
            if (uiEvent.Handled) return;

            if (VerticalScrollBarVisible && VerticalScrollBar != null) await VerticalScrollBar.HandleMouseMoveEventAsync(uiWindow, uiEvent);
            if (uiEvent.Handled) return;

            await base.HandleMouseMoveEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleMouseWheelScrollEventAsync(UIWindow uiWindow, UIMouseWheelScrollEvent uiEvent)
        {
            await base.HandleMouseWheelScrollEventAsync(uiWindow, uiEvent);

            if (uiEvent.Handled) return;

            if (HorizontalScrollBarVisible && HorizontalScrollBar != null) await HorizontalScrollBar.HandleMouseWheelScrollEventAsync(uiWindow, uiEvent);
            if (VerticalScrollBarVisible && VerticalScrollBar != null) await VerticalScrollBar.HandleMouseWheelScrollEventAsync(uiWindow, uiEvent);

        }

        public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            await base.HandleHoverChangedAsync(uiWindow, uiEvent);
        }

    }
}