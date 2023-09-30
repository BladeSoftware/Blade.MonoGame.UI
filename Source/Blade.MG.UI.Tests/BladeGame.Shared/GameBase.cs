using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BladeGame
{
    public abstract class GameBase : Game
    {
        protected ConcurrentSet<GameEntity> GameEntities = new ConcurrentSet<GameEntity>();

        public Viewport Viewport => GraphicsDevice.Viewport;
        public int DisplayWidth => GraphicsDevice.DisplayMode.Width;
        public int DisplayHeight => GraphicsDevice.DisplayMode.Height;
        public Rectangle TitleSafeArea => GraphicsDevice.DisplayMode.TitleSafeArea;

        public KeyboardState KeyboardState;
        public KeyboardState LastKeyboardState;

        //public abstract Task InitGame(GameInfo gameInfo);

        //public abstract Task InitLevel(GameInfo gameInfo);

        //public abstract void GameLogic(GameInfo gameInfo, float FrameElapsedTime);

        //public abstract void GameRender(SpriteBatch spriteBatch, GameInfo gameInfo, float FrameElapsedTime);

        public bool AddGameEntity(GameEntity entity)
        {
            entity.Initialize();

            entity.LoadResources();
            
            return GameEntities.TryAdd(entity);
        }

        public bool RemoveGameEntity(GameEntity entity)
        {
            if (GameEntities.TryRemove(entity))
            {
                entity.Dispose();

                return true;
            }

            return false;
        }

        /// <summary>
        /// e.g. var ship = GetComponents < ShipComponent >()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetGameEntities<T>() where T : GameEntity
        {
            List<T> components = GameEntities.OfType<T>().ToList<T>();
            return components;
        }

        /// <summary>
        /// e.g. var ship = GetComponents < ShipComponent >().FirstOrDefault();
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetGameEntity<T>() where T : GameEntity
        {
            T component = GameEntities.OfType<T>().FirstOrDefault();
            return component;
        }

        private bool CheckPressedKeys()
        {
            bool keyPressed = false;

            foreach (Keys key in KeyboardState.GetPressedKeys())
            {
                if (LastKeyboardState.IsKeyUp(key))
                {
                    keyPressed = true;
                }
            }
            return keyPressed;
        }


    }
}
