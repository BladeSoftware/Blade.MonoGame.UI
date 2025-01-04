using Blade.MG.Primitives;
using Blade.MG.UI.Components;
using Blade.MG.UI.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Blade.MG.UI.Controls
{
    public class Border : Control
    {
        public Binding<Color> BorderColor { get; set; } = new Color();
        public Binding<float> BorderThickness { get; set; } = 0f;
        public Binding<float> CornerRadius { get; set; } = 0f;

        public Binding<int> Elevation { get; set; } = 0;


        public Border()
        {
            BorderColor.Value = Color.White;
            BorderThickness.Value = 1f;
            CornerRadius.Value = 1f;
            Elevation.Value = 0;

            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;

            IsHitTestVisible = false;
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            // Draw the shadow if Elevation is > 0
            if (Elevation.Value > 0)
            {

                try
                {
                    using var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
                    context.Renderer.ClipToRect(layoutBounds);

                    //Rectangle shadowRect = finalRect with { X = finalRect.X - Elevation.Value, Y = finalRect.Y - Elevation.Value, Width = finalRect.Width + 3 * Elevation.Value, Height = finalRect.Height + 3 * Elevation.Value };
                    //Rectangle shadowRect = finalRect with { X = finalRect.X + Elevation.Value, Y = finalRect.Y + Elevation.Value, Width = finalRect.Width + Elevation.Value, Height = finalRect.Height + Elevation.Value };
                    Rectangle shadowRect = FinalRect with { X = FinalRect.X + Elevation.Value, Y = FinalRect.Y + Elevation.Value, Width = FinalRect.Width, Height = FinalRect.Height };
                    context.Renderer.FillRoundedRect(spriteBatch, shadowRect, 8, new Color(Color.LightGray, 0.35f));

                    //context.Renderer.DrawRoundedRect(finalRect, CornerRadius.Value, BorderColor.Value, BorderThickness.Value);
                    //context.Renderer.FillRect(shadowRect, new Color(Color.LightBlue, 0.8f));
                    //context.Renderer.FillRect(shadowRect, BackgroundTexture, new Color(Color.LightBlue, 0.5f));

                }
                finally
                {
                    context.Renderer.EndBatch();
                }
            }


            // If we have rounded corners, then we need to draw the stencil mask
            if (CornerRadius.Value > 0)
            {
                // Render the border background
                DrawStencil(context, FinalRect);
            }


            // Render the Contents
            base.RenderControl(context, layoutBounds, parentTransform);


            // If we have rounded corners, then we need to restore the stencil mask
            if (CornerRadius.Value > 0)
            {
                //context.Renderer.BeginBatch(depthStencilState: Renderer.UIRenderer.stencilStateReplaceAlways, effect: alphaTestEffect, blendState: Renderer.UIRenderer.blendStateStencilOnly);
                using var spriteBatch = context.Renderer.BeginBatch(depthStencilState: UIRenderer.stencilStateReplaceAlways, blendState: UIRenderer.blendStateStencilOnly, transform: null);
                context.Renderer.FillRect(spriteBatch, FinalRect, Color.White);
                context.Renderer.EndBatch();
            }


            // Render the border
            if (BorderThickness.Value > 0 && BorderColor.Value != Color.Transparent)
            {
                try
                {
                    using var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
                    //context.Renderer.ClipToRect(layoutBounds with { Width = layoutBounds.Width - 1, Height = layoutBounds.Height - 1 });
                    context.Renderer.ClipToRect(layoutBounds);

                    context.Renderer.DrawRoundedRect(spriteBatch, FinalRect, CornerRadius.Value, BorderColor.Value, BorderThickness.Value);
                }
                finally
                {
                    context.Renderer.EndBatch();
                }
            }

        }


        /// <summary>
        /// Use a Depth Stencil to render the border background with rounded corners
        /// </summary>
        /// <param name="context"></param>
        private void DrawStencil(UIContext context, Rectangle rectangle)
        {
            //projectionMatrix = Matrix.CreateOrthographicOffCenter(0,
            //              context.Game.GraphicsDevice.PresentationParameters.BackBufferWidth,
            //              context.Game.GraphicsDevice.PresentationParameters.BackBufferHeight,
            //              0, 0, 1);

            //alphaTestEffect.Projection = projectionMatrix;


            // Draw Stencil
            using var spriteBatch = context.Renderer.BeginBatch(depthStencilState: UIRenderer.stencilStateZeroAlways, blendState: UIRenderer.blendStateStencilOnly, transform: null);
            //using var spriteBatch = context.Renderer.BeginBatch(null);

            //Primitives2D.FillRoundedRectCornersInverted(spriteBatch, rectangle, CornerRadius.Value, Color.White);
            MaskRoundedRectCornersInverted(spriteBatch, rectangle, CornerRadius.Value);

            //context.SpriteBatch.End();
            context.Renderer.EndBatch();




            //// Draw background to be 'clipped' to stencil
            //try
            //{
            //    context.Renderer.BeginBatch(depthStencilState: stencilState2);
            //    //context.SpriteBatch.Begin(SpriteSortMode.Immediate, depthStencilState: stencilState2);

            //    if (BackgroundTexture == null || BackgroundTexture.Value == null)
            //    {
            //        context.Renderer.FillRect(finalRect, Background.Value);
            //        //Primitives2D.FillRect(context.Game, context.SpriteBatch, finalRect, Background.Value);
            //    }
            //    else
            //    {
            //        context.Renderer.FillRect(finalRect, BackgroundTexture.Value, Background.Value);
            //        //context.SpriteBatch.Draw(BackgroundTexture.Value, finalRect, Background.Value);
            //    }
            //}
            //finally
            //{
            //    context.Renderer.EndBatch();
            //    //context.SpriteBatch.End();
            //}

            //// If we have rounded corners, then we need to restore the stencil mask
            //if (CornerRadius.Value > 0)
            //{
            //    context.Renderer.BeginBatch();
            //    context.Renderer.FillRect(finalRect, Color.White);
            //    context.Renderer.EndBatch();
            //}

        }


        public static void MaskRoundedRectCornersInverted(SpriteBatch spriteBatch, Rectangle rectangle, float radius)
        {
            MaskRoundedRectCornersInverted(spriteBatch, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom, radius);
        }

        public static void MaskRoundedRectCornersInverted(SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, float radius)
        {
            Color color = Color.White;

            int yPixelOffset = 0;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                // Cater for rendering difference on Android : PlatformID.Unix
                bool halfPixelOffset = spriteBatch.GraphicsDevice.UseHalfPixelOffset;
                yPixelOffset = halfPixelOffset ? 0 : 1;
            }


            float width = x2 - x1;
            float height = y2 - y1;

            if (width == 0 || height == 0) return;
            if (width < 0)
            {
                float t1 = x1;
                x1 = x2;
                x2 = t1;
                //width = -width;
            }

            if (radius < 0)
            {
                radius = 0;
            }

            float lineWidth = 0f;

            float cx1 = x1 + radius;
            float cy1 = y1 + radius - 1;
            float cx2 = x2 - radius;
            float cy2 = y2 - radius;

            Vector2 cTL = new Vector2(cx1 + 0.5f, cy1 + 0.5f);

            bool crossover = false;
            Vector2 p3 = Vector2.Zero;

            float oy = float.NegativeInfinity;

            float x = radius;
            for (int a = 0; a < radius; a++)
            {
                Vector2 p2 = new Vector2(cx1 - x - 0.5f, cy1 - a);
                if (!crossover)
                {
                    p3 = new Vector2(cx1 - a, cy1 - x - 0.5f);
                }

                if (!crossover && p2.X >= p3.X - lineWidth)
                {
                    crossover = true;
                }

                if (p2.X > p3.X)
                {
                    break;
                }


                float dist3 = Vector2.Distance(p2, cTL) - radius;

                while (dist3 > 1f)
                {
                    p2 = p2 with { X = p2.X + 1f };
                    p3 = p3 with { Y = p3.Y + 1f };

                    dist3 = Vector2.Distance(p2, cTL) - radius;
                    x -= 1;
                }

                // First pixel in row is anti-aliased
                //float alias = 1f - dist3;
                var pixelColor = Color.White; //color with { A = (byte)((float)color.A * alias) };

                var p2TL = p2 with { Y = p2.Y + 0 };
                var p2TR = p2 with { X = -p2.X + cx1 + cx2 - 1 };
                var p2BL = p2 with { Y = -p2.Y + cy1 + cy2 - yPixelOffset };
                var p2BR = p2 with { X = -p2.X + cx1 + cx2 - 1, Y = -p2.Y + cy1 + cy2 - yPixelOffset };

                float len = p2TL.X - x1 + 1;
                spriteBatch.Draw(Primitives2D.PixelTexture(spriteBatch.GraphicsDevice), p2TL with { X = x1 }, null, color, 0f, new Vector2(0, 0), new Vector2(len, 1f), SpriteEffects.None, 0f);
                spriteBatch.Draw(Primitives2D.PixelTexture(spriteBatch.GraphicsDevice), p2TR with { X = x2 - len }, null, color, 0f, new Vector2(0, 0), new Vector2(len, 1f), SpriteEffects.None, 0f);
                spriteBatch.Draw(Primitives2D.PixelTexture(spriteBatch.GraphicsDevice), p2BL with { X = x1 }, null, color, 0f, new Vector2(0, 0), new Vector2(len, 1f), SpriteEffects.None, 0f);
                spriteBatch.Draw(Primitives2D.PixelTexture(spriteBatch.GraphicsDevice), p2BR with { X = x2 - len }, null, color, 0f, new Vector2(0, 0), new Vector2(len, 1f), SpriteEffects.None, 0f);

                if (p2.Y > p3.Y && oy != p3.Y)
                {
                    oy = p3.Y;

                    var p3TL = p3 with { Y = p3.Y + 1 - yPixelOffset };
                    var p3TR = p3 with { X = -p3.X + cx1 + cx2 - 2, Y = p3.Y + 1 - yPixelOffset };
                    var p3BL = p3 with { Y = -p3.Y + cy1 + cy2 - yPixelOffset };
                    var p3BR = p3 with { X = -p3.X + cx1 + cx2 - 2, Y = -p3.Y + cy1 + cy2 - yPixelOffset };

                    len = p3TL.X - x1 + 1;

                    spriteBatch.Draw(Primitives2D.PixelTexture(spriteBatch.GraphicsDevice), p3TL with { X = x1 }, null, color, 0f, new Vector2(0, 0), new Vector2(len, 1f), SpriteEffects.None, 0f);
                    spriteBatch.Draw(Primitives2D.PixelTexture(spriteBatch.GraphicsDevice), p3TR with { X = x2 - len }, null, color, 0f, new Vector2(0, 0), new Vector2(len, 1f), SpriteEffects.None, 0f);
                    spriteBatch.Draw(Primitives2D.PixelTexture(spriteBatch.GraphicsDevice), p3BL with { X = x1 }, null, color, 0f, new Vector2(0, 0), new Vector2(len, 1f), SpriteEffects.None, 0f);
                    spriteBatch.Draw(Primitives2D.PixelTexture(spriteBatch.GraphicsDevice), p3BR with { X = x2 - len }, null, color, 0f, new Vector2(0, 0), new Vector2(len, 1f), SpriteEffects.None, 0f);

                }


            }

        }


        //private void InitStencilBufferVars(UIContext context)
        //{
        //    lock (this)
        //    {
        //        if (alphaTestEffect == null)
        //        {
        //            projectionMatrix = Matrix.CreateOrthographicOffCenter(0,
        //                                      context.Game.GraphicsDevice.PresentationParameters.BackBufferWidth,
        //                                      context.Game.GraphicsDevice.PresentationParameters.BackBufferHeight,
        //                                      0, 0, 1);

        //            alphaTestEffect = new AlphaTestEffect(context.Game.GraphicsDevice)
        //            {
        //                Projection = projectionMatrix,
        //                VertexColorEnabled = true,
        //                ReferenceAlpha = 0
        //            };


        //            //stencilState1 = new DepthStencilState
        //            //{
        //            //    StencilEnable = true,
        //            //    StencilFunction = CompareFunction.Always,
        //            //    StencilPass = StencilOperation.Replace,
        //            //    ReferenceStencil = 1,
        //            //    DepthBufferEnable = false
        //            //};

        //            //stencilState2 = new DepthStencilState
        //            //{
        //            //    StencilEnable = true,
        //            //    StencilFunction = CompareFunction.LessEqual,
        //            //    StencilPass = StencilOperation.Keep,
        //            //    ReferenceStencil = 1,
        //            //    DepthBufferEnable = false
        //            //};

        //            //stencilState3 = new DepthStencilState
        //            //{
        //            //    StencilEnable = true,
        //            //    StencilFunction = CompareFunction.Always,
        //            //    StencilPass = StencilOperation.Zero,
        //            //    ReferenceStencil = 1,
        //            //    DepthBufferEnable = false
        //            //};


        //            ////https://gamedev.stackexchange.com/questions/130970/draw-only-on-stencil-buffer-monogame
        //            //blendState = new BlendState()
        //            //{
        //            //    ColorWriteChannels = ColorWriteChannels.None
        //            //};

        //        }
        //    }
        //}
    }
}
