using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace BladeGame
{
    /// <summary>
    /// A value of Fill will cause your image to stretch to completely fill the output area. 
    /// When the output area and the image have different aspect ratios, the image is distorted by this stretching. 
    /// 
    /// A value of Uniform will attempt to fill the output area, while preserving the aspect ratio of the image.
    /// There will be blank areas on the top/bottom or left/right if the image doesn't have the same aspect 
    /// ratio as the output area.
    /// 
    /// A value of UniformToFill will fill the output area, while preserving the apsect ration of the image.
    /// Parts of the image on the top/bottom or left/right will be clipped if the image doesn't have the same
    /// aspect ration as the output area.
    /// </summary>
    public enum BlitStretch
    {
        None,
        Fill,
        Uniform,
        UniformToFill
    }

    public static class GameHelper
    {


        //        /// <summary>
        //        /// e.g. 
        //        /// using System.Reflection;
        //        /// 
        //        /// Assembly assembly = GetType().GetTypeInfo().Assembly;
        //        /// string resourceID = "Asteroids.Media.Images.Background.png";
        //        /// SKBitmap background = LoadBitmapFromResource(assembly, resourceID)
        //        /// </summary>
        //        /// <param name="assembly"></param>
        //        /// <param name="resourceID"></param>
        //        /// <returns></returns>
        //        public static SKBitmap LoadBitmapFromResource(Assembly assembly, string resourceID)
        //        {
        //            //string resourceID = "Asteroids.Media.Images.Asteroid1.png";
        //            //Assembly assembly = GetType().GetTypeInfo().Assembly;

        //            using (Stream stream = assembly.GetManifestResourceStream(resourceID))
        //            using (SKManagedStream skStream = new SKManagedStream(stream))
        //            {
        //                return SKBitmap.Decode(skStream);
        //            }

        //        }

        //        /// <summary>
        //        /// https://developer.xamarin.com/guides/xamarin-forms/advanced/skiasharp/basics/bitmaps/
        //        /// </summary>
        //        /// <param name="uri"></param>
        //        /// <returns></returns>
        //        public static async Task<SKBitmap> LoadBitmapFromUri(Uri uri)
        //        {
        //            //Uri uri = new Uri("http://developer.xamarin.com/demo/IMG_3256.JPG?width=480");

        //            WebRequest request = WebRequest.Create(uri);
        //            WebResponse response = await request.GetResponseAsync();

        //            SKBitmap webBitmap = null;

        //            try
        //            {
        //                using (Stream stream = response.GetResponseStream())
        //                using (MemoryStream memStream = new MemoryStream())
        //                {
        //                    stream.CopyTo(memStream);
        //                    memStream.Seek(0, SeekOrigin.Begin);

        //                    using (SKManagedStream skStream = new SKManagedStream(memStream))
        //                    {
        //                        webBitmap = SKBitmap.Decode(skStream);
        //                    }
        //                }
        //            }
        //            catch
        //            {
        //                // TODO:...
        //            }

        //            return webBitmap;


        //            //request.BeginGetResponse((IAsyncResult arg) =>
        //            //{
        //            //    SKBitmap webBitmap = null;

        //            //    try
        //            //    {
        //            //        using (Stream stream = request.EndGetResponse(arg).GetResponseStream())
        //            //        using (MemoryStream memStream = new MemoryStream())
        //            //        {
        //            //            stream.CopyTo(memStream);
        //            //            memStream.Seek(0, SeekOrigin.Begin);

        //            //            using (SKManagedStream skStream = new SKManagedStream(memStream))
        //            //            {
        //            //                webBitmap = SKBitmap.Decode(skStream);
        //            //            }
        //            //        }
        //            //    }
        //            //    catch
        //            //    {
        //            //        // TODO:...
        //            //    }

        //            //    return webBitmap;

        //            //    //Device.BeginInvokeOnMainThread(() => canvasView.InvalidateSurface());

        //            //}, null);
        //        }


        //        /// <summary>
        //        /// Draw a bitmap with various scaling options
        //        /// </summary>
        //        /// <param name="skCanvas"></param>
        //        /// <param name="bitmap"></param>
        //        /// <param name="dest"></param>
        //        /// <param name="stretch"></param>
        //        public static void DrawBitmap(SKCanvas skCanvas, SKBitmap bitmap, SKRect dest, BlitStretch stretch)
        //        {
        //            GameHelper.DrawBitmap(skCanvas, bitmap, dest, null, stretch);
        //        }

        //        /// <summary>
        //        /// Draw a bitmap with various scaling options
        //        /// </summary>
        //        /// <param name="skCanvas"></param>
        //        /// <param name="bitmap"></param>
        //        /// <param name="dest"></param>
        //        /// <param name="paint"></param>
        //        /// <param name="stretch"></param>
        //        public static void DrawBitmap(SKCanvas skCanvas, SKBitmap bitmap, SKRect dest, SKPaint paint, BlitStretch stretch)
        //        {
        //            float srcAspect = (float)bitmap.Height / (float)bitmap.Width;
        //            float dstAspect = (float)dest.Height / (float)dest.Width;

        //            float scaleX = dest.Width / (float)bitmap.Width;
        //            float scaleY = dest.Height / (float)bitmap.Height;


        //            // Scale 1 - Scale the Width to fill the destination, then calculate Height using original Aspect Ration
        //            float size1Width = (float)bitmap.Width * scaleX;
        //            float size1Height = size1Width * srcAspect;

        //            // Scale 2 - Scale the Height to fill the destination, then calculate Width using original Aspect Ration
        //            float size2Height = (float)bitmap.Height * scaleY;
        //            float size2Width = size2Height / srcAspect;


        //            SKRect source;

        //            if (stretch == BlitStretch.Fill)
        //            {
        //                source = SKRect.Create(bitmap.Width, bitmap.Height);
        //            }
        //            else if (stretch == BlitStretch.UniformToFill)
        //            {
        //                if (size1Height < dest.Height)
        //                {
        //                    float srcWidth = (float)bitmap.Height / dstAspect;
        //                    float halfDW = ((float)bitmap.Width - srcWidth) / 2f;

        //                    source = SKRect.Create(halfDW, 0, srcWidth, bitmap.Height);
        //                }
        //                else
        //                {
        //                    float srcHeight = (float)bitmap.Width * dstAspect;
        //                    float halfDH = ((float)bitmap.Height - srcHeight) / 2f;

        //                    source = SKRect.Create(0, halfDH, bitmap.Width, srcHeight);
        //                }

        //                dest.Inflate(1f,1f);
        //            }
        //            else //if (stretch == BlitStretch.Uniform)
        //            {
        //                source = SKRect.Create(bitmap.Width, bitmap.Height);

        //                if (size1Height > dest.Height)
        //                {
        //                    // Calculate Half the Delta Width
        //                    float halfDW = (dest.Width - size2Width) / 2f;
        //                    dest = SKRect.Create(dest.Left + halfDW, dest.Top, size2Width, dest.Bottom);
        //                }
        //                else
        //                {
        //                    // Calculate Half the Delta Height
        //                    float halfDH = (dest.Height - size1Height) / 2f;
        //                    dest = SKRect.Create(dest.Left, dest.Top + halfDH, dest.Right, size1Height);
        //                }

        //                dest.Inflate(1f, 1f);
        //            }

        //            skCanvas.DrawBitmap(bitmap, source, dest, paint);
        //        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        public static void DecomposeMatrix2D(ref Matrix matrix, out Vector2 position, out float rotation, out Vector2 scale)
        {
            Vector3 position3, scale3;
            Quaternion rotationQ;

            matrix.Decompose(out scale3, out rotationQ, out position3);

            Vector2 direction = Vector2.Transform(Vector2.UnitX, rotationQ);
            rotation = (float)Math.Atan2((double)(direction.Y), (double)(direction.X));
            position = new Vector2(position3.X, position3.Y);
            scale = new Vector2(scale3.X, scale3.Y);
        }

        /// <summary>
        /// Given a Source and Destination Rectangle and a Scaling option, returns the Source and Destination Clip Rectangles that
        /// should be used to achive the desired Scaling.
        /// </summary>
        public static void GetStrechRects(Rectangle srcRect, Rectangle dstRect, BlitStretch stretch, out Rectangle srcClipRect, out Rectangle dstClipRect)
        {
            float srcAspect = (float)srcRect.Height / (float)srcRect.Width;
            float dstAspect = (float)dstRect.Height / (float)dstRect.Width;

            float scaleX = dstRect.Width / (float)srcRect.Width;
            float scaleY = dstRect.Height / (float)srcRect.Height;


            // Scale 1 - Scale the Width to fill the destination, then calculate Height using original Aspect Ratio
            float size1Width = (float)srcRect.Width * scaleX;
            float size1Height = size1Width * srcAspect;

            // Scale 2 - Scale the Height to fill the destination, then calculate Width using original Aspect Ration
            float size2Height = (float)srcRect.Height * scaleY;
            float size2Width = size2Height / srcAspect;

            if (stretch == BlitStretch.None)
            {
                srcClipRect = srcRect;
                dstClipRect = new Rectangle(dstRect.Left, dstRect.Top, srcRect.Width, srcRect.Height);
            }
            else if (stretch == BlitStretch.Fill)
            {
                srcClipRect = srcRect;
                dstClipRect = dstRect;
            }
            else if (stretch == BlitStretch.UniformToFill)
            {
                if (size1Height < dstRect.Height)
                {
                    float srcWidth = (float)srcRect.Height / dstAspect;
                    float halfDW = ((float)srcRect.Width - srcWidth) / 2f;

                    srcClipRect = new Rectangle(srcRect.Left + (int)halfDW, srcRect.Top, (int)srcWidth, srcRect.Height);
                }
                else
                {
                    float srcHeight = (float)srcRect.Width * dstAspect;
                    float halfDH = ((float)srcRect.Height - srcHeight) / 2f;

                    srcClipRect = new Rectangle(srcRect.Left, srcRect.Top + (int)halfDH, (int)srcRect.Width, (int)srcHeight);
                }

                dstClipRect = dstRect;
            }
            else //if (stretch == BlitStretch.Uniform)
            {
                srcClipRect = srcRect;

                if (size1Height > dstRect.Height)
                {
                    // Calculate Half the Delta Width
                    float halfDW = (dstRect.Width - size2Width) / 2f;
                    dstClipRect = new Rectangle(dstRect.Left + (int)halfDW, dstRect.Top, (int)size2Width, dstRect.Height);
                }
                else
                {
                    // Calculate Half the Delta Height
                    float halfDH = (dstRect.Height - size1Height) / 2f;
                    dstClipRect = new Rectangle(dstRect.Left, dstRect.Top + (int)halfDH, dstRect.Width, (int)size1Height);
                }

            }
        }

    }
}
