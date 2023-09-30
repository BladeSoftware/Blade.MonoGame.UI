//using System;
//using System.Collections.Generic;
//using System.Text;
//using Blade.Games.UI;
//using Blade.Games.UI.Components;
//using Blade.Games.UI.Controls;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Content;

//namespace GameStudio.UI
//{
//    public class TestUI
//    {
//        //public static BladeWindow BuildScreen(Game game)
//        //{
//        //    //var window1 = new BladeWindow();

//        //    //var label2 = new Label();
//        //    //label2.Name = "Label1";
//        //    //label2.Text = "Test Label";
//        //    //label2.Width = 100;
//        //    //label2.Height = 20;

//        //    //window1.Children.Add(label2);
//        //    //return window1;


//        //    /*
//        //    var window = new BladeWindow();

//        //    var panel1 = new BladePanel();
//        //    panel1.Width = float.NaN; // Auto
//        //    panel1.Height = float.NaN; // Auto 
//        //    //panel1.Width = 250;
//        //    //panel1.Height = 250;
//        //    panel1.SetBackground(Color.green, 0.5f);

//        //    var panel2 = new BladePanel();
//        //    panel2.Width = 150;
//        //    panel2.Height = 150;
//        //    panel2.SetBackground(Color.blue, 0.5f);

//        //    panel1.Children.Add(panel2);

//        //    window.Children.Add(panel1);
//        //    return window;
//        //    */



//        //    var window = new BladeWindow();
//        //    window.Init(game);

//        //    var grid1 = new Grid();
//        //    grid1.Name = "GRID1";
//        //    grid1.Width = float.NaN;
//        //    //grid1.Width = 200;
//        //    grid1.Height = float.NaN;
//        //    //grid1.SetBackground(Color.cyan, 0.5f);

//        //    grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
//        //    grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
//        //    grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });

//        //    grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
//        //    grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
//        //    grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });


//        //    window.AddChild(grid1);


//        //    var panel1 = new Panel();
//        //    panel1.Name = "PANEL1";
//        //    panel1.Width = float.NaN;
//        //    panel1.Height = float.NaN;
//        //    //panel1.Width = 250;
//        //    //panel1.Height = 250;
//        //    panel1.HorizontalAlignment = HorizontalAlignmentType.Stretch;
//        //    panel1.VerticalAlignment = VerticalAlignmentType.Stretch;
//        //    panel1.Background = new Color(Color.Red, 0.5f);
//        //    grid1.AddChild(panel1, 0, 0);
//        //    grid1.SetColumn(panel1, 0);
//        //    grid1.SetRow(panel1, 0);


//        //    var panel2 = new Panel();
//        //    panel2.Name = "PANEL2";
//        //    panel2.Width = 150;
//        //    panel2.Height = 150;
//        //    panel2.Background = new Color(Color.Green, 0.5f);
//        //    panel2.HorizontalAlignment = HorizontalAlignmentType.Right;
//        //    panel2.VerticalAlignment = VerticalAlignmentType.Bottom;

//        //    grid1.AddChild(panel2, 1, 0);


//        //    var panel3 = new Panel();
//        //    panel3.Name = "PANEL3";
//        //    panel3.Width = float.NaN;
//        //    panel3.Height = float.NaN;
//        //    panel3.Width = 150;
//        //    panel3.Height = 150;
//        //    panel3.HorizontalAlignment = HorizontalAlignmentType.Right;
//        //    panel3.VerticalAlignment = VerticalAlignmentType.Top;
//        //    panel3.Background = new Color(Color.Blue, 0.5f);
//        //    grid1.AddChild(panel3, 2, 0);


//        //    var panel4 = new Panel();
//        //    panel4.Name = "PANEL4";
//        //    panel4.Width = 50;
//        //    panel4.Height = 50;
//        //    panel4.Background = new Color(Color.Yellow, 0.5f);
//        //    panel4.HorizontalAlignment = HorizontalAlignmentType.Left;
//        //    panel4.VerticalAlignment = VerticalAlignmentType.Top;

//        //    grid1.AddChild(panel4, 0, 1);


//        //    var panel5 = new Panel();
//        //    panel5.Name = "PANEL5";
//        //    panel5.Background = new Color(Color.Cyan, 0.5f);
//        //    panel5.HorizontalAlignment = HorizontalAlignmentType.Stretch;
//        //    panel5.VerticalAlignment = VerticalAlignmentType.Stretch;

//        //    grid1.AddChild(panel5, 1, 1);

//        //    var panel6 = new Panel();
//        //    panel6.Name = "PANEL6";
//        //    panel6.Background = new Color(Color.Gray, 0.5f);
//        //    panel6.HorizontalAlignment = HorizontalAlignmentType.Stretch;
//        //    panel6.VerticalAlignment = VerticalAlignmentType.Stretch;

