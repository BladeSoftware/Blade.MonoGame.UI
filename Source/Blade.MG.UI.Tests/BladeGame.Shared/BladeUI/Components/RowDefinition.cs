using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BladeGame.BladeUI.Components
{
    public class RowDefinition
    {
        public float ActualHeight { get; set; }
        public GridLength Height { get; set; }
        public float MaxHeight { get; set; }
        public float MinHeight { get; set; }

        public RowDefinition()
        {

        }

    }
}
