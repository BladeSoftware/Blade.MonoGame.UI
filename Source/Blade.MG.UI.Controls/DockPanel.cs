using Blade.MG.UI;
using Blade.MG.UI.Components;
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

        private SplitterBar leftSplitter, rightSplitter, topSplitter, bottomSplitter;
        private int leftWidth = 120, rightWidth = 120, topHeight = 80, bottomHeight = 80;

        public DockPanel()
        {
            LeftPanel = new Panel();
            RightPanel = new Panel();
            TopPanel = new Panel();
            BottomPanel = new Panel();
            CenterPanel = new Panel();

            leftSplitter = new SplitterBar(SplitterOrientation.Horizontal) { Thickness = 6 };
            rightSplitter = new SplitterBar(SplitterOrientation.Horizontal) { Thickness = 6 };
            topSplitter = new SplitterBar(SplitterOrientation.Vertical) { Thickness = 6 };
            bottomSplitter = new SplitterBar(SplitterOrientation.Vertical) { Thickness = 6 };

            leftSplitter.OnDrag = (val) => { leftWidth = Math.Max(40, Math.Min(val, FinalContentRect.Width - rightWidth - 40)); StateHasChanged(); };
            rightSplitter.OnDrag = (val) => { rightWidth = Math.Max(40, Math.Min(FinalContentRect.Width - leftWidth - val, FinalContentRect.Width - leftWidth - 40)); StateHasChanged(); };
            topSplitter.OnDrag = (val) => { topHeight = Math.Max(30, Math.Min(val, FinalContentRect.Height - bottomHeight - 30)); StateHasChanged(); };
            bottomSplitter.OnDrag = (val) => { bottomHeight = Math.Max(30, Math.Min(FinalContentRect.Height - topHeight - val, FinalContentRect.Height - topHeight - 30)); StateHasChanged(); };

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

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
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
    }
}