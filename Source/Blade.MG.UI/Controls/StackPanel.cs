using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    public class StackPanel : ScrollPanel
    {
        public Orientation Orientation = Orientation.Horizontal;
        public bool StretchLastChild = false;

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
            //if (string.Equals(Name, "QBC")) { }
            //IsWidthVirtual = Orientation == Orientation.Horizontal;
            //IsHeightVirtual = Orientation == Orientation.Vertical;

            //foreach (var child in Children)
            //{
            //    // Don't allow Center Alignment types in a stack panel
            //    if (Orientation == Orientation.Horizontal && child.HorizontalAlignment.Value == HorizontalAlignmentType.Center)
            //    {
            //        child.HorizontalAlignment = HorizontalAlignmentType.Left;
            //    }

            //    if (Orientation == Orientation.Vertical && child.VerticalAlignment.Value == VerticalAlignmentType.Center)
            //    {
            //        child.VerticalAlignment = VerticalAlignmentType.Top;
            //    }
            //}

            base.Measure(context, ref availableSize, ref parentMinMax);

            // -- Measure Children ---
            float desiredWidth = 0f;
            float desiredHeight = 0f;

            var desiredsize = DesiredSize;

            //foreach (var child in CollectionsMarshal.AsSpan(Children))
            foreach (var child in Children)
            {
                // Don't allow Center Alignment types in a stack panel
                //if (Orientation == Orientation.Horizontal && child.HorizontalAlignment.Value == HorizontalAlignmentType.Center)
                //{
                //    child.HorizontalAlignment = HorizontalAlignmentType.Left;
                //}

                //if (Orientation == Orientation.Vertical && child.VerticalAlignment.Value == VerticalAlignmentType.Center)
                //{
                //    child.VerticalAlignment = VerticalAlignmentType.Top;
                //}

                //child.Measure(context, ref availableSize, ref parentMinMax);

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
                else
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


            // -- Merge Child Desired Size with this Stack Panel's Desired Size --
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
            // Arrange the layout for the inherited scroll panel and it's scrollbars
            //base.Arrange(context, layoutBounds, parentLayoutBounds);
            //base.Arrange(context, layoutBounds, layoutBounds);

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

            //if (string.Equals(Name, "AnimationCellStackPanel"))
            //{
            //    rect = rect with { X = 240, Width = 0, Height = 0 };
            //}


            // Adjust the child control layout to stack them
            int left = 0;
            int top = 0;

            foreach (var childCtrl in Children)
            //foreach (var childCtrl in CollectionsMarshal.AsSpan<UIComponent>((List<UIComponent>)Children))
            {
                if (childCtrl == child)
                {
                    // Only arrange the current child as we would already have called arranged on the previous children in the list
                    //var childLayoutBounds = rect with { X = rect.X + left, Y = rect.Y + top};
                    var childLayoutBounds = rect with
                    {
                        X = rect.X + left,
                        Y = rect.Y + top,
                        Width = Orientation == Orientation.Horizontal ? (int)child.DesiredSize.Width : rect.Width,
                        Height = Orientation == Orientation.Vertical ? (int)child.DesiredSize.Height : rect.Height
                    };

                    childCtrl.Arrange(context, childLayoutBounds, rect);
                    //childCtrl.Arrange(context, rect, rect);


                    if (Orientation == Orientation.Horizontal)
                    {
                        if (childCtrl.Visible.Value == Visibility.Collapsed)
                        {
                            childCtrl.FinalRect = childCtrl.FinalRect with { X = rect.Left + left };
                            childCtrl.FinalContentRect = childCtrl.FinalContentRect with { X = childCtrl.FinalRect.Left };
                        }
                        else
                        {
                            childCtrl.FinalRect = childCtrl.FinalRect with { X = rect.Left + left + child.Margin.Value.Left };
                            childCtrl.FinalContentRect = childCtrl.FinalContentRect with { X = childCtrl.FinalRect.Left + childCtrl.Padding.Value.Left };
                        }
                    }
                    else
                    {
                        if (childCtrl.Visible.Value == Visibility.Collapsed)
                        {
                            childCtrl.FinalRect = childCtrl.FinalRect with { Y = rect.Top + top };
                            childCtrl.FinalContentRect = childCtrl.FinalContentRect with { Y = childCtrl.FinalRect.Top };
                        }
                        else
                        {
                            childCtrl.FinalRect = childCtrl.FinalRect with { Y = rect.Top + top + child.Margin.Value.Top };
                            childCtrl.FinalContentRect = childCtrl.FinalContentRect with { Y = childCtrl.FinalRect.Top + childCtrl.Padding.Value.Top };
                        }
                    }

                    break;
                }


                if (childCtrl.Visible.Value != Visibility.Collapsed)
                {
                    if (Orientation == Orientation.Horizontal)
                    {
                        int cw = childCtrl.FinalRect.Width + childCtrl.Margin.Value.Left + childCtrl.Margin.Value.Right;
                        left += cw;
                    }
                    else
                    {
                        int ch = childCtrl.FinalRect.Height + childCtrl.Margin.Value.Top + childCtrl.Margin.Value.Bottom;
                        top += ch;
                    }
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
            }

            return child.FinalRect;
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            //if (string.Equals(Name, "AnimationCellStackPanel")) { }

            base.RenderControl(context, Rectangle.Intersect(layoutBounds, FinalRect), parentTransform);
        }

    }
}
