using Microsoft.VisualStudio.Threading;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    public class ModalBase : UIWindow
    {
        // Services
        private UIManager uiManager;

        private AsyncManualResetEvent asyncManualResetEvent = new AsyncManualResetEvent(false);
        //private ManualResetEventSlim manualResetEventSlim = new ManualResetEventSlim(false);

        public override void Initialize(Game game)
        {
            base.Initialize(game);

            uiManager = game.Services.GetService<UIManager>();
        }

        public void Show()
        {
            uiManager.Add(this);
        }

        //public async Task ShowAsync(Game game)
        //{
        //    manualResetEventSlim.Reset();

        //    UIManager.Add(this, game);

        //    manualResetEventSlim.Wait();
        //}

        public async Task ShowAsync()
        {
            asyncManualResetEvent.Reset();

            uiManager.Add(this);

            //await Task.Run(async () =>
            //{
            //    await asyncManualResetEvent.WaitAsync();
            //}).ConfigureAwait(true);

            await asyncManualResetEvent.WaitAsync();
        }

        public void CloseModal()
        {
            uiManager.Remove(this);
        }

        public void ReturnAsyncResult()
        {
            asyncManualResetEvent.Set();
            //manualResetEventSlim.Set();
        }

    }
}
