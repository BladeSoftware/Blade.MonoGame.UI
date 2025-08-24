using Blade.MG.UI.Components;
using Microsoft.VisualStudio.Threading;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
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
        public bool InsetLeftRightPanels { get; set; } = true;


        private SplitterBar leftSplitter;
        private SplitterBar rightSplitter;
        private SplitterBar topSplitter;
        private SplitterBar bottomSplitter;

        private Length leftWidth = 120;
        private Length rightWidth = 120;
        private Length topHeight = 80;
        private Length bottomHeight = 80;

        //private int initalClampSize = 32000;

        private int leftWidthPixels => (int)LeftWidth.ToPixelsWidth(this, FinalRect);
        private int rightWidthPixels => (int)RightWidth.ToPixelsWidth(this, FinalRect);
        private int topHeightPixels => (int)TopHeight.ToPixelsWidth(this, FinalRect);
        private int bottomHeightPixels => (int)BottomHeight.ToPixelsWidth(this, FinalRect);


        private bool firstRender = true;
        private Length initialLeftWidth;
        private Length initialRightWidth;
        private Length initialTopHeight;
        private Length initialBottomHeight;

        public Length LeftWidth
        {
            get => IsLeftPanelVisible ? leftWidth : 0;
            set
            {
                if (ActualWidth <= 0)
                {
                    //leftWidth = Math.Clamp(value, 0, Math.Max(0, initalClampSize));
                    leftWidth = value;
                }
                else
                {
                    //leftWidth = (int)Math.Clamp(value, 0, Math.Max(0, ActualWidth - RightWidth - MinLeftWidth));
                    leftWidth = (int)Math.Clamp(value.ToPixelsWidth(this, FinalContentRect), 0, Math.Max(0, ActualWidth - rightWidthPixels - MinLeftWidth));
                }
            }
        }

        public Length RightWidth
        {
            get => IsRightPanelVisible ? rightWidth : 0;
            set
            {
                if (ActualWidth <= 0)
                {
                    //rightWidth = Math.Clamp(value, 0, Math.Max(0, initalClampSize));
                    rightWidth = value;
                }
                else
                {
                    rightWidth = (int)Math.Clamp(value.ToPixelsWidth(this, FinalContentRect), 0, Math.Max(0, ActualWidth - leftWidthPixels - MinRightWidth));
                }
            }
        }

        public Length TopHeight
        {
            get => IsTopPanelVisible ? topHeight : 0;
            set
            {
                if (ActualWidth <= 0)
                {
                    //topHeight = Math.Clamp(value, 0, Math.Max(0, initalClampSize));
                    topHeight = value;
                }
                else
                {
                    topHeight = (int)Math.Clamp(value.ToPixelsWidth(this, FinalContentRect), 0, Math.Max(0, ActualHeight - bottomHeightPixels - MinTopHeight));
                }
            }
        }

        public Length BottomHeight
        {
            get => IsBottomPanelVisible ? bottomHeight : 0;
            set
            {
                if (ActualWidth <= 0)
                {
                    //bottomHeight = Math.Clamp(value, 0, Math.Max(0, initalClampSize));
                    bottomHeight = value;
                }
                else
                {
                    bottomHeight = (int)Math.Clamp(value.ToPixelsWidth(this, FinalContentRect), 0, Math.Max(0, ActualHeight - topHeightPixels - MinBottomHeight));
                }
            }
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
                    leftWidth = (int)Math.Clamp(leftWidthPixels, 0, Math.Max(MinLeftWidth, ActualWidth - rightWidthPixels - MinLeftWidth));
                    rightWidth = (int)Math.Clamp(rightWidthPixels, 0, Math.Max(MinRightWidth, ActualWidth - leftWidthPixels - MinRightWidth));
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
                    rightWidth = (int)Math.Clamp(rightWidthPixels, 0, Math.Max(MinRightWidth, ActualWidth - leftWidthPixels - MinRightWidth));
                    leftWidth = (int)Math.Clamp(leftWidthPixels, 0, Math.Max(MinLeftWidth, ActualWidth - rightWidthPixels - MinLeftWidth));
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
                    topHeight = (int)Math.Clamp(topHeightPixels, 0, Math.Max(MinTopHeight, ActualHeight - bottomHeightPixels - MinTopHeight));
                    bottomHeight = (int)Math.Clamp(bottomHeightPixels, 0, Math.Max(MinBottomHeight, ActualHeight - topHeightPixels - MinBottomHeight));
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
                    bottomHeight = (int)Math.Clamp(bottomHeightPixels, 0, Math.Max(MinBottomHeight, ActualHeight - topHeightPixels - MinBottomHeight));
                    topHeight = (int)Math.Clamp(topHeightPixels, 0, Math.Max(MinTopHeight, ActualHeight - bottomHeightPixels - MinTopHeight));
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

            initialLeftWidth = leftWidth;
            initialRightWidth = rightWidth;
            initialTopHeight = topHeight;
            initialBottomHeight = bottomHeight;


            int initialSize = 0;

            leftSplitter.OnDragStart = (drag) =>
            {
                // Store initial width
                initialSize = leftWidthPixels;
            };

            leftSplitter.OnDragging = (drag) =>
            {
                int newWidth = initialSize + drag.Delta.X;
                LeftWidth = (int)Math.Max(MinLeftWidth, Math.Min(newWidth, ActualWidth - rightWidthPixels - MinLeftWidth));
                StateHasChanged();
            };

            rightSplitter.OnDragStart = (drag) =>
            {
                // Store initial width
                initialSize = rightWidthPixels;
            };

            rightSplitter.OnDragging = (drag) =>
            {
                int newWidth = initialSize - drag.Delta.X;
                RightWidth = (int)Math.Max(MinRightWidth, Math.Min(newWidth, ActualWidth - leftWidthPixels - MinRightWidth));
                StateHasChanged();
            };

            topSplitter.OnDragStart = (drag) =>
            {
                // Store initial height
                initialSize = topHeightPixels;
            };

            topSplitter.OnDragging = (drag) =>
            {
                int newHeight = initialSize + drag.Delta.Y;
                TopHeight = (int)Math.Max(MinTopHeight, Math.Min(newHeight, ActualHeight - bottomHeightPixels - MinTopHeight));
                StateHasChanged();
            };

            bottomSplitter.OnDragStart = (drag) =>
            {
                // Store initial height
                initialSize = bottomHeightPixels;
            };

            bottomSplitter.OnDragging = (drag) =>
            {
                int newHeight = initialSize - drag.Delta.Y;
                BottomHeight = (int)Math.Max(MinBottomHeight, Math.Min(newHeight, ActualHeight - topHeightPixels - MinBottomHeight));
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
            //base.Arrange(context, layoutBounds, parentLayoutBounds);
            ArrangeSelf(context, layoutBounds, parentLayoutBounds);

            if (firstRender)
            {
                firstRender = false;

                LeftWidth = initialLeftWidth;
                RightWidth = initialRightWidth;
                TopHeight = initialTopHeight;
                BottomHeight = initialBottomHeight;

            }

            int lw = leftWidthPixels;
            int rw = rightWidthPixels;
            int th = topHeightPixels;
            int bh = bottomHeightPixels;

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