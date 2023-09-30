//using System;
//using System.Collections.Generic;
//using System.Text;
//using Blade.Games;
//using Blade.Games.UI;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;

//namespace Blade.UI
//{
//    public class UIGameEntity : GameEntity
//    {
//        public static UIGameEntity Instance { get; set; } = new UIGameEntity();

//        private UIGameEntity()
//        {

//        }

//        public override void LoadContent()
//        {
//            UIManager.LoadResources();
//        }

//        public override void Logic(GameTime gameTime)
//        {
//            UIManager.Logic(gameTime);
//        }

//        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
//        {
//            UIManager.Render(spriteBatch.GraphicsDevice, gameTime);
//        }

//        public override void OnCollison(GameEntity gameEntity)
//        {

//        }
//    }
//}
