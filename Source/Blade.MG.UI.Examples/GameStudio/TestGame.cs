using Blade.MG.Input;
using Blade.UI;
using Examples.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Examples
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TestGame : Game
    {
        private GraphicsDeviceManager graphicsDeviceManager;


        public TestGame()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // --- Set to Full Screen Mode
            graphicsDeviceManager.IsFullScreen = false;
            graphicsDeviceManager.HardwareModeSwitch = false;

            IsFixedTimeStep = false;

            // --- Set a pre-defined window size
            graphicsDeviceManager.PreferredBackBufferWidth = 1280;
            graphicsDeviceManager.PreferredBackBufferHeight = 720;

            // -- Set allowed Orientations
            graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            // -- Should the mouse cursor be visible
            IsMouseVisible = true;

            graphicsDeviceManager.ApplyChanges();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            //var layerUI = Layers.Add(LayerType.UI);
            //layerUI.AddEntity(UIManager.Instance);

            UIManager.Clear();
            UIManager.Add(new ComponentTesterUI(), this);

        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            base.UnloadContent();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override async void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Check Input
            if (InputManager.IsKeyDown(Keys.Escape))
            {
#if !__IOS__
                this.Exit();
#endif
                return;
            }

            try
            {
                InputManager.Instance.Update();
                await UIManager.Instance.UpdateAsync(gameTime).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            UIManager.Instance.Draw(this.GraphicsDevice, null, gameTime);
        }

    }

}
