using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI.Controls
{
    public class Image : UIComponentDrawable
    {
        [JsonIgnore]
        [XmlIgnore]
        public Texture2D ImageTexture { get; set; }

        public TextureLayout ImageLayout { get; set; }

        private ImageLayoutResult ImageLayoutResult { get; set; }

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

            desiredWidth = FloatHelper.IsNaN(desiredWidth) ? availableSize.Width : desiredWidth;
            desiredHeight = FloatHelper.IsNaN(desiredHeight) ? availableSize.Height : desiredHeight;

            float maxWidth = MaxWidth.ToPixels(availableSize.Width);
            float maxHeight = MaxHeight.ToPixels(availableSize.Height);
            float minWidth = MinWidth.ToPixels(availableSize.Width);
            float minHeight = MinHeight.ToPixels(availableSize.Height);

            if (!float.IsNaN(minWidth) && desiredWidth < minWidth) desiredWidth = minWidth;
            if (!float.IsNaN(minHeight) && desiredHeight < minHeight) desiredHeight = minHeight;
            if (!float.IsNaN(maxWidth) && desiredWidth > maxWidth) desiredWidth = maxWidth;
            if (!float.IsNaN(maxHeight) && desiredHeight > maxHeight) desiredHeight = maxHeight;

            int layoutWidth = float.IsNaN(desiredWidth) ? ImageTexture?.Width ?? 0 : (int)desiredWidth;
            int layoutHeight = float.IsNaN(desiredHeight) ? ImageTexture?.Height ?? 0 : (int)desiredHeight;

            var layoutRect = new Rectangle(0, 0, layoutWidth, layoutHeight);

            var layoutParams = ImageLayout.GetLayoutRect(ImageTexture, layoutRect);

            if (ImageTexture != null)
            {
                if (FloatHelper.IsNaN(Width))
                {
                    desiredWidth = layoutParams.LayoutRect.Width;
                }

                if (FloatHelper.IsNaN(Height))
                {
                    desiredHeight = layoutParams.LayoutRect.Height;
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


            // We can't Center / Stretch align for some controls, e.g. StackPanel
            // TODO: Need a more generic way to handle this rather then detecting StackPanel
            //     : e.g. Parent.ChildrenCanArangeHorizontally / Vertically.... 
            if (Parent is StackPanel)
            {
                var parentStackPanel = Parent as StackPanel;
                if (parentStackPanel.Orientation == Orientation.Horizontal)
                {
                    // Horizontal Alignment
                    ImageLayout.HorizontalAlignment = HorizontalAlignmentType.Left; //Parent.HorizontalContentAlignment.Value;
                }
                else
                {
                    // Vertical Aligment
                    ImageLayout.VerticalAlignment = VerticalAlignmentType.Top; // Parent.VerticalContentAlignment.Value;
                }
            }


            //ClampDesiredSize(availableSize, parentMinMax);

            //parentMinMax.Merge(MinWidth, MinHeight, MaxWidth, MaxHeight, availableSize);
            //base.Measure(context, ref availableSize, ref parentMinMax);=
        }


        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            //if (string.Equals(Name, "ZYX")) { }
            base.Arrange(context, layoutBounds, parentLayoutBounds);

            //var tmpDesiredSize = DesiredSize;

            ImageLayoutResult = ImageLayout.GetLayoutRect(ImageTexture, FinalContentRect);

            // Override the Measured DesiredSize with the final actual dimensions
            //DesiredSize = new Size(ImageLayoutResult.LayoutRect.Width, ImageLayoutResult.LayoutRect.Height);

            //base.Arrange(context, ImageLayoutResult.LayoutRect, layoutBounds);

            //ImageLayoutResult = ImageLayout.GetLayoutRect(ImageTexture, FinalContentRect);

            //if (ImageTexture != null) { }

            //DesiredSize = tmpDesiredSize;
            //base.Arrange(context, ImageLayoutResult.LayoutRect, parentLayoutBounds);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            if (string.Equals(Name, "ZYX")) { }

            var renderBounds = Rectangle.Intersect(layoutBounds, FinalContentRect);
            //base.RenderControl(context, renderBounds, parentTransform);

            //            FinalRect = layoutBounds;
            base.RenderControl(context, layoutBounds, parentTransform);

            if (ImageTexture != null)
            {
                // ImageLayoutResult = ImageLayout.GetLayoutRect(ImageTexture, FinalContentRect);
                var scale = new Vector2(ImageLayout.TextureScale.X / ImageLayoutResult.Scale.X, ImageLayout.TextureScale.Y / ImageLayoutResult.Scale.Y);

                //context.Renderer.DrawTexture(ImageTexture, ImageLayoutResult.LayoutRect, ImageLayout, scale, FinalContentRect);
                context.Renderer.DrawTexture(ImageTexture, ImageLayoutResult.LayoutRect, ImageLayout, scale, layoutBounds);

                //using var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
                //Primitives2D.DrawRect(spriteBatch, ImageLayoutResult.LayoutRect, Color.HotPink, 3);
            }
        }

        public override void Dispose()
        {
            if (ImageTexture is RenderTarget2D)
            {
                ImageTexture?.Dispose();
            }

            base.Dispose();
        }

    }
}
