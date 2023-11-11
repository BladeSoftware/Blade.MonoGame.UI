using Blade.MG.Input;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI
{
    public partial class UIManager //: GameEntity
    {
        private async Task HandleGamePadInputAsync(UIWindow eventLockedWindow, UIComponent eventLockedControl, bool propagateEvents, GameTime gameTime)
        {
            //// Handle Game Pad input
            //lastGamePadState = gamePadState;
            //gamePadState = GamePad.GetState(PlayerIndex.One);

            if (!propagateEvents)
            {
                return;
            }

            if (InputManager.GamePadStatePlayer1.IsConnected)
            {
                // ...
            }

            await Task.CompletedTask;
        }

    }
}
