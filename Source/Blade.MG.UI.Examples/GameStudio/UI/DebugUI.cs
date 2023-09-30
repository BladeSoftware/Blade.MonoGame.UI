//using Blade.UI;
//using Blade.UI.Components;
//using Blade.UI.Controls;
//using Microsoft.Xna.Framework;

//namespace Examples.UI
//{
//    public class DebugUI : UIWindow
//    {
//        //private SpriteFont fontArial;
//        //private SpriteFont fontArial8;

//        public override void Initialize(Game game)
//        {
//            base.Initialize(game);
//        }

//        public override void LoadContent()
//        {
//            base.LoadContent();

//            //fontArial = Game.Content.Load<SpriteFont>("Fonts/Arial");
//            //fontArial8 = Game.Content.Load<SpriteFont>("Fonts/Arial-8");

//            BuildScreen(base.Game);
//        }

//        public void BuildScreen(Game game)
//        {
//            var grid1 = new Grid()
//            {
//                Height = 14,
//                //Background = Color.Transparent
//                Background = new Color(Color.DarkRed, 0.5f)
//            };


//            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 50f) });
//            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Auto), MinWidth = 20 });
//            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Auto), MinWidth = 20 });
//            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 50f) });
//            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Auto), MinWidth = 20 });
//            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Auto), MinWidth = 20 });
//            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });

//            grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 2) });
//            grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 10) });
//            grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 2) });

//            base.AddChild(grid1);


//            // FPS
//            var fps = new Label
//            {
//                Text = new Binding<string>(() => "FPS"),
//                HorizontalContentAlignment = HorizontalAlignmentType.Right,
//                VerticalContentAlignment = VerticalAlignmentType.Center,
//                TextColor = new Binding<Color>(Color.White),
//                //SpriteFont = new Binding<SpriteFontBase>(fontArial8)
//            };
//            grid1.AddChild(fps, 1, 1);

//            var label2 = new Label
//            {
//                Text = new Binding<string, float, FloatToStringBindingConverter>(() => GameState.ScreenRedawRate, (value) => { }),
//                HorizontalContentAlignment = HorizontalAlignmentType.Left,
//                VerticalContentAlignment = VerticalAlignmentType.Center,
//                TextColor = new Binding<Color>(Color.White),
//                Margin = new Thickness(15, 0, 0, 0),
//                //SpriteFont = new Binding<SpriteFontBase>(fontArial8)
//            };

//            grid1.AddChild(label2, 2, 1);
//        }
//    }
//}
