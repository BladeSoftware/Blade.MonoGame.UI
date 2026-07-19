using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;

namespace Examples.UI.HelpPages
{
    public class HelpPage_ToggleSwitch : Panel
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
                    Title = "Toggle Switches",
                    Description = "ToggleSwitch states: On, Off, and disabled - click anywhere on a switch (or press Space/Enter while it's focused) to flip it."
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

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 90) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 90) });

            layoutPanel.AddChild(grid);

            // Row 0: enabled, On and Off
            grid.AddChild(BuildField(new ToggleSwitch()
            {
                Text = "On",
                IsEnabled = true,
                IsOn = true,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Center,
            }), 0, 0);

            grid.AddChild(BuildField(new ToggleSwitch()
            {
                Text = "Off",
                IsEnabled = true,
                IsOn = false,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Center,
            }), 1, 0);

            grid.AddChild(BuildField(new ToggleSwitch()
            {
                Text = "Notifications",
                IsEnabled = true,
                IsOn = true,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Center,
            }), 2, 0);

            // Row 1: disabled, On and Off
            grid.AddChild(BuildField(new ToggleSwitch()
            {
                Text = "Disabled (On)",
                IsEnabled = false,
                IsOn = true,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Center,
            }), 0, 1);

            grid.AddChild(BuildField(new ToggleSwitch()
            {
                Text = "Disabled (Off)",
                IsEnabled = false,
                IsOn = false,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Center,
            }), 1, 1);
        }

        private static Section BuildField(UIComponent field)
        {
            field.Margin = new Thickness(5, 10);

            return new Section() { Content = field };
        }
    }
}
