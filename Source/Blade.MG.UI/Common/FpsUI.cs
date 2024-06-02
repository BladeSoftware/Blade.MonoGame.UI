using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Common
{
    public class FpsUI : UIWindow
    {
        public static int DebugUIHeight = 24;

        public static float ScreenRedawRate = 60f;
        public static float PhysicsRate = 60f;

        private DateTime lastRedawTime = DateTime.Now;
        private DateTime lastPhysicsTime = DateTime.Now;


        //private SpriteFont fontArial;
        //private SpriteFont fontArial8;

        public override void Initialize(Game game)
        {
            base.Initialize(game);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            //fontArial = Game.Content.Load<SpriteFont>("Fonts/Arial");
            //fontArial8 = Game.Content.Load<SpriteFont>("Fonts/Arial-8");

            BuildScreen(Game);
        }

        public override void PerformLayout(GameTime gameTime)
        {
            // Ensure we're always on top
            //UIManager.EnsureTopMost(this);

            var currentTime = DateTime.Now;
            double elapsed = (currentTime - lastPhysicsTime).Milliseconds;
            PhysicsRate = PhysicsRate * 0.95f + (float)(1000.0 / elapsed * 0.05);
            lastPhysicsTime = currentTime;

            if (!float.IsNormal(PhysicsRate))
            {
                PhysicsRate = 0f;
            }

            base.PerformLayout(gameTime);
        }

        public override void RenderLayout(GameTime gameTime)
        {
            var currentTime = DateTime.Now;
            double elapsed = (currentTime - lastRedawTime).Milliseconds;
            ScreenRedawRate = ScreenRedawRate * 0.95f + (float)(1000.0 / elapsed * 0.05);
            lastRedawTime = currentTime;

            if (!float.IsNormal(ScreenRedawRate))
            {
                ScreenRedawRate = 0f;
            }

            base.RenderLayout(gameTime);
        }

        public void BuildScreen(Game game)
        {
            var grid1 = new Grid()
            {
                // Height = 14,
                //Background = Color.Transparent
                Background = new Color(Color.DarkRed, 0.75f),
                VerticalAlignment = VerticalAlignmentType.Top
            };


            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 50f) });
            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Auto), MinWidth = 20 });
            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Auto), MinWidth = 20 });
            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 50f) });
            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Auto), MinWidth = 20 });
            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Auto), MinWidth = 20 });
            //grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });

            grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 2) });
            grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, Math.Max(0, DebugUIHeight - 4)) });
            grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 2) });

            base.AddChild(grid1);


            // FPS
            var fps = new Label
            {
                Text = new Binding<string>(() => "Render FPS"),
                //HorizontalContentAlignment = HorizontalAlignmentType.Right,
                //VerticalContentAlignment = VerticalAlignmentType.Center,
                TextColor = new Binding<Color>(Color.White),
                //SpriteFont = new Binding<SpriteFontBase>(fontArial8),
                FontName = "Default",
                FontSize = DebugUIHeight - 4
            };
            grid1.AddChild(fps, 1, 1);

            var label2 = new Label
            {
                //Text = new Binding<string, float, FloatToStringBindingConverter>(() => GameState.ScreenRedawRate, (value) => { }),
                Text = new Binding<string, float, FloatToStringBindingConverter>(() => ScreenRedawRate, (value) => { }),
                //HorizontalContentAlignment = HorizontalAlignmentType.Left,
                //VerticalContentAlignment = VerticalAlignmentType.Center,
                TextColor = new Binding<Color>(Color.White),
                Margin = new Thickness(15, 0, 0, 0),
                //SpriteFont = new Binding<SpriteFontBase>(fontArial8),
                FontName = "Default",
                FontSize = DebugUIHeight - 4

            };

            grid1.AddChild(label2, 2, 1);


            var physicsfps = new Label
            {
                Text = new Binding<string>(() => "Physics FPS"),
                //HorizontalContentAlignment = HorizontalAlignmentType.Right,
                //VerticalContentAlignment = VerticalAlignmentType.Center,
                TextColor = new Binding<Color>(Color.White),
                //SpriteFont = new Binding<SpriteFontBase>(fontArial8),
                FontName = "Default",
                FontSize = DebugUIHeight - 4
            };
            grid1.AddChild(physicsfps, 4, 1);

            var label3 = new Label
            {
                //Text = new Binding<string, float, FloatToStringBindingConverter>(() => GameState.ScreenRedawRate, (value) => { }),
                Text = new Binding<string, float, FloatToStringBindingConverter>(() => PhysicsRate, (value) => { }),
                //HorizontalContentAlignment = HorizontalAlignmentType.Left,
                //VerticalContentAlignment = VerticalAlignmentType.Center,
                TextColor = new Binding<Color>(Color.White),
                Margin = new Thickness(15, 0, 0, 0),
                //SpriteFont = new Binding<SpriteFontBase>(fontArial8),
                FontName = "Default",
                FontSize = DebugUIHeight - 4

            };

            grid1.AddChild(label3, 5, 1);

            // Start off hidden
            Visible.Value = Visibility.Hidden;
        }


        public override Task HandleKeyPressAsync(UIWindow uiWindow, UIKeyEvent uiEvent)
        {
            if (uiEvent.Key == Microsoft.Xna.Framework.Input.Keys.OemTilde)
            {
                if (Visible.Value == Visibility.Visible)
                {
                    Visible.Value = Visibility.Collapsed;
                    UIManager.RenderControlHitBoxes = false;
                }
                else
                {
                    Visible.Value = Visibility.Visible;
                    UIManager.RenderControlHitBoxes = true;
                }

                uiEvent.Handled = true;
                return Task.CompletedTask;
            }

            return base.HandleKeyPressAsync(uiWindow, uiEvent);
        }

    }
}
