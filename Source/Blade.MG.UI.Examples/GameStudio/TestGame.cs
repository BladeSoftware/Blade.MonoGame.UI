using Blade.MG.Input;
using Blade.MG.UI;
using Blade.MG.UI.Common;
using Examples.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private UIManager uiManager;


        public TestGame()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this) { PreferHalfPixelOffset = false };
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
            // Register Standard Services
            //Services.AddService(typeof(SceneManager), sceneManager = new SceneManager(this));
            Services.AddService(typeof(UIManager), uiManager = new UIManager(this));

            // Inject the services we need
            //uiManager = this.Services.GetService<UIManager>();


            // --- Set to Full Screen Mode
            graphicsDeviceManager.IsFullScreen = false;
            graphicsDeviceManager.HardwareModeSwitch = false;
            Window.AllowUserResizing = true;

            IsFixedTimeStep = false;

            if (graphicsDeviceManager.IsFullScreen)
            {
                graphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            else
            {
                // --- Set a pre-defined window size
                graphicsDeviceManager.PreferredBackBufferWidth = 1280;
                graphicsDeviceManager.PreferredBackBufferHeight = 720;
            }

            // --- Set a pre-defined window size
            //var viewport = graphicsDeviceManager.GraphicsDevice.Viewport.Bounds;
            //if (viewport.Width < viewport.Height)
            //{
            //    // Portrait mode
            //    graphicsDeviceManager.PreferredBackBufferWidth = 720;
            //    graphicsDeviceManager.PreferredBackBufferHeight = 1280;
            //}
            //else
            //{
            //    // Landscape mode
            //    graphicsDeviceManager.PreferredBackBufferWidth = 1280;
            //    graphicsDeviceManager.PreferredBackBufferHeight = 720;
            //}

            //var x = graphicsDeviceManager.PreferredBackBufferWidth;
            //var y = graphicsDeviceManager.PreferredBackBufferHeight;


            // -- Set allowed Orientations
            graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            // -- Should the mouse cursor be visible
            IsMouseVisible = true;

            graphicsDeviceManager.ApplyChanges();

            base.Initialize();

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
            uiManager.Clear();
            uiManager.Add(new ComponentTesterUI());
            uiManager.Add(new FpsUI());

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
            if (InputManager.Keyboard.IsKeyDown(Keys.Escape))
            {
#if !__IOS__
                this.Exit();
#endif
                return;
            }

            try
            {
                InputManager.Update();
                await uiManager.UpdateAsync(gameTime).ConfigureAwait(true);
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

            GraphicsDevice.Clear(Color.CornflowerBlue);

            //var viewBounds = GraphicsDevice.Viewport.Bounds;
            //viewBounds = new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);


            //GraphicsDevice.ScissorRectangle = Rectangle.Empty; // viewport.Bounds;

            //using (var sb = new SpriteBatch(GraphicsDevice))
            //{
            //    sb.Begin();
            //    Blade.MG.Primitives.Primitives2D.FillRect(sb, viewBounds, Color.Pink);

            //    viewBounds.Inflate(-5, -5);
            //    Blade.MG.Primitives.Primitives2D.FillRect(sb, viewBounds, Color.Purple);

            //    sb.End();
            //}

            uiManager.Draw(null, gameTime);
        }

    }

}
