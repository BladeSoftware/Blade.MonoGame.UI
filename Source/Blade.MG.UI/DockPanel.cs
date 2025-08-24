using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Microsoft.VisualStudio.Threading;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI
{
    public class DockPanel : Container
    {
        public Panel LeftPanel { get; private set; }
        public Panel RightPanel { get; private set; }
        public Panel TopPanel { get; private set; }
        public Panel BottomPanel { get; private set; }
        public Panel CenterPanel { get; private set; }

        /// <summary>
        /// If true, left/right panels are inset between top/bottom panels.
        /// If false, top/bottom panels are inset between left/right panels.
        /// </summary>
        public bool InsetLeftRightPanels { get; set; } = false;


        private SplitterBar leftSplitter;
        private SplitterBar rightSplitter;
        private SplitterBar topSplitter;
        private SplitterBar bottomSplitter;

        private int leftWidth = 120;
        private int rightWidth = 120;
        private int topHeight = 80;
        private int bottomHeight = 80;

        public int LeftWidth
        {
            get => IsLeftPanelVisible ? leftWidth : 0;
            set => leftWidth = (int)Math.Clamp(value, 0, ActualWidth - RightWidth - MinLeftWidth);
        }

        public int RightWidth
        {
            get => IsRightPanelVisible ? rightWidth : 0;
            set => rightWidth = (int)Math.Clamp(value, 0, ActualWidth - LeftWidth - MinRightWidth);
        }

        public int TopHeight
        {
            get => IsTopPanelVisible ? topHeight : 0;
            set => topHeight = (int)Math.Clamp(value, 0, ActualHeight - BottomHeight - MinTopHeight);
        }

        public int BottomHeight
        {
            get => IsBottomPanelVisible ? bottomHeight : 0;
            set => bottomHeight = (int)Math.Clamp(value, 0, ActualHeight - TopHeight - MinBottomHeight);
        }

        private int splitterThickness = 8;
        public int SplitterThickness
        {
            get => splitterThickness;
            set
            {
                splitterThickness = Math.Clamp(value, 0, 100);

                if (leftSplitter != null) leftSplitter.Thickness = value;
                if (rightSplitter != null) rightSplitter.Thickness = value;
                if (topSplitter != null) topSplitter.Thickness = value;
                if (bottomSplitter != null) bottomSplitter.Thickness = value;
            }
        }

        public int MinLeftWidth { get; set; } = 40;
        public int MinRightWidth { get; set; } = 40;
        public int MinTopHeight { get; set; } = 40;
        public int MinBottomHeight { get; set; } = 40;

        // Add properties to control visibility
        public bool IsLeftPanelVisible
        {
            get => LeftPanel.Visible == Visibility.Visible;
            set
            {
                LeftPanel.Visible.Value = value ? Visibility.Visible : Visibility.Collapsed;
                leftSplitter.Visible.Value = value ? Visibility.Visible : Visibility.Collapsed;

                // Recalculate to ensure it fits within bounds
                if (value && ActualWidth > 0)
                {
                    leftWidth = (int)Math.Clamp(leftWidth, 0, Math.Max(MinLeftWidth, ActualWidth - RightWidth - MinLeftWidth));
                    rightWidth = (int)Math.Clamp(rightWidth, 0, Math.Max(MinRightWidth, ActualWidth - LeftWidth - MinRightWidth));
                }
            }
        }

        public bool IsRightPanelVisible
        {
            get => RightPanel.Visible == Visibility.Visible;
            set
            {
                RightPanel.Visible.Value = value ? Visibility.Visible : Visibility.Collapsed;
                rightSplitter.Visible.Value = value ? Visibility.Visible : Visibility.Collapsed;

                // Recalculate to ensure it fits within bounds
                if (value && ActualWidth > 0)
                {
                    rightWidth = (int)Math.Clamp(rightWidth, 0, Math.Max(MinRightWidth, ActualWidth - LeftWidth - MinRightWidth));
                    leftWidth = (int)Math.Clamp(leftWidth, 0, Math.Max(MinLeftWidth, ActualWidth - RightWidth - MinLeftWidth));
                }
            }
        }

        public bool IsTopPanelVisible
        {
            get => TopPanel.Visible == Visibility.Visible;
            set
            {
                TopPanel.Visible.Value = value ? Visibility.Visible : Visibility.Collapsed;
                topSplitter.Visible.Value = value ? Visibility.Visible : Visibility.Collapsed;

                // Recalculate to ensure it fits within bounds
                if (value && ActualHeight > 0)
                {
                    topHeight = (int)Math.Clamp(topHeight, 0, Math.Max(MinTopHeight, ActualHeight - BottomHeight - MinTopHeight));
                    bottomHeight = (int)Math.Clamp(bottomHeight, 0, Math.Max(MinBottomHeight, ActualHeight - TopHeight - MinBottomHeight));
                }
            }
        }

        public bool IsBottomPanelVisible
        {
            get => BottomPanel.Visible == Visibility.Visible;
            set
            {
                BottomPanel.Visible.Value = value ? Visibility.Visible : Visibility.Collapsed;
                bottomSplitter.Visible.Value = value ? Visibility.Visible : Visibility.Collapsed;

                // Recalculate to ensure it fits within bounds
                if (value && ActualHeight > 0)
                {
                    bottomHeight = (int)Math.Clamp(bottomHeight, 0, Math.Max(MinBottomHeight, ActualHeight - TopHeight - MinBottomHeight));
                    topHeight = (int)Math.Clamp(topHeight, 0, Math.Max(MinTopHeight, ActualHeight - BottomHeight - MinTopHeight));
                }
            }
        }


        public DockPanel()
        {
            LeftPanel = new Panel();
            RightPanel = new Panel();
            TopPanel = new Panel();
            BottomPanel = new Panel();
            CenterPanel = new Panel();

            leftSplitter = new SplitterBar(SplitterOrientation.Horizontal) { Thickness = splitterThickness };
            rightSplitter = new SplitterBar(SplitterOrientation.Horizontal) { Thickness = splitterThickness };
            topSplitter = new SplitterBar(SplitterOrientation.Vertical) { Thickness = splitterThickness };
            bottomSplitter = new SplitterBar(SplitterOrientation.Vertical) { Thickness = splitterThickness };

            IsLeftPanelVisible = true;
            IsRightPanelVisible = true;
            IsTopPanelVisible = true;
            IsBottomPanelVisible = true;

        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            IsHitTestVisible = true;

            int initialSize = 0;

            leftSplitter.OnDragStart = (drag) =>
            {
                // Store initial width
                initialSize = LeftWidth;
            };

            leftSplitter.OnDragging = (drag) =>
            {
                int newWidth = initialSize + drag.Delta.X;
                LeftWidth = (int)Math.Max(MinLeftWidth, Math.Min(newWidth, ActualWidth - RightWidth - MinLeftWidth));
                StateHasChanged();
            };

            rightSplitter.OnDragStart = (drag) =>
            {
                // Store initial width
                initialSize = RightWidth;
            };

            rightSplitter.OnDragging = (drag) =>
            {
                int newWidth = initialSize - drag.Delta.X;
                RightWidth = (int)Math.Max(MinRightWidth, Math.Min(newWidth, ActualWidth - LeftWidth - MinRightWidth));
                StateHasChanged();
            };

            topSplitter.OnDragStart = (drag) =>
            {
                // Store initial height
                initialSize = TopHeight;
            };

            topSplitter.OnDragging = (drag) =>
            {
                int newHeight = initialSize + drag.Delta.Y;
                TopHeight = (int)Math.Max(MinTopHeight, Math.Min(newHeight, ActualHeight - BottomHeight - MinTopHeight));
                StateHasChanged();
            };

            bottomSplitter.OnDragStart = (drag) =>
            {
                // Store initial height
                initialSize = BottomHeight;
            };

            bottomSplitter.OnDragging = (drag) =>
            {
                int newHeight = initialSize - drag.Delta.Y;
                BottomHeight = (int)Math.Max(MinBottomHeight, Math.Min(newHeight, ActualHeight - TopHeight - MinBottomHeight));
                StateHasChanged();
            };


            AddChild(LeftPanel);
            AddChild(RightPanel);
            AddChild(TopPanel);
            AddChild(BottomPanel);
            AddChild(CenterPanel);

            AddChild(leftSplitter);
            AddChild(rightSplitter);
            AddChild(topSplitter);
            AddChild(bottomSplitter);

        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);

            int lw = LeftWidth;
            int rw = RightWidth;
            int th = TopHeight;
            int bh = BottomHeight;

            if (InsetLeftRightPanels)
            {
                // Left/Right panels are inset between Top/Bottom panels (default)
                LeftPanel.Arrange(context, new Rectangle(layoutBounds.Left, layoutBounds.Top + th, lw, layoutBounds.Height - th - bh), layoutBounds);
                leftSplitter.Arrange(context, new Rectangle(layoutBounds.Left + lw, layoutBounds.Top + th, leftSplitter.Thickness, layoutBounds.Height - th - bh), layoutBounds);

                RightPanel.Arrange(context, new Rectangle(layoutBounds.Right - rw, layoutBounds.Top + th, rw, layoutBounds.Height - th - bh), layoutBounds);
                rightSplitter.Arrange(context, new Rectangle(layoutBounds.Right - rw - rightSplitter.Thickness, layoutBounds.Top + th, rightSplitter.Thickness, layoutBounds.Height - th - bh), layoutBounds);

                TopPanel.Arrange(context, new Rectangle(layoutBounds.Left, layoutBounds.Top, layoutBounds.Width, th), layoutBounds);
                topSplitter.Arrange(context, new Rectangle(layoutBounds.Left, layoutBounds.Top + th, layoutBounds.Width, topSplitter.Thickness), layoutBounds);

                BottomPanel.Arrange(context, new Rectangle(layoutBounds.Left, layoutBounds.Bottom - bh, layoutBounds.Width, bh), layoutBounds);
                bottomSplitter.Arrange(context, new Rectangle(layoutBounds.Left, layoutBounds.Bottom - bh - bottomSplitter.Thickness, layoutBounds.Width, bottomSplitter.Thickness), layoutBounds);

                CenterPanel.Arrange(context, new Rectangle(layoutBounds.Left + lw + leftSplitter.Thickness,
                                                           layoutBounds.Top + th + topSplitter.Thickness,
                                                           layoutBounds.Width - lw - rw - leftSplitter.Thickness - rightSplitter.Thickness,
                                                           layoutBounds.Height - th - bh - topSplitter.Thickness - bottomSplitter.Thickness),
                                    layoutBounds);
            }
            else
            {
                // Top/Bottom panels are inset between Left/Right panels
                LeftPanel.Arrange(context, new Rectangle(layoutBounds.Left, layoutBounds.Top, lw, layoutBounds.Height), layoutBounds);
                leftSplitter.Arrange(context, new Rectangle(layoutBounds.Left + lw, layoutBounds.Top, leftSplitter.Thickness, layoutBounds.Height), layoutBounds);

                RightPanel.Arrange(context, new Rectangle(layoutBounds.Right - rw, layoutBounds.Top, rw, layoutBounds.Height), layoutBounds);
                rightSplitter.Arrange(context, new Rectangle(layoutBounds.Right - rw - rightSplitter.Thickness, layoutBounds.Top, rightSplitter.Thickness, layoutBounds.Height), layoutBounds);

                TopPanel.Arrange(context, new Rectangle(layoutBounds.Left + lw + leftSplitter.Thickness, layoutBounds.Top, layoutBounds.Width - lw - rw - leftSplitter.Thickness - rightSplitter.Thickness, th), layoutBounds);
                topSplitter.Arrange(context, new Rectangle(layoutBounds.Left + lw + leftSplitter.Thickness, layoutBounds.Top + th, layoutBounds.Width - lw - rw - leftSplitter.Thickness - rightSplitter.Thickness, topSplitter.Thickness), layoutBounds);

                BottomPanel.Arrange(context, new Rectangle(layoutBounds.Left + lw + leftSplitter.Thickness, layoutBounds.Bottom - bh, layoutBounds.Width - lw - rw - leftSplitter.Thickness - rightSplitter.Thickness, bh), layoutBounds);
                bottomSplitter.Arrange(context, new Rectangle(layoutBounds.Left + lw + leftSplitter.Thickness, layoutBounds.Bottom - bh - bottomSplitter.Thickness, layoutBounds.Width - lw - rw - leftSplitter.Thickness - rightSplitter.Thickness, bottomSplitter.Thickness), layoutBounds);

                CenterPanel.Arrange(context, new Rectangle(layoutBounds.Left + lw + leftSplitter.Thickness,
                                                           layoutBounds.Top + th + topSplitter.Thickness,
                                                           layoutBounds.Width - lw - rw - leftSplitter.Thickness - rightSplitter.Thickness,
                                                           layoutBounds.Height - th - bh - topSplitter.Thickness - bottomSplitter.Thickness),
                                    layoutBounds);
            }
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            base.RenderControl(context, layoutBounds, parentTransform);
        }
    }
}