using System;
using System.Collections.Generic;
using System.Text;
using BladeGame.Physics.Colliders;
using Microsoft.Xna.Framework;

namespace BladeGame.Sprites
{
    public abstract class Sprite2D : SpriteBase
    {
        /// <summary>
        /// Transformations applied to the Image. Does not affect movement of sprite.
        /// e.g. If you need to Translate / Scale / Rotate the image to the correct Size / Forward direction
        /// </summary>
        public SpriteTexture2D CurrentFrame { get; set; }
        //protected Matrix frameMatrix = Matrix.Identity;
        //public Matrix FrameMatrix { get { return frameMatrix; } set { frameMatrix = value; } }

        /// <summary>
        /// Transformations applied to the Image. Does not affect movement of sprite.
        /// e.g. If you need to Translate / Scale / Rotate the image to the correct Size / Forward direction
        /// </summary>
        protected Matrix imageMatrix = Matrix.Identity;
        public Matrix ImageMatrix { get { return imageMatrix; } set { imageMatrix = value; } }

        /// <summary>
        /// Transformations applied to the Sprite. Affects Location and Orientation of sprite.
        /// Scale / Rotate the Sprite
        /// </summary>
        protected Matrix spriteMatrix = Matrix.Identity;
        public Matrix SpriteMatrix { get { return spriteMatrix; } set { spriteMatrix = value; } }

        //protected readonly Vector2 pointZero = new Vector2(0, 0);
        //protected readonly Vector2 pointUp = new Vector2(0, -1);
        //protected readonly Vector2 pointRight = new Vector2(1, 0);

        protected static readonly Vector3 pointZero = new Vector3(0f, 0f, 0f);
        protected readonly Vector3 axisX = new Vector3(1f, 0f, 0f);
        protected readonly Vector3 axisY = new Vector3(0f, -1f, 0f);
        protected readonly Vector3 axisZ = new Vector3(0f, 0f, 1f);

        public Vector2 Location { get; set; } = new Vector2(0f, 0f);

        public bool DoCollisionChecks { get; set; } = true;
        public List<Collider> Colliders = new List<Collider>();


        /// <summary>
        /// ctor
        /// </summary>
        public Sprite2D(GameBase game) : base(game)
        {

        }

        //        public Matrix GetImageMatrix()
        //        {
        //            Matrix transform = Matrix.Multiply(frameMatrix, imageMatrix);
        ////            transform = Matrix.Multiply(transform, spriteMatrix);
        ////            transform = Matrix.Multiply(transform, Matrix.CreateTranslation(Location.X, Location.Y, Location.Z));

        //            return transform;
        //        }

        public Matrix GetWorldMatrix()
        {
            Matrix transform = Matrix.Multiply(CurrentFrame.FrameMatrix, imageMatrix);
            transform = Matrix.Multiply(transform, spriteMatrix);
            transform = Matrix.Multiply(transform, Matrix.CreateTranslation(Location.X, Location.Y, 0f));

            return transform;
        }


        /// <summary>
        /// Returns a unit vector pointing in the direction the sprite is facing
        /// </summary>
        /// <returns></returns>
        public Vector3 ForwardVec()
        {
            return Vector3.Transform(axisY, spriteMatrix);
        }

        /// <summary>
        /// Returns a unit vector pointing to the Right, perpendicular to the direction the sprite is facing
        /// </summary>
        /// <returns></returns>
        public Vector3 RightVec()
        {
            return Vector3.Transform(axisX, spriteMatrix);
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

        /// <summary>
        /// Move the sprite to the specified location
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public virtual void TranslateTo(float x, float y)
        {
            Location = new Vector2(x, y);
        }

        /// <summary>
        /// Move the sprite by the specified amounts
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public virtual void Translate(float x, float y)
        {
            Location = new Vector2(Location.X + x, Location.Y + y);
        }

        /// <summary>
        /// Rotate the sprite by the specified angle (in degrees)
        /// </summary>
        /// <param name="degrees"></param>
        public virtual void RotateDeg(float angleDeg)
        {
            spriteMatrix = Matrix.Multiply(spriteMatrix, Matrix.CreateRotationZ(MathHelper.ToRadians(angleDeg)));
        }

        /// <summary>
        /// Rotate the sprite by the specified angle (in radians)
        /// </summary>
        /// <param name="angleRad"></param>
        public virtual void RotateRad(float angleRad)
        {
            spriteMatrix = Matrix.Multiply(spriteMatrix, Matrix.CreateRotationZ(angleRad));
        }

        ////public virtual void Scale(double scale)
        ////{
        ////    spriteMatrix.MulScale(scale, scale);
        ////}

        /// <summary>
        /// Move the sprite along the Forward Vector
        /// </summary>
        /// <param name="dist"></param>
        public virtual void MoveForward(float dist)
        {
            Vector3 forward = Vector3.Transform(axisY, spriteMatrix);
            Translate(forward.X * dist, forward.Y * dist);
        }

        /// <summary>
        /// Move the sprite along the Right Vector
        /// </summary>
        /// <param name="dist"></param>
        public virtual void MoveRight(float dist)
        {
            Vector3 right = Vector3.Transform(axisX, spriteMatrix);
            Translate(right.X * dist, right.Y * dist);
        }


        // TODO: Move in the given direction (directcion should be normalised)
        //public virtual void Move(SKPoint direction, float dist)
        //{
        //}

        // TODO: Move toward the given point
        //public virtual void MoveTowards(SKPoint point, float dist)
        //{
        //}

        // TODO: Move along arc / bezier / path ??

        ///// <summary>
        ///// Returns a Rectangle which will contain the visual image
        ///// </summary>
        //public IntRect BoundsRect
        //{
        //    get
        //    {
        //        Vector2[] vertices = Bounds;

        //        int minX = (int)Math.Min(Math.Min(vertices[0].X, vertices[1].X), Math.Min(vertices[2].X, vertices[3].X));
        //        int minY = (int)Math.Min(Math.Min(vertices[0].Y, vertices[1].Y), Math.Min(vertices[2].Y, vertices[3].Y));
        //        int maxX = (int)Math.Max(Math.Max(vertices[0].X, vertices[1].X), Math.Max(vertices[2].X, vertices[3].X));
        //        int maxY = (int)Math.Max(Math.Max(vertices[0].Y, vertices[1].Y), Math.Max(vertices[2].Y, vertices[3].Y));

        //        int width = maxX - minX + 1;
        //        int height = maxY - minY + 1;

        //        return new IntRect((int)minX, (int)minY, width, height);
        //    }
        //}

    }
}
