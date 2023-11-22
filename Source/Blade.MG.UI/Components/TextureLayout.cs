using FontStashSharp.Rasterizers.StbTrueTypeSharp;
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

        /// <summary>
        /// Amount to re-size the image within the Layout Bounds
        /// e.g. 0.5 = show the image twice within the area
        /// </summary>
        public Vector2 TextureScale { get; set; } = Vector2.One;

        /// <summary>
        /// Only used when StretchType = None / Tile / TileHorizontal / TileVertical
        /// Amount to resize the Image Layout bounds
        /// </summary>
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
            float widthRatio = srcImageRect.Width / (float)layoutBounds.Width;
            float heightRatio = srcImageRect.Height / (float)layoutBounds.Height;

            bool canGrow = !(StretchType == StretchType.Fill_ShrinkOnly || StretchType == StretchType.Uniform_ShrinkOnly || StretchType == StretchType.UniformToFill_ShrinkOnly);
            bool canShrink = !(StretchType == StretchType.Fill_GrowOnly || StretchType == StretchType.Uniform_GrowOnly || StretchType == StretchType.UniformToFill_GrowOnly);


            Rectangle dstImageRect = layoutBounds;
            Vector2 scale = Vector2.One;

            switch (StretchType)
            {
                case StretchType.None:
                    dstImageRect = new Rectangle(layoutBounds.Left, layoutBounds.Top, (int)(Texture.Width * ImageScale.X), (int)(Texture.Height * ImageScale.Y));
                    //scale = new Vector2(1f / ImageScale.X, 1f / ImageScale.Y);
                    scale = ImageScale;
                    break;

                case StretchType.Fill:
                case StretchType.Fill_ShrinkOnly:
                case StretchType.Fill_GrowOnly:

                    if (widthRatio < 1f && !canGrow)
                    {
                        widthRatio = 1f;
                    }
                    else if (widthRatio > 1f && !canShrink)
                    {
                        widthRatio = 1f;
                    }

                    if (heightRatio < 1f && !canGrow)
                    {
                        heightRatio = 1f;
                    }
                    else if (heightRatio > 1f && !canShrink)
                    {
                        heightRatio = 1f;
                    }

                    scale = new Vector2(1f / widthRatio, 1f / heightRatio);

                    dstImageRect = new Rectangle(layoutBounds.Left, layoutBounds.Top, (int)(srcImageRect.Width * scale.X), (int)(srcImageRect.Height * scale.Y));

                    break;

                case StretchType.Uniform:
                case StretchType.Uniform_ShrinkOnly:
                case StretchType.Uniform_GrowOnly:

                    float minFactor = 1f;

                    if (widthRatio <= 1f && heightRatio <= 1f)
                    {
                        // Image Width and Height are smaller then layout width and height
                        if (canGrow)
                        {
                            minFactor = widthRatio > heightRatio ? widthRatio : heightRatio;
                        }
                    }
                    else if (widthRatio >= 1f && heightRatio >= 1f)
                    {
                        // Image Width and Height are larger then layout width and height
                        if (canShrink)
                        {
                            minFactor = widthRatio > heightRatio ? widthRatio : heightRatio;
                        }
                    }
                    else if (widthRatio >= 1f)
                    {
                        // Image Width is larger then layout width, Image Height is smaller then layout height
                        if (canShrink)
                        {
                            minFactor = widthRatio;
                        }
                    }
                    else
                    {
                        // Image Width is smaller then layout width, Image Height is larger then layout height
                        if (canShrink)
                        {
                            minFactor = heightRatio;
                        }
                    }


                    minFactor = 1f / minFactor;
                    scale = new Vector2(minFactor, minFactor);

                    dstImageRect = layoutBounds with { Width = (int)(Texture.Width * minFactor), Height = (int)(Texture.Height * minFactor) };
                    break;

                case StretchType.UniformToFill:
                case StretchType.UniformToFill_ShrinkOnly:
                case StretchType.UniformToFill_GrowOnly:

                    float maxFactor = 1f;

                    if (widthRatio <= 1f && heightRatio <= 1f)
                    {
                        // Image Width and Height are smaller then layout width and height
                        if (canGrow)
                        {
                            maxFactor = widthRatio < heightRatio ? widthRatio : heightRatio;
                        }
                    }
                    else if (widthRatio >= 1f && heightRatio >= 1f)
                    {
                        // Image Width and Height are larger then layout width and height
                        if (canShrink)
                        {
                            maxFactor = widthRatio < heightRatio ? widthRatio : heightRatio;
                        }
                    }
                    else if (widthRatio < 1f)
                    {
                        // Image Width is smaller then layout width, Image Height is larger then layout height
                        if (canGrow)
                        {
                            maxFactor = widthRatio;
                        }
                    }
                    else
                    {
                        // Image Width is larger then layout width, Image Height is smaller then layout height
                        if (canGrow)
                        {
                            maxFactor = heightRatio;
                        }
                    }


                    maxFactor = 1f / maxFactor;
                    scale = new Vector2(maxFactor, maxFactor);

                    dstImageRect = layoutBounds with { Width = (int)(Texture.Width * maxFactor), Height = (int)(Texture.Height * maxFactor) };
                    break;


                case StretchType.Tile:
                    dstImageRect = layoutBounds;
                    //scale = new Vector2(1f / layoutScaleX, 1f / layoutScaleY);
                    break;

                case StretchType.TileHorizontal:
                    dstImageRect = new Rectangle(layoutBounds.X, layoutBounds.Y, layoutBounds.Width, (int)(Texture.Height * ImageScale.Y));
                    //scale = new Vector2(1f / layoutScaleX, 1f);
                    break;

                case StretchType.TileVertical:
                    dstImageRect = new Rectangle(layoutBounds.X, layoutBounds.Y, (int)(Texture.Width * ImageScale.X), layoutBounds.Height);
                    //scale = new Vector2(1f, 1f / layoutScaleY);
                    break;

            }

            return (dstImageRect, scale);
        }
    }
}
