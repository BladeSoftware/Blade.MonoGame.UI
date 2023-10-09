using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.HelpPages;
using Microsoft.Xna.Framework;

namespace Examples.UI
{
    public class ComponentTesterUI : UIWindow
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
            var gridMainLayout = new Grid()
            {
                //Height = 14,
                //Background = Color.Transparent,
                //Background = new Color(Color.DarkSlateBlue, 0.5f),
                Background = new Color(Color.DarkBlue, 1f),
                //Margin = new Thickness(0, 15, 0, 0),

                //Margin = new Thickness(250, 100, 250, 100),
                //Padding = new Thickness(0, 0, 0, 0),

                //Transform = new Transform() with { Rotation = new Vector3(0f, 0f, 3.1415f / 3f), CenterPoint = new Vector3(game.Viewport.Width / 2f, game.Viewport.Height / 2f, 0f) }
            };


            //gridMainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            gridMainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Auto) });
            gridMainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });

            gridMainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });

            base.AddChild(gridMainLayout);

            var stackPanel1 = new StackPanel()
            {
                Background = Color.Green,
                Width = 400,
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                HorizontalContentAlignment = HorizontalAlignmentType.Stretch,
                VerticalContentAlignment = VerticalAlignmentType.Top

            };

            var content = new Control()
            {
                Background = Color.White
            };

            gridMainLayout.AddChild(stackPanel1, 0, 0);
            gridMainLayout.AddChild(content, 1, 0);

            int verticalMargin = 5;

            stackPanel1.AddChild(new Button()
            {
                Text = "Panel",
                Height = 50,
                Margin = new Thickness(0, verticalMargin, 0, verticalMargin),
                Padding = new Thickness(20, 0),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                OnClick = (sender, uiEvents) =>
                {
                    content.Content = new HelpPage_Panel();
                }
            });

            stackPanel1.AddChild(new Button()
            {
                Text = "Button",
                Height = 50,
                Margin = new Thickness(0, verticalMargin, 0, verticalMargin),
                Padding = new Thickness(20, 0),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                OnClick = (sender, uiEvents) =>
                {
                    content.Content = new HelpPage_Button();
                }
            });

            stackPanel1.AddChild(new Button()
            {
                Text = "Label",
                Height = 50,
                Margin = new Thickness(0, 5, 0, verticalMargin),
                Padding = new Thickness(20, 0),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                OnClick = (sender, uiEvents) =>
                {
                    content.Content = new HelpPage_Label();
                }
            });

            stackPanel1.AddChild(new Button()
            {
                Text = "Form Fields",
                Height = 50,
                Margin = new Thickness(0, 5, 0, verticalMargin),
                Padding = new Thickness(20, 0),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                OnClick = (sender, uiEvents) =>
                {
                    content.Content = new HelpPage_FormFields();
                }
            });

            stackPanel1.AddChild(new Button()
            {
                Text = "Tree View",
                Height = 50,
                Margin = new Thickness(0, verticalMargin, 0, verticalMargin),
                Padding = new Thickness(20, 0),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                OnClick = (sender, uiEvents) =>
                {
                    content.Content = null;
                }
            });

            //var panel1 = new Panel
            //{
            //    HorizontalContentAlignment = HorizontalAlignmentType.Center,
            //    VerticalContentAlignment = VerticalAlignmentType.Center,
            //    Background = Color.DarkSlateBlue
            //};
            //gridMainLayout.AddChild(panel1, 0, 0);

            //var label1 = new Label
            //{
            //    Text = new Binding<string>(() => "Label 1"),
            //    HorizontalContentAlignment = HorizontalAlignmentType.Center,
            //    VerticalContentAlignment = VerticalAlignmentType.Center,
            //    TextColor = Color.Red,
            //    SpriteFont = fontArial8,
            //    Background = Color.LightYellow,
            //    Padding = new Thickness(20)

            //};
            //gridMainLayout.AddChild(label1, 0, 0);

            //var treeView = new TreeView
            //{
            //    //Text = new Binding<string>(() => "Label 2"),
            //    HorizontalContentAlignment = HorizontalAlignmentType.Center,
            //    VerticalContentAlignment = VerticalAlignmentType.Center,
            //    //TextColor = Color.White,
            //    //SpriteFont = fontArial8,
            //    Background = Color.White
            //};
            //gridMainLayout.AddChild(treeView, 0, 0);

            //TreeNode c1;
            //TreeNode c2;
            //TreeNode c3;

            //int n = 0;

            //TreeNode rootNode = new TreeNode { IsExpanded = true, Text = $"Root Node : n={++n}", Children = new List<ITreeNode>() };
            //rootNode.Children.Add(c1 = new TreeNode { IsExpanded = true, Text = $"Child Node 1 : n={++n}", Children = new List<ITreeNode>() });
            //c1.Children.Add(new TreeNode { IsExpanded = true, Text = $"Child Node 1 - A : n={++n}", Children = new List<ITreeNode>() });

            //for (int i = 2; i < 20; i++)
            //{
            //    //rootNode.Nodes.Add(c1 = new TreeNode { IsExpanded = true, NodeText = $"Child Node {i} : n={++n}", Nodes = new List<ITreeNode>() });
            //    rootNode.Children.Add(c1 = new TreeNode { IsExpanded = true, Text = $"Super Duper Long Child Node Text to see what happens if we go over the limit Child Node {i} : n={++n}", Children = new List<ITreeNode>() });

            //    for (int j = 0; j < 3; j++)
            //    {
            //        c1.Children.Add(c2 = new TreeNode { IsExpanded = true, Text = $"Sub Node {j} : n={++n}", Children = new List<ITreeNode>() });

            //        for (int k = 0; k < 3; k++)
            //        {
            //            c2.Children.Add(c3 = new TreeNode { IsExpanded = true, Text = $"Sub-Sub Node {k} : n={++n}", Children = new List<ITreeNode>() });

            //            for (int l = 0; l < 3; l++)
            //            {
            //                c3.Children.Add(new TreeNode { IsExpanded = true, Text = $"Sub-Sub-Sub Node {l} : n={++n}", Children = new List<ITreeNode>() });
            //            }
            //        }
            //    }

            //}


            //treeView.RootNode = rootNode;




            //var label2 = new Label
            //{
            //    Text = new Binding<string>(() => "Label 2"),
            //    HorizontalContentAlignment = HorizontalAlignmentType.Center,
            //    VerticalContentAlignment = VerticalAlignmentType.Center,
            //    TextColor = Color.White,
            //    SpriteFont = fontArial8,
            //    Background = Color.DimGray
            //};
            //gridMainLayout.AddChild(label2, 1, 0);

            //var image1 = new Image
            //{

            //    HorizontalContentAlignment = HorizontalAlignmentType.Center,
            //    VerticalContentAlignment = VerticalAlignmentType.Center,
            //    Background = Color.Red,
            //    BackgroundTexture = game.Content.Load<Texture2D>("Images/blade_logo_1116x540")
            //};
            //gridMainLayout.AddChild(image1, 1, 0);

            //var animationEditor = new AnimationEditor
            //{

            //    HorizontalContentAlignment = HorizontalAlignmentType.Center,
            //    VerticalContentAlignment = VerticalAlignmentType.Center,
            //    Background = Color.DimGray
            //};
            //gridMainLayout.AddChild(animationEditor, 1, 0);


        }


        //public static BladeWindow BuildScreen(Game game)
        //{
        //    //var window1 = new BladeWindow();

        //    //var label2 = new Label();
        //    //label2.Name = "Label1";
        //    //label2.Text = "Test Label";
        //    //label2.Width = 100;
        //    //label2.Height = 20;

        //    //window1.Children.Add(label2);
        //    //return window1;


        //    /*
        //    var window = new BladeWindow();

        //    var panel1 = new BladePanel();
        //    panel1.Width = float.NaN; // Auto
        //    panel1.Height = float.NaN; // Auto 
        //    //panel1.Width = 250;
        //    //panel1.Height = 250;
        //    panel1.SetBackground(Color.green, 0.5f);

        //    var panel2 = new BladePanel();
        //    panel2.Width = 150;
        //    panel2.Height = 150;
        //    panel2.SetBackground(Color.blue, 0.5f);

        //    panel1.Children.Add(panel2);

        //    window.Children.Add(panel1);
        //    return window;
        //    */



        //    var window = new BladeWindow();
        //    window.Init(game);

        //    var grid1 = new Grid();
        //    grid1.Name = "GRID1";
        //    grid1.Width = float.NaN;
        //    //grid1.Width = 200;
        //    grid1.Height = float.NaN;
        //    //grid1.SetBackground(Color.cyan, 0.5f);

        //    grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
        //    grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
        //    grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });

        //    grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
        //    grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
        //    grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });


        //    window.AddChild(grid1);


        //    var panel1 = new Panel();
        //    panel1.Name = "PANEL1";
        //    panel1.Width = float.NaN;
        //    panel1.Height = float.NaN;
        //    //panel1.Width = 250;
        //    //panel1.Height = 250;
        //    panel1.HorizontalAlignment = HorizontalAlignmentType.Stretch;
        //    panel1.VerticalAlignment = VerticalAlignmentType.Stretch;
        //    panel1.Background = new Color(Color.Red, 0.5f);
        //    grid1.AddChild(panel1, 0, 0);
        //    grid1.SetColumn(panel1, 0);
        //    grid1.SetRow(panel1, 0);


        //    var panel2 = new Panel();
        //    panel2.Name = "PANEL2";
        //    panel2.Width = 150;
        //    panel2.Height = 150;
        //    panel2.Background = new Color(Color.Green, 0.5f);
        //    panel2.HorizontalAlignment = HorizontalAlignmentType.Right;
        //    panel2.VerticalAlignment = VerticalAlignmentType.Bottom;

        //    grid1.AddChild(panel2, 1, 0);


        //    var panel3 = new Panel();
        //    panel3.Name = "PANEL3";
        //    panel3.Width = float.NaN;
        //    panel3.Height = float.NaN;
        //    panel3.Width = 150;
        //    panel3.Height = 150;
        //    panel3.HorizontalAlignment = HorizontalAlignmentType.Right;
        //    panel3.VerticalAlignment = VerticalAlignmentType.Top;
        //    panel3.Background = new Color(Color.Blue, 0.5f);
        //    grid1.AddChild(panel3, 2, 0);


        //    var panel4 = new Panel();
        //    panel4.Name = "PANEL4";
        //    panel4.Width = 50;
        //    panel4.Height = 50;
        //    panel4.Background = new Color(Color.Yellow, 0.5f);
        //    panel4.HorizontalAlignment = HorizontalAlignmentType.Left;
        //    panel4.VerticalAlignment = VerticalAlignmentType.Top;

        //    grid1.AddChild(panel4, 0, 1);


        //    var panel5 = new Panel();
        //    panel5.Name = "PANEL5";
        //    panel5.Background = new Color(Color.Cyan, 0.5f);
        //    panel5.HorizontalAlignment = HorizontalAlignmentType.Stretch;
        //    panel5.VerticalAlignment = VerticalAlignmentType.Stretch;

        //    grid1.AddChild(panel5, 1, 1);

        //    var panel6 = new Panel();
        //    panel6.Name = "PANEL6";
        //    panel6.Background = new Color(Color.Gray, 0.5f);
        //    panel6.HorizontalAlignment = HorizontalAlignmentType.Stretch;
        //    panel6.VerticalAlignment = VerticalAlignmentType.Stretch;

        //    grid1.AddChild(panel6, 2, 1);


        //    var panel7 = new Panel();
        //    panel7.Name = "PANEL7";
        //    panel7.Background = new Color(Color.White, 0.5f);
        //    panel7.HorizontalAlignment = HorizontalAlignmentType.Left;
        //    panel7.VerticalAlignment = VerticalAlignmentType.Top;

        //    grid1.AddChild(panel7, 0, 2);


        //    var panel8 = new Panel();
        //    panel8.Name = "PANEL5";
        //    panel8.Background = new Color(Color.Magenta, 0.5f);
        //    panel8.HorizontalAlignment = HorizontalAlignmentType.Stretch;
        //    panel8.VerticalAlignment = VerticalAlignmentType.Stretch;

        //    grid1.AddChild(panel8, 1, 2);

        //    var panel9 = new Panel();
        //    panel9.Name = "PANEL6";
        //    panel9.Background = new Color(Color.Yellow, 0.5f);
        //    panel9.HorizontalAlignment = HorizontalAlignmentType.Stretch;
        //    panel9.VerticalAlignment = VerticalAlignmentType.Stretch;

        //    grid1.AddChild(panel9, 2, 2);



        //    var label1 = new Label();
        //    label1.Name = "Label1";
        //    label1.Text = "Test Label";
        //    //label1.Width = 100;
        //    //label1.Height = 20;
        //    label1.HorizontalAlignment = HorizontalAlignmentType.Center;
        //    label1.VerticalAlignment = VerticalAlignmentType.Center;
        //    label1.FontColor = Color.White;

        //    //grid1.Children.Add(label1);
        //    //grid1.SetColumn(label1, 0);
        //    //grid1.SetRow(label1, 1);
        //    panel4.AddChild(label1);


        //    //var text1 = new TextBox();
        //    //text1.Name = "Text1";
        //    //text1.Text = "Hello, World!";
        //    //text1.Width = 100;
        //    //text1.Height = 100;

        //    //grid1.Children.Add(text1);
        //    //grid1.SetColumn(text1, 1);
        //    //grid1.SetRow(text1, 1);

        //    //var rect1 = new Rectangle();
        //    //rect1.SetBackground(Color.yellow, 0.5f);
        //    //grid1.Children.Add(rect1);
        //    //grid1.SetColumn(rect1, 0);
        //    //grid1.SetRow(rect1, 1);



        //    return window;
        //}

        //public BladeWindow BuildScreen2(Game game)
        //{
        //    var window = new BladeWindow();
        //    window.Init(game);


        //    //var p1 = new Panel
        //    //{
        //    //    Background = Color.Brown,
        //    //    HorizontalAlignment = HorizontalAlignmentType.Left,
        //    //    VerticalAlignment = VerticalAlignmentType.Top,
        //    //    HorizontalContentAlignment = HorizontalAlignmentType.Left,
        //    //    VerticalContentAlignment = VerticalAlignmentType.Bottom,
        //    //    MinWidth = 50,
        //    //    MinHeight = 50,
        //    //    Margin = new Thickness(1, 25, 0, 0)
        //    //};
        //    //window.Children.Add(p1);
        //    //var l1 = new Label
        //    //{
        //    //    Text = "210",
        //    //    HorizontalAlignment = HorizontalAlignmentType.Stretch,
        //    //    VerticalAlignment = VerticalAlignmentType.Stretch,
        //    //    HorizontalContentAlignment = HorizontalAlignmentType.Left,
        //    //    VerticalContentAlignment = VerticalAlignmentType.Center,
        //    //    FontColor = Color.White,
        //    //    Margin = new Thickness(10),
        //    //    Padding = new Thickness(20, 200, 20, 200),
        //    //    Background = new Color(Color.DarkOliveGreen, 0.8f)
        //    //};
        //    //p1.Children.Add(l1);
        //    //return window;



        //    var grid1 = new Grid();
        //    //grid1.Name = "GRID1";
        //    //grid1.Background = Color.Red;

        //    grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
        //    grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
        //    grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 4f) });


        //    grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 20) });
        //    grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });
        //    //grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
        //    //grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
        //    //grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });

        //    window.AddChild(grid1);


        //    var panel1 = new Panel
        //    {
        //        Background = Color.Yellow
        //    };
        //    grid1.AddChild(panel1, 0, 1);

        //    var panel2 = new Panel
        //    {
        //        Background = Color.Blue
        //    };
        //    grid1.AddChild(panel2, 1, 1);

        //    var panel3 = new Panel
        //    {
        //        Name = "Panel3",
        //        Background = Color.Green,
        //        Margin = new Thickness(10),
        //        Padding = new Thickness(10)
        //    };
        //    grid1.AddChild(panel3, 1, 1);

        //    var panel4 = new Panel
        //    {
        //        Background = Color.Orange
        //    };
        //    panel3.AddChild(panel4);

        //    //var label1 = new Label
        //    //{
        //    //    Text = "Score",
        //    //    HorizontalAlignment = HorizontalAlignmentType.Stretch,
        //    //    VerticalAlignment = VerticalAlignmentType.Stretch,
        //    //    HorizontalContentAlignment = HorizontalAlignmentType.Right,
        //    //    VerticalContentAlignment = VerticalAlignmentType.Center,
        //    //    FontColor = Color.White,
        //    //    MinHeight = 100
        //    //    //Background = Color.DarkOliveGreen,
        //    //    //Margin = new Thickness(10)
        //    //};
        //    //grid1.Children.Add(label1);
        //    //grid1.SetColumn(label1, 0);
        //    //grid1.SetRow(label1, 1);

        //    panel4.AddChild(new Panel { HorizontalAlignment = HorizontalAlignmentType.Left, VerticalAlignment = VerticalAlignmentType.Top, Width = 50, Height = 50, Background = Color.Navy, Margin = new Thickness(15, 15, 0, 0) });
        //    panel4.AddChild(new Panel { HorizontalAlignment = HorizontalAlignmentType.Left, VerticalAlignment = VerticalAlignmentType.Bottom, Width = 50, Height = 50, Background = Color.Navy, Margin = new Thickness(15 + 50, 0, 0, 15) });

        //    var label2 = new Label
        //    {
        //        Text = "210",
        //        HorizontalAlignment = HorizontalAlignmentType.Stretch,
        //        VerticalAlignment = VerticalAlignmentType.Stretch,
        //        HorizontalContentAlignment = HorizontalAlignmentType.Left,
        //        VerticalContentAlignment = VerticalAlignmentType.Center,
        //        FontColor = Color.White,
        //        Margin = new Thickness(15),
        //        Padding = new Thickness(20, 50, 20, 50),
        //        Background = new Color(Color.DarkOliveGreen, 0.8f)
        //    };

        //    panel4.AddChild(label2);

        //    //grid1.Children.Add(label2);
        //    //grid1.SetColumn(label2, 1);
        //    //grid1.SetRow(label2, 1);

        //    //var panel2 = new Panel();
        //    //panel2.Name = "PANEL2";
        //    //panel2.Width = 150;
        //    //panel2.Height = 150;
        //    //panel2.SetBackground(new Color(Color.Green, 0.5f));
        //    //panel2.HorizontalAlignment = HorizontalAlignmentType.Right;
        //    //panel2.VerticalAlignment = VerticalAlignmentType.Bottom;

        //    //grid1.Children.Add(panel2);
        //    //grid1.SetColumn(panel2, 1);
        //    //grid1.SetRow(panel2, 0);


        //    //var panel3 = new Panel();
        //    //panel3.Name = "PANEL3";
        //    //panel3.Width = float.NaN;
        //    //panel3.Height = float.NaN;
        //    //panel3.Width = 150;
        //    //panel3.Height = 150;
        //    //panel3.HorizontalAlignment = HorizontalAlignmentType.Right;
        //    //panel3.VerticalAlignment = VerticalAlignmentType.Top;
        //    //panel3.SetBackground(new Color(Color.Blue, 0.5f));
        //    //grid1.Children.Add(panel3);
        //    //grid1.SetColumn(panel3, 2);
        //    //grid1.SetRow(panel3, 0);


        //    //var panel4 = new Panel();
        //    //panel4.Name = "PANEL4";
        //    //panel4.Width = 50;
        //    //panel4.Height = 50;
        //    //panel4.SetBackground(new Color(Color.Yellow, 0.5f));
        //    //panel4.HorizontalAlignment = HorizontalAlignmentType.Left;
        //    //panel4.VerticalAlignment = VerticalAlignmentType.Top;

        //    //grid1.Children.Add(panel4);
        //    //grid1.SetColumn(panel4, 0);
        //    //grid1.SetRow(panel4, 1);


        //    //var panel5 = new Panel();
        //    //panel5.Name = "PANEL5";
        //    //panel5.SetBackground(new Color(Color.Cyan, 0.5f));
        //    //panel5.HorizontalAlignment = HorizontalAlignmentType.Stretch;
        //    //panel5.VerticalAlignment = VerticalAlignmentType.Stretch;

        //    //grid1.Children.Add(panel5);
        //    //grid1.SetColumn(panel5, 1);
        //    //grid1.SetRow(panel5, 1);

        //    //var panel6 = new Panel();
        //    //panel6.Name = "PANEL6";
        //    //panel6.SetBackground(new Color(Color.Gray, 0.5f));
        //    //panel6.HorizontalAlignment = HorizontalAlignmentType.Stretch;
        //    //panel6.VerticalAlignment = VerticalAlignmentType.Stretch;

        //    //grid1.Children.Add(panel6);
        //    //grid1.SetColumn(panel6, 2);
        //    //grid1.SetRow(panel6, 1);


        //    //var panel7 = new Panel();
        //    //panel7.Name = "PANEL7";
        //    //panel7.SetBackground(new Color(Color.White, 0.5f));
        //    //panel7.HorizontalAlignment = HorizontalAlignmentType.Left;
        //    //panel7.VerticalAlignment = VerticalAlignmentType.Top;

        //    //grid1.Children.Add(panel7);
        //    //grid1.SetColumn(panel7, 0);
        //    //grid1.SetRow(panel7, 2);


        //    //var panel8 = new Panel();
        //    //panel8.Name = "PANEL5";
        //    //panel8.SetBackground(new Color(Color.Magenta, 0.5f));
        //    //panel8.HorizontalAlignment = HorizontalAlignmentType.Stretch;
        //    //panel8.VerticalAlignment = VerticalAlignmentType.Stretch;

        //    //grid1.Children.Add(panel8);
        //    //grid1.SetColumn(panel8, 1);
        //    //grid1.SetRow(panel8, 2);

        //    //var panel9 = new Panel();
        //    //panel9.Name = "PANEL6";
        //    //panel9.SetBackground(new Color(Color.Yellow, 0.5f));
        //    //panel9.HorizontalAlignment = HorizontalAlignmentType.Stretch;
        //    //panel9.VerticalAlignment = VerticalAlignmentType.Stretch;

        //    //grid1.Children.Add(panel9);
        //    //grid1.SetColumn(panel9, 2);
        //    //grid1.SetRow(panel9, 2);



        //    //var label1 = new Label();
        //    //label1.Name = "Label1";
        //    //label1.Text = "Test Label";
        //    ////label1.Width = 100;
        //    ////label1.Height = 20;
        //    //label1.HorizontalAlignment = HorizontalAlignmentType.Center;
        //    //label1.VerticalAlignment = VerticalAlignmentType.Center;
        //    //label1.FontColor = Color.White;

        //    ////grid1.Children.Add(label1);
        //    ////grid1.SetColumn(label1, 0);
        //    ////grid1.SetRow(label1, 1);
        //    //panel4.Children.Add(label1);


        //    ////var text1 = new TextBox();
        //    ////text1.Name = "Text1";
        //    ////text1.Text = "Hello, World!";
        //    ////text1.Width = 100;
        //    ////text1.Height = 100;

        //    ////grid1.Children.Add(text1);
        //    ////grid1.SetColumn(text1, 1);
        //    ////grid1.SetRow(text1, 1);

        //    ////var rect1 = new Rectangle();
        //    ////rect1.SetBackground(Color.yellow, 0.5f);
        //    ////grid1.Children.Add(rect1);
        //    ////grid1.SetColumn(rect1, 0);
        //    ////grid1.SetRow(rect1, 1);



        //    return window;
        //}

        // void ScoreTable()
        // {
        //     var win = Screen.width * 0.6;
        //     var w1 = win * 0.35; var w2 = win * 0.15; var w3 = win * 0.35;

        //for (var line in Scores.Split("\n"[0]))
        //     {
        //         fields = line.Split("\t"[0]);
        //         if (fields.length >= 3)
        //         {
        //             GUILayout.BeginHorizontal();
        //             GUILayout.Label(fields[0], GUILayout.Width(w1));
        //             GUILayout.Label(fields[1], GUILayout.Width(w2));
        //             GUILayout.Label(fields[2], GUILayout.Width(w3));
        //             GUILayout.EndHorizontal();
        //         }
        //     }
        // }


        //private void TestBinding()
        //{

        //    string value1 = "value 1";

        //    Class1 class1 = new Class1();

        //    class1.bindings.Add(new Binding<string>(() => value1, (p) => value1 = p));

        //}


        //class Class1 : TestBind
        //{
        //    public string field1 = "field1_vlaue";

        //    private string prop1 = "prop1_value";
        //    public string Prop1 { get { return prop1; } set { Set(ref prop1, value); } }
        //}


        //public class BindInfo
        //{
        //    public Type BindType;
        //    public Object Bind1;
        //    public Object Bind2;
        //}

        ///// <summary>
        ///// Implementation of <see cref="INotifyPropertyChanged"/>
        ///// </summary>
        //public abstract class TestBind : INotifyPropertyChanged
        //{
        //    /// <summary>
        //    /// Multicast event for property change notifications.
        //    /// </summary>
        //    public event PropertyChangedEventHandler PropertyChanged;


        //    //public List<BindInfo> bindings = new List<BindInfo>();
        //    public List<IBinding> bindings = new List<IBinding>();

        //    public void Bind<T>(ref T bind1, ref T bind2)
        //    {
        //        //this.bindings.Add(new BindInfo
        //        //{
        //        //    BindType = typeof(T),
        //        //    Bind1 = bind1,
        //        //    Bind2 = bind2
        //        //});
        //    }

        //    //abstract void CheckBindings(ref T storage, T value, [CallerMemberName] string propertyName = null);

        //    /// <summary>
        //    /// 
        //    /// </summary>
        //    /// <param name="propertyName"></param>
        //    public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        //    {
        //        try
        //        {
        //            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //            var propertyChanged = PropertyChanged;
        //            if (propertyChanged != null)
        //            {
        //                propertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //            }
        //        }
        //        catch
        //        {
        //            // nothing
        //        }
        //    }

        //    //private Dictionary<string, List<Action>> PropertyListeners = new Dictionary<string, List<Action>>();

        //    /// <summary>
        //    /// Checks if a property already matches a desired value.  Sets the property and
        //    /// notifies listeners only when necessary.
        //    /// </summary>
        //    /// <typeparam name="T">Type of the property.</typeparam>
        //    /// <param name="storage">Reference to a property with both getter and setter.</param>
        //    /// <param name="value">Desired value for the property.</param>
        //    /// <param name="propertyName">Name of the property used to notify listeners.  This
        //    /// value is optional and can be provided automatically when invoked from compilers that
        //    /// support CallerMemberName.</param>
        //    /// <returns>True if the value was changed, false if the existing value matched the
        //    /// desired value.</returns>
        //    protected bool Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        //    {
        //        if (object.Equals(storage, value))
        //            return false;

        //        storage = value;
        //        RaisePropertyChanged(propertyName);

        //        return true;
        //    }

        //}


    }
}
