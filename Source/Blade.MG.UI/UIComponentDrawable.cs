using Blade.UI.Components;
using Blade.UI.Theming;
using Microsoft.Xna.Framework;

namespace Blade.UI
{
    public abstract class UIComponentDrawable : UIComponentEvents
    {
        public UITheme Theme => ParentWindow?.Context?.Theme ?? (this as UIWindow)?.Context?.Theme ?? UIManager.DefaultTheme;

        public Binding<Color> Background { get; set; } = Color.Transparent;

        //[field: NonSerialized]
        //public Texture2D BackgroundTexture { get; set; }

        public TextureLayout BackgroundTexture { get; set; }


        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            base.RenderControl(context, layoutBounds, parentTransform);

            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            if (Background.Value != Color.Transparent)
            {
                try
                {
                    context.Renderer.BeginBatch(transform: parentTransform);
                    //context.Renderer.FillRect(FinalContentRect, Background.Value, layoutBounds);
                    context.Renderer.FillRect(FinalRect, Background.Value, layoutBounds);
                }
                finally
                {
                    context.Renderer.EndBatch();
                }
            }

            if (BackgroundTexture?.Texture != null)
            {
                var layoutParams = BackgroundTexture.GetLayoutRect(FinalContentRect);
                var scale = new Vector2(layoutParams.scale.X / BackgroundTexture.TextureScale.X, layoutParams.scale.Y / BackgroundTexture.TextureScale.Y);
                context.Renderer.DrawTexture(layoutParams.dstRect, BackgroundTexture, scale, FinalContentRect);
            }
        }

        //// TODO: Move this to a helper class
        //public void GetImageStrech(Rectangle layoutBounds, TextureLayout textureLayout)
        //{
        //    Rectangle srcImageRect = textureLayout.Texture.Bounds;

        //    //float aspect = srcImageRect.Width / (float)srcImageRect.Height;
        //    float scaleX = srcImageRect.Width / (float)layoutBounds.Width; //(float)finalContentRect.Width;
        //    float scaleY = srcImageRect.Height / (float)layoutBounds.Height; //(float)finalContentRect.Height;

        //    Rectangle dstImageRect = textureLayout.Texture.Bounds;

        //    switch (textureLayout.StretchType)
        //    {
        //        case StretchType.None:
        //            dstImageRect = new Rectangle(layoutBounds.Left, layoutBounds.Top, BackgroundTexture.Texture.Width, BackgroundTexture.Texture.Height);
        //            break;

        //        case StretchType.Fill:
        //            dstImageRect = FinalRect; //finalContentRect;
        //            break;

        //        case StretchType.Uniform:
        //            float maxFactor = scaleX > scaleY ? scaleX : scaleY;

        //            if (maxFactor < 1f)
        //            {
        //                maxFactor = 1f / maxFactor;
        //            }

        //            dstImageRect = layoutBounds with { Width = (int)(BackgroundTexture.Texture.Width * maxFactor), Height = (int)(BackgroundTexture.Texture.Height * maxFactor) };


        //            //imageRect = new Rectangle(layoutBounds.Left, layoutBounds.Top, BackgroundTexture.Width, BackgroundTexture.Height); 
        //            break;

        //        case StretchType.UniformToFill:
        //            float minFactor = scaleX < scaleY ? scaleX : scaleY;

        //            if (minFactor < 1f)
        //            {
        //                minFactor = 1f / minFactor;
        //            }

        //            dstImageRect = layoutBounds with { Width = (int)(BackgroundTexture.Texture.Width * minFactor), Height = (int)(BackgroundTexture.Texture.Height * minFactor) };
        //            break;

        //    }
        //}


    }
}


