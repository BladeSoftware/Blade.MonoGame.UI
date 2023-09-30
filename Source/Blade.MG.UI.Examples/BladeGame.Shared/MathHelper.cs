//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace BladeGame
//{
//    public static class SKMathHelper
//    {
//        public const float PI = 3.14159265358979323846f;

//        /// <summary>
//        /// Returns a new vector perpendicular to the 2D vector
//        /// </summary>
//        /// <param name="v1"></param>
//        /// <returns></returns>
//        public static SKPoint CalcPerpendicular(SKPoint v1)
//        {
//            return new SKPoint(v1.Y, -v1.X);
//        }

//        /// <summary>
//        /// Returns the Dot / Scalar product of the two vectors
//        /// </summary>
//        /// <param name="v1"></param>
//        /// <param name="v2"></param>
//        /// <returns></returns>
//        public static float Dot(SKPoint v1, SKPoint v2)
//        {
//            return v1.X * v2.X + v1.Y * v2.Y;
//        }

//        /// <summary>
//        /// Normalize the Vector
//        /// </summary>
//        /// <param name="v"></param>
//        /// <returns></returns>
//        public static SKPoint Normalize(SKPoint v)
//        {
//            float magnitude = (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
//            return new SKPoint(v.X / magnitude, v.Y / magnitude);
//        }

//        /// <summary>
//        /// Returns the Magnitude / Length of the Vector
//        /// </summary>
//        /// <param name="v"></param>
//        /// <returns></returns>
//        public static float Magnitude(SKPoint v)
//        {
//            return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
//        }

//        /// <summary>
//        /// Converts Degrees to Radians
//        /// </summary>
//        /// <param name="deg"></param>
//        /// <returns></returns>
//        public static float ToRad(float deg)
//        {
//            return deg / (180f / PI);
//        }

//        /// <summary>
//        /// Converts Radians to Degrees
//        /// </summary>
//        /// <param name="rad"></param>
//        /// <returns></returns>
//        public static float ToDeg(float rad)
//        {
//            return rad / (PI / 180f);
//        }
//    }
//}
