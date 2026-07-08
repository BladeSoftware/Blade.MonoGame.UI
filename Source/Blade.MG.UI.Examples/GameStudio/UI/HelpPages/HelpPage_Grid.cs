using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Theming;
using Examples.UI.Components;
using Microsoft.Xna.Framework;
using System;

namespace Examples.UI.HelpPages
{
    public class HelpPage_Grid : Panel
    {
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
                    Title = "Grid",
                    Description = "Rows and columns sized in Star units share space proportionally, like flex-grow."
                });

            var border = new Border()
            {
                BorderThickness = new Thickness(1),
                BorderColor = new Binding<Color>(() => Theme.Outline),
                Margin = new Thickness(30, 10, 30, 20),
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Height = 500,
            };

            layoutPanel.AddChild(border);
            layoutPanel.StretchLastChild = true;

            var grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Margin = new Thickness(4),
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 2f) });

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });

            border.Content = grid;

            grid.AddChild(BuildCell("Col 0 / Row 0", t => t.PrimaryContainer, t => t.OnPrimaryContainer), 0, 0);
            grid.AddChild(BuildCell("Col 1 / Row 0 (2x width)", t => t.SecondaryContainer, t => t.OnSecondaryContainer), 1, 0);
            grid.AddChild(BuildCell("Col 0 / Row 1", t => t.TertiaryContainer, t => t.OnTertiaryContainer), 0, 1);
            grid.AddChild(BuildCell("Col 1 / Row 1 (2x width)", t => t.ErrorContainer, t => t.OnErrorContainer), 1, 1);
        }

        private Control BuildCell(string text, Func<UITheme, Color> background, Func<UITheme, Color> foreground)
        {
            var cell = new Control()
            {
                Background = new Binding<Color>(() => background(Theme)),
                Margin = new Thickness(4),
            };

            cell.Content = new Label()
            {
                Text = text,
                TextColor = new Binding<Color>(() => foreground(Theme)),
                HorizontalAlignment = HorizontalAlignmentType.Center,
                VerticalAlignment = VerticalAlignmentType.Center,
            };

            return cell;
        }

    }
}
