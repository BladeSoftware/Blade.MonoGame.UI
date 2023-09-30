//using System;
//using System.Collections.Generic;
//using System.Text;
//using GameStudio.Shared.GameComponents;
//using GameStudio.UI;
//using Blade.Games.UI;
//using Blade.Games.UI.Components;
//using Blade.Games.UI.Controls;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.Graphics;
//using Blade.Games;
//using Game.Shared.Scenes;
//using Game.Shared.Scenes.Air1918;

//namespace Examples.UI
//{
//    public class InGameUI : UIWindow //GameEntity
//    {
//        private UIWindow ui;
//        private Air1918Scene Model;

//        public InGameUI(Air1918Scene model)
//        {
//            Model = model;
//        }

//        public override void LoadContent()
//        {
//            ui = BuildScreen(Game);
//        }

//        //public override void Logic(GameTime gameTime)
//        //{
//        //    ui.Logic(gameTime);
//        //}

//        //public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
//        //{
//        //    //ui.Draw(spriteBatch, gameTime);
//        //    ui.RenderLayout(Game.Viewport.Bounds);
//        //}

//        public override void Dispose()
//        {
//            base.Dispose();
//        }


//        public UIWindow BuildScreen(BladeGame game)
//        {
//            var window = new UIWindow();
//            window.Initialize(game);

//            var grid1 = new Grid()
//            {
//                Background = Color.Transparent
//            };


//            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 50f) });
//            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Auto), MinWidth = 20 });
//            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Auto), MinWidth = 20 });
//            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 50f) });
//            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Auto), MinWidth = 20 });
//            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Auto), MinWidth = 20 });
//            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });


//            grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 20) });
//            grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });
//            //grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 4f) });
//            //grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
//            //grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
//            //grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 4f) });
//            //grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
//            //grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });

//            window.AddChild(grid1);


//            // Score
//            var label1 = new Label
//            {
//                Text = new Binding<string>(() => "Score"),
//                HorizontalContentAlignment = HorizontalAlignmentType.Right,
//                VerticalContentAlignment = VerticalAlignmentType.Center,
//                TextColor = new Binding<Color>(Color.White),
//            };
//            grid1.AddChild(label1, 1, 1);

//            var label2 = new Label
//            {
//                Text = new Binding<string, int, IntToStringBindingConverter>(() => GameState.Score, (value) => GameState.Score = value),
//                HorizontalContentAlignment = HorizontalAlignmentType.Left,
//                VerticalContentAlignment = VerticalAlignmentType.Center,
//                TextColor = new Binding<Color>(Color.White),
//                Margin = new Thickness(15, 0, 0, 0)
//            };

//            grid1.AddChild(label2, 2, 1);



//            // Lives
//            var label3 = new Label
//            {
//                Text = new Binding<string>(() => "Lives"),
//                HorizontalContentAlignment = HorizontalAlignmentType.Right,
//                VerticalContentAlignment = VerticalAlignmentType.Center,
//                TextColor = new Binding<Color>(Color.White),
//            };
//            grid1.AddChild(label3, 4, 1);

//            var label4 = new Label
//            {
//                Text = new Binding<string, int, IntToStringBindingConverter>(() => GameState.Lives),
//                HorizontalContentAlignment = HorizontalAlignmentType.Left,
//                VerticalContentAlignment = VerticalAlignmentType.Center,
//                TextColor = new Binding<Color>(Color.White),
//                Margin = new Thickness(15, 0, 0, 0)
//            };

//            grid1.AddChild(label4, 5, 1);



//            return window;
//        }

//    }
//}
