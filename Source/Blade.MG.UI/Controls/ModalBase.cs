using Microsoft.VisualStudio.Threading;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    public class ModalBase : UIWindow
    {
        private AsyncManualResetEvent asyncManualResetEvent = new AsyncManualResetEvent(false);
        //private ManualResetEventSlim manualResetEventSlim = new ManualResetEventSlim(false);


        public void Show()
        {
            UIManager.Add(this);
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

            UIManager.Add(this);

            //await Task.Run(async () =>
            //{
            //    await asyncManualResetEvent.WaitAsync();
            //}).ConfigureAwait(true);

            await asyncManualResetEvent.WaitAsync();
        }

        public void CloseModal()
        {
            UIManager.Remove(this);
        }

        public void ReturnAsyncResult()
        {
            asyncManualResetEvent.Set();
            //manualResetEventSlim.Set();
        }

    }
}
