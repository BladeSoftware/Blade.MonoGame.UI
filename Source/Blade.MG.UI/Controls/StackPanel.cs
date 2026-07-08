using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    public class StackPanel : ScrollPanel
    {
        public Orientation Orientation = Orientation.Horizontal;
        public bool StretchLastChild = false;

        // Running offset along the stacking axis, advanced once per child during Arrange
        // instead of being recomputed by rescanning all prior siblings on every call to
        // GetChildBoundingBox (which was O(n) per child, O(n^2) per layout pass).
        private int stackOffset;

        public StackPanel()
        {
            IsHitTestVisible = false;
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);

            if (Orientation == Orientation.Horizontal)
            {
                IsWidthVirtual = true;
                IsHeightVirtual = false;
            }
            else
            {
                IsWidthVirtual = false;
                IsHeightVirtual = true;
            }

            float desiredWidth = 0f;
            float desiredHeight = 0f;

            foreach (var child in Children)
            {
                // Constrain child measurement based on orientation
                Size childAvailableSize = availableSize;
                if (Orientation == Orientation.Vertical)
                {
                    // Constrain width, allow height to be unconstrained
                    childAvailableSize = new Size(availableSize.Width, float.NaN);
                }
                else // Horizontal
                {
                    // Constrain height, allow width to be unconstrained
                    childAvailableSize = new Size(float.NaN, availableSize.Height);
                }

                Layout childParentMinMax = parentMinMax;
                child.Measure(context, ref childAvailableSize, ref childParentMinMax);

                if (Orientation == Orientation.Horizontal)
                {
                    if (!float.IsNaN(child.DesiredSize.Width))
                    {
                        desiredWidth += child.DesiredSize.Width;
                    }
                    if (!float.IsNaN(child.DesiredSize.Height) && child.DesiredSize.Height > desiredHeight)
                    {
                        desiredHeight = child.DesiredSize.Height;
                    }
                }
                else // Vertical
                {
                    if (!float.IsNaN(child.DesiredSize.Height))
                    {
                        desiredHeight += child.DesiredSize.Height;
                    }
                    if (!float.IsNaN(child.DesiredSize.Width) && child.DesiredSize.Width > desiredWidth)
                    {
                        desiredWidth = child.DesiredSize.Width;
                    }
                }
            }

            // Merge Child Desired Size with this Stack Panel's Desired Size
            if (!float.IsNaN(desiredWidth) && !float.IsNaN(DesiredSize.Width) && desiredWidth < DesiredSize.Width)
            {
                desiredWidth = DesiredSize.Width;
            }
            if (!float.IsNaN(desiredHeight) && !float.IsNaN(DesiredSize.Height) && desiredHeight < DesiredSize.Height)
            {
                desiredHeight = DesiredSize.Height;
            }

            DesiredSize = new Size(desiredWidth, desiredHeight);

            ClampDesiredSize(availableSize, parentMinMax);
        }


        /// <summary>
        /// Layout Children
        /// </summary>
        /// <param name="layoutBounds">Size of Parent Container</param>
        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            // Arrange the scroll panel first to setup the scrollbars and content area
            base.ArrangeSelf(context, layoutBounds, layoutBounds);

            if (StretchLastChild)
            {
                // Stretch the last child to fill any remaining space
                if (Children != null && Children.Count > 0)
                {
                    var lastChild = Children[Children.Count - 1];

                    if (Orientation == Orientation.Horizontal)
                    {
                        int usedWidth = 0;
                        foreach (var child in Children)
                        {
                            if (child != lastChild && child.Visible.Value != Visibility.Collapsed)
                            {
                                usedWidth += (int)child.DesiredSize.Width + child.Margin.Value.Left + child.Margin.Value.Right;
                            }
                        }
                        int remainingWidth = layoutBounds.Width - usedWidth - lastChild.Margin.Value.Left - lastChild.Margin.Value.Right;
                        if (remainingWidth > 0)
                        {
                            lastChild.DesiredSize = new Size(remainingWidth, lastChild.DesiredSize.Height);
                        }
                    }
                    else
                    {
                        int usedHeight = 0;
                        foreach (var child in Children)
                        {
                            if (child != lastChild && child.Visible.Value != Visibility.Collapsed)
                            {
                                usedHeight += (int)child.DesiredSize.Height + child.Margin.Value.Top + child.Margin.Value.Bottom;
                            }
                        }
                        int remainingHeight = layoutBounds.Height - usedHeight - lastChild.Margin.Value.Top - lastChild.Margin.Value.Bottom;
                        if (remainingHeight > 0)
                        {
                            lastChild.DesiredSize = new Size(lastChild.DesiredSize.Width, remainingHeight);
                        }
                    }
                }
            }

            // Arrange all the children within the content area of the scroll panel
            stackOffset = 0;
            base.Arrange(context, layoutBounds, layoutBounds);
        }

        /// <summary>
        /// Calculate Childs layout rectangle
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public override Rectangle GetChildBoundingBox(UIContext context, UIComponent child)
        {
            var rect = base.GetChildBoundingBox(context, child);

            // Position this child using the running offset accumulated from prior siblings
            // in this Arrange pass (see stackOffset), rather than rescanning all prior
            // siblings on every call.
            int left = Orientation == Orientation.Horizontal ? stackOffset : 0;
            int top = Orientation == Orientation.Vertical ? stackOffset : 0;

            var childLayoutBounds = rect with
            {
                X = rect.X + left,
                Y = rect.Y + top,
                Width = Orientation == Orientation.Horizontal ? (int)child.DesiredSize.Width : rect.Width,
                Height = Orientation == Orientation.Vertical ? (int)child.DesiredSize.Height : rect.Height
            };

            child.Arrange(context, childLayoutBounds, rect);

            if (Orientation == Orientation.Horizontal)
            {
                if (child.Visible.Value == Visibility.Collapsed)
                {
                    child.FinalRect = child.FinalRect with { X = rect.Left + left };
                    child.FinalContentRect = child.FinalContentRect with { X = child.FinalRect.Left };
                }
                else
                {
                    child.FinalRect = child.FinalRect with { X = rect.Left + left + child.Margin.Value.Left };
                    child.FinalContentRect = child.FinalContentRect with { X = child.FinalRect.Left + child.Padding.Value.Left };
                }
            }
            else
            {
                if (child.Visible.Value == Visibility.Collapsed)
                {
                    child.FinalRect = child.FinalRect with { Y = rect.Top + top };
                    child.FinalContentRect = child.FinalContentRect with { Y = child.FinalRect.Top };
                }
                else
                {
                    child.FinalRect = child.FinalRect with { Y = rect.Top + top + child.Margin.Value.Top };
                    child.FinalContentRect = child.FinalContentRect with { Y = child.FinalRect.Top + child.Padding.Value.Top };
                }
            }

            // Add the Margin back in
            if (child.Visible.Value == Visibility.Collapsed)
            {
                child.FinalRect = child.FinalRect with
                {
                    X = child.FinalRect.X - child.Margin.Value.Left,
                    Y = child.FinalRect.Y - child.Margin.Value.Top
                };
            }
            else
            {
                child.FinalRect = child.FinalRect with
                {
                    X = child.FinalRect.X - child.Margin.Value.Left,
                    Y = child.FinalRect.Y - child.Margin.Value.Top,
                    Width = child.FinalRect.Width + child.Margin.Value.Left + child.Margin.Value.Right,
                    Height = child.FinalRect.Height + child.Margin.Value.Top + child.Margin.Value.Bottom,
                };

                // Advance the running offset for the next sibling. child.FinalRect.Width/Height
                // already has the margin folded in (see reassignment above), so adding the
                // margin again here would double-count it and push every subsequent sibling
                // one extra margin too far along the stacking axis.
                if (Orientation == Orientation.Horizontal)
                {
                    stackOffset += child.FinalRect.Width;
                }
                else
                {
                    stackOffset += child.FinalRect.Height;
                }
            }

            return child.FinalRect;
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            base.RenderControl(context, Rectangle.Intersect(layoutBounds, FinalRect), parentTransform);
        }

    }
}
