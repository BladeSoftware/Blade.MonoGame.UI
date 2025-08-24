using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
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

        private SplitterBar leftSplitter, rightSplitter, topSplitter, bottomSplitter;
        private int leftWidth = 120, rightWidth = 120, topHeight = 80, bottomHeight = 80;

        private int splitterThickness = 8;

        public int MinLeftWidth { get; set; } = 40;
        public int MinRightWidth { get; set; } = 40;
        public int MinTopHeight { get; set; } = 40;
        public int MinBottomHeight { get; set; } = 40;


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

        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            IsHitTestVisible = true;

            int initialSize = 0;

            leftSplitter.OnDragStart = (drag) =>
            {
                // Store initial width
                initialSize = leftWidth;
            };

            leftSplitter.OnDragging = (drag) =>
            {
                int newWidth = initialSize + drag.Delta.X;
                leftWidth = (int)Math.Max(40, Math.Min(newWidth, ActualWidth - rightWidth - MinLeftWidth));
                StateHasChanged();
            };

            rightSplitter.OnDragStart = (drag) =>
            {
                // Store initial width
                initialSize = rightWidth;
            };

            rightSplitter.OnDragging = (drag) =>
            {
                int newWidth = initialSize - drag.Delta.X;
                rightWidth = (int)Math.Max(40, Math.Min(newWidth, ActualWidth - leftWidth - MinRightWidth));
                StateHasChanged();
            };

            topSplitter.OnDragStart = (drag) =>
            {
                // Store initial height
                initialSize = topHeight;
            };

            topSplitter.OnDragging = (drag) =>
            {
                int newHeight = initialSize + drag.Delta.Y;
                topHeight = (int)Math.Max(30, Math.Min(newHeight, ActualHeight - bottomHeight - MinTopHeight));
                StateHasChanged();
            };

            bottomSplitter.OnDragStart = (drag) =>
            {
                // Store initial height
                initialSize = bottomHeight;
            };

            bottomSplitter.OnDragging = (drag) =>
            {
                int newHeight = initialSize - drag.Delta.Y;
                bottomHeight = (int)Math.Max(30, Math.Min(newHeight, ActualHeight - topHeight - MinBottomHeight));
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

            //// Measure each docked panel and splitter
            //Size leftSize = new Size(leftWidth, Math.Max(0, availableSize.Height - topHeight - bottomHeight));
            //Size rightSize = new Size(rightWidth, Math.Max(0, availableSize.Height - topHeight - bottomHeight));
            //Size topSize = new Size(Math.Max(0, availableSize.Width), topHeight);
            //Size bottomSize = new Size(Math.Max(0, availableSize.Width), bottomHeight);

            //Layout dummyLayout = parentMinMax;

            //LeftPanel.Measure(context, ref leftSize, ref dummyLayout);
            //RightPanel.Measure(context, ref rightSize, ref dummyLayout);
            //TopPanel.Measure(context, ref topSize, ref dummyLayout);
            //BottomPanel.Measure(context, ref bottomSize, ref dummyLayout);

            //// Splitters
            //Size splitterVSize = new Size(leftSplitter.Thickness, leftSize.Height);
            //Size splitterHSize = new Size(topSize.Width, topSplitter.Thickness);

            //leftSplitter.Measure(context, ref splitterVSize, ref dummyLayout);
            //rightSplitter.Measure(context, ref splitterVSize, ref dummyLayout);
            //topSplitter.Measure(context, ref splitterHSize, ref dummyLayout);
            //bottomSplitter.Measure(context, ref splitterHSize, ref dummyLayout);

            //// Center panel fills remaining space
            //float centerWidth = Math.Max(0, availableSize.Width - leftWidth - rightWidth - leftSplitter.Thickness - rightSplitter.Thickness);
            //float centerHeight = Math.Max(0, availableSize.Height - topHeight - bottomHeight - topSplitter.Thickness - bottomSplitter.Thickness);
            //Size centerSize = new Size(centerWidth, centerHeight);

            //CenterPanel.Measure(context, ref centerSize, ref dummyLayout);

            //// Desired size is the available size (DockPanel stretches to fill parent)
            //DesiredSize = new Size(availableSize.Width, availableSize.Height);

            //ClampDesiredSize(availableSize, parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);

            int lw = leftWidth;
            int rw = rightWidth;
            int th = topHeight;
            int bh = bottomHeight;

            // Left Panel
            LeftPanel.Arrange(context, new Rectangle(layoutBounds.Left, layoutBounds.Top + th, lw, layoutBounds.Height - th - bh), layoutBounds);

            // Left Splitter
            leftSplitter.Arrange(context, new Rectangle(layoutBounds.Left + lw, layoutBounds.Top + th, leftSplitter.Thickness, layoutBounds.Height - th - bh), layoutBounds);

            // Right Panel
            RightPanel.Arrange(context, new Rectangle(layoutBounds.Right - rw, layoutBounds.Top + th, rw, layoutBounds.Height - th - bh), layoutBounds);

            // Right Splitter
            rightSplitter.Arrange(context, new Rectangle(layoutBounds.Right - rw - rightSplitter.Thickness, layoutBounds.Top + th, rightSplitter.Thickness, layoutBounds.Height - th - bh), layoutBounds);

            // Top Panel
            TopPanel.Arrange(context, new Rectangle(layoutBounds.Left, layoutBounds.Top, layoutBounds.Width, th), layoutBounds);

            // Top Splitter
            topSplitter.Arrange(context, new Rectangle(layoutBounds.Left, layoutBounds.Top + th, layoutBounds.Width, topSplitter.Thickness), layoutBounds);

            // Bottom Panel
            BottomPanel.Arrange(context, new Rectangle(layoutBounds.Left, layoutBounds.Bottom - bh, layoutBounds.Width, bh), layoutBounds);

            // Bottom Splitter
            bottomSplitter.Arrange(context, new Rectangle(layoutBounds.Left, layoutBounds.Bottom - bh - bottomSplitter.Thickness, layoutBounds.Width, bottomSplitter.Thickness), layoutBounds);

            // Center Panel
            CenterPanel.Arrange(context, new Rectangle(layoutBounds.Left + lw + leftSplitter.Thickness,
                                                       layoutBounds.Top + th + topSplitter.Thickness,
                                                       layoutBounds.Width - lw - rw - leftSplitter.Thickness - rightSplitter.Thickness,
                                                       layoutBounds.Height - th - bh - topSplitter.Thickness - bottomSplitter.Thickness),
                                layoutBounds);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            base.RenderControl(context, layoutBounds, parentTransform);
        }
    }
}