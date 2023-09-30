//using System;
//using System.Collections.Generic;
//using System.Text;
//using GameStudio.Shared.GameComponents;
//using Blade.Games;
//using Blade.Games.UI;
//using Blade.Games.UI.Components;
//using Blade.Games.UI.Controls;
//using Game.Shared.Scenes;
//using Game.Shared.Scenes.Air1918;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;

//namespace GameStudio.Shared.UI
//{
//    public class StartScreenUI : UIWindow //GameEntity
//    {
//        private Air1918Scene Model;

//        public StartScreenUI(Air1918Scene model)
//        {
//            Model = model;
//        }

//        public override void LoadContent()
//        {
//            BuildScreen();
//        }

//        //public override void Logic(GameTime gameTime)
//        //{
//        //    ui.Logic(gameTime);
//        //}

//        //public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
//        //{
//        //    //spriteBatch.Begin(SpriteSortMode.Immediate);
//        //    //spriteBatch.DrawString(fontArial, "Score", new Vector2(100, 20), Color.White);
//        //    //spriteBatch.DrawString(fontArial, GameState.Score.ToString(), new Vector2(150, 20), Color.White);
//        //    //spriteBatch.End();

//        //    //spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);

//        //    ui.RenderLayout(game.Viewport.Bounds);
//        //}

//        public override void Dispose()
//        {
//            base.Dispose();
//        }


//        public void BuildScreen()
//        {
//            //var window = new UIWindow();
//            //window.Init(game);


//            var grid1 = new Grid()
//            {
//                Background = Color.Transparent
//            };


//            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
//            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
//            grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 4f) });


//            grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 20) });
//            grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });
//            grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 4f) });
//            grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
//            grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
//            grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 4f) });
//            //grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
//            //grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });

//            base.AddChild(grid1);


//            //var label2 = new Label
//            //{
//            //    //Text = new Reference<int>(() => GameState.Score, (value) => GameState.Score = value),
//            //    Text = new Binding<string, int, IntToStringBindingConverter>(() => GameState.Score, (value) => GameState.Score = value),
//            //    HorizontalContentAlignment = HorizontalAlignmentType.Left,
//            //    VerticalContentAlignment = VerticalAlignmentType.Center,
//            //    TextColor = new Binding<Color>(Color.White),
//            //    Margin = new Thickness(15, 0, 0, 0)
//            //};

//            //Binding<int> test = new Binding<int>(0);
//            //label2.Text = new Binding<string, int, IntToStringBindingConverter>(() => test.Value);
//            //GameState.Score.Value = 10;
//            //int x = GameState.Score.Value;
//            //string y = label2.Text.Value;

//            //grid1.AddChild(label2, 1, 1);

//            var button1 = new Button
//            {
//                Text = "Start Game",
//                HorizontalAlignment = HorizontalAlignmentType.Center,
//                VerticalAlignment = VerticalAlignmentType.Center,
//                HorizontalContentAlignment = HorizontalAlignmentType.Center,
//                VerticalContentAlignment = VerticalAlignmentType.Center,
//                Background = new Binding<Color>(Color.Transparent),
//                FontColor = Color.White,
//                MinWidth = 250
//            };

//            button1.OnClick = (uiEvent) => { Model.StartNewGame(); };
//            grid1.AddChild(button1, 0, 3, 3, 1);

//            var button2 = new Button
//            {
//                Text = "Options",
//                HorizontalAlignment = HorizontalAlignmentType.Center,
//                VerticalAlignment = VerticalAlignmentType.Center,
//                HorizontalContentAlignment = HorizontalAlignmentType.Center,
//                VerticalContentAlignment = VerticalAlignmentType.Center,
//                FontColor = Color.White,
//                MinWidth = 250
//            };

//            grid1.AddChild(button2, 0, 3, 4, 1);


//        }

//    }
//}
