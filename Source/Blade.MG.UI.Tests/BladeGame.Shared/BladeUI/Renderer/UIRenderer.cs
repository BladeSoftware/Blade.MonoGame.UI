using System;
using System.Collections.Generic;
using System.Text;
using BladeGame.BladeUI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeGame.BladeUI.Renderer
{
    public class UIRenderer
    {
        protected UIContext Context { get; set; }
        private RasterizerState rasterizerState { get; set; }

        public UIRenderer(UIContext context)
        {
            this.Context = context;

            this.rasterizerState = new RasterizerState
            {
                ScissorTestEnable = true,
                CullMode = CullMode.None
            };

        }

        private Stack<Rectangle> clippingRect = new Stack<Rectangle>();

        private Rectangle ViewportBounds => Context.SpriteBatch.GraphicsDevice.Viewport.Bounds;

        public void BeginBatch(SpriteBatch spriteBatch)
        {
            clippingRect.Push(spriteBatch.GraphicsDevice.ScissorRectangle);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, rasterizerState, null, null);
        }

        public void EndBatch(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.GraphicsDevice.ScissorRectangle = clippingRect.Pop();
        }

        public void ClipToRect(Rectangle rect)
        {
            Rectangle viewport = ViewportBounds;

            int left = rect.Left;
            int top = rect.Top;
            int right = rect.Right;
            int bottom = rect.Bottom;

            if (left < viewport.Left) left = viewport.Left;
            if (left > viewport.Right) left = viewport.Right;
            if (right < viewport.Left) right = viewport.Left;
            if (right > viewport.Right) right = viewport.Right;
            if (top < viewport.Top) top = viewport.Top;
            if (top > viewport.Bottom) top = viewport.Bottom;
            if (bottom < viewport.Top) bottom = viewport.Top;
            if (bottom > viewport.Bottom) bottom = viewport.Bottom;

            int width = right - left + 1;
            int height = bottom - top + 1;

            if (width < 0) width = 0;
            if (height < 0) height = 0;


            Context.SpriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(left, top, width, height);
        }

        public void FillRect(Rectangle rectangle, Color color, Rectangle? clippingRect = null)
        {
            try
            {
                BeginBatch(Context.SpriteBatch);

                if (clippingRect != null)
                {
                    ClipToRect(clippingRect.Value);
                }

                Context.SpriteBatch.Draw(Context.Pixel, rectangle, color);
            }
            finally
            {
                EndBatch(Context.SpriteBatch);
            }
        }

        public void FillRect(Rectangle rectangle, Texture2D texture2D, Color color, Rectangle? clippingRect = null)
        {
            try
            {
                BeginBatch(Context.SpriteBatch);

                if (clippingRect != null)
                {
                    ClipToRect(clippingRect.Value);
                }

                Context.SpriteBatch.Draw(texture2D, rectangle, color);  // TODO: Implement Scaling Options ? Uniform / UniformToFit / Repeat? etc.
            }
            finally
            {
                EndBatch(Context.SpriteBatch);
            }
        }

        public void DrawString(Rectangle rectangle, string text, SpriteFont spriteFont, Color? color, HorizontalAlignmentType horizontalAlignment = HorizontalAlignmentType.Left, VerticalAlignmentType verticalAlignment = VerticalAlignmentType.Center)
        {
            try
            {
                BeginBatch(Context.SpriteBatch);

                SpriteFont font = spriteFont ?? Context.DefaultFont;
                Vector2 textSize = font.MeasureString(text);

                float x;
                float y;

                switch (horizontalAlignment)
                {
                    case HorizontalAlignmentType.Left:
                        x = rectangle.Left;
                        break;

                    case HorizontalAlignmentType.Center:
                    case HorizontalAlignmentType.Stretch:
                        x = ((rectangle.Left + rectangle.Right) / 2) - ((int)textSize.X / 2);
                        break;

                    case HorizontalAlignmentType.Right:
                        x = rectangle.Right - (int)textSize.X;
                        break;

                    default:
                        x = rectangle.Left;
                        break;
                }

                switch (verticalAlignment)
                {
                    case VerticalAlignmentType.Top:
                        y = rectangle.Top;
                        break;

                    case VerticalAlignmentType.Center:
                    case VerticalAlignmentType.Stretch:
                        y = ((rectangle.Top + rectangle.Bottom) / 2) - ((int)textSize.Y / 2);
                        break;

                    case VerticalAlignmentType.Bottom:
                        y = rectangle.Bottom - (int)textSize.Y;
                        break;

                    default:
                        y = rectangle.Left;
                        break;
                }

                Vector2 textPosition = new Vector2(x, y);
                ClipToRect(rectangle);

                Context.SpriteBatch.DrawString(font, text, textPosition, color ?? Color.White);

            }
            finally
            {
                EndBatch(Context.SpriteBatch);
            }
        }


    }
}
