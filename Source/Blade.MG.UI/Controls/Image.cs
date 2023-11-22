using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blade.MG.UI.Controls
{
    public class Image : UIComponentDrawable
    {
        public TextureLayout ImageTexture { get; set; }

        public Image()
        {
            Background = Color.Transparent;

            //HorizontalAlignment = HorizontalAlignmentType.Left;
            //VerticalAlignment = VerticalAlignmentType.Top;
            HorizontalAlignment = HorizontalAlignmentType.Center;
            VerticalAlignment = VerticalAlignmentType.Center;
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            parentMinMax.Merge(MinWidth, MinHeight, MaxWidth, MaxHeight, availableSize);

            float desiredWidth = Width.ToPixels(availableSize.Width);
            float desiredHeight = Height.ToPixels(availableSize.Height);

            desiredWidth = FloatHelper.IsNaN(desiredWidth) ? (int)availableSize.Width : (int)desiredWidth;
            desiredHeight = FloatHelper.IsNaN(desiredHeight) ? (int)availableSize.Height : (int)desiredHeight;

            float maxWidth = MaxWidth.ToPixels(availableSize.Width);
            float maxHeight = MaxHeight.ToPixels(availableSize.Height);
            float minWidth = MinWidth.ToPixels(availableSize.Width);
            float minHeight = MinHeight.ToPixels(availableSize.Height);

            if (!float.IsNaN(minWidth) && desiredWidth < minWidth) desiredWidth = minWidth;
            if (!float.IsNaN(minHeight) && desiredHeight < minHeight) desiredHeight = minHeight;
            if (!float.IsNaN(maxWidth) && desiredWidth > maxWidth) desiredWidth = maxWidth;
            if (!float.IsNaN(maxHeight) && desiredHeight > maxHeight) desiredHeight = maxHeight;


            var layoutParams = ImageTexture.GetLayoutRect(new Rectangle(0, 0, (int)desiredWidth, (int)desiredHeight));
            //var scale = new Vector2(layoutParams.scale.X / ImageTexture.TextureScale.X, layoutParams.scale.Y / ImageTexture.TextureScale.Y);

            if (ImageTexture?.Texture != null)
            {
                if (FloatHelper.IsNaN(Width))
                {
                    //desiredWidth = ImageTexture.Texture.Width * layoutParams.scale.X;
                    desiredWidth = layoutParams.dstRect.Width;
                }

                if (FloatHelper.IsNaN(Height))
                {
                    //desiredHeight = ImageTexture.Texture.Height * layoutParams.scale.Y;
                    desiredHeight = layoutParams.dstRect.Height;
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

            //base.Measure(context, ref availableSize, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);

            //Rectangle imageRect = FinalContentRect;

            //if (ImageTexture.Texture == null)
            //{
            //    return;
            //}

            ////float aspect = ImageTexture.Width / (float)ImageTexture.Height;
            //float scaleX = ImageTexture.Texture.Width / (float)layoutBounds.Width; //(float)finalContentRect.Width;
            //float scaleY = ImageTexture.Texture.Height / (float)layoutBounds.Height; //(float)finalContentRect.Height;

            //switch (ImageScaling)
            //{
            //    case StretchType.None:
            //        imageRect = new Rectangle(FinalContentRect.Left, FinalContentRect.Top, ImageTexture.Texture.Width, ImageTexture.Texture.Height);
            //        break;

            //    case StretchType.Fill:
            //        imageRect = FinalRect; //finalContentRect;
            //        break;

            //    case StretchType.Uniform:
            //        float maxFactor = scaleX > scaleY ? scaleX : scaleY;

            //        if (maxFactor < 1f)
            //        {
            //            maxFactor = 1f / maxFactor;
            //        }

            //        imageRect = FinalContentRect with { Width = (int)(ImageTexture.Texture.Width * maxFactor), Height = (int)(ImageTexture.Texture.Height * maxFactor) };


            //        //imageRect = new Rectangle(finalContentRect.Left, finalContentRect.Top, ImageTexture.Width, ImageTexture.Height); 
            //        break;

            //    case StretchType.UniformToFill:
            //        float minFactor = scaleX < scaleY ? scaleX : scaleY;

            //        if (minFactor < 1f)
            //        {
            //            minFactor = 1f / minFactor;
            //        }

            //        imageRect = FinalContentRect with { Width = (int)(ImageTexture.Texture.Width * minFactor), Height = (int)(ImageTexture.Texture.Height * minFactor) };
            //        break;

            //}


            //if (imageRect.Width != FinalContentRect.Width)
            //{
            //    //switch (HorizontalAlignment.Value)
            //    //{
            //    //    case HorizontalAlignmentType.Left: Left = finalContentRect.Left; break;
            //    //    case HorizontalAlignmentType.Right: Left = finalContentRect.Left + finalContentRect.Width - imageRect.Width; break;
            //    //    case HorizontalAlignmentType.Center: Left = finalContentRect.Left + (finalContentRect.Width - imageRect.Width) / 2; break;
            //    //}

            //    switch (HorizontalAlignment.Value)
            //    {
            //        //case HorizontalAlignmentType.Left: break;
            //        case HorizontalAlignmentType.Right: FinalContentRect.X = FinalContentRect.Left + FinalContentRect.Width - imageRect.Width; break;
            //        case HorizontalAlignmentType.Center: FinalContentRect.X = FinalContentRect.Left + (FinalContentRect.Width - imageRect.Width) / 2; break;
            //    }

            //    Left = FinalContentRect.Left;

            //    FinalContentRect.Width = imageRect.Width;
            //}

            //if (imageRect.Height != FinalContentRect.Height)
            //{
            //    //switch (VerticalAlignment.Value)
            //    //{
            //    //    case VerticalAlignmentType.Top: Top = finalContentRect.Top; break;
            //    //    case VerticalAlignmentType.Bottom: Top = finalContentRect.Top + finalContentRect.Height - imageRect.Height; break;
            //    //    case VerticalAlignmentType.Center: Top = finalContentRect.Top + (finalContentRect.Height - imageRect.Height) / 2; break;
            //    //}

            //    switch (VerticalAlignment.Value)
            //    {
            //        //case VerticalAlignmentType.Top: break;
            //        case VerticalAlignmentType.Bottom: FinalContentRect.Y = FinalContentRect.Top + FinalContentRect.Height - imageRect.Height; break;
            //        case VerticalAlignmentType.Center: FinalContentRect.Y = FinalContentRect.Top + (FinalContentRect.Height - imageRect.Height) / 2; break;
            //    }

            //    Top = FinalContentRect.Top;

            //    FinalContentRect.Height = imageRect.Height;
            //}

            ////finalContentRect = Rectangle.Intersect(finalContentRect, imageRect);

        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            var renderBounds = Rectangle.Intersect(layoutBounds, FinalContentRect);
            base.RenderControl(context, renderBounds, parentTransform);


            if (ImageTexture?.Texture != null)
            {
                var layoutParams = ImageTexture.GetLayoutRect(FinalContentRect);
                //var scale = new Vector2(layoutParams.scale.X / ImageTexture.TextureScale.X, layoutParams.scale.Y / ImageTexture.TextureScale.Y);
                var scale = new Vector2(ImageTexture.TextureScale.X / layoutParams.scale.X, ImageTexture.TextureScale.Y / layoutParams.scale.Y);

                context.Renderer.DrawTexture(layoutParams.dstRect, ImageTexture, scale, renderBounds);
            }
            //else if (Color != Color.Transparent)
            //{
            //    try
            //    {
            //        context.Renderer.BeginBatch(transform: parentTransform);
            //        context.Renderer.FillRect(FinalContentRect, Color, layoutBounds);
            //    }
            //    finally
            //    {
            //        context.Renderer.EndBatch();
            //    }
            //}


        }

        public override void Dispose()
        {
            if (ImageTexture?.Texture is RenderTarget2D)
            {
                ImageTexture?.Texture?.Dispose();
            }

            base.Dispose();
        }

    }
}
