using Blade.MG.UI;
using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks;

namespace BladeUI.UnitTesting.Fakes
{
    internal class FakeUIManager : UIManager
    {
        private static TimeSpan oneFrameTimeSpan = TimeSpan.FromMilliseconds(1.0 / 60.0); // 1/60th of a second for 60 FPS
        private static GameTime oneFrameGameTime = new GameTime(oneFrameTimeSpan, oneFrameTimeSpan);


        public FakeUIManager() : base(FakeGame.Instance)
        {
        }

        public Task PerformLayout()
        {
            Update(oneFrameGameTime);
            return Task.CompletedTask;
        }

        public void AddUI(UIWindow ui)
        {
            Add(ui);
        }

        public void ClearUI()
        {
            Clear();
        }


    }
}
