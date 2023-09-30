using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BladeGame.BladeUI;
using BladeGame.BladeUI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeGame.BladeUI.Controls
{
    public class Panel : Container
    {
        public Color Background { get; set; }
        public Texture2D BackgroundTexture { get; set; }

        public Panel()
        {
            Background = Color.Transparent;
        }

        //public override Rectangle GetChildBoundingBox(BladeControl child)
        //{
        //    float x = 0;
        //    float w = 0;
        //    float y = 0;
        //    float h = 0;

        //    x = Left;
        //    y = Top;

        //    w = ActualWidth;
        //    h = ActualHeight;

        //    return new Rectangle((int)x, (int)y, (int)w, (int)h);
        //}

        public override void Measure(UIContext context, Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, availableSize, ref parentMinMax);

            //DesiredSize = new Size(DesiredSize.Width + Padding.Left + Padding.Right, DesiredSize.Height + Padding.Top + Padding.Bottom);

            //ClampDesiredSize(availableSize);
        }

        public override void Arrange(Rectangle layoutBounds)
        {
            base.Arrange(layoutBounds);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds)
        {
            //base.RenderControl(context, layoutBounds);

            if (BackgroundTexture != null)
            {
                // Draw a rectangle using the 1x1 texture we created
                context.Renderer.FillRect(finalRect, BackgroundTexture, Background); // TODO: Implement Scaling Options ? Uniform / UniformToFit / Repeat? etc.
            }
            else if (this.Background != Color.Transparent)
            {
                context.Renderer.FillRect(finalRect, Background);
            }

            foreach (var child in Children)
            {
                //if (child.Name == "Panel3") { } // Debugging

                //child.RenderControl(context, finalRect);
                child.RenderControl(context, finalContentRect);
            }

        }
    }
}
