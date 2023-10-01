using Blade.MG.Primitives;
using Blade.MG.UI.Components;
using Blade.MG.UI.Renderer;
using Microsoft.Xna.Framework;


namespace Blade.MG.UI.Controls
{
    public class Border : Control
    {
        public Binding<Color> BorderColor { get; set; } = new Color();
        public Binding<float> BorderThickness { get; set; } = 0f;
        public Binding<float> CornerRadius { get; set; } = 0f;

        public Binding<int> Elevation { get; set; } = 0;


        //public Binding<Color> Background { get; set; } = new Color();
        //public Binding<Texture2D> BackgroundTexture { get; set; } = new Binding<Texture2D>();

        //private static AlphaTestEffect alphaTestEffect;
        //private static Matrix projectionMatrix;

        //private static BlendState blendState; // Blend State to only write to Depth Buffer and not Back Buffer

        public Border()
        {
            BorderColor.Value = Color.White;
            BorderThickness.Value = 1f;
            CornerRadius.Value = 1f;
            Elevation.Value = 0;

            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;
            HorizontalContentAlignment = HorizontalAlignmentType.Center;
            VerticalContentAlignment = VerticalAlignmentType.Center;

            HitTestVisible = false;

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
            //if (alphaTestEffect == null)
            //{
            //    InitStencilBufferVars(context);
            //}


            // Draw the shadow if Elevation is > 0
            if (Elevation.Value > 0)
            {

                try
                {
                    context.Renderer.BeginBatch(transform: parentTransform);
                    context.Renderer.ClipToRect(layoutBounds);

                    //Rectangle shadowRect = finalRect with { X = finalRect.X - Elevation.Value, Y = finalRect.Y - Elevation.Value, Width = finalRect.Width + 3 * Elevation.Value, Height = finalRect.Height + 3 * Elevation.Value };
                    //Rectangle shadowRect = finalRect with { X = finalRect.X + Elevation.Value, Y = finalRect.Y + Elevation.Value, Width = finalRect.Width + Elevation.Value, Height = finalRect.Height + Elevation.Value };
                    Rectangle shadowRect = FinalRect with { X = FinalRect.X + Elevation.Value, Y = FinalRect.Y + Elevation.Value, Width = FinalRect.Width, Height = FinalRect.Height };
                    context.Renderer.FillRoundedRect(shadowRect, 8, new Color(Color.LightGray, 0.35f));

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
            if (BorderThickness.Value > 0 && CornerRadius.Value > 0)
            {
                // Render the border background
                DrawStencil(context, FinalRect);
            }


            //// Draw background
            //try
            //{
            //    context.Renderer.BeginBatch(transform: parentTransform);
            //    context.Renderer.ClipToRect(layoutBounds);

            //    if (BackgroundTexture == null || BackgroundTexture == null)
            //    {
            //        context.Renderer.FillRect(finalRect, Background.Value);
            //    }
            //    else
            //    {
            //        context.Renderer.FillRect(finalRect, BackgroundTexture, Background.Value);
            //    }
            //}
            //finally
            //{
            //    context.Renderer.EndBatch();
            //}



            // Render the Contents
            base.RenderControl(context, layoutBounds, parentTransform);


            // If we have rounded corners, then we need to restore the stencil mask
            if (BorderThickness.Value > 0 && CornerRadius.Value > 0)
            {
                //alphaTestEffect.Projection = projectionMatrix;

                //context.Renderer.BeginBatch(depthStencilState: Renderer.UIRenderer.stencilStateReplaceAlways, effect: alphaTestEffect, blendState: Renderer.UIRenderer.blendStateStencilOnly);
                context.Renderer.BeginBatch(depthStencilState: UIRenderer.stencilStateReplaceAlways, blendState: UIRenderer.blendStateStencilOnly, transform: null);
                context.Renderer.FillRect(FinalRect, Color.White);
                context.Renderer.EndBatch();
            }


            // Render the border
            if (BorderThickness.Value > 0 && BorderColor.Value != Color.Transparent)
            {
                try
                {
                    context.Renderer.BeginBatch(transform: parentTransform);
                    //context.Renderer.ClipToRect(layoutBounds with { Width = layoutBounds.Width - 1, Height = layoutBounds.Height - 1 });
                    context.Renderer.ClipToRect(layoutBounds);

                    context.Renderer.DrawRoundedRect(FinalRect, CornerRadius.Value, BorderColor.Value, BorderThickness.Value);
                }
                finally
                {
                    context.Renderer.EndBatch();
                }
            }


            ////-- Testing---
            //try
            //{
            //    BasicEffect basicEffect = new BasicEffect(context.Game.GraphicsDevice);

            //    basicEffect.Projection = Matrix.CreateOrthographicOffCenter(context.Game.Viewport.TitleSafeArea, 0f, 10f);
            //    //basicEffect.Projection = Matrix.CreateOrthographicOffCenter(ViewportBounds, 0f, 10f);

            //    //context.Renderer.BeginBatch(transform: parentTransform, blendState: BlendState.AlphaBlend, effect: basicEffect);
            //    context.Renderer.BeginBatch(transform: parentTransform, blendState: BlendState.NonPremultiplied);
            //    //context.Renderer.BeginBatch(transform: parentTransform);

            //    context.Renderer.ClipToRect(layoutBounds);

            //    //Rectangle shadowRect = finalRect with { X = finalRect.X - Elevation.Value, Y = finalRect.Y - Elevation.Value, Width = finalRect.Width + 3 * Elevation.Value, Height = finalRect.Height + 3 * Elevation.Value };
            //    Rectangle shadowRect = finalRect with { X = finalRect.X + Elevation.Value, Y = finalRect.Y + Elevation.Value, Width = finalRect.Width + Elevation.Value, Height = finalRect.Height + Elevation.Value };
            //    context.Renderer.FillRoundedRect(shadowRect, 8, new Color(Color.LightBlue, 0.25f));

            //    //context.Renderer.DrawRoundedRect(finalRect, CornerRadius.Value, BorderColor.Value, BorderThickness.Value);
            //    //context.Renderer.FillRect(shadowRect, new Color(Color.LightBlue, 0.8f));
            //    //context.Renderer.FillRect(shadowRect, BackgroundTexture, new Color(Color.LightBlue, 0.5f));

            //}
            //finally
            //{
            //    context.Renderer.EndBatch();
            //}


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
            context.Renderer.BeginBatch(depthStencilState: UIRenderer.stencilStateZeroAlways, blendState: UIRenderer.blendStateStencilOnly, transform: null);
            //context.Renderer.BeginBatch(depthStencilState: Renderer.UIRenderer.stencilStateZeroAlways, effect: alphaTestEffect, blendState: Renderer.UIRenderer.blendStateStencilOnly);
            //context.SpriteBatch.Begin(SpriteSortMode.Immediate, depthStencilState: stencilState1, effect: alphaTestEffect, blendState: blendState);
            //Primitives2D.FillRoundedRect(context.Game, context.SpriteBatch, finalRect, CornerRadius.Value, Color.White);

            //Primitives2D.FillRoundedRectCornersInverted(context.Game, context.SpriteBatch, finalRect, CornerRadius.Value, Color.White);
            Primitives2D.FillRoundedRectCornersInverted(context.SpriteBatch, rectangle, CornerRadius.Value, Color.White);

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
