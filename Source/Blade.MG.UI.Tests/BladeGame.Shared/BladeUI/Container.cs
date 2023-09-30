using System;
using System.Collections.Generic;
using System.Text;
using BladeGame.BladeUI.Components;
using BladeGame.Shared.BladeUI.Components;
using Microsoft.Xna.Framework;

namespace BladeGame.BladeUI
{
    public class Container : UIComponent
    {
        public IEnumerable<UIComponent> Children { get; private set; } = new List<UIComponent>();

        public Container()
        {

        }

        public void AddChild(UIComponent item, UIComponent parent = null)
        {
            item.Parent = parent ?? this;
            ((List<UIComponent>)Children).Add(item);
        }

        public override void Measure(UIContext context, Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, availableSize, ref parentMinMax);

            float desiredWidth = float.NaN;
            float desiredHeight = float.NaN;

            //availableSize = new Size(availableSize.Width - Padding.Left - Padding.Right, availableSize.Height - Padding.Top - Padding.Bottom);

            foreach (var child in Children)
            {
                child.Measure(context, availableSize, ref parentMinMax);

                if (!float.IsNaN(child.DesiredSize.Width))
                {
                    if (float.IsNaN(desiredWidth))
                    {
                        desiredWidth = child.DesiredSize.Width;
                    }
                    else if (child.DesiredSize.Width > desiredWidth)
                    {
                        desiredWidth = child.DesiredSize.Width;
                    }
                }

                if (!float.IsNaN(child.DesiredSize.Height))
                {
                    if (float.IsNaN(desiredHeight))
                    {
                        desiredHeight = child.DesiredSize.Height;
                    }
                    else if (child.DesiredSize.Height > desiredHeight)
                    {
                        desiredHeight = child.DesiredSize.Height;
                    }
                }

            }


            if (!float.IsNaN(Width))
            {
                desiredWidth = Width;
            }

            if (!float.IsNaN(Height))
            {
                desiredHeight = Height;
            }


            desiredWidth += Margin.Value.Left + Margin.Value.Right;
            desiredHeight += Margin.Value.Top + Margin.Value.Bottom;

            DesiredSize = new Size(desiredWidth, desiredHeight);

            ClampDesiredSize(availableSize, parentMinMax);

            //dw = DesiredSize.Width;
            //dh = DesiredSize.Height;

            //if (!float.IsNaN(dw) && dw > availableSize.Width)
            //{
            //    dw = availableSize.Width;
            //}

            //if (!float.IsNaN(dh) && dh > availableSize.Height)
            //{
            //    dh = availableSize.Height;
            //}

            //DesiredSize = new Size(dw, dh);
        }

        public override void Arrange(Rectangle layoutBounds)
        {
            base.Arrange(layoutBounds);

            foreach (var child in Children)
            {
                //if (child.Name == "Label1")  { }

                //child.Arrange(new BladeRect(finalRect.Left, finalRect.Top, ActualWidth, ActualHeight));
                child.Arrange(GetChildBoundingBox(child));
            }

        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds)
        {
            base.RenderControl(context, layoutBounds);

            // Render Child controls
            foreach (var child in Children)
            {
                child.RenderControl(context, finalRect);
            }
        }
    }
}
