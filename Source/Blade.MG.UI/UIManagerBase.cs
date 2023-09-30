using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blade.UI
{
    public abstract class UIManagerBase
    {
        public abstract void LoadContent();
        public abstract Task UpdateAsync(GameTime gameTime);
        public abstract void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, GameTime gameTime);


    }
}
