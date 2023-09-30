using Blade.UI;
using Microsoft.Xna.Framework;

namespace BladeUI.UnitTesting
{
    public class EmptyUI : UIWindow
    {

        public override void Initialize(Game game)
        {
            base.Initialize(game);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            BuildScreen(base.Game);
        }

        public void BuildScreen(Game game)
        {


        }


        public override void PerformLayout(GameTime gameTime)
        {
            base.PerformLayout(gameTime);
        }

    }
}
