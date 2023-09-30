using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeGame.Shared.BladeUI
{
    public class Quad2D
    {
        private static VertexDeclaration quadVertexDecl = new VertexDeclaration(new VertexElement[]
        {
              new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
              new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
              new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        });


        public VertexPositionNormalTexture[] Vertices;
        public int[] Indexes;

        public Texture2D Texture;

        private BasicEffect quadEffect;

        //public Vector3 Origin;
        //public Vector3 Up;
        //public Vector3 Normal;
        //public Vector3 Left;
        //public Vector3 TopLeft;
        //public Vector3 TopRight;
        //public Vector3 BottomRight;
        //public Vector3 BottomLeft;


        //public Quad2D(Vector2 origin, float width, float height, Texture2D texture)
        //{
        //    this.Vertices = new VertexPositionNormalTexture[4];
        //    this.Indexes = new int[6];
        //    this.Texture = texture;
        //    //this.Origin = origin;
        //    //this.Normal = normal;
        //    //this.Up = up;

        //    //// Calculate the quad corners
        //    //Vector3 Left = new Vector3(-1, 0, 0);  //Vector3.Cross(normal, this.Up);
        //    //Vector3 uppercenter = (this.Up * height / 2) + origin;

        //    //Vector3 TopLeft = uppercenter + (this.Left * width / 2);
        //    //Vector3 TopRight = uppercenter - (this.Left * width / 2);
        //    //Vector3 BottomLeft = this.TopLeft - (this.Up * height);
        //    //Vector3 BottomRight = this.TopRight - (this.Up * height);

        //    //this.FillVertices();
        //}

        public Quad2D(float x1, float y1, float x2, float y2, Texture2D texture)
        {
            this.Vertices = new VertexPositionNormalTexture[4];
            this.Indexes = new int[6];


            Vector3 TopLeft = new Vector3(x1, y1, 0f);
            Vector3 TopRight = new Vector3(x2, y1, 0f);
            Vector3 BottomRight = new Vector3(x2, y2, 0f);
            Vector3 BottomLeft = new Vector3(x1, y2, 0f);

            Vector3 normal = Vector3.Cross((TopRight - TopLeft), (BottomLeft - TopLeft));
            normal.Normalize();

            Vector2 textureTopLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureTopRight = new Vector2(2.0f, 0.0f);
            Vector2 textureBottomRight = new Vector2(2.0f, 1.0f);
            Vector2 textureBottomLeft = new Vector2(0.0f, 1.0f);

            // Fill Vertices
            this.Vertices[0].Position = TopLeft;
            this.Vertices[0].TextureCoordinate = textureTopLeft;
            this.Vertices[0].Normal = normal;

            this.Vertices[1].Position = TopRight;
            this.Vertices[1].TextureCoordinate = textureTopRight;
            this.Vertices[1].Normal = normal;

            this.Vertices[2].Position = BottomRight;
            this.Vertices[2].TextureCoordinate = textureBottomRight;
            this.Vertices[2].Normal = normal;

            this.Vertices[3].Position = BottomLeft;
            this.Vertices[3].TextureCoordinate = textureBottomLeft;
            this.Vertices[3].Normal = normal;


            // Define triangles from the Vertices
            this.Indexes[0] = 0;
            this.Indexes[1] = 2;
            this.Indexes[2] = 1;

            this.Indexes[3] = 2;
            this.Indexes[4] = 0;
            this.Indexes[5] = 3;

            this.Texture = texture;

        }

        public void RenderQuad(GraphicsDevice graphicsDevice, Matrix world, Matrix view, Matrix projection)
        {
            // Test Draw Quad
            BasicEffect quadEffect = new BasicEffect(graphicsDevice);
            quadEffect.AmbientLightColor = new Vector3(1.0f, 1.0f, 1.0f);
            quadEffect.LightingEnabled = true;
            quadEffect.World = world;
            quadEffect.View = view;
            quadEffect.Projection = projection;
            quadEffect.TextureEnabled = true;
            quadEffect.Texture = Texture;



            //-- Draw Quad
            foreach (EffectPass pass in quadEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                     PrimitiveType.TriangleList,
                     this.Vertices, 0, 4,
                     this.Indexes, 0, 2);

            }

        }
    }
}
