using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;
using Microsoft.Xna.Framework;

namespace Examples.UI.HelpPages
{
    public class HelpPage_ProgressBar : Panel
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
                    Title = "Progress Bars",
                    Description = "Determinate (bound to a value) and indeterminate (animated sweep, for unknown-duration work) progress."
                });

            var grid = new Grid()
            {
                Width = "80%",
                HorizontalAlignment = HorizontalAlignmentType.Center,
                Margin = new Thickness(0, 20, 0, 0),
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });

            layoutPanel.AddChild(grid);

            grid.AddChild(BuildInteractiveSection(), 0, 0);
            grid.AddChild(BuildFixedValueSection(), 0, 1);
            grid.AddChild(BuildIndeterminateSection(), 0, 2);
        }

        // Slider drives the ProgressBar's Value directly - a live demo of both controls together.
        private Section BuildInteractiveSection()
        {
            var progressBar = new ProgressBar()
            {
                Minimum = 0f,
                Maximum = 100f,
                Value = 35f,
                Width = "100%",
                Margin = new Thickness(0, 12, 0, 12),
            };

            var valueLabel = new Label()
            {
                Text = "35%",
                TextColor = new Binding<Color>(() => Theme.OnSurface),
                HorizontalAlignment = HorizontalAlignmentType.Center,
            };

            var slider = new Slider()
            {
                Orientation = Orientation.Horizontal,
                Minimum = 0f,
                Maximum = 100f,
                Value = 35f,
                Width = "100%",
                Margin = new Thickness(0, 12, 0, 0),
                OnValueChanged = v =>
                {
                    progressBar.Value.Value = v;
                    valueLabel.Text.Value = $"{v:0}%";
                },
            };

            var stack = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };
            stack.AddChild(new Label { Text = "Drag the slider to drive the progress bar", Margin = new Thickness(0, 0, 0, 4) });
            stack.AddChild(valueLabel);
            stack.AddChild(progressBar);
            stack.AddChild(slider);

            return new Section() { Content = stack, Margin = new Thickness(5, 10) };
        }

        private Section BuildFixedValueSection()
        {
            var stack = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };

            stack.AddChild(new Label { Text = "Fixed values", Margin = new Thickness(0, 0, 0, 8) });
            stack.AddChild(new ProgressBar { Value = 15f, Width = "100%", Margin = new Thickness(0, 0, 0, 10) });
            stack.AddChild(new ProgressBar { Value = 60f, Width = "100%", Margin = new Thickness(0, 0, 0, 10) });
            stack.AddChild(new ProgressBar { Value = 100f, Width = "100%" });

            return new Section() { Content = stack, Margin = new Thickness(5, 10) };
        }

        private Section BuildIndeterminateSection()
        {
            var stack = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };

            stack.AddChild(new Label { Text = "Indeterminate (unknown duration)", Margin = new Thickness(0, 0, 0, 8) });
            stack.AddChild(new ProgressBar { IsIndeterminate = true, Width = "100%" });

            // Section's Border is cached by default (see Section.EnableCaching) - the sweep is
            // driven by DateTime.UtcNow, not a Binding<T>, so nothing ever invalidates that cache
            // and the animation would freeze after its first render.
            return new Section() { Content = stack, Margin = new Thickness(5, 10), EnableContentCaching = false };
        }
    }
}
