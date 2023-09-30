using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BladeGame.BladeUI.Components
{
    public class ColumnDefinition
    {
        public GridLength Width { get; set; }
        public float MaxWidth { get; set; }
        public float MinWidth { get; set; }

        public float ActualWidth { get; set; }

        public ColumnDefinition()
        {

        }

    }
}
