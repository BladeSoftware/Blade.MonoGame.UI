using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Components
{
    public record struct ImageLayoutResult
    {
        public Rectangle LayoutRect;
        public Vector2 Scale;

        public static ImageLayoutResult Empty = new ImageLayoutResult { LayoutRect = Rectangle.Empty, Scale = Vector2.Zero };

        public ImageLayoutResult()
        {
        }

        public ImageLayoutResult(Rectangle dstRect, Vector2 scale)
        {
            this.LayoutRect = dstRect;
            this.Scale = scale;
        }

    }
}
