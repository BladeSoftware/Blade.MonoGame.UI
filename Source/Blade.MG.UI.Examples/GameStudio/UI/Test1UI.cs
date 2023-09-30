using Blade.UI;
using Blade.UI.Components;
using Blade.UI.Controls;
using Microsoft.Xna.Framework;

namespace Examples.UI
{
    public class Test1UI : UIWindow
    {

        public override void Initialize(Game game)
        {
            base.Initialize(game);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            BuildScreen(base.Game);
        }

        public override void PerformLayout(GameTime gameTime)
        {
            base.PerformLayout(gameTime);
        }

        public void BuildScreen(Game game)
        {
            var mainLayout = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignmentType.Center,
                //Height = 14,
                //Background = Color.Transparent,
                //Background = new Color(Color.DarkSlateBlue, 0.5f),
                Background = Color.LightBlue,
                Width = "400px",
                HorizontalScrollBarVisible = false,
                VerticalScrollBarVisible = false,
                //Margin = new Thickness(0, 15, 0, 0),

                //Margin = new Thickness(250, 100, 250, 100),
                //Padding = new Thickness(0, 0, 0, 0),

                //Transform = new Transform() with { Rotation = new Vector3(0f, 0f, 3.1415f / 3f), CenterPoint = new Vector3(game.Viewport.Width / 2f, game.Viewport.Height / 2f, 0f) }
            };

            base.AddChild(mainLayout);


            var content1 = new Control()
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Width = "50%",
                Background = Color.Red
            };

            mainLayout.AddChild(content1);

            //var content2 = new Control()
            //{
            //    HorizontalAlignment = HorizontalAlignmentType.Stretch,
            //    VerticalAlignment = VerticalAlignmentType.Stretch,
            //    Width = "50%",
            //    Background = Color.Blue
            //};

            //mainLayout.AddChild(content2);

        }


    }
}
