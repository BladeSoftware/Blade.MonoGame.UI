using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeGame.Shared.BladeUI
{
    public class Quad
    {
        public VertexPositionNormalTexture[] Vertices;
        public Vector3 Origin;
        public Vector3 Up;
        public Vector3 Normal;
        public Vector3 Left;
        public Vector3 TopLeft;
        public Vector3 TopRight;
        public Vector3 BottomRight;
        public Vector3 BottomLeft;
        public int[] Indexes;


        public Quad(Vector3 origin, Vector3 normal, Vector3 up, float width, float height)
        {
            this.Vertices = new VertexPositionNormalTexture[4];
            this.Indexes = new int[6];
            this.Origin = origin;
            this.Normal = normal;
            this.Up = up;

            // Calculate the quad corners
            this.Left = Vector3.Cross(normal, this.Up);
            Vector3 uppercenter = (this.Up * height / 2) + origin;
            this.TopLeft = uppercenter + (this.Left * width / 2);
            this.TopRight = uppercenter - (this.Left * width / 2);
            this.BottomLeft = this.TopLeft - (this.Up * height);
            this.BottomRight = this.TopRight - (this.Up * height);

            this.FillVertices();
        }

        public Quad(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft)
        {
            this.TopLeft = topLeft;
            this.TopRight = topRight;
            this.BottomRight = bottomRight;
            this.BottomLeft = bottomLeft;

            this.Normal = Vector3.Cross((TopRight - TopLeft), (BottomLeft - TopLeft));

            this.FillVertices();
        }

        private void FillVertices()
        {
            Vector2 textureTopLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureTopRight = new Vector2(1.0f, 0.0f);
            Vector2 textureBottomRight = new Vector2(1.0f, 1.0f);
            Vector2 textureBottomLeft = new Vector2(0.0f, 1.0f);

            for (int i = 0; i < this.Vertices.Length; i++)
            {
                this.Vertices[i].Normal = this.Normal;
            }

            this.Vertices[0].Position = this.BottomLeft;
            this.Vertices[0].TextureCoordinate = textureBottomLeft;
            this.Vertices[1].Position = this.TopLeft;
            this.Vertices[1].TextureCoordinate = textureTopLeft;
            this.Vertices[2].Position = this.BottomRight;
            this.Vertices[2].TextureCoordinate = textureBottomRight;
            this.Vertices[3].Position = this.TopRight;
            this.Vertices[3].TextureCoordinate = textureTopRight;

            this.Indexes[0] = 0;
            this.Indexes[1] = 1;
            this.Indexes[2] = 2;
            this.Indexes[3] = 2;
            this.Indexes[4] = 1;
            this.Indexes[5] = 3;
        }
    }
}
