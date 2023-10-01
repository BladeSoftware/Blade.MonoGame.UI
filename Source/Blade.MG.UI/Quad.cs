using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blade.MG.UI
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
            Vertices = new VertexPositionNormalTexture[4];
            Indexes = new int[6];
            Origin = origin;
            Normal = normal;
            Up = up;

            // Calculate the quad corners
            Left = Vector3.Cross(normal, Up);
            Vector3 uppercenter = Up * height / 2 + origin;
            TopLeft = uppercenter + Left * width / 2;
            TopRight = uppercenter - Left * width / 2;
            BottomLeft = TopLeft - Up * height;
            BottomRight = TopRight - Up * height;

            FillVertices();
        }

        public Quad(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;

            Normal = Vector3.Cross(TopRight - TopLeft, BottomLeft - TopLeft);

            FillVertices();
        }

        private void FillVertices()
        {
            Vector2 textureTopLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureTopRight = new Vector2(1.0f, 0.0f);
            Vector2 textureBottomRight = new Vector2(1.0f, 1.0f);
            Vector2 textureBottomLeft = new Vector2(0.0f, 1.0f);

            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Normal = Normal;
            }

            Vertices[0].Position = BottomLeft;
            Vertices[0].TextureCoordinate = textureBottomLeft;
            Vertices[1].Position = TopLeft;
            Vertices[1].TextureCoordinate = textureTopLeft;
            Vertices[2].Position = BottomRight;
            Vertices[2].TextureCoordinate = textureBottomRight;
            Vertices[3].Position = TopRight;
            Vertices[3].TextureCoordinate = textureTopRight;

            Indexes[0] = 0;
            Indexes[1] = 1;
            Indexes[2] = 2;
            Indexes[3] = 2;
            Indexes[4] = 1;
            Indexes[5] = 3;
        }
    }
}