//        //    grid1.AddChild(panel6, 2, 1);


//        //    var panel7 = new Panel();
//        //    panel7.Name = "PANEL7";
//        //    panel7.Background = new Color(Color.White, 0.5f);
//        //    panel7.HorizontalAlignment = HorizontalAlignmentType.Left;
//        //    panel7.VerticalAlignment = VerticalAlignmentType.Top;

//        //    grid1.AddChild(panel7, 0, 2);


//        //    var panel8 = new Panel();
//        //    panel8.Name = "PANEL5";
//        //    panel8.Background = new Color(Color.Magenta, 0.5f);
//        //    panel8.HorizontalAlignment = HorizontalAlignmentType.Stretch;
//        //    panel8.VerticalAlignment = VerticalAlignmentType.Stretch;

//        //    grid1.AddChild(panel8, 1, 2);

//        //    var panel9 = new Panel();
//        //    panel9.Name = "PANEL6";
//        //    panel9.Background = new Color(Color.Yellow, 0.5f);
//        //    panel9.HorizontalAlignment = HorizontalAlignmentType.Stretch;
//        //    panel9.VerticalAlignment = VerticalAlignmentType.Stretch;

//        //    grid1.AddChild(panel9, 2, 2);



//        //    var label1 = new Label();
//        //    label1.Name = "Label1";
//        //    label1.Text = "Test Label";
//        //    //label1.Width = 100;
//        //    //label1.Height = 20;
//        //    label1.HorizontalAlignment = HorizontalAlignmentType.Center;
//        //    label1.VerticalAlignment = VerticalAlignmentType.Center;
//        //    label1.FontColor = Color.White;

//        //    //grid1.Children.Add(label1);
//        //    //grid1.SetColumn(label1, 0);
//        //    //grid1.SetRow(label1, 1);
//        //    panel4.AddChild(label1);


//        //    //var text1 = new TextBox();
//        //    //text1.Name = "Text1";
//        //    //text1.Text = "Hello, World!";
//        //    //text1.Width = 100;
//        //    //text1.Height = 100;

//        //    //grid1.Children.Add(text1);
//        //    //grid1.SetColumn(text1, 1);
//        //    //grid1.SetRow(text1, 1);

//        //    //var rect1 = new Rectangle();
//        //    //rect1.SetBackground(Color.yellow, 0.5f);
//        //    //grid1.Children.Add(rect1);
//        //    //grid1.SetColumn(rect1, 0);
//        //    //grid1.SetRow(rect1, 1);



//        //    return window;
//        //}

//        //public BladeWindow BuildScreen2(Game game)
//        //{
//        //    var window = new BladeWindow();
//        //    window.Init(game);


//        //    //var p1 = new Panel
//        //    //{
//        //    //    Background = Color.Brown,
//        //    //    HorizontalAlignment = HorizontalAlignmentType.Left,
//        //    //    VerticalAlignment = VerticalAlignmentType.Top,
//        //    //    HorizontalContentAlignment = HorizontalAlignmentType.Left,
//        //    //    VerticalContentAlignment = VerticalAlignmentType.Bottom,
//        //    //    MinWidth = 50,
//        //    //    MinHeight = 50,
//        //    //    Margin = new Thickness(1, 25, 0, 0)
//        //    //};
//        //    //window.Children.Add(p1);
//        //    //var l1 = new Label
//        //    //{
//        //    //    Text = "210",
//        //    //    HorizontalAlignment = HorizontalAlignmentType.Stretch,
//        //    //    VerticalAlignment = VerticalAlignmentType.Stretch,
//        //    //    HorizontalContentAlignment = HorizontalAlignmentType.Left,
//        //    //    VerticalContentAlignment = VerticalAlignmentType.Center,
//        //    //    FontColor = Color.White,
//        //    //    Margin = new Thickness(10),
//        //    //    Padding = new Thickness(20, 200, 20, 200),
//        //    //    Background = new Color(Color.DarkOliveGreen, 0.8f)
//        //    //};
//        //    //p1.Children.Add(l1);
//        //    //return window;



//        //    var grid1 = new Grid();
//        //    //grid1.Name = "GRID1";
//        //    //grid1.Background = Color.Red;

//        //    grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
//        //    grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
//        //    grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 4f) });


//        //    grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 20) });
//        //    grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });
//        //    //grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
//        //    //grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
//        //    //grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });

//        //    window.AddChild(grid1);


//        //    var panel1 = new Panel
//        //    {
//        //        Background = Color.Yellow
//        //    };
//        //    grid1.AddChild(panel1, 0, 1);

