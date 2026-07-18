using Blade.MG.UI.Controls;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeUI.UnitTesting.Tests.Controls
{
    [TestClass]
    public class TestPopupReshow
    {
        // Popup.ShowAt/Close resolve UIManager via Game.Services - FakeGame.Instance is a
        // singleton shared across the whole test run, so each test must (re-)register its own
        // FakeUIManager rather than relying on one left over from a previous test (mirrors
        // TestTooltip.cs's RegisterUIManager).
        private static void RegisterUIManager(Blade.MG.UI.UIManager uiManager)
        {
            if (FakeGame.Instance.Services.GetService(typeof(Blade.MG.UI.UIManager)) != null)
            {
                FakeGame.Instance.Services.RemoveService(typeof(Blade.MG.UI.UIManager));
            }

            FakeGame.Instance.Services.AddService(typeof(Blade.MG.UI.UIManager), uiManager);
        }

        [TestMethod]
        public void ShowingClosingThenReshowing_DoesNotThrow()
        {
            var uiManager = new FakeUIManager();
            RegisterUIManager(uiManager);
            var game = FakeGame.Instance;

            using var spriteBatch = new SpriteBatch(game.GraphicsDevice);
            using var renderTarget = new RenderTarget2D(game.GraphicsDevice, 800, 600);
            var gameTime = new GameTime(System.TimeSpan.FromMilliseconds(16), System.TimeSpan.FromMilliseconds(16));

            var popup = new Popup();

            popup.ShowAt(game, new Point(10, 10));
            // Mirrors HelpPage_Popup.cs: Content is only ever assigned on the very first show,
            // matching a lazy-populate-once pattern.
            popup.Content = new Label { Text = "Hello" };
            uiManager.PerformLayout();
            uiManager.Draw(spriteBatch, gameTime, renderTarget);

            popup.Close(game);
            uiManager.PerformLayout();

            popup.ShowAt(game, new Point(20, 20));
            uiManager.PerformLayout();
            uiManager.Draw(spriteBatch, gameTime, renderTarget);
        }
    }
}
