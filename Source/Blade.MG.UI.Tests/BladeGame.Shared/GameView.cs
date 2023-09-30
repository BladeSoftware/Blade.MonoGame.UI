//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using SkiaSharp.Views.Forms;
//using Xamarin.Forms;

//namespace Asteroids.BladeGame
//{
//    public abstract class GameView : ContentView
//    {
//        //private System.Timers.Timer refreshTimer;
//        private DateTime lastFrameStart = DateTime.Now;
//        private float frameDelay;

//        private int renderFrameCount = 0;
//        private DateTime lastRenderFPSCalc = DateTime.Now;

//        private int logicFrameCount = 0;
//        private DateTime lastLogicFPSCalc = DateTime.Now;

//        protected float FrameElapsedTime;

//        private GameInfo gameInfo;
//        protected GameBase CurrentGame;


//        private bool IsInitialized = false;
//        private bool stopGameFlag = false;

//        private SKGLView skGLView;


//        private float desiredFrameRate;
//        protected float DesiredFrameRate
//        {
//            get { return desiredFrameRate; }
//            set
//            {
//                desiredFrameRate = Math.Min(Math.Max(value, 1f), 120f); // Limit Frame Rate to between 1 and 120 fps
//                frameDelay = 1000f / desiredFrameRate;
//            }
//        }


//        public GameView()
//        {
//            skGLView = new SKGLView();
//            skGLView.PaintSurface += SkGLView_PaintSurface;

//            //skGLView.EnableTouchEvents = true;

//            Content = skGLView;


//            //KeyDown += CustomPage_KeyDown;
//            //KeyUp += CustomPage_KeyUp;

//        }

//        protected void StartGameLoop()
//        {
//            //if (refreshTimer != null)
//            //{
//            //    refreshTimer.Dispose();
//            //}

//            //refreshTimer = new System.Timers.Timer();
//            //refreshTimer.Interval = 1; 
//            //refreshTimer.Elapsed += RefreshTimer_Elapsed;

//            //refreshTimer.Start();

//            if (CurrentGame == null)
//            {
//                throw new Exception("CurrentGame is null");
//            }

//            IsInitialized = false;
//            stopGameFlag = false;


//            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => InvalidateSurfaces());

//            System.Threading.Tasks.Task.Run(async () =>
//            {
//                Debug.WriteLine("Starting Game Loop");

//                while (!stopGameFlag)
//                {
//                    if (IsInitialized)
//                    {
//                        RefreshTimer_Elapsed(null, null);
//                        //System.Threading.Thread.Sleep(1);
//                    }
//                    else if (gameInfo != null)
//                    {
//                        await InitGame();
//                        IsInitialized = true;
//                    }
//                }

//                Debug.WriteLine("Exiting Game Loop");
//            }
//            );

//        }


//        protected void StopGameLoop()
//        {
//            Debug.WriteLine("Stopping Game Loop");

//            //refreshTimer.Stop();
//            stopGameFlag = true;

//        }

//        private void RefreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
//        {
//            DateTime frameStart = DateTime.Now;
//            //float elapsedMillies = (float)Math.Ceiling((frameStart - lastFrameStart).TotalMilliseconds);
//            float elapsedMillies = (float)(frameStart - lastFrameStart).TotalMilliseconds;

//            //System.Diagnostics.Debug.WriteLine("Frame Delay : " + frameDelay.ToString() + "  Elapsed : " + elapsedMillies.ToString());

//            if (elapsedMillies >= frameDelay)
//            {
//                FrameElapsedTime = (float)(frameStart - lastFrameStart).TotalSeconds;
//                lastFrameStart = frameStart;

//                GameLogicBase(gameInfo, FrameElapsedTime);

//                Xamarin.Forms.Device.BeginInvokeOnMainThread(() => InvalidateSurfaces());
//            }
//        }

//        private void GameLogicBase(GameInfo gameInfo, float FrameElapsedTime)
//        {
//            // Wait for gameInfo to be initialized
//            if (gameInfo == null) return;

//            // Calculate the number of FPS the Game Logic is running at
//            DateTime currentFPSCalc = DateTime.Now;
//            if ((currentFPSCalc - lastLogicFPSCalc).TotalSeconds > 1.0)
//            {
//                gameInfo.LogicFPS = logicFrameCount;
//                logicFrameCount = 0;
//                lastLogicFPSCalc = currentFPSCalc;
//            }
//            logicFrameCount++;


//            // Let user perform Game Logic
//            CurrentGame.GameLogic(gameInfo, FrameElapsedTime);
//        }

//        private async Task InitGame()
//        {
//            // Init Game Global Resources
//            await CurrentGame.InitGame(gameInfo);

//            // Init Game Level Resources
//            await CurrentGame.InitLevel(gameInfo);

//        }

//        private void SkGLView_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
//        {
//            if (gameInfo == null)
//            {
//                gameInfo = GameServices.GameInfo;
//                gameInfo.Width = e.RenderTarget.Width;
//                gameInfo.Height = e.RenderTarget.Height;
//            }

//            if (CurrentGame == null || !IsInitialized)
//            {
//                return;
//            }

//            // Update gameInfo
//            gameInfo.Width = e.RenderTarget.Width;
//            gameInfo.Height = e.RenderTarget.Height;

            
//            // Calculate the number of FPS the Game Rendering is running at
//            DateTime currentFPSCalc = DateTime.Now;
//            if ((currentFPSCalc - lastRenderFPSCalc).TotalSeconds > 1.0)
//            {
//                gameInfo.RenderFPS = renderFrameCount;
//                renderFrameCount = 0;
//                lastRenderFPSCalc = currentFPSCalc;
//            }
//            renderFrameCount++;


//            CurrentGame.GameRender(e, gameInfo, FrameElapsedTime);
//        }

//        private void InvalidateSurfaces()
//        {
//            skGLView.InvalidateSurface();
//        }

//        //---------------------------

//        public void SendKeyDown(string key)
//        {
//            if (GameComponent.Keys.TryAdd(key))
//            {
//                // Fire KeyDown ?
//                // Fire KeyPress ?
//            }
//        }

//        public void SendKeyUp(string key)
//        {
//            if (GameComponent.Keys.TryRemove(key))
//            {
//                // Fire KeyUp ?
//            }
//        }

//        //public event EventHandler<KeyEventArgs> KeyDown;
//        //public event EventHandler<KeyEventArgs> KeyUp;
//        //public event EventHandler<KeyEventArgs> KeyPressed;

//        //public void SendKeyDown(object sender, KeyEventArgs e)
//        //{
//        //    KeyDown?.Invoke(sender, e);
//        //}

//        //public void SendKeyUp(object sender, KeyEventArgs e)
//        //{
//        //    KeyUp?.Invoke(sender, e);
//        //}


//        //private void CustomPage_KeyDown(object sender, KeyEventArgs e)
//        //{
//        //    //System.Diagnostics.Debug.WriteLine("Key down : " + e.Key);
//        //    if (GameComponent.Keys.TryAdd(e.Key))
//        //    {
//        //        KeyPressed?.Invoke(sender, e);
//        //    }
//        //}

//        //private void CustomPage_KeyUp(object sender, KeyEventArgs e)
//        //{
//        //    //System.Diagnostics.Debug.WriteLine("Key up   : " + e.Key);
//        //    GameComponent.Keys.TryRemove(e.Key);
//        //}

//        //private void GameView_KeyPressed(object sender, KeyEventArgs e)
//        //{

//        //}

//    }

//    //public class KeyEventArgs : EventArgs
//    //{
//    //    public string Key { get; set; }
//    //}

//}