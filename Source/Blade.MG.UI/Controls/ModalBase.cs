﻿using Microsoft.VisualStudio.Threading;
using Microsoft.Xna.Framework;

namespace Blade.UI.Controls
{
    public class ModalBase : UIWindow
    {
        private AsyncManualResetEvent asyncManualResetEvent = new AsyncManualResetEvent(false);
        //private ManualResetEventSlim manualResetEventSlim = new ManualResetEventSlim(false);


        public void Show(Game game)
        {
            UIManager.Add(this, game);
        }

        //public async Task ShowAsync(Game game)
        //{
        //    manualResetEventSlim.Reset();

        //    UIManager.Add(this, game);

        //    manualResetEventSlim.Wait();
        //}

        public async Task ShowAsync(Game game)
        {
            asyncManualResetEvent.Reset();

            UIManager.Add(this, game);

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