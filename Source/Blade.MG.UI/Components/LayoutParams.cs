using Microsoft.Xna.Framework;

namespace Blade.UI.Components
{
    public struct Layout
    {
        public float MinWidth;
        public float MinHeight;
        public float MaxWidth;
        public float MaxHeight;

        public Layout(float minWidth, float minHeight, float maxWidth, float maxHeight)
        {
            MinWidth = minWidth;
            MinHeight = minHeight;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
        }

        public Layout(Length minWidth, Length minHeight, Length maxWidth, Length maxHeight, float parentWidth, float parentHeight)
        {
            MinWidth = minWidth.ToPixels(parentWidth);
            MinHeight = minHeight.ToPixels(parentHeight);
            MaxWidth = maxWidth.ToPixels(parentWidth);
            MaxHeight = maxHeight.ToPixels(parentHeight);
        }

        public Layout(Length minWidth, Length minHeight, Length maxWidth, Length maxHeight, Size parentSize)
        {
            MinWidth = minWidth.ToPixels(parentSize.Width);
            MinHeight = minHeight.ToPixels(parentSize.Height);
            MaxWidth = maxWidth.ToPixels(parentSize.Width);
            MaxHeight = maxHeight.ToPixels(parentSize.Height);
        }

        public Layout(Length minWidth, Length minHeight, Length maxWidth, Length maxHeight, Rectangle parentRect)
        {
            MinWidth = minWidth.ToPixels(parentRect.Width);
            MinHeight = minHeight.ToPixels(parentRect.Height);
            MaxWidth = maxWidth.ToPixels(parentRect.Width);
            MaxHeight = maxHeight.ToPixels(parentRect.Height);
        }


        public void Merge(float minWidth, float minHeight, float maxWidth, float maxHeight)
        {
            MinWidth = !float.IsNaN(minWidth) ? minWidth : MinWidth;
            MinHeight = !float.IsNaN(minHeight) ? minHeight : MinHeight;
            MaxWidth = !float.IsNaN(maxWidth) ? maxWidth : MaxWidth;
            MaxHeight = !float.IsNaN(maxHeight) ? maxHeight : MaxHeight;
        }
        public void Merge(Length minWidth, Length minHeight, Length maxWidth, Length maxHeight, Size parentSize, float parentWidth, float parentHeight)
        {
            Merge(minWidth.ToPixels(parentWidth),
                  minHeight.ToPixels(parentHeight),
                  maxWidth.ToPixels(parentWidth),
                  maxHeight.ToPixels(parentHeight));
        }

        public void Merge(Length minWidth, Length minHeight, Length maxWidth, Length maxHeight, Size parentSize)
        {
            Merge(minWidth.ToPixels(parentSize.Width),
                  minHeight.ToPixels(parentSize.Height),
                  maxWidth.ToPixels(parentSize.Width),
                  maxHeight.ToPixels(parentSize.Height));
        }

        public void Merge(Length minWidth, Length minHeight, Length maxWidth, Length maxHeight, Rectangle parentRect)
        {
            Merge(minWidth.ToPixels(parentRect.Width),
                  minHeight.ToPixels(parentRect.Height),
                  maxWidth.ToPixels(parentRect.Width),
                  maxHeight.ToPixels(parentRect.Height));
        }

    }
}
