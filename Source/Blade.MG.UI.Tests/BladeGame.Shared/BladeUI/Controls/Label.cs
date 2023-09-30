using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BladeGame.BladeUI;
using BladeGame.BladeUI.Components;
using BladeGame.Shared.BladeUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeGame.BladeUI.Controls
{
    public class Label : Control
    {
        public Binding<string> Text { get; set; } = new Binding<string>();
        public Binding<SpriteFont> SpriteFont { get; set; } = new Binding<SpriteFont>();
        public Binding<Color> TextColor { get; set; } = new Binding<Color>();

        //public bool WordWrap { get; set; }

        public Label()
        {
            Text = null;
            //WordWrap = false;
            SpriteFont = null; // Use default font
            TextColor.Value = Color.White;
            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;
            HorizontalContentAlignment = HorizontalAlignmentType.Center;
            VerticalContentAlignment = VerticalAlignmentType.Center;
        }


        public override void Measure(UIContext context, Size availableSize, ref Layout parentMinMax)
        {
            parentMinMax.Merge(MinWidth, MinHeight, MaxWidth, MaxHeight);

            float desiredWidth = Width;
            float desiredHeight = Height;

            if ((float.IsNaN(Width) || float.IsNaN(Height)) && Text != null)
            {
                SpriteFont font = SpriteFont != null ? SpriteFont.Value : context.DefaultFont;
                Vector2 textSize = font.MeasureString(Text.ToString());

                if (float.IsNaN(Width))
                {
                    desiredWidth = textSize.X;
                }

                if (float.IsNaN(Height))
                {
                    desiredHeight = textSize.Y;
                }
            }

            if (Margin != null)
            {
                desiredWidth += Margin.Value.Left + Margin.Value.Right;
                desiredHeight += Margin.Value.Top + Margin.Value.Bottom;
            }

            if (Padding != null)
            {
                desiredWidth += Padding.Value.Left + Padding.Value.Right;
                desiredHeight += Padding.Value.Top + Padding.Value.Bottom;
            }

            DesiredSize = new Size(desiredWidth, desiredHeight);

            ClampDesiredSize(availableSize, parentMinMax);

            //base.Measure(context, availableSize);
        }

        public override void Arrange(Rectangle layoutBounds)
        {
            base.Arrange(layoutBounds);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds)
        {
            base.RenderControl(context, layoutBounds);

            context.Renderer.DrawString(finalContentRect, Text.ToString(), SpriteFont?.Value, TextColor?.Value, HorizontalContentAlignment.Value, VerticalContentAlignment.Value);
        }
    }
}
