using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BladeGame.BladeUI.Components
{
    public struct Size
    {
        public float Width;
        public float Height;

        public Size(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public override string ToString()
        {
            return "W: " + Width + ", H:" + Height;
        }
    }
}
