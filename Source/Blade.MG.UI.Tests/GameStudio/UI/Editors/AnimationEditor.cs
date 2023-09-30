//using Blade.UI.Components;
//using Blade.UI.Controls;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;

//namespace GameStudio.UI.Editors
//{
//    public class AnimationEditor : Panel
//    {
//        protected override void InitTemplate()
//        {
//            base.InitTemplate();

//            var gridMainLayout = new Grid()
//            {
//                //Height = 14,
//                //Background = Color.Transparent,
//                //Background = new Color(Color.DarkSlateBlue, 0.5f),
//                Background = new Color(Color.DarkOrange, 0.5f),
//                Margin = new Thickness(0, 0, 0, 0),
//                Padding = new Thickness(0, 0, 0, 0)
//            };


//            gridMainLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });

//            gridMainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
//            gridMainLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });

//            base.AddChild(gridMainLayout);


//            //var image1 = new Image
//            //{

//            //    HorizontalContentAlignment = HorizontalAlignmentType.Center,
//            //    VerticalContentAlignment = VerticalAlignmentType.Center,
//            //    Background = Color.Red,
//            //    BackgroundTexture = Content.Load<Texture2D>("Images/blade_logo_1116x540")
//            //};
//            //gridMainLayout.AddChild(image1, 0, 0);


//            var stackPanel = new StackPanel
//            {
//                Orientation = Orientation.Horizontal,
//                Background = Color.DarkBlue,
//                HorizontalAlignment = HorizontalAlignmentType.Stretch,
//                VerticalAlignment = VerticalAlignmentType.Stretch,

//                HorizontalContentAlignment = HorizontalAlignmentType.Left,
//                VerticalContentAlignment = VerticalAlignmentType.Center,

//                //Margin = new Thickness(50, 50, 50, 50),
//                //Padding = new Thickness(50, 50, 50, 50),


//                //MaxHeight = 300,
//                //MinHeight = 300

//                //MaxWidth = 300
//                //MinHeight = 300


//            };

//            gridMainLayout.AddChild(stackPanel, 0, 1);

//            stackPanel.AddChild(new Image { BackgroundTexture = ContentManager.Load<Texture2D>("Images/lash/lash_freefall_dive_1"), VerticalAlignment = VerticalAlignmentType.Center });
//            stackPanel.AddChild(new Image { BackgroundTexture = ContentManager.Load<Texture2D>("Images/lash/lash_freefall_dive_2"), VerticalAlignment = VerticalAlignmentType.Center });
//            stackPanel.AddChild(new Image { BackgroundTexture = ContentManager.Load<Texture2D>("Images/lash/lash_freefall_dive_3"), VerticalAlignment = VerticalAlignmentType.Center });
//            stackPanel.AddChild(new Image { BackgroundTexture = ContentManager.Load<Texture2D>("Images/lash/lash_freefall_dive_4"), VerticalAlignment = VerticalAlignmentType.Center });
//            stackPanel.AddChild(new Image { BackgroundTexture = ContentManager.Load<Texture2D>("Images/lash/lash_freefall_dive_5"), VerticalAlignment = VerticalAlignmentType.Center });
//            stackPanel.AddChild(new Image { BackgroundTexture = ContentManager.Load<Texture2D>("Images/lash/lash_freefall_dive_6"), VerticalAlignment = VerticalAlignmentType.Center });

//            stackPanel.AddChild(new Image { BackgroundTexture = ContentManager.Load<Texture2D>("Images/lash/lash_freefall_para_1"), VerticalAlignment = VerticalAlignmentType.Center });
//            stackPanel.AddChild(new Image { BackgroundTexture = ContentManager.Load<Texture2D>("Images/lash/lash_freefall_para_2"), VerticalAlignment = VerticalAlignmentType.Center });
//            stackPanel.AddChild(new Image { BackgroundTexture = ContentManager.Load<Texture2D>("Images/lash/lash_freefall_para_3"), VerticalAlignment = VerticalAlignmentType.Center });
//            stackPanel.AddChild(new Image { BackgroundTexture = ContentManager.Load<Texture2D>("Images/lash/lash_freefall_para_4"), VerticalAlignment = VerticalAlignmentType.Center });
//            stackPanel.AddChild(new Image { BackgroundTexture = ContentManager.Load<Texture2D>("Images/lash/lash_freefall_para_5"), VerticalAlignment = VerticalAlignmentType.Center });
//            stackPanel.AddChild(new Image { BackgroundTexture = ContentManager.Load<Texture2D>("Images/lash/lash_freefall_para_6"), VerticalAlignment = VerticalAlignmentType.Center });
//            stackPanel.AddChild(new Image { BackgroundTexture = ContentManager.Load<Texture2D>("Images/lash/lash_freefall_para_7"), VerticalAlignment = VerticalAlignmentType.Center });

//            stackPanel.AddChild(new Image { BackgroundTexture = ContentManager.Load<Texture2D>("Images/lash/lash_freefall_static_1"), VerticalAlignment = VerticalAlignmentType.Center });
//            stackPanel.AddChild(new Image { BackgroundTexture = ContentManager.Load<Texture2D>("Images/lash/lash_freefall_static_2"), VerticalAlignment = VerticalAlignmentType.Center });
//            stackPanel.AddChild(new Image { BackgroundTexture = ContentManager.Load<Texture2D>("Images/lash/lash_freefall_static_3"), VerticalAlignment = VerticalAlignmentType.Center });


//        }

//    }
//}
