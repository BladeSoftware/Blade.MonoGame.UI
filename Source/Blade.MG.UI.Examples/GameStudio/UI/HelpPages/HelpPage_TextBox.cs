using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;

namespace Examples.UI.HelpPages
{
    public class HelpPage_TextBox : Panel
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
                    Title = "Text Boxes",
                    Description = "TextBox variants: Standard, Filled, and Outlined - each shown enabled and disabled."
                });

            var grid = new Grid()
            {
                Width = "80%",
                HorizontalAlignment = HorizontalAlignmentType.Center,
                Margin = new Thickness(0, 20, 0, 0),
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 150) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 150) });

            layoutPanel.AddChild(grid);

            // Row 0: enabled variants
            grid.AddChild(BuildField(new TextBox()
            {
                Text = "Hello, World!",
                Label = "Standard",
                HelperText = "Variant.Standard",
                Variant = Variant.Standard,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
            }), 0, 0);

            grid.AddChild(BuildField(new TextBox()
            {
                Text = "Hello, World!",
                Label = "Filled",
                HelperText = "Variant.Filled",
                Variant = Variant.Filled,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
            }), 1, 0);

            grid.AddChild(BuildField(new TextBox()
            {
                Text = "Hello, World!",
                Label = "Outlined",
                HelperText = "Variant.Outlined",
                Variant = Variant.Outlined,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
            }), 2, 0);

            // Row 1: disabled variants
            grid.AddChild(BuildField(new TextBox()
            {
                Text = "Hello, World!",
                Label = "Standard (disabled)",
                Variant = Variant.Standard,
                IsEnabled = false,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
            }), 0, 1);

            grid.AddChild(BuildField(new TextBox()
            {
                Text = "Hello, World!",
                Label = "Filled (disabled)",
                Variant = Variant.Filled,
                IsEnabled = false,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
            }), 1, 1);

            grid.AddChild(BuildField(new TextBox()
            {
                Text = "Hello, World!",
                Label = "Outlined (disabled)",
                Variant = Variant.Outlined,
                IsEnabled = false,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
            }), 2, 1);
        }

        private static Section BuildField(UIComponent field)
        {
            field.Margin = new Thickness(5, 10);

            return new Section() { Content = field };
        }
    }
}
