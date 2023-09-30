using System;
using System.Collections.Generic;
using System.Text;
using BladeGame.Sprites;
using Microsoft.Xna.Framework;

namespace BladeGame.Primitives
{
    public struct Quad2D
    {
        public Vector2 TL { get; set; }
        public Vector2 TR { get; set; }
        public Vector2 BR { get; set; }
        public Vector2 BL { get; set; }

        public Quad2D(Rectangle rectangle)
        {
            TL = new Vector2(rectangle.Left, rectangle.Top);
            TR = new Vector2(rectangle.Right, rectangle.Top);
            BR = new Vector2(rectangle.Right, rectangle.Bottom);
            BL = new Vector2(rectangle.Left, rectangle.Bottom);
        }

        public Quad2D(Rectangle rectangle, Matrix matrix)
        {
            TL = Vector2.Transform(new Vector2(rectangle.Left, rectangle.Top), matrix);
            TR = Vector2.Transform(new Vector2(rectangle.Right, rectangle.Top), matrix);
            BR = Vector2.Transform(new Vector2(rectangle.Right, rectangle.Bottom), matrix);
            BL = Vector2.Transform(new Vector2(rectangle.Left, rectangle.Bottom), matrix);
        }

        public Quad2D(Sprite2D sprite2D) : this(sprite2D.CurrentFrame.Bounds, sprite2D.GetWorldMatrix())
        {

        }

        public Rectangle Bounds()
        {
            float minX = TL.X;
            float maxX = TL.X;
            float minY = TL.Y;
            float maxY = TL.Y;

            if (TR.X < minX) minX = TR.X;
            if (BR.X < minX) minX = BR.X;
            if (BL.X < minX) minX = BL.X;

            if (TR.X > maxX) maxX = TR.X;
            if (BR.X > maxX) maxX = BR.X;
            if (BL.X > maxX) maxX = BL.X;

            if (TR.Y < minY) minY = TR.Y;
            if (BR.Y < minY) minY = BR.Y;
            if (BL.Y < minY) minY = BL.Y;

            if (TR.Y > maxY) maxY = TR.Y;
            if (BR.Y > maxY) maxY = BR.Y;
            if (BL.Y > maxY) maxY = BL.Y;

            return new Rectangle((int)minX, (int)minY, (int)(maxX - minX + 1f), (int)(maxY - minY + 1f));
        }

        /// <summary>
        /// Find the center of the Quad
        /// </summary>
        /// <returns></returns>
        public Vector2 Center()
        {
            Rectangle bounds = Bounds();

            float cx = (bounds.Left + bounds.Right) / 2f;
            float cy = (bounds.Top + bounds.Bottom) / 2f;

            return new Vector2(cx, cy);
        }

        /// <summary>
        /// Assumes the Quad defines a Square.
        /// Returns the Radius for a Circle with origin at the center of the Square and fitting inside the Square
        /// (The 4 sides should form tangents to the circle)
        /// </summary>
        /// <returns></returns>
        public float RadiusInsideSquare()
        {
            // -- Calc radius of a circle to fit inside the Square (TL, TR, BR, BL)
            float dx = TR.X - TL.X;
            float dy = TR.Y - TL.Y;

            float diameter = (float)Math.Sqrt(dx * dx + dy * dy);
            float radius = diameter / 2f;

            return radius;
        }

        /// <summary>
        /// Assumes the Quad defines a Square.
        /// Returns the Radius for a Circle with origin at the center of the Square and passing through the 4 corners.
        /// </summary>
        /// <returns></returns>
        public float RadiusThroughVertices()
        {
            Vector2 center = Center();

            // -- Calc radius of a circle to pass through the four vertices of the Square (TL, TR, BR, BL)
            float dx = TR.X - center.X;
            float dy = TR.Y - center.Y;
            float radius = (float)Math.Sqrt(dx * dx + dy * dy);

            return radius;
        }

    }
}
