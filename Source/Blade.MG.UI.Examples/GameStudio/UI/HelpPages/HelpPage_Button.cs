using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;

namespace Examples.UI.HelpPages
{
    public class HelpPage_Button : Panel
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
                    Title = "Buttons",
                    Description = "HorizontalAlignment / VerticalAlignment position a button within its cell."
                });

            var alignmentGrid = new Grid()
            {
                Width = "80%",
                HorizontalAlignment = HorizontalAlignmentType.Center,
                Margin = new Thickness(0, 20, 0, 0),
            };

            alignmentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });
            alignmentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });
            alignmentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });

            alignmentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 150) });
            alignmentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 150) });
            alignmentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 150) });

            layoutPanel.AddChild(alignmentGrid);

            var verticalAligns = new[] { VerticalAlignmentType.Top, VerticalAlignmentType.Center, VerticalAlignmentType.Bottom };
            var horizontalAligns = new[] { HorizontalAlignmentType.Left, HorizontalAlignmentType.Center, HorizontalAlignmentType.Right };

            for (int row = 0; row < verticalAligns.Length; row++)
            {
                for (int col = 0; col < horizontalAligns.Length; col++)
                {
                    var vAlign = verticalAligns[row];
                    var hAlign = horizontalAligns[col];

                    alignmentGrid.AddChild(
                        new Section()
                        {
                            Content = new Button()
                            {
                                Text = $"{hAlign} / {vAlign}",
                                MaxWidth = "75%",
                                Height = "40%",
                                HorizontalAlignment = hAlign,
                                VerticalAlignment = vAlign,
                            }
                        },
                        col, row);
                }
            }
        }

    }
}
