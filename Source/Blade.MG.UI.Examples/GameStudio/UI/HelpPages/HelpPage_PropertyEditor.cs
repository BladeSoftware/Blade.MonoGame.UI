using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;
using GameStudio;
using Microsoft.Xna.Framework;
using System.ComponentModel.DataAnnotations;

namespace Blade.MG.UI.Examples.GameStudio.UI.HelpPages
{
    public class HelpPage_PropertyEditor : Panel
    {
        public class ExampleSettings
        {
            public string Name { get; set; }
            public int Health { get; set; }
            public bool IsActive { get; set; }
            public DifficultyLevel Difficulty { get; set; }
            public Country Country { get; set; }
        }

        public enum DifficultyLevel
        {
            Easy,
            Medium,
            Hard
        }


        public HelpPage_PropertyEditor()
        {

        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            var exampleObject = new ExampleSettings
            {
                Name = "Player",
                Health = 100,
                IsActive = true,
                Difficulty = DifficultyLevel.Medium
            };



            var layoutPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,

                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };


            base.AddChild(layoutPanel);

            layoutPanel.AddChild(
                new PageHeader()
                {
                    Padding = new Thickness(30, 0, 0, 0),
                    Title = "Property Editor"
                });


            //var horizStack = new StackPanel()
            //{
            //    Orientation = Orientation.Horizontal,
            //    HorizontalAlignment = HorizontalAlignmentType.Stretch,
            //    VerticalAlignment = VerticalAlignmentType.Top,
            //    Margin = new Thickness(20, 10, 20, 10),
            //    HorizontalScrollBarVisible = false,
            //    VerticalScrollBarVisible = false,
            //};

            //layoutPanel.AddChild(horizStack);



            var border = new Border()
            {
                BorderThickness = new Thickness(2),
                BorderColor = Color.Gray,
                Margin = new Thickness(20),
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                //Background = Color.HotPink,
                //Width = 800,
                Height = 600,
            };

            layoutPanel.AddChild(border);



            var propertyEditor = new PropertyEditor
            {
                TargetObject = exampleObject,
                Margin = new Thickness(20),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };

            border.Content = propertyEditor;
            //layoutPanel.AddChild(propertyEditor);


            //// Testing
            //var grid = new Grid
            //{
            //    Name = "Example_Grid",
            //    Margin = new Thickness(4),
            //    Padding = new Thickness(4),
            //    HorizontalAlignment = HorizontalAlignmentType.Stretch,
            //    VerticalAlignment = VerticalAlignmentType.Top,

            //    Background = Color.HotPink,

            //};

            //grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            //grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });

            //grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 30) });
            //grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 30) });
            //grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 30) });

            ////stackPanel.AddChild(grid);
            //layoutPanel.AddChild(grid);

            //grid.AddChild(new Panel
            //{
            //    Background = Color.AliceBlue,
            //}, 0, 0);

            //grid.AddChild(new Panel
            //{
            //    Background = Color.CornflowerBlue,
            //}, 1, 0);

            //grid.AddChild(new Panel
            //{
            //    Background = Color.YellowGreen,
            //}, 0, 1);

            //grid.AddChild(new Panel
            //{
            //    Background = Color.Orange,
            //}, 1, 1);

        }


    }
}