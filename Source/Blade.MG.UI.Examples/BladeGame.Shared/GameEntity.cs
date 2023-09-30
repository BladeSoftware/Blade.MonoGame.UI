using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BladeGame
{
    public abstract class GameEntity : IDisposable
    {
        //public static readonly ConcurrentSet<String> Keys = new ConcurrentSet<string>();
        protected GameBase Game;

        public GameEntity(GameBase game)
        {
            this.Game = game;
        }

        public virtual void Initialize()
        {
        }

        public virtual void LoadResources()
        {
        }

        public virtual void Logic(GameTime gameTime)
        {
        }

        public virtual void Render(SpriteBatch spriteBatch, GameTime gameTime)
        {
        }

        public virtual void OnCollison(GameEntity gameEntity)
        {
        }

        public virtual void Dispose()
        {

        }
    }
}
