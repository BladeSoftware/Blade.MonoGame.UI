using System;

namespace GameStudio.DesktopGL
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new GameStudioGame())
                game.Run();
        }
    }
}
