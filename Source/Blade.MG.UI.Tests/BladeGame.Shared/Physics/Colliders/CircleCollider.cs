using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace BladeGame.Physics.Colliders
{
    /// <summary>
    /// Creates a Circular Collision Area
    /// </summary>
    public class CircleCollider : Collider
    {
        /// <summary>
        /// Circle Radius
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// Optional Offset from Sprite Location
        /// </summary>
        public Vector2 Offset { get; set; }

        public CircleCollider()
        {

        }

        public CircleCollider(float radius)
        {
            this.Offset = new Vector2(0f, 0f);
            this.Radius = radius;
        }

        public CircleCollider(Vector2 offset, float radius)
        {
            this.Offset = offset;
            this.Radius = radius;
        }

        public CircleCollider(float offsetX, float offsetY, float radius)
        {
            this.Offset = new Vector2(offsetX, offsetY);
            this.Radius = radius;
        }
    }
}
