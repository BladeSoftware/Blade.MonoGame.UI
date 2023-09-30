using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeGame.Sprites
{
    public class SpriteTexture2D
    {
        /// <summary>
        /// Transformations applied to the Image. Does not affect movement of sprite.
        /// e.g. If you need to Translate / Scale / Rotate the image to the correct Size / Forward direction
        /// </summary>
        private Matrix imageMatrix = Matrix.Identity;
        public Matrix FrameMatrix { get { return imageMatrix; } set { imageMatrix = value; } }

        public Texture2D Texture2D { get; private set; }
        public Rectangle Bounds { get; set; }

        public int Width => Bounds.Width;
        public int Height => Bounds.Height;


        public SpriteTexture2D()
        {

        }

        public SpriteTexture2D(Texture2D texture2D)
        {
            this.Texture2D = texture2D;
            this.Bounds = texture2D.Bounds;
        }

        public SpriteTexture2D(Texture2D texture2D, Rectangle bounds)
        {
            this.Texture2D = texture2D;
            this.Bounds = bounds;
        }

        public void DrawSprite(SpriteBatch spriteBatch, Matrix matrix, float depth = 0f)
        {
            GameHelper.DecomposeMatrix2D(ref matrix, out var position, out var rotation, out var scale);

            spriteBatch.Draw(Texture2D, position, Bounds, Color.White, rotation, new Vector2(0f, 0f), scale, SpriteEffects.None, depth);

            //spriteBatch.Draw(asteroidImg.Texture2D, position, null, Color.White, rotation, new Vector2(0f, 0f), scale, SpriteEffects.None, 0.5f);
        }

        public virtual void ImageTranslate(float x, float y, float z = 0f)
        {
            imageMatrix = Matrix.Multiply(imageMatrix, Matrix.CreateTranslation(x, y, z));
        }

        public virtual void ImageRotateDeg(float angleDeg)
        {
            imageMatrix = Matrix.Multiply(imageMatrix, Matrix.CreateRotationZ(MathHelper.ToRadians(angleDeg)));
        }

        public virtual void ImageRotateRad(float angleRad)
        {
            imageMatrix = Matrix.Multiply(imageMatrix, Matrix.CreateRotationZ(angleRad));
        }

        public virtual void ImageScale(float scale)
        {
            imageMatrix = Matrix.Multiply(imageMatrix, Matrix.CreateScale(scale, scale, 1f));
        }

        public virtual void ImageScale(float scaleX, float scaleY, float scaleZ = 1f)
        {
            imageMatrix = Matrix.Multiply(imageMatrix, Matrix.CreateScale(scaleX, scaleY, scaleZ));
        }

    }
}
