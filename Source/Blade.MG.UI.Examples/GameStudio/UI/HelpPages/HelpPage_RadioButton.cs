using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;

namespace Examples.UI.HelpPages
{
    public class HelpPage_RadioButton : Panel
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
                    Title = "Radio Buttons",
                    Description = "RadioButtons sharing the same GroupName are mutually exclusive - checking one unchecks the rest of the group."
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

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });

            layoutPanel.AddChild(grid);

            grid.AddChild(BuildGroupSection("Size", new[] { "Small", "Medium", "Large" }, checkedIndex: 1), 0, 0);
            grid.AddChild(BuildGroupSection("Shipping", new[] { "Standard", "Express", "Overnight" }, checkedIndex: 0), 1, 0);
            grid.AddChild(BuildDisabledSection(), 2, 0);
        }

        private static Section BuildGroupSection(string groupName, string[] options, int checkedIndex)
        {
            var stack = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };

            for (int i = 0; i < options.Length; i++)
            {
                stack.AddChild(new RadioButton()
                {
                    Text = options[i],
                    GroupName = groupName,
                    IsChecked = i == checkedIndex,
                    Margin = new Thickness(0, 4),
                    HorizontalAlignment = HorizontalAlignmentType.Left,
                });
            }

            return new Section() { Content = stack, Margin = new Thickness(5, 10) };
        }

        private static Section BuildDisabledSection()
        {
            var stack = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };

            stack.AddChild(new RadioButton()
            {
                Text = "Disabled (checked)",
                GroupName = "DisabledExample",
                IsChecked = true,
                IsEnabled = false,
                Margin = new Thickness(0, 4),
                HorizontalAlignment = HorizontalAlignmentType.Left,
            });

            stack.AddChild(new RadioButton()
            {
                Text = "Disabled (unchecked)",
                GroupName = "DisabledExample",
                IsChecked = false,
                IsEnabled = false,
                Margin = new Thickness(0, 4),
                HorizontalAlignment = HorizontalAlignmentType.Left,
            });

            stack.AddChild(new RadioButton()
            {
                Text = "Standalone (no group)",
                IsChecked = false,
                Margin = new Thickness(0, 4),
                HorizontalAlignment = HorizontalAlignmentType.Left,
            });

            return new Section() { Content = stack, Margin = new Thickness(5, 10) };
        }
    }
}
