using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BladeGame.BladeUI.Components
{
    public struct GridLength
    {
        public GridUnitType GridUnitType;
        public float Value;

        public bool IsAbsolute
        {
            get { return GridUnitType == GridUnitType.Pixel; }
        }

        public bool IsAuto
        {
            get { return GridUnitType == GridUnitType.Auto; }
        }

        public bool IsStar
        {
            get { return GridUnitType == GridUnitType.Star; }
        }

        public GridLength(GridUnitType gridUnitType, float value = 0f)
        {
            GridUnitType = gridUnitType;
            Value = value;
        }
    }
}
