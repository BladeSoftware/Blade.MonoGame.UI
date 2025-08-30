using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Examples.GameStudio.UI.HelpPages
{
    public class HelpPage_Grid : Panel
    {

        public HelpPage_Grid()
        {

        }

        protected override void InitTemplate()
        {
            base.InitTemplate();


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
                    Title = "Grid"
                });



            var border = new Border()
            {
                BorderThickness = 2,
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
            layoutPanel.StretchLastChild = true;


            var grid = new Grid
            {
                Name = "Example_Grid",
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Margin = new Thickness(4),
                Padding = new Thickness(4),
                Background = Color.HotPink,
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });

            //grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 30) });
            //grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 30) });
            //grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 30) });

            //layoutPanel.AddChild(grid);
            border.Content = grid;

            grid.AddChild(new Panel
            {
                Background = Color.AliceBlue,
            }, 0, 0);

            grid.AddChild(new Panel
            {
                Background = Color.CornflowerBlue,
            }, 1, 0);

            grid.AddChild(new Panel
            {
                Background = Color.YellowGreen,
            }, 0, 1);

            grid.AddChild(new Panel
            {
                Background = Color.Orange,
            }, 1, 1);

        }


    }
}