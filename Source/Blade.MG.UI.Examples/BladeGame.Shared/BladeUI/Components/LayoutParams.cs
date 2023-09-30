using System;
using System.Collections.Generic;
using System.Text;

namespace BladeGame.BladeUI.Components
{
    public struct Layout
    {
        public float MinWidth;
        public float MinHeight;
        public float MaxWidth;
        public float MaxHeight;

        public Layout(float minWidth, float minHeight, float maxWidth, float maxHeight)
        {
            MinWidth = minWidth;
            MinHeight = minHeight;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
        }

        public void Merge(float minWidth, float minHeight, float maxWidth, float maxHeight)
        {
            MinWidth = !float.IsNaN(minWidth) ? minWidth : MinWidth;
            MinHeight = !float.IsNaN(minHeight) ? minHeight : MinHeight;
            MaxWidth = !float.IsNaN(maxWidth) ? maxWidth : MaxWidth;
            MaxHeight = !float.IsNaN(maxHeight) ? maxHeight : MaxHeight;
        }

    }
}
