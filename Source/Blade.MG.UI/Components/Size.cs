using Microsoft.Xna.Framework;

namespace Blade.UI.Components
{
    public struct Size
    {
        public float Width;
        public float Height;

        public Size(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public Size(Rectangle rectangle)
        {
            Width = rectangle.Width;
            Height = rectangle.Height;
        }

        public override string ToString()
        {
            return "W: " + Width + ", H:" + Height;
        }
    }
}
