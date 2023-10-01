using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blade.MG.UI.Components
{
    public class TextureLayout
    {
        [field: NonSerialized]
        public Texture2D Texture { get; set; }

        public StretchType StretchType { get; set; } = StretchType.None;
        //public StretchDirection StretchDirection { get; set; } = StretchDirection.Both;

        public Vector2 TextureScale { get; set; } = Vector2.One;

        /// <summary>Only used when StretchType = None / Tile / TileHorizontal / TileVertical</summary>
        public Vector2 ImageScale { get; set; } = Vector2.One;

        public HorizontalAlignmentType HorizontalAlignment { get; set; } = HorizontalAlignmentType.Center;
        public VerticalAlignmentType VerticalAlignment { get; set; } = VerticalAlignmentType.Center;

        public Color Tint { get; set; } = Color.White;


        public TextureLayout()
        {

        }

        public (Rectangle dstRect, Vector2 scale) GetLayoutRect(Rectangle layoutBounds)
        {
            if (Texture == null)
            {
                return (Rectangle.Empty, Vector2.Zero);
            }

            Rectangle srcImageRect = Texture.Bounds;

            //float aspect = srcImageRect.Width / (float)srcImageRect.Height;
            float layoutScaleX = srcImageRect.Width / (float)layoutBounds.Width;
            float layoutScaleY = srcImageRect.Height / (float)layoutBounds.Height;

            Rectangle dstImageRect = layoutBounds;
            Vector2 scale = Vector2.One;

            switch (StretchType)
            {
                case StretchType.None:
                    dstImageRect = new Rectangle(layoutBounds.Left, layoutBounds.Top, Texture.Width, Texture.Height);
                    scale = new Vector2(1f / ImageScale.X, 1f / ImageScale.Y);
                    break;

                case StretchType.Fill:
                    dstImageRect = layoutBounds;
                    scale = new Vector2(layoutScaleX, layoutScaleY);
                    break;

                case StretchType.Uniform:
                    float maxFactor = layoutScaleX > layoutScaleY ? layoutScaleX : layoutScaleY;

                    scale = new Vector2(maxFactor, maxFactor);

                    maxFactor = 1f / maxFactor;

                    dstImageRect = layoutBounds with { Width = (int)(Texture.Width * maxFactor), Height = (int)(Texture.Height * maxFactor) };
                    break;

                case StretchType.UniformToFill:
                    float minFactor = layoutScaleX < layoutScaleY ? layoutScaleX : layoutScaleY;
                    scale = new Vector2(minFactor, minFactor);

                    minFactor = 1f / minFactor;


                    dstImageRect = layoutBounds with { Width = (int)(Texture.Width * minFactor), Height = (int)(Texture.Height * minFactor) };
                    break;


                case StretchType.Tile:
                    dstImageRect = layoutBounds;
                    break;

                case StretchType.TileHorizontal:
                    dstImageRect = new Rectangle(layoutBounds.X, layoutBounds.Y, layoutBounds.Width, Texture.Height);
                    break;

                case StretchType.TileVertical:
                    dstImageRect = new Rectangle(layoutBounds.X, layoutBounds.Y, Texture.Width, layoutBounds.Height);
                    break;

            }

            dstImageRect = new Rectangle(dstImageRect.X, dstImageRect.Y, (int)(dstImageRect.Width * ImageScale.X), (int)(dstImageRect.Height * ImageScale.Y));

            return (dstImageRect, scale);
        }
    }
}
