using Blade.MG.Primitives;
using Blade.MG.UI.Components;
using Blade.MG.UI.Services;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blade.MG.UI.Renderer
{
    public class UIRenderer
    {
        private class State
        {
            public Rectangle ClipRect;
            //public DepthStencilState DepthStencilState;
            //public BlendState BlendState;
            //public Effect Effect;
        }

        protected UIContext Context { get; set; }
        //public GraphicsDevice GraphicsDevice => Context?.Game?.GraphicsDevice;
        public GraphicsDevice GraphicsDevice { get; set; }
        //private SpriteBatch spriteBatch { get; set; }

        public RasterizerState rasterizerState { get; private set; }

        public static DepthStencilState stencilStateReplaceAlways; // Always replace stencil pixels, no depth test
        public static DepthStencilState stencilStateZeroAlways; // Use to clear stencil pixels
        public static DepthStencilState stencilStateKeepLessEqual;

        public static BlendState blendStateStencilOnly; // Blend State to only write to Depth Buffer and not Back Buffer

        public UIRenderer(UIContext context)
        {
            Context = context;
            GraphicsDevice = Context?.GraphicsDevice;
            //spriteBatch = context.SpriteBatch;

            rasterizerState = new RasterizerState
            {
                ScissorTestEnable = true,
                CullMode = CullMode.None,
                //FillMode = FillMode.WireFrame
                FillMode = FillMode.Solid
            };

            InitStates(context);
        }

        private static Texture2D filledTriangleTexture = null;
        public static Texture2D FilledTriangleTexture(SpriteBatch spriteBatch)
        {
            // return Content.Load<Texture2D>("Images/circle");
            if (filledTriangleTexture == null)
            {
                using (var newSpriteBatch = new SpriteBatch(spriteBatch.GraphicsDevice))
                {
                    var saveRenderTargets = newSpriteBatch.GraphicsDevice.GetRenderTargets();

                    filledTriangleTexture = new RenderTarget2D(newSpriteBatch.GraphicsDevice, 20, 20);
                    newSpriteBatch.GraphicsDevice.SetRenderTarget((RenderTarget2D)filledTriangleTexture);
                    newSpriteBatch.GraphicsDevice.Clear(Color.Transparent);

                    newSpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
                    Primitives2D.FillCircle(newSpriteBatch, 18f / 2f, 18 / 2f, 8f, Color.White);
                    //                    Primitives2D.FillRect(newSpriteBatch, 0, 0, 20, 20, Color.Green);
                    //newSpriteBatch.Draw(Primitives2D.PixelTexture(newSpriteBatch), new Vector2(0, 0), null, Color.Blue, 0f, new Vector2(0, 0), new Vector2(10f, 10f), SpriteEffects.None, 0);
                    newSpriteBatch.End();

                    newSpriteBatch.GraphicsDevice.SetRenderTargets(saveRenderTargets);
                }
            }

            return filledTriangleTexture;
        }

        private void InitStates(UIContext context)
        {
            lock (this)
            {
                // Always replace the Stencil value
                stencilStateReplaceAlways = new DepthStencilState
                {
                    StencilEnable = true,
                    StencilFunction = CompareFunction.Always,
                    StencilPass = StencilOperation.Replace,
                    ReferenceStencil = 1,
                    DepthBufferEnable = false
                };

                stencilStateZeroAlways = new DepthStencilState
                {
                    StencilEnable = true,
                    StencilFunction = CompareFunction.Always,
                    StencilPass = StencilOperation.Zero,
                    ReferenceStencil = 1,
                    DepthBufferEnable = false
                };



                stencilStateKeepLessEqual = new DepthStencilState
                {
                    StencilEnable = true,
                    StencilFunction = CompareFunction.LessEqual,
                    StencilPass = StencilOperation.Keep,
                    ReferenceStencil = 1,
                    DepthBufferEnable = false
                };

                //https://gamedev.stackexchange.com/questions/130970/draw-only-on-stencil-buffer-monogame
                blendStateStencilOnly = new BlendState()
                {
                    ColorWriteChannels = ColorWriteChannels.None,  // Don't write to the Colour buffer
                };

            }
        }

        private Stack<State> renderState = new Stack<State>();
        private Stack<DepthStencilState> depthStencil = new Stack<DepthStencilState>();

        private Rectangle ViewportBounds => GraphicsDevice.Viewport.Bounds;
        //private Rectangle ViewportBounds => Context.Game.GraphicsDevice.Viewport.Bounds;


        //private DepthStencilState currentDepthStencilState = DepthStencilState.Default;
        //private BlendState currentBlendState = BlendState.AlphaBlend;
        //private Effect currentEffect = null;

        //public void BeginBatch(SpriteBatch spriteBatch)

        private static DepthStencilState stencilState2 = new DepthStencilState
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.LessEqual,
            StencilPass = StencilOperation.Keep,
            ReferenceStencil = 1,
            DepthBufferEnable = false
        };

        public SpriteBatch BeginBatch(Transform? transform, SpriteSortMode spriteSortMode = SpriteSortMode.Immediate, DepthStencilState depthStencilState = null, Effect effect = null, BlendState blendState = null)
        {
            PushState();

            //BlendState currentBlendState = blendState ?? BlendState.AlphaBlend;
            BlendState currentBlendState = blendState ?? BlendState.NonPremultiplied;
            //DepthStencilState currentDepthStencilState = depthStencilState ?? stencilStateKeepLessEqual;
            //DepthStencilState currentDepthStencilState = depthStencilState ?? DepthStencilState.None;
            DepthStencilState currentDepthStencilState = depthStencilState ?? stencilState2;

            Effect currentEffect = effect;

            SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch.Begin(spriteSortMode, currentBlendState, SamplerState.LinearClamp, currentDepthStencilState, rasterizerState, currentEffect, transform?.GetMatrix());

            return spriteBatch;
        }

        public void EndBatch()
        {
            //spriteBatch.End();

            PopState();

            //var state = clippingRect.Pop();
            //spriteBatch.GraphicsDevice.ScissorRectangle = state.ClipRect;
            //lastDepthStencilState = state.DepthStencilState;
        }

        public void PushState()
        {
            var state = new State
            {
                ClipRect = GraphicsDevice.ScissorRectangle,
                //DepthStencilState = currentDepthStencilState,
                //BlendState = currentBlendState,
                //Effect = currentEffect
            };

            renderState.Push(state);

            //currentDepthStencilState = depthStencilState ?? currentDepthStencilState;
            //currentEffect = effect ?? currentEffect;
            //currentBlendState = blendState ?? currentBlendState;

        }

        public void PopState()
        {
            var state = renderState.Pop();

            GraphicsDevice.ScissorRectangle = state.ClipRect;

            //currentDepthStencilState = state.DepthStencilState;
        }

        public void ClearStencilBuffer()
        {
            using var spriteBatch = BeginBatch(depthStencilState: stencilStateReplaceAlways, effect: null, blendState: blendStateStencilOnly, transform: null);
            //Primitives2D.FillRect(Context.Game, spriteBatch, ViewportBounds, Color.White);
            Primitives2D.FillRect(spriteBatch, ViewportBounds, Color.White);
            EndBatch();

        }

        public void ClipToRect(Rectangle rect)
        {
            Rectangle clippedRect = Rectangle.Intersect(rect, ViewportBounds);

            if (clippedRect == Rectangle.Empty)
            {
                GraphicsDevice.ScissorRectangle = new Rectangle(-1, -1, 1, 1);
            }
            else
            {
                GraphicsDevice.ScissorRectangle = new Rectangle(clippedRect.Left, clippedRect.Top, clippedRect.Width, clippedRect.Height);
            }
        }

        public void FillRect(SpriteBatch spriteBatch, Rectangle rectangle, Color color, Rectangle? clippingRect = null)
        {
            if (clippingRect != null)
            {
                ClipToRect(clippingRect.Value);
            }

            spriteBatch.Draw(Context.Pixel, rectangle, color);
        }

        public void DrawRect(SpriteBatch spriteBatch, Rectangle rectangle, Color color, Rectangle? clippingRect = null)
        {
            if (clippingRect != null)
            {
                ClipToRect(clippingRect.Value);
            }

            Primitives2D.DrawRect(spriteBatch, rectangle, color);
        }

        //public void FillRect(Rectangle rectangle, Transform transform, Color color, Rectangle? clippingRect = null)
        //{
        //    FillRect(rectangle, transform.GetMatrix(), color, clippingRect);
        //}

        //public void FillRect(Rectangle rectangle, Matrix matrix, Color color, Rectangle? clippingRect = null)
        //{
        //    if (clippingRect != null)
        //    {
        //        ClipToRect(clippingRect.Value);
        //    }

        //    if (matrix == Matrix.Identity)
        //    {
        //        spriteBatch.Draw(Context.Pixel, rectangle, color);
        //    }
        //    else
        //    {
        //        //spriteBatch.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.PointList, 0, 0, 1);
        //        BasicEffect basicEffect = new BasicEffect(spriteBatch.GraphicsDevice);

        //        basicEffect.Projection = Matrix.CreateOrthographicOffCenter(ViewportBounds, 0f, 10f);

        //        //basicEffect.Texture = myTexture;
        //        //basicEffect.TextureEnabled = true;
        //        basicEffect.DiffuseColor = color.ToVector3();
        //        basicEffect.Alpha = (float)color.A / 255f;

        //        VertexPositionTexture[] vert = new VertexPositionTexture[4];
        //        //float n = 200f;
        //        //vert[0].Position = new Vector3(0, 0, 0);
        //        //vert[1].Position = new Vector3(n, 0, 0);
        //        //vert[2].Position = new Vector3(0, n, 0);
        //        //vert[3].Position = new Vector3(n, n, 0);

        //        vert[0].Position = Vector3.Transform(new Vector3(rectangle.X, rectangle.Y, 0f), matrix);
        //        vert[1].Position = Vector3.Transform(new Vector3(rectangle.X + rectangle.Width, rectangle.Y, 0f), matrix);
        //        vert[2].Position = Vector3.Transform(new Vector3(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height, 0f), matrix);
        //        vert[3].Position = Vector3.Transform(new Vector3(rectangle.X, rectangle.Y + rectangle.Height, 0f), matrix);


        //        vert[0].TextureCoordinate = new Vector2(0, 0);
        //        vert[1].TextureCoordinate = new Vector2(1, 0);
        //        vert[2].TextureCoordinate = new Vector2(1, 1);
        //        vert[3].TextureCoordinate = new Vector2(0, 1);

        //        int[] ind = new int[6];
        //        ind[0] = 0;
        //        ind[1] = 1;
        //        ind[2] = 2;
        //        ind[3] = 2;
        //        ind[4] = 3;
        //        ind[5] = 0;

        //        foreach (EffectPass effectPass in basicEffect.CurrentTechnique.Passes)
        //        {
        //            effectPass.Apply();

        //            spriteBatch.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(
        //                PrimitiveType.TriangleList, vert, 0, vert.Length, ind, 0, ind.Length / 3);
        //        }
        //    }
        //}

        //public void FillRect(Rectangle rectangle, Texture2D texture2D, Color color, Rectangle? clippingRect = null)
        //{
        //    if (clippingRect != null)
        //    {
        //        ClipToRect(clippingRect.Value);
        //    }

        //    spriteBatch.Draw(texture2D ?? Context.Pixel, rectangle, color);  // TODO: Implement Scaling Options ? Uniform / UniformToFit / Repeat? etc.
        //}

        public void DrawTexture(Texture2D texture, Rectangle rectangle, TextureLayout textureLayout, Vector2 scale, Rectangle? clippingRect = null)
        {
            var viewport = GraphicsDevice.Viewport;

            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);

            Matrix view = Matrix.Identity;
            //if (clippingRect != null)
            //{
            //    float translateX = 0f;
            //    float translateY = 0f;

            //    float imgOffsetX = rectangle.Left - clippingRect.Value.Left;
            //    float imgOffsetY = rectangle.Top - clippingRect.Value.Top;


            //    //if (imgOffsetX >= 0)
            //    //{
            //    switch (textureLayout.HorizontalAlignment)
            //    {
            //        case HorizontalAlignmentType.Left: translateX = 0f; break;
            //        case HorizontalAlignmentType.Center: translateX = (clippingRect.Value.Width - rectangle.Width) / 2f; break;
            //        case HorizontalAlignmentType.Right: translateX = clippingRect.Value.Width - rectangle.Width; break;
            //        default: translateX = (clippingRect.Value.Width - rectangle.Width) / 2f; break;

            //    }
            //    //}
            //    //else
            //    //{
            //    //    // Doesn't work if we have scrollbars because the scroll position conflicts with the translateX
            //    //    // i.e. We need to adjust the scrollbars to center / align the image rather then use the translation
            //    //    // except that:
            //    //    //   - We don't know we're inside a control with scrollbars. Can maybe check isWidthVirtual? but that still doesn't give a reference to the scrollbars 
            //    //    //   - if we adjust the scrollbars on every frame, then the user can't move them as we'll keep resetting the position :(
            //    //}

            //    //if (imgOffsetY >= 0)
            //    //{
            //    switch (textureLayout.VerticalAlignment)
            //    {
            //        case VerticalAlignmentType.Top: translateY = 0f; break;
            //        case VerticalAlignmentType.Center: translateY = (clippingRect.Value.Height - rectangle.Height) / 2f; break;
            //        case VerticalAlignmentType.Bottom: translateY = clippingRect.Value.Height - rectangle.Height; break;
            //        default: translateY = (clippingRect.Value.Height - rectangle.Height) / 2f; break;
            //    }
            //    //}
            //    //else
            //    //{
            //    //    // Same issue as HorizontalAlignment above
            //    //}

            //    view = Matrix.CreateTranslation(translateX, translateY, 0);
            //}


            using BasicEffect basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.World = Matrix.Identity;
            basicEffect.View = view;  // Matrix.Identity
            basicEffect.Projection = projection;
            basicEffect.Texture = texture;
            basicEffect.TextureEnabled = true;
            basicEffect.DiffuseColor = textureLayout.Tint.ToVector3();
            basicEffect.Alpha = textureLayout.Tint.A / 255f;

            Context.Renderer.PushState();

            if (clippingRect != null)
            {
                Context.Renderer.ClipToRect(clippingRect.Value);
            }

            float scaleX = rectangle.Width / (float)texture.Width;
            float scaleY = rectangle.Height / (float)texture.Height;

            int backgroundScaleX = (int)(scaleX * texture.Width * scale.X);
            int backgroundScaleY = (int)(scaleY * texture.Height * scale.Y);

            // Draw background image
            using var spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, null, Context.Renderer.rasterizerState, basicEffect);
            spriteBatch.Draw(texture, rectangle, new Rectangle(0, 0, backgroundScaleX, backgroundScaleY), Color.White);
            spriteBatch.End();

            Context.Renderer.PopState();

        }

        public void DrawRoundedRect(SpriteBatch spriteBatch, Rectangle rectangle, float cornerRadius, Color borderColor, float borderThickness)
        {
            Primitives2D.DrawRoundedRect(spriteBatch, rectangle, cornerRadius, borderColor, borderThickness, true);
        }

        public void FillRoundedRect(SpriteBatch spriteBatch, Rectangle rectangle, float cornerRadius, Color color)
        {
            Primitives2D.FillRoundedRect(spriteBatch, rectangle, cornerRadius, color);
        }


        public void DrawString(SpriteBatch spriteBatch, Rectangle rectangle, string text, SpriteFontBase spriteFont, Color? color, HorizontalAlignmentType horizontalAlignment = HorizontalAlignmentType.Left, VerticalAlignmentType verticalAlignment = VerticalAlignmentType.Center, Rectangle? clippingRect = null)
        {
            SpriteFontBase font = spriteFont ?? FontService.GetFontOrDefault(null, null);
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
                    x = (rectangle.Left + rectangle.Right) / 2 - (int)textSize.X / 2;
                    if (x < rectangle.Left)
                    {
                        //x = rectangle.Left;
                        x = rectangle.Right - textSize.X;
                    }
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
                    y = (rectangle.Top + rectangle.Bottom) / 2 - (int)textSize.Y / 2;
                    if (y < rectangle.Top)
                    {
                        y = rectangle.Top;
                    }
                    break;

                case VerticalAlignmentType.Bottom:
                    y = rectangle.Bottom - (int)textSize.Y;
                    break;

                default:
                    y = rectangle.Left;
                    break;
            }

            Vector2 textPosition = new Vector2(x, y);

            if (clippingRect == null)
            {
                ClipToRect(rectangle);
            }
            else
            {
                ClipToRect(Rectangle.Intersect(clippingRect.Value, rectangle));
            }


            //spriteBatch.DrawString(font, text, textPosition, color ?? Color.White);
            spriteBatch.DrawString(font, text, textPosition, color ?? Color.White);
        }


    }
}
