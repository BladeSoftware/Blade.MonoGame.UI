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

        private bool isInitialized = false;

        public override void Initialize(Game game)
        {
            base.Initialize(game);

            uiManager = game.Services.GetService<UIManager>();

            if (uiManager == null)
            {
                throw new InvalidOperationException("UIManager service not found. Make sure UIManager is added to the Game Services.");
            }

            isInitialized = true;
        }

        public void Show()
        {
            if (uiManager == null) { }

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

            if (uiManager == null) { } 
                
            uiManager.Add(this);

            //await Task.Run(async () =>
            //{
            //    await asyncManualResetEvent.WaitAsync();
            //}).ConfigureAwait(true);

            await asyncManualResetEvent.WaitAsync();
        }

        public void CloseModal()
        {
            uiManager = uiManager ?? Game.Services.GetService<UIManager>();
            uiManager.Remove(this);
        }

        public void ReturnAsyncResult()
        {
            asyncManualResetEvent.Set();
            //manualResetEventSlim.Set();
        }

    }
}
