using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeGame
{
    public class GameInfo
    {
        //public float FrameElapsedTime { get; set; }

        public int LogicFPS { get; set; }
        public int RenderFPS { get; set; }

        public volatile int Width;
        public volatile int Height;

        
        /// <summary>
        /// Gets a rectangle with the current width and height.
        /// </summary>
        public Rectangle Rect { get { return new Rectangle(0, 0, Width - 1, Height - 1); } }
    }
}
