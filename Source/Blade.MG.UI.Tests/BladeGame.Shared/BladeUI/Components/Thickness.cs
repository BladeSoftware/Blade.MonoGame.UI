using System;
using System.Collections.Generic;
using System.Text;

namespace BladeGame.Shared.BladeUI.Components
{
    public struct Thickness
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;

        public Thickness(int uniformLength)
        {
            Left = Right = Top = Bottom = uniformLength;
        }

        public Thickness(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public Thickness(int leftRight, int topBottom)
        {
            Left = leftRight;
            Top = topBottom;
            Right = leftRight;
            Bottom = topBottom;
        }
    }
}
