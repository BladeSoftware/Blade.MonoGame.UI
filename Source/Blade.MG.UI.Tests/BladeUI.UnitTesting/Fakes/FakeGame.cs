using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeUI.UnitTesting.Fakes
{
    internal class FakeGame : Game
    {
        private static FakeGame instance;
        public static FakeGame Instance
        {
            get
            {
                if (instance == null) { instance = new FakeGame(); }
                return instance;
            }
        }

        private GraphicsDeviceManager graphicsDeviceManager;


        public FakeGame()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            IsMouseVisible = true;


            // Run once to initialise things
            SuppressDraw();
            RunOneFrame();
        }

        protected override void Initialize()
        {
            base.Initialize();

            // --- Set to Full Screen Mode
            graphicsDeviceManager.IsFullScreen = false;
            graphicsDeviceManager.HardwareModeSwitch = false;

            IsFixedTimeStep = false;

            // --- Set a pre-defined window size
            graphicsDeviceManager.PreferredBackBufferWidth = 800;
            graphicsDeviceManager.PreferredBackBufferHeight = 600;

            // -- Set allowed Orientations
            graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            // -- Should the mouse cursor be visible
            IsMouseVisible = true;

            graphicsDeviceManager.ApplyChanges();

        }

        protected override void LoadContent()
        {
            //_spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        private bool firstRun = true;
        protected override void Update(GameTime gameTime)
        {
            if (firstRun)
            {
                // Load the default Mock Scene
                firstRun = false;
                //RequestSceneChange(typeof(FakeScene));
                SuppressDraw();
            }

            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }


        //public new void RequestSceneChange(Type sceneType)
        //{
        //    base.RequestSceneChange(sceneType);

        //    SuppressDraw();
        //    RunOneFrame();
        //}

        public void RunOneFrame_SuppressDraw()
        {
            SuppressDraw();
            RunOneFrame();
        }

        public Rectangle LayoutRectangle
        {
            get
            {
                if (GraphicsDevice.PresentationParameters.IsFullScreen)
                {
                    return GraphicsDevice.Viewport.TitleSafeArea;
                }
                else
                {
                    return GraphicsDevice.Viewport.Bounds;
                }
            }
        }


    }
}
