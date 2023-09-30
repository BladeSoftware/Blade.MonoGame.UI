using System;
using System.Collections.Generic;
using System.Text;

namespace BladeGame
{
    public static class GameServices
    {
        internal static Random Rnd = new Random((int)DateTime.Now.Ticks);

        internal static GameInfo GameInfo { get; set; } = new GameInfo();

    }
}
