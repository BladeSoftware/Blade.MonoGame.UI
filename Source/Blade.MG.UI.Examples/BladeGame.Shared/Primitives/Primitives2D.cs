using System;
using System.Collections.Generic;
using System.Text;
using BladeGame.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BladeGame.Shared.Primitives
{
    public static class Primitives2D
    {
        //private static ContentManager contentLoader = new ContentManager(Game.Se, "Content");

        //private static Texture2D Pixel(Game game) => contentLoader.Load<Texture2D>("Images/pixel");
        //private static Texture2D Circle(Game game) => contentLoader.Load<Texture2D>("Images/circle");

        private static Texture2D Pixel(Game game) => game.Content.Load<Texture2D>("Images/pixel");
        private static Texture2D Circle(Game game) => game.Content.Load<Texture2D>("Images/circle");
        private static Texture2D CircleSolid(Game game) => game.Content.Load<Texture2D>("Images/circle_solid");

        #region Pixels
        public static void DrawPixel(Game game, SpriteBatch spriteBatch, Vector2 position, Color color, float scale = 1f)
        {
            DrawPixel(game, spriteBatch, position.X, position.Y, color, scale);
        }

        public static void DrawPixel(Game game, SpriteBatch spriteBatch, Vector3 position, Color color, float scale = 1f)
        {
            DrawPixel(game, spriteBatch, position.X, position.Y, color, scale);
        }

        public static void DrawPixel(Game game, SpriteBatch spriteBatch, float x, float y, Color color, float scale = 1f)
        {
            spriteBatch.Draw(Pixel(game), new Vector2(x - scale / 2f, y - scale / 2f), null, color, 0f, new Vector2(0, 0), scale, SpriteEffects.None, 0f);
        }
        #endregion

        #region Lines
        public static void DrawLine(Game game, SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Color color, float lineWidth = 1f)
        {
            DrawLine(game, spriteBatch, p1.X, p1.Y, p2.X, p2.Y, color, lineWidth);
        }

        public static void DrawLine(Game game, SpriteBatch spriteBatch, Vector3 p1, Vector3 p2, Color color, float lineWidth = 1f)
        {
            DrawLine(game, spriteBatch, p1.X, p1.Y, p2.X, p2.Y, color, lineWidth);
        }

        public static void DrawLine(Game game, SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color, float lineWidth = 1f)
        {
            float rotation = 0f;
            float xd = x2 - x1;
            float yd = y2 - y1;
            float len = (float)Math.Sqrt(xd * xd + yd * yd);

            xd /= len;
            yd /= len;

            if (xd < -1f) xd = -1f;
            if (xd > 1f) xd = 1f;

            rotation = (float)Math.Acos(xd);

            if (yd < 0)
            {
                rotation = MathHelper.TwoPi - rotation;
            }

            x1 -= xd * (lineWidth / 2f);
            y1 -= yd * (lineWidth / 2f);

            x1 -= -yd * (lineWidth / 2f);
            y1 -= xd * (lineWidth / 2f);

            spriteBatch.Draw(Pixel(game), new Vector2(x1, y1), null, color, rotation, new Vector2(0, 0), new Vector2(len, lineWidth), SpriteEffects.None, 0f);
        }

        public static void DrawHLine(Game game, SpriteBatch spriteBatch, float y, float x1, float x2, Color color, float lineWidth = 1f)
        {
            float rotation = 0f;
            float len = x2 - x1;

            if (len == 0) return;
            if (len < 0)
            {
                float t = x1;
                x1 = x2;
                x2 = t;
                len = -len;
            }

            float hw = lineWidth / 2f;

            spriteBatch.Draw(Pixel(game), new Vector2(x1, y - hw), null, color, rotation, new Vector2(0, 0), new Vector2(len, lineWidth), SpriteEffects.None, 0f);
        }

        public static void DrawVLine(Game game, SpriteBatch spriteBatch, float x, float y1, float y2, Color color, float lineWidth = 1f)
        {
            float rotation = 0f;
            float len = y2 - y1;

            if (len == 0) return;
            if (len < 0)
            {
                float t = y1;
                y1 = y2;
                y2 = t;
                len = -len;
            }

            float hw = lineWidth / 2f;

            spriteBatch.Draw(Pixel(game), new Vector2(x - hw, y1), null, color, rotation, new Vector2(0, 0), new Vector2(lineWidth, len), SpriteEffects.None, 0f);
        }
        #endregion

        #region Rectangles
        public static void DrawRect(Game game, SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color, float lineWidth = 1f)
        {
            float hw = lineWidth / 2f;

            Primitives2D.DrawHLine(game, spriteBatch, y1, x1, x2, color, lineWidth);
            Primitives2D.DrawVLine(game, spriteBatch, x2, y1 - hw, y2 + hw, color, lineWidth);
            Primitives2D.DrawHLine(game, spriteBatch, y2, x1, x2, color, lineWidth);
            Primitives2D.DrawVLine(game, spriteBatch, x1, y1 - hw, y2 + hw, color, lineWidth);
        }

        public static void DrawRect(Game game, SpriteBatch spriteBatch, Rectangle rectangle, Color color, float lineWidth = 1f)
        {
            Primitives2D.DrawHLine(game, spriteBatch, rectangle.Top, rectangle.Left, rectangle.Right, color, lineWidth);
            Primitives2D.DrawVLine(game, spriteBatch, rectangle.Right, rectangle.Top, rectangle.Bottom, color, lineWidth);
            Primitives2D.DrawHLine(game, spriteBatch, rectangle.Bottom, rectangle.Right, rectangle.Left, color, lineWidth);
            Primitives2D.DrawVLine(game, spriteBatch, rectangle.Left, rectangle.Bottom, rectangle.Top, color, lineWidth);
        }

        public static void FillRect(Game game, SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color)
        {
            float rotation = 0f;
            float width = x2 - x1;
            float height = y2 - y1;

            if (width == 0 || height == 0) return;
            if (width < 0)
            {
                float t = x1;
                x1 = x2;
                x2 = t;
                width = -width;
            }

            if (height < 0)
            {
                height = -height;
            }

            spriteBatch.Draw(Pixel(game), new Vector2(x1, y1), null, color, rotation, new Vector2(0, 0), new Vector2(width, height), SpriteEffects.None, 0f);
        }

        public static void FillRect(Game game, SpriteBatch spriteBatch, Rectangle rectangle, Color color)
        {
            float rotation = 0f;


            spriteBatch.Draw(Pixel(game), new Vector2(rectangle.Left, rectangle.Top), null, color, rotation, new Vector2(0, 0), new Vector2(rectangle.Width, rectangle.Height), SpriteEffects.None, 0f);

            //Primitives2D.DrawHLine(game, spriteBatch, rectangle.Top, rectangle.Left, rectangle.Right, color, lineWidth);
            //Primitives2D.DrawVLine(game, spriteBatch, rectangle.Right, rectangle.Top, rectangle.Bottom, color, lineWidth);
            //Primitives2D.DrawHLine(game, spriteBatch, rectangle.Bottom, rectangle.Right, rectangle.Left, color, lineWidth);
            //Primitives2D.DrawVLine(game, spriteBatch, rectangle.Left, rectangle.Bottom, rectangle.Top, color, lineWidth);
        }
        #endregion

        #region Circle Bitmaps
        public static void DrawCircleFast(Game game, SpriteBatch spriteBatch, Vector2 position, float radius, Color color)
        {
            DrawCircleFast(game, spriteBatch, position.X, position.Y, radius, color);
        }

        public static void DrawCircleFast(Game game, SpriteBatch spriteBatch, Vector3 position, float radius, Color color)
        {
            DrawCircleFast(game, spriteBatch, position.X, position.Y, radius, color);
        }

        public static void DrawCircleFast(Game game, SpriteBatch spriteBatch, float x, float y, float radius, Color color)
        {
            spriteBatch.Draw(Circle(game), new Vector2(x, y), null, color, 0f, new Vector2(50, 50), radius / 50f, SpriteEffects.None, 0f);
        }


        public static void FillCircleFast(Game game, SpriteBatch spriteBatch, Vector2 position, float radius, Color color)
        {
            FillCircleFast(game, spriteBatch, position.X, position.Y, radius, color);
        }

        public static void FillCircleFast(Game game, SpriteBatch spriteBatch, Vector3 position, float radius, Color color)
        {
            FillCircleFast(game, spriteBatch, position.X, position.Y, radius, color);
        }

        public static void FillCircleFast(Game game, SpriteBatch spriteBatch, float x, float y, float radius, Color color)
        {
            spriteBatch.Draw(CircleSolid(game), new Vector2(x, y), null, color, 0f, new Vector2(50, 50), radius / 50f, SpriteEffects.None, 0f);
        }

        #endregion

        #region Ellipse Bitmaps
        public static void DrawEllipseFast(Game game, SpriteBatch spriteBatch, Vector2 position, Vector2 radius, Color color)
        {
            DrawEllipseFast(game, spriteBatch, position.X, position.Y, radius.X, radius.Y, color);
        }

        public static void DrawEllipseFast(Game game, SpriteBatch spriteBatch, Vector3 position, float radiusX, float radiusY, Color color)
        {
            DrawEllipseFast(game, spriteBatch, position.X, position.Y, radiusX, radiusY, color);
        }

        public static void DrawEllipseFast(Game game, SpriteBatch spriteBatch, Vector3 position, Vector2 radius, Color color)
        {
            DrawEllipseFast(game, spriteBatch, position.X, position.Y, radius.X, radius.Y, color);
        }

        public static void DrawEllipseFast(Game game, SpriteBatch spriteBatch, Vector3 position, Vector3 radius, Color color)
        {
            DrawEllipseFast(game, spriteBatch, position.X, position.Y, radius.X, radius.Y, color);
        }

        public static void DrawEllipseFast(Game game, SpriteBatch spriteBatch, float x, float y, float radiusX, float radiusY, Color color)
        {
            spriteBatch.Draw(Circle(game), new Vector2(x, y), null, color, 0f, new Vector2(50, 50), new Vector2(radiusX / 50f, radiusY / 50f), SpriteEffects.None, 0f);
        }
        #endregion

        #region Circles
        public static void DrawCircle(Game game, SpriteBatch spriteBatch, float x, float y, float radius, Color color, float lineWidth)
        {
            if (spriteBatch == null)
            {
                throw new ArgumentNullException(nameof(spriteBatch));
            }

            int target = 0;
            int a = (int)radius;
            int b = 0;
            int t;


            Vector2 pixelScale = new Vector2(lineWidth, lineWidth);
            float hw = lineWidth / 2f;

            int r2 = a * a; // radius^2;
            while (a >= b)
            {

                b = (int)Math.Round(Math.Sqrt(r2 - a * a));

                // SWAP(target, b);
                t = target; target = b; b = t;

                while (b < target)
                {
                    int af = (100 * a) / 100;
                    int bf = (100 * b) / 100;

                    Color color2 = Color.Red;

                    spriteBatch.Draw(Pixel(game), new Vector2(x + af - hw, y + b - hw), null, color, 0f, new Vector2(0, 0), pixelScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(Pixel(game), new Vector2(x + bf - hw, y + a - hw), null, color, 0f, new Vector2(0, 0), pixelScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(Pixel(game), new Vector2(x - af - hw, y + b - hw), null, color, 0f, new Vector2(0, 0), pixelScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(Pixel(game), new Vector2(x - bf - hw, y + a - hw), null, color, 0f, new Vector2(0, 0), pixelScale, SpriteEffects.None, 0f);

                    spriteBatch.Draw(Pixel(game), new Vector2(x - af - hw, y - b - hw), null, color, 0f, new Vector2(0, 0), pixelScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(Pixel(game), new Vector2(x - bf - hw, y - a - hw), null, color, 0f, new Vector2(0, 0), pixelScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(Pixel(game), new Vector2(x + af - hw, y - b - hw), null, color, 0f, new Vector2(0, 0), pixelScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(Pixel(game), new Vector2(x + bf - hw, y - a - hw), null, color, 0f, new Vector2(0, 0), pixelScale, SpriteEffects.None, 0f);


                    //            spriteBatch.Draw(Pixel(game), new Vector2(x + af, y + b), null, color, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0f);
                    //            spriteBatch.Draw(Pixel(game), new Vector2(x + bf, y + a), null, color, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0f);
                    //            spriteBatch.Draw(Pixel(game), new Vector2(x - af, y + b), null, color, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0f);
                    //            spriteBatch.Draw(Pixel(game), new Vector2(x - bf, y + a), null, color, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0f);

                    //            spriteBatch.Draw(Pixel(game), new Vector2(x - af, y - b), null, color, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0f);
                    //            spriteBatch.Draw(Pixel(game), new Vector2(x - bf, y - a), null, color, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0f);
                    //            spriteBatch.Draw(Pixel(game), new Vector2(x + af, y - b), null, color, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0f);
                    //            spriteBatch.Draw(Pixel(game), new Vector2(x + bf, y - a), null, color, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0f);

                    b += 1;
                }
                a -= 1;
            };

        }

        public static void FillCircle(Game game, SpriteBatch spriteBatch, float x, float y, float radius, Color color)
        {
            if (spriteBatch == null)
            {
                throw new ArgumentNullException(nameof(spriteBatch));
            }

            int target = 0;
            int a = (int)radius;
            int b = 0;
            int t;

            int r2 = a * a; // radius^2;
            while (a >= b)
            {

                b = (int)Math.Round(Math.Sqrt(r2 - a * a));

                // SWAP(target, b);
                t = target; target = b; b = t;

                while (b < target)
                {
                    int af = (100 * a) / 100;
                    int bf = (100 * b) / 100;

                    //IF Border<> No_Border THEN
                    //  BEGIN
                    //    VPutPixel(x+af, y + b, Border );
                    //VPutPixel(x + bf, y + a, Border);
                    //VPutPixel(x - af, y + b, Border);
                    //VPutPixel(x - bf, y + a, Border);
                    //VPutPixel(x - af, y - b, Border);
                    //VPutPixel(x - bf, y - a, Border);
                    //VPutPixel(x + af, y - b, Border);
                    //VPutPixel(x + bf, y - a, Border);
                    //END;


                    DrawHLine(game, spriteBatch, y - a, x - bf, x + bf, color);
                    DrawHLine(game, spriteBatch, y - b, x - af, x + af, color);
                    DrawHLine(game, spriteBatch, y + b, x - af, x + af, color);
                    DrawHLine(game, spriteBatch, y + a, x - bf, x + bf, color);

                    b += 1;
                }
                a -= 1;
            };

        }
        #endregion

        #region Ellipses
        //        {---------------------------------------------------------------------------}
        //    PROCEDURE VEllipse(X1, Y1, Xr, Yr : Integer);
        //    VAR X, Y : Integer;
        //    A,ASquared,TwoASquared : Longint;
        //    B,BSquared,TwoBSquared : Longint;
        //    D,Dx,Dy                : Longint;

        //    PROCEDURE DoFast;
        //    BEGIN
        //      While(Y > 0) DO  { Fast Slope }
        //IF(D< 0) THEN
        //BEGIN
        //              Inc(X);
        //VPutPixel(X1+X, Y1+Y, Colour);
        //VPutPixel(X1-X, Y1+Y, Colour);
        //VPutPixel(X1+X, Y1-Y, Colour);
        //VPutPixel(X1-X, Y1-Y, Colour);
        //Dx := Dx + TwoBSquared;
        //              D := D + Dx;
        //            END
        //          ELSE
        //            BEGIN
        //              Dec(Y);
        //VPutPixel(X1+X+1, Y1+Y, Colour);
        //VPutPixel(X1-X-1, Y1+Y, Colour);
        //VPutPixel(X1+X+1, Y1-Y, Colour);
        //VPutPixel(X1-X-1, Y1-Y, Colour);
        //Dy := Dy - TwoASquared;
        //              D := D + ASquared - Dy;
        //            END;
        //      END;

        //    PROCEDURE DoSlow;
        //BEGIN
        //  While(Dx<Dy )  DO       { While Curve is slow }
        //          IF(D > 0) THEN
        //           BEGIN
        //              Dec(Y);
        //Dy := Dy - TwoASquared;
        //              D := D - Dy;
        //              VPutPixel(X1+X+1, Y1+Y, Colour);
        //VPutPixel(X1-X-1, Y1+Y, Colour);
        //VPutPixel(X1+X+1, Y1-Y, Colour);
        //VPutPixel(X1-X-1, Y1-Y, Colour);
        //Inc(X);
        //Dx := Dx + TwoBSquared;
        //              D := D + BSquared + Dx;
        //            END
        //          ELSE
        //            BEGIN
        //              INC(X);
        //VPutPixel(X1+X, Y1+Y, Colour);
        //VPutPixel(X1-X, Y1+Y, Colour);
        //VPutPixel(X1+X, Y1-Y, Colour);
        //VPutPixel(X1-X, Y1-Y, Colour);
        //Dx := Dx + TwoBSquared;
        //              D := D + BSquared + Dx;
        //            END;
        //      END;

        //BEGIN
        //  X := 0;
        //  Y := Yr;
        //  IF(Xr<Yr) THEN
        // BEGIN
        //      A := Minor(Xr, Yr);
        //B := Major(Xr, Yr);
        //END;
        //  IF(XR >= Yr) THEN
        //   BEGIN
        //      A := Major(Xr, Yr);
        //B := Minor(Xr, Yr);
        //END;
        //  ASquared := A* A;
        //TwoASquared := 2 * ASquared;

        //BSquared := B* B;
        //TwoBSquared := 2 * BSquared;
        //DX := 0;
        //  DY := TwoASquared* B;

        //VPutPixel(X1, Y1+Y, Colour);
        //VPutPixel(X1, Y1-Y, Colour);
        //VPutPixel(X1+Xr+1, Y1, Colour);
        //VPutPixel(X1-Xr-1, Y1, Colour);

        //IF(XR<Yr) THEN
        //BEGIN
        //      D := Trunc(BSquared - ASquared* B + ASquared );
        //DoFast;
        //      D := Trunc(D + (3* (ASquared - BSquared) /2 - (Dx + Dy)) /2);
        //      DoSlow;
        //    END;
        //  IF(XR >= Yr) THEN
        //   BEGIN
        //      D := Trunc(BSquared - ASquared* B + ASquared );
        //DoSlow;
        //      D := Trunc(D + (3* (ASquared - BSquared) /2 - (Dx + Dy)) /2);
        //      DoFast;
        //    END;
        //END;

        //{---------------------------------------------------------------------------}
        //PROCEDURE VFillEllipse(X1, Y1, Xr, Yr : Integer);
        //VAR X, Y                    : Integer;
        //    A,ASquared,TwoASquared : Longint;
        //    B,BSquared,TwoBSquared : Longint;
        //    D,Dx,Dy                : Longint;
        //    Hold                   : Byte;

        //    PROCEDURE DoFast;
        //BEGIN
        //  While(Y > 0) DO  { Fast Slope }
        //          IF(D< 0) THEN
        //          BEGIN
        //              Inc(X);
        //              { FCol,FCol2,Texture); }
        //              FunLine(X1-X, Y1+Y, X1+X);
        //FunLine(X1-X, Y1-Y, X1+X);
        //Dx := Dx + TwoBSquared;
        //              D := D + Dx;
        //            END
        //          ELSE
        //            BEGIN
        //              Dec(Y);
        //FunLine(X1-X-1, Y1+Y, X1+X+1);
        //FunLine(X1-X-1, Y1-Y, X1+X+1);
        //Dy := Dy - TwoASquared;
        //              D := D + ASquared - Dy;
        //            END;
        //      END;

        //    PROCEDURE DoSlow;
        //BEGIN
        //  While(Dx<Dy )  DO       { While Curve is slow }
        //          IF(D > 0) THEN
        //           BEGIN
        //              Dec(Y);
        //Dy := Dy - TwoASquared;
        //              D := D - Dy;
        //              FunLine(X1-X-1, Y1+Y, X1+X+1);
        //FunLine(X1-X-1, Y1-Y, X1+X+1);
        //Inc(X);
        //Dx := Dx + TwoBSquared;
        //              D := D + BSquared + Dx;
        //            END
        //          ELSE
        //            BEGIN
        //              INC(X);
        //FunLine(X1-X-1, Y1+Y, X1+X+1);
        //FunLine(X1-X-1, Y1-Y, X1+X+1);
        //Dx := Dx + TwoBSquared;
        //              D := D + BSquared + Dx;
        //            END;
        //      END;
        //BEGIN
        //  X := 0;
        //  Y := Yr;
        //  IF(Xr<Yr) THEN
        // BEGIN
        //      A := Minor(Xr, Yr);
        //B := Major(Xr, Yr);
        //END;
        //  IF(XR >= Yr) THEN
        //   BEGIN
        //      A := Major(Xr, Yr);
        //B := Minor(Xr, Yr);
        //END;
        //  ASquared := A* A;
        //TwoASquared := 2 * ASquared;

        //BSquared := B* B;
        //TwoBSquared := 2 * BSquared;
        //DX := 0;
        //  DY := TwoASquared* B;

        //IF(XR<Yr) THEN
        //BEGIN
        //      D := Trunc(BSquared - ASquared* B + ASquared );
        //DoFast;
        //      D := Trunc(D + (3* (ASquared - BSquared) /2 - (Dx + Dy)) /2);
        //      DoSlow;
        //    END;
        //  IF(XR >= Yr) THEN
        //   BEGIN
        //      D := Trunc(BSquared - ASquared* B + ASquared );
        //DoSlow;
        //      D := Trunc(D + (3* (ASquared - BSquared) /2 - (Dx + Dy)) /2);
        //      DoFast;
        //    END;
        //  IF Border<> No_Border THEN
        //    BEGIN
        //      Hold := Colour;
        //Colour := Border;
        //      Border := Hold;
        //      VEllipse(X1, Y1, Xr, Yr);
        //Hold := Border;
        //      BorDer := Colour;
        //      Colour := hold;
        //    END;
        //END;
        #endregion

        #region Quads
        public static void DrawQuad(Game game, SpriteBatch spriteBatch, Quad2D quad, Color color, float lineWidth = 1f)
        {
            Primitives2D.DrawLine(game, spriteBatch, quad.TL, quad.TR, color, lineWidth);
            Primitives2D.DrawLine(game, spriteBatch, quad.TR, quad.BR, color, lineWidth);
            Primitives2D.DrawLine(game, spriteBatch, quad.BR, quad.BL, color, lineWidth);
            Primitives2D.DrawLine(game, spriteBatch, quad.BL, quad.TL, color, lineWidth);
        }
        #endregion

        #region Rounded Rectangles

        public static void DrawRoundedRect(Game game, SpriteBatch spriteBatch, Rectangle rectangle, float radius, Color color, float lineWidth = 1f)
        {
            DrawRoundedRect(game, spriteBatch, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom, radius, color, lineWidth);
        }

        public static void DrawRoundedRect(Game game, SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, float radius, Color color, float lineWidth = 1f)
        {
            float rotation = 0f;
            float width = x2 - x1;
            float height = y2 - y1;

            if (width == 0 || height == 0) return;
            if (width < 0)
            {
                float t1 = x1;
                x1 = x2;
                x2 = t1;
                width = -width;
            }

            if (height < 0)
            {
                height = -height;
            }

            // Draw Rounded Corners
            int target = 0;
            int a = (int)radius;
            int b = 0;
            int t;

            Vector2 pixelScale = new Vector2(lineWidth, lineWidth);
            float hw = lineWidth / 2f;

            float cx1 = x1 + radius;
            float cy1 = y1 + radius;
            float cx2 = x2 - radius;
            float cy2 = y2 - radius;

            int r2 = a * a; // radius^2;
            while (a >= b)
            {

                b = (int)Math.Round(Math.Sqrt(r2 - a * a));

                // SWAP(target, b);
                t = target; target = b; b = t;

                while (b < target)
                {
                    int af = (100 * a) / 100;
                    int bf = (100 * b) / 100;


                    //DrawHLine(game, spriteBatch, cy1 - b, cx1 - af, cx2 + af, color);
                    //DrawHLine(game, spriteBatch, cy1 - a, cx1 - bf, cx2 + bf, color);

                    //DrawHLine(game, spriteBatch, cy2 + b, cx1 - af, cx2 + af, color);
                    //DrawHLine(game, spriteBatch, cy2 + a, cx1 - bf, cx2 + bf, color);

                    // TL
                    spriteBatch.Draw(Pixel(game), new Vector2(cx1 - af - hw, cy1 - b - hw), null, color, 0f, new Vector2(0, 0), pixelScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(Pixel(game), new Vector2(cx1 - bf - hw, cy1 - a - hw), null, color, 0f, new Vector2(0, 0), pixelScale, SpriteEffects.None, 0f);

                    // TR
                    spriteBatch.Draw(Pixel(game), new Vector2(cx2 + af - hw, cy1 - b - hw), null, color, 0f, new Vector2(0, 0), pixelScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(Pixel(game), new Vector2(cx2 + bf - hw, cy1 - a - hw), null, color, 0f, new Vector2(0, 0), pixelScale, SpriteEffects.None, 0f);

                    // BR
                    spriteBatch.Draw(Pixel(game), new Vector2(cx2 + af - hw, cy2 + b - hw), null, color, 0f, new Vector2(0, 0), pixelScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(Pixel(game), new Vector2(cx2 + bf - hw, cy2 + a - hw), null, color, 0f, new Vector2(0, 0), pixelScale, SpriteEffects.None, 0f);

                    // BL
                    spriteBatch.Draw(Pixel(game), new Vector2(cx1 - af - hw, cy2 + b - hw), null, color, 0f, new Vector2(0, 0), pixelScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(Pixel(game), new Vector2(cx1 - bf - hw, cy2 + a - hw), null, color, 0f, new Vector2(0, 0), pixelScale, SpriteEffects.None, 0f);

                    b += 1;
                }
                a -= 1;
            }

            // Draw Middle Lines
            DrawHLine(game, spriteBatch, y1+1f, x1 + radius + hw, x2 - radius + hw, color, lineWidth);
            DrawHLine(game, spriteBatch, y2-1f, x1 + radius + hw, x2 - radius + hw , color, lineWidth);

            DrawVLine(game, spriteBatch, x1+1f, y1 + radius + hw, y2 - radius + hw, color, lineWidth);
            DrawVLine(game, spriteBatch, x2-1f, y1 + radius + hw, y2 - radius + hw, color, lineWidth);

        }

        public static void FillRoundedRect(Game game, SpriteBatch spriteBatch, Rectangle rectangle, float radius, Color color)
        {
            FillRoundedRect(game, spriteBatch, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom, radius, color);
        }

        public static void FillRoundedRect(Game game, SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, float radius, Color color)
        {
            float rotation = 0f;
            float width = x2 - x1;
            float height = y2 - y1;

            if (width == 0 || height == 0) return;
            if (width < 0)
            {
                float t1 = x1;
                x1 = x2;
                x2 = t1;
                width = -width;
            }

            if (height < 0)
            {
                height = -height;
            }

            // Draw Rounded Corners
            int target = 0;
            int a = (int)radius;
            int b = 0;
            int t;


            int r2 = a * a; // radius^2;
            while (a >= b)
            {

                b = (int)Math.Round(Math.Sqrt(r2 - a * a));

                // SWAP(target, b);
                t = target; target = b; b = t;

                while (b < target)
                {
                    int af = (100 * a) / 100;
                    int bf = (100 * b) / 100;

                    Color color2 = Color.Red;

                    float cx1 = x1 + radius;
                    float cy1 = y1 + radius;
                    float cx2 = x2 - radius;
                    float cy2 = y2 - radius;

                    // Draw Top
                    DrawHLine(game, spriteBatch, cy1 - b, cx1 - af - 1f, cx2 + af, color);
                    DrawHLine(game, spriteBatch, cy1 - a, cx1 - bf - 1f, cx2 + bf, color);

                    // Draw Bottom
                    DrawHLine(game, spriteBatch, cy2 + b, cx1 - af - 1f, cx2 + af, color);
                    DrawHLine(game, spriteBatch, cy2 + a, cx1 - bf - 1f, cx2 + bf, color);

                    b += 1;
                }
                a -= 1;
            }

            // Draw Middle Rectangle
            spriteBatch.Draw(Pixel(game), new Vector2(x1, y1 + radius), null, color, rotation, new Vector2(0, 0), new Vector2(width-1f, height - radius * 2f), SpriteEffects.None, 0f);
        }

        #endregion
    }
}
