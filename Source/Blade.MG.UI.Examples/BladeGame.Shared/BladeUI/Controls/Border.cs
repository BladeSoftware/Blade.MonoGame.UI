using System;
using System.Collections.Generic;
using System.Text;
using BladeGame.BladeUI.Components;
using BladeGame.Shared.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeGame.BladeUI.Controls
{
    public class Border : Control
    {
        public Binding<Color> BorderColor { get; set; } = new Binding<Color>();
        public Binding<float> BorderThickness { get; set; } = new Binding<float>();
        public Binding<float> CornerRadius { get; set; } = new Binding<float>();

        public Binding<Color> Background { get; set; } = new Binding<Color>();
        public Binding<Texture2D> BackgroundTexture { get; set; } = new Binding<Texture2D>();

        private static AlphaTestEffect alphaTestEffect;
        private static Matrix projectionMatrix;
        private static DepthStencilState stencilState1;
        private static DepthStencilState stencilState2;
        private static BlendState blendState; // Blend State to only write to Depth Buffer and not Back Buffer

        public Border()
        {
            BorderColor.Value = Color.White;
            BorderThickness.Value = 1f;
            CornerRadius.Value = 1f;

            HorizontalAlignment = HorizontalAlignmentType.Center;
            VerticalAlignment = VerticalAlignmentType.Center;
            HorizontalContentAlignment = HorizontalAlignmentType.Center;
            VerticalContentAlignment = VerticalAlignmentType.Center;
        }

        public override void Measure(UIContext context, Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, availableSize, ref parentMinMax);
        }

        public override void Arrange(Rectangle layoutBounds)
        {
            base.Arrange(layoutBounds);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds)
        {
            if (alphaTestEffect == null)
            {
                InitStencilBufferVars(context);
            }


            // Render the border background
            RenderBorderBackground(context);

            // Render the Contents
            base.RenderControl(context, layoutBounds);

            // Render the border
            context.Renderer.BeginBatch(context.SpriteBatch);
            Primitives2D.DrawRoundedRect(context.Game, context.SpriteBatch, finalRect, CornerRadius.Value, BorderColor.Value, BorderThickness.Value);
            context.Renderer.EndBatch(context.SpriteBatch);
        }

        private void InitStencilBufferVars(UIContext context)
        {
            lock (this)
            {
                if (alphaTestEffect == null)
                {
                    projectionMatrix = Matrix.CreateOrthographicOffCenter(0,
                                              context.Game.GraphicsDevice.PresentationParameters.BackBufferWidth,
                                              context.Game.GraphicsDevice.PresentationParameters.BackBufferHeight,
                                              0, 0, 1);

                    alphaTestEffect = new AlphaTestEffect(context.Game.GraphicsDevice)
                    {
                        Projection = projectionMatrix,
                        VertexColorEnabled = true,
                        ReferenceAlpha = 0
                    };


                    stencilState1 = new DepthStencilState
                    {
                        StencilEnable = true,
                        StencilFunction = CompareFunction.Always,
                        StencilPass = StencilOperation.Replace,
                        ReferenceStencil = 1,
                        DepthBufferEnable = false
                    };

                    stencilState2 = new DepthStencilState
                    {
                        StencilEnable = true,
                        StencilFunction = CompareFunction.LessEqual,
                        StencilPass = StencilOperation.Keep,
                        ReferenceStencil = 1,
                        DepthBufferEnable = false
                    };

                    //https://gamedev.stackexchange.com/questions/130970/draw-only-on-stencil-buffer-monogame
                    blendState = new BlendState()
                    {
                        ColorWriteChannels = ColorWriteChannels.None
                    };

                }
            }
        }

        /// <summary>
        /// Use a Depth Stencil to render the border background with rounded corners
        /// </summary>
        /// <param name="context"></param>
        private void RenderBorderBackground(UIContext context)
        {
            projectionMatrix = Matrix.CreateOrthographicOffCenter(0,
                          context.Game.GraphicsDevice.PresentationParameters.BackBufferWidth,
                          context.Game.GraphicsDevice.PresentationParameters.BackBufferHeight,
                          0, 0, 1);

            alphaTestEffect.Projection = projectionMatrix;

            // Draw Stencil
            context.SpriteBatch.Begin(SpriteSortMode.Immediate, depthStencilState: stencilState1, effect: alphaTestEffect, blendState: blendState);
            Primitives2D.FillRoundedRect(context.Game, context.SpriteBatch, finalRect, CornerRadius.Value, Color.White);
            context.SpriteBatch.End();


            // Draw Image to be 'clipped' to stencil
            context.SpriteBatch.Begin(SpriteSortMode.Immediate, depthStencilState: stencilState2);

            if (BackgroundTexture == null || BackgroundTexture.Value == null)
            {
                //context.Renderer.FillRect(finalRect, Background);
                Primitives2D.FillRect(context.Game, context.SpriteBatch, finalRect, Background.Value);
            }
            else
            {
                //context.Renderer.FillRect(finalRect, BackgroundTexture, Background);
                context.SpriteBatch.Draw(BackgroundTexture.Value, finalRect, Background.Value);
            }
            context.SpriteBatch.End();

        }
    }
}
