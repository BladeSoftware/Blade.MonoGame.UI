using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BladeGame.BladeUI.Components
{
    internal class UIHelper
    {
        internal static float Clamp(float value, float min, float max)
        {
            if (!double.IsNaN(min) && (value < min))
            {
                value = min;
            }
            if (!double.IsNaN(max) && (value > max))
            {
                value = max;
            }

            return value;
        }

    }
}