//        //    var panel2 = new Panel
//        //    {
//        //        Background = Color.Blue
//        //    };
//        //    grid1.AddChild(panel2, 1, 1);

//        //    var panel3 = new Panel
//        //    {
//        //        Name = "Panel3",
//        //        Background = Color.Green,
//        //        Margin = new Thickness(10),
//        //        Padding = new Thickness(10)
//        //    };
//        //    grid1.AddChild(panel3, 1, 1);

//        //    var panel4 = new Panel
//        //    {
//        //        Background = Color.Orange
//        //    };
//        //    panel3.AddChild(panel4);

//        //    //var label1 = new Label
//        //    //{
//        //    //    Text = "Score",
//        //    //    HorizontalAlignment = HorizontalAlignmentType.Stretch,
//        //    //    VerticalAlignment = VerticalAlignmentType.Stretch,
//        //    //    HorizontalContentAlignment = HorizontalAlignmentType.Right,
//        //    //    VerticalContentAlignment = VerticalAlignmentType.Center,
//        //    //    FontColor = Color.White,
//        //    //    MinHeight = 100
//        //    //    //Background = Color.DarkOliveGreen,
//        //    //    //Margin = new Thickness(10)
//        //    //};
//        //    //grid1.Children.Add(label1);
//        //    //grid1.SetColumn(label1, 0);
//        //    //grid1.SetRow(label1, 1);

//        //    panel4.AddChild(new Panel { HorizontalAlignment = HorizontalAlignmentType.Left, VerticalAlignment = VerticalAlignmentType.Top, Width = 50, Height = 50, Background = Color.Navy, Margin = new Thickness(15, 15, 0, 0) });
//        //    panel4.AddChild(new Panel { HorizontalAlignment = HorizontalAlignmentType.Left, VerticalAlignment = VerticalAlignmentType.Bottom, Width = 50, Height = 50, Background = Color.Navy, Margin = new Thickness(15 + 50, 0, 0, 15) });

//        //    var label2 = new Label
//        //    {
//        //        Text = "210",
//        //        HorizontalAlignment = HorizontalAlignmentType.Stretch,
//        //        VerticalAlignment = VerticalAlignmentType.Stretch,
//        //        HorizontalContentAlignment = HorizontalAlignmentType.Left,
//        //        VerticalContentAlignment = VerticalAlignmentType.Center,
//        //        FontColor = Color.White,
//        //        Margin = new Thickness(15),
//        //        Padding = new Thickness(20, 50, 20, 50),
//        //        Background = new Color(Color.DarkOliveGreen, 0.8f)
//        //    };

//        //    panel4.AddChild(label2);

//        //    //grid1.Children.Add(label2);
//        //    //grid1.SetColumn(label2, 1);
//        //    //grid1.SetRow(label2, 1);

//        //    //var panel2 = new Panel();
//        //    //panel2.Name = "PANEL2";
//        //    //panel2.Width = 150;
//        //    //panel2.Height = 150;
//        //    //panel2.SetBackground(new Color(Color.Green, 0.5f));
//        //    //panel2.HorizontalAlignment = HorizontalAlignmentType.Right;
//        //    //panel2.VerticalAlignment = VerticalAlignmentType.Bottom;

//        //    //grid1.Children.Add(panel2);
//        //    //grid1.SetColumn(panel2, 1);
//        //    //grid1.SetRow(panel2, 0);


//        //    //var panel3 = new Panel();
//        //    //panel3.Name = "PANEL3";
//        //    //panel3.Width = float.NaN;
//        //    //panel3.Height = float.NaN;
//        //    //panel3.Width = 150;
//        //    //panel3.Height = 150;
//        //    //panel3.HorizontalAlignment = HorizontalAlignmentType.Right;
//        //    //panel3.VerticalAlignment = VerticalAlignmentType.Top;
//        //    //panel3.SetBackground(new Color(Color.Blue, 0.5f));
//        //    //grid1.Children.Add(panel3);
//        //    //grid1.SetColumn(panel3, 2);
//        //    //grid1.SetRow(panel3, 0);


//        //    //var panel4 = new Panel();
//        //    //panel4.Name = "PANEL4";
//        //    //panel4.Width = 50;
//        //    //panel4.Height = 50;
//        //    //panel4.SetBackground(new Color(Color.Yellow, 0.5f));
//        //    //panel4.HorizontalAlignment = HorizontalAlignmentType.Left;
//        //    //panel4.VerticalAlignment = VerticalAlignmentType.Top;

//        //    //grid1.Children.Add(panel4);
//        //    //grid1.SetColumn(panel4, 0);
//        //    //grid1.SetRow(panel4, 1);


