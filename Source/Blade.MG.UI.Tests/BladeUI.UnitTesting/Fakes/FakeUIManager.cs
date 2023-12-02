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


        public FakeUIManager()
        {
            UIManager.Instance = this;
            UIManager.Instance.Initialize(FakeGame.Instance);
        }

        public async Task PerformLayout()
        {
            await UpdateAsync(oneFrameGameTime).ConfigureAwait(true);
        }

        public void AddUI(UIWindow ui)
        {
            Add(ui);
        }

        public void ClearUI()
        {
            UIManager.Clear();
            //for (int i = UI.Count - 1; i >= 0; i--)
            //{
            //    var window = UI?.ElementAtOrDefault(i);
            //    UI?.RemoveAt(i);
            //    UnloadWindow(window);
            //}
        }


    }
}
