using System;
using System.Collections.Generic;
using System.Text;
using BladeGame.Shared.Primitives;
using BladeGame.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeGame.Physics.Colliders
{
    public abstract class Collider
    {
        /// <summary>
        /// Use IsStatic = False for colliders that are attached to 'static' walls, floors etc. Anything that doesn't move.
        /// Use IsStatic = True for colliers that are attached to 'dynamic' object. i.e. Anything that does move.
        /// 
        /// We don't test for collisions between  two static objects as it's assumed they are fixed and will never interact.
        /// </summary>
        public bool IsStatic = false;

        public static bool DoCollisionDetection(Game game, Sprite2D entity1, Collider collider1, Sprite2D entity2, Collider collider2)
        {
            // Don't check for collisions between two static colliders
            if (collider1.IsStatic && collider2.IsStatic)
            {
                return false;
            }

            // Determin the type of collision to test for
            if (collider1 is CircleCollider && collider2 is CircleCollider)
            {
                return CircleCircleCollision(game, entity1, (CircleCollider)collider1, entity2, (CircleCollider)collider2);
            }
            else
            {
                // Unknown collider type
                return false;
            }
        }

        private static bool CircleCircleCollision(Game game, Sprite2D entity1, CircleCollider circle1, Sprite2D entity2, CircleCollider circle2)
        {
            Vector3 v1 = Vector3.TransformNormal(new Vector3(circle1.Radius, 0f, 0f), entity1.GetWorldMatrix());
            Vector3 v2 = Vector3.TransformNormal(new Vector3(circle2.Radius, 0f, 0f), entity2.GetWorldMatrix());

            // Calculate the distance between the centers of both circles
            float dx = (entity2.Location.X + circle2.Offset.X) - (entity1.Location.X + circle1.Offset.X);
            float dy = (entity2.Location.Y + circle2.Offset.Y) - (entity1.Location.Y + circle1.Offset.Y);

            // Calculate the Square of the distance between the centers of both circles
            float distSqr = dx * dx + dy * dy;

            float rad1 = v1.Length();
            float rad2 = v2.Length();

            float radSqr = rad1 * rad1 + rad2 * rad2;

            // Calculate the Square of the distance between the edges of the cricles
            bool isCollision = distSqr <= radSqr;


//#if DEBUG
//            using (var spriteBatch = new SpriteBatch(game.GraphicsDevice))
//            {
//                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, null, null, null);

//                Primitives2D.DrawCircle(game, spriteBatch, entity1.Location + circle1.Offset, Color.Green, rad1);
//                Primitives2D.DrawCircle(game, spriteBatch, entity2.Location + circle2.Offset, Color.Green, rad2);

//                //Primitives2D.DrawLine(game, spriteBatch, entity1.Location, new Vector2(entity1.Location.X + rad1, entity1.Location.Y), Color.Blue, 3);
//                //Primitives2D.DrawLine(game, spriteBatch, entity2.Location, new Vector2(entity2.Location.X + rad2, entity2.Location.Y), Color.Blue, 3);

//                Vector2 v = new Vector2(dx, dy);
//                v.Normalize();
//                float dist = (float)Math.Sqrt(distSqr);

//                Vector2 start = entity1.Location + new Vector2(v.X * rad1, v.Y * rad1);
//                Vector2 end = start + new Vector2(v.X * dist, v.Y * dist);

//                //Primitives2D.DrawLine(game, spriteBatch, start, end, Color.Blue, 5);

//                //end = entity1.Location + new Vector2(v.X * (dist - rad2 - rad1), v.Y * (dist - rad2 - rad1));
//                end = start + new Vector2(v.X * (dist - rad2 - rad1), v.Y * (dist - rad2 - rad1));
//                Primitives2D.DrawLine(game, spriteBatch, start, end, Color.White, 2);


//                if (isCollision)
//                {
//                    Primitives2D.DrawLine(game, spriteBatch, entity1.Location, entity2.Location, Color.Red, 3);
//                }

//                spriteBatch.End();
//            }
//#endif

            return isCollision;
        }
    }
}
