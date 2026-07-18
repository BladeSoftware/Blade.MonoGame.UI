using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;
using Microsoft.Xna.Framework;

namespace Examples.UI.HelpPages
{
    public class HelpPage_Slider : Panel
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
                    Title = "Sliders",
                    Description = "Drag the thumb, click the track, or use arrow keys/Home/End once focused."
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

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 220) });

            layoutPanel.AddChild(grid);

            grid.AddChild(BuildHorizontalSlider(0f, 100f, 40f), 0, 0);
            grid.AddChild(BuildSteppedSlider(), 1, 0);
            grid.AddChild(BuildVerticalSlider(), 2, 0);
        }

        private Section BuildHorizontalSlider(float min, float max, float initialValue)
        {
            var valueLabel = new Label()
            {
                Text = initialValue.ToString("0"),
                TextColor = new Binding<Color>(() => Theme.OnSurface),
                HorizontalAlignment = HorizontalAlignmentType.Center,
                Margin = new Thickness(0, 0, 0, 12),
            };

            var slider = new Slider()
            {
                Orientation = Orientation.Horizontal,
                Minimum = min,
                Maximum = max,
                Value = initialValue,
                Width = 200,
                HorizontalAlignment = HorizontalAlignmentType.Center,
                OnValueChanged = v => valueLabel.Text.Value = v.ToString("0"),
            };

            var stack = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Center,
                VerticalAlignment = VerticalAlignmentType.Center,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };
            stack.AddChild(new Label { Text = "Horizontal (0-100)", HorizontalAlignment = HorizontalAlignmentType.Center, Margin = new Thickness(0, 0, 0, 8) });
            stack.AddChild(valueLabel);
            stack.AddChild(slider);

            return new Section() { Content = stack, Margin = new Thickness(5, 10) };
        }

        private Section BuildSteppedSlider()
        {
            var valueLabel = new Label()
            {
                Text = "5",
                TextColor = new Binding<Color>(() => Theme.OnSurface),
                HorizontalAlignment = HorizontalAlignmentType.Center,
                Margin = new Thickness(0, 0, 0, 12),
            };

            var slider = new Slider()
            {
                Orientation = Orientation.Horizontal,
                Minimum = 0f,
                Maximum = 10f,
                Value = 5f,
                Width = 200,
                HorizontalAlignment = HorizontalAlignmentType.Center,
                OnValueChanged = v => valueLabel.Text.Value = v.ToString("0"),
            };

            var stack = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Center,
                VerticalAlignment = VerticalAlignmentType.Center,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };
            stack.AddChild(new Label { Text = "Narrow range (0-10)", HorizontalAlignment = HorizontalAlignmentType.Center, Margin = new Thickness(0, 0, 0, 8) });
            stack.AddChild(valueLabel);
            stack.AddChild(slider);

            return new Section() { Content = stack, Margin = new Thickness(5, 10) };
        }

        private Section BuildVerticalSlider()
        {
            var valueLabel = new Label()
            {
                Text = "50",
                TextColor = new Binding<Color>(() => Theme.OnSurface),
                HorizontalAlignment = HorizontalAlignmentType.Center,
                Margin = new Thickness(0, 8, 0, 0),
            };

            var slider = new Slider()
            {
                Orientation = Orientation.Vertical,
                Minimum = 0f,
                Maximum = 100f,
                Value = 50f,
                HorizontalAlignment = HorizontalAlignmentType.Center,
                OnValueChanged = v => valueLabel.Text.Value = v.ToString("0"),
            };

            // A vertical Slider always stretches to fill its available height (see
            // Slider.Measure) - nested directly as a StackPanel's stacking-axis child it would
            // get auto/natural height instead of a stretchable budget (like any other middle
            // child of a vertical StackPanel) and collapse to zero. Wrapping it in a Panel with
            // an explicit Height gives it concrete space to stretch into.
            var sliderHost = new Panel
            {
                Height = 120,
                HorizontalAlignment = HorizontalAlignmentType.Center,
            };
            sliderHost.AddChild(slider);

            var stack = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Center,
                VerticalAlignment = VerticalAlignmentType.Center,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };
            stack.AddChild(new Label { Text = "Vertical (0-100)", HorizontalAlignment = HorizontalAlignmentType.Center, Margin = new Thickness(0, 0, 0, 8) });
            stack.AddChild(sliderHost);
            stack.AddChild(valueLabel);

            return new Section() { Content = stack, Margin = new Thickness(5, 10) };
        }
    }
}
