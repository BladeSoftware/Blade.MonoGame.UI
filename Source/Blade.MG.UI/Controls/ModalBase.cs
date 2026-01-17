using Blade.MG.UI.Components;
using Blade.MG.UI.Models;
using Microsoft.VisualStudio.Threading;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    public class ModalBase : UIWindow
    {
        // Services
        private UIManager uiManager;

        private AsyncManualResetEvent asyncManualResetEvent = new AsyncManualResetEvent(false);

        private bool isInitialized = false;

        //public override void Initialize(Game game)
        //{
        //    base.Initialize(game);

        //    uiManager = game.Services.GetService<UIManager>();

        //    if (uiManager == null)
        //    {
        //        throw new InvalidOperationException("UIManager service not found. Make sure UIManager is added to the Game Services.");
        //    }

        //    isInitialized = true;
        //}

        /// <summary>
        /// Shows the modal synchronously (non-blocking, does not wait for result)
        /// </summary>
        public void Show(Game game)
        {
            uiManager ??= game.Services.GetService<UIManager>();

            if (uiManager == null)
            {
                throw new InvalidOperationException("UIManager service not found. Make sure UIManager is added to the Game Services.");
            }

            uiManager.Add(this);

            this.Visible = Visibility.Visible;
        }

        //public async Task ShowAsync(Game game)
        //{
        //    manualResetEventSlim.Reset();

        //    uiManager = uiManager ?? game.Services.GetService<UIManager>();
        //    uiManager.Add(this);

        //    manualResetEventSlim.Wait();
        //}

        /// <summary>
        /// Shows the modal asynchronously and waits for user interaction to complete.
        /// The game loop continues to run while waiting.
        /// </summary>
        public async Task ShowAsync(Game game)
        {
            asyncManualResetEvent.Reset();

            uiManager ??= game.Services.GetService<UIManager>();

            if (uiManager == null)
            {
                throw new InvalidOperationException("UIManager service not found. Make sure UIManager is added to the Game Services.");
            }

            uiManager.Add(this);
            this.Visible = Visibility.Visible;

            //await Task.Run(async () =>
            //{
            //    await asyncManualResetEvent.WaitAsync();
            //}).ConfigureAwait(false);

            // Wait for the modal to be closed
            // Game loop continues while waiting here
            await asyncManualResetEvent.WaitAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Shows the modal with a timeout
        /// </summary>
        public async Task<bool> ShowAsync(Game game, TimeSpan timeout)
        {
            asyncManualResetEvent.Reset();

            uiManager ??= game.Services.GetService<UIManager>();

            if (uiManager == null)
            {
                throw new InvalidOperationException("UIManager service not found.");
            }

            uiManager.Add(this);
            this.Visible = Visibility.Visible;

            // Returns true if completed, false if timed out
            var cancellationToken = new CancellationTokenSource(timeout);
            await asyncManualResetEvent.WaitAsync(cancellationToken.Token).ConfigureAwait(false);

            return !cancellationToken.Token.IsCancellationRequested;
        }

        /// <summary>
        /// Closes the modal and removes it from the UI manager
        /// </summary>
        public void CloseModal()
        {
            uiManager ??= Game.Services.GetService<UIManager>();
            uiManager.Remove(this);

            asyncManualResetEvent.Set();
        }

        public void ReturnAsyncResult()
        {
            //asyncManualResetEvent.Set();
        }

    }
}
