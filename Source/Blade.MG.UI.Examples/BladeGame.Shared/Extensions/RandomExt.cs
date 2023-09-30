using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class RandomExt
    {
        public static float NextFloat(this Random rnd)
        {
            return (float)rnd.NextDouble();
        }
    }
}