//        //    //var panel5 = new Panel();
//        //    //panel5.Name = "PANEL5";
//        //    //panel5.SetBackground(new Color(Color.Cyan, 0.5f));
//        //    //panel5.HorizontalAlignment = HorizontalAlignmentType.Stretch;
//        //    //panel5.VerticalAlignment = VerticalAlignmentType.Stretch;

//        //    //grid1.Children.Add(panel5);
//        //    //grid1.SetColumn(panel5, 1);
//        //    //grid1.SetRow(panel5, 1);

//        //    //var panel6 = new Panel();
//        //    //panel6.Name = "PANEL6";
//        //    //panel6.SetBackground(new Color(Color.Gray, 0.5f));
//        //    //panel6.HorizontalAlignment = HorizontalAlignmentType.Stretch;
//        //    //panel6.VerticalAlignment = VerticalAlignmentType.Stretch;

//        //    //grid1.Children.Add(panel6);
//        //    //grid1.SetColumn(panel6, 2);
//        //    //grid1.SetRow(panel6, 1);


//        //    //var panel7 = new Panel();
//        //    //panel7.Name = "PANEL7";
//        //    //panel7.SetBackground(new Color(Color.White, 0.5f));
//        //    //panel7.HorizontalAlignment = HorizontalAlignmentType.Left;
//        //    //panel7.VerticalAlignment = VerticalAlignmentType.Top;

//        //    //grid1.Children.Add(panel7);
//        //    //grid1.SetColumn(panel7, 0);
//        //    //grid1.SetRow(panel7, 2);


//        //    //var panel8 = new Panel();
//        //    //panel8.Name = "PANEL5";
//        //    //panel8.SetBackground(new Color(Color.Magenta, 0.5f));
//        //    //panel8.HorizontalAlignment = HorizontalAlignmentType.Stretch;
//        //    //panel8.VerticalAlignment = VerticalAlignmentType.Stretch;

//        //    //grid1.Children.Add(panel8);
//        //    //grid1.SetColumn(panel8, 1);
//        //    //grid1.SetRow(panel8, 2);

//        //    //var panel9 = new Panel();
//        //    //panel9.Name = "PANEL6";
//        //    //panel9.SetBackground(new Color(Color.Yellow, 0.5f));
//        //    //panel9.HorizontalAlignment = HorizontalAlignmentType.Stretch;
//        //    //panel9.VerticalAlignment = VerticalAlignmentType.Stretch;

//        //    //grid1.Children.Add(panel9);
//        //    //grid1.SetColumn(panel9, 2);
//        //    //grid1.SetRow(panel9, 2);



//        //    //var label1 = new Label();
//        //    //label1.Name = "Label1";
//        //    //label1.Text = "Test Label";
//        //    ////label1.Width = 100;
//        //    ////label1.Height = 20;
//        //    //label1.HorizontalAlignment = HorizontalAlignmentType.Center;
//        //    //label1.VerticalAlignment = VerticalAlignmentType.Center;
//        //    //label1.FontColor = Color.White;

//        //    ////grid1.Children.Add(label1);
//        //    ////grid1.SetColumn(label1, 0);
//        //    ////grid1.SetRow(label1, 1);
//        //    //panel4.Children.Add(label1);


//        //    ////var text1 = new TextBox();
//        //    ////text1.Name = "Text1";
//        //    ////text1.Text = "Hello, World!";
//        //    ////text1.Width = 100;
//        //    ////text1.Height = 100;

//        //    ////grid1.Children.Add(text1);
//        //    ////grid1.SetColumn(text1, 1);
//        //    ////grid1.SetRow(text1, 1);

//        //    ////var rect1 = new Rectangle();
//        //    ////rect1.SetBackground(Color.yellow, 0.5f);
//        //    ////grid1.Children.Add(rect1);
//        //    ////grid1.SetColumn(rect1, 0);
//        //    ////grid1.SetRow(rect1, 1);



//        //    return window;
//        //}

//        // void ScoreTable()
//        // {
//        //     var win = Screen.width * 0.6;
//        //     var w1 = win * 0.35; var w2 = win * 0.15; var w3 = win * 0.35;

//        //for (var line in Scores.Split("\n"[0]))
//        //     {
//        //         fields = line.Split("\t"[0]);
//        //         if (fields.length >= 3)
//        //         {
//        //             GUILayout.BeginHorizontal();
//        //             GUILayout.Label(fields[0], GUILayout.Width(w1));
//        //             GUILayout.Label(fields[1], GUILayout.Width(w2));
//        //             GUILayout.Label(fields[2], GUILayout.Width(w3));
//        //             GUILayout.EndHorizontal();
//        //         }
//        //     }
//        // }

//    }
//}
