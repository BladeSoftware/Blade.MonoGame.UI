//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Assets.Scripts.BladeUI.Components;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;

//namespace BladeGame.BladeUI.Controls
//{
//    public class Rectangle : BladeControl
//    {
//        //// Texture Cache contains 1x1 Texture2D's of different colors, indexed by Color string e.g. #CCAA77FF
//        //private static ConcurrentDictionary<String, Texture2D> textureCache = new ConcurrentDictionary<string, Texture2D>();

//        private Texture2D background;
//        public Texture2D Background { get { return background; } set { background = value; } }

//        public void SetBackground(Color color)
//        {
//            if (Background != null)
//            {
//                // TODO: How to deallocate a texture ?
//                Background = null;
//            }

//            // USe 1x1 pixel White
//            //string colorStr = color.ToString(); // Format might be different...

//            //if (!textureCache.TryGetValue(colorStr, out background))
//            //{
//            //    // Create a 1x1 texture using the colour
//            //    Background = new Texture2D(1, 1);
//            //    Background.SetPixel(0, 0, color);
//            //    Background.wrapMode = TextureWrapMode.Repeat;
//            //    Background.Apply();


//            //}

//        }

//        public override void RenderControl(BladeRect layoutBounds)
//        {
//            //Debug.Log(Name);

//            if (Background == null)
//            {
//                return;
//            }

//            //var rect = new Rect(0, 0, ActualWidth, ActualHeight);
//            //var controlRect = new Rect(layoutBounds.Left, layoutBounds.Top, layoutBounds.Width, layoutBounds.Height);
//            var controlRect = new Rect(Left, Top, ActualWidth, ActualHeight);

//            GUIStyle rectStyle = new GUIStyle(GUI.skin.box);
//            rectStyle.normal.background = Background;
//            rectStyle.border = new RectOffset(25, 25, 25, 25);


//            // Draw a rectangle using the 1x1 texture we created
//            GUI.Box(layoutBounds.ToRect(), GUIContent.none, rectStyle);
//            //GUI.DrawTexture(controlRect, Background);
//        }
//    }
//}
