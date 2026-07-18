using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;
using Microsoft.Xna.Framework;

namespace Examples.UI.HelpPages
{
    public class HelpPage_Tooltip : Panel
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
                    Title = "Tooltips",
                    Description = "Hover over a button and hold still - the tooltip appears after a short delay and hides as soon as the hover ends."
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

            layoutPanel.AddChild(grid);

            var game = ParentWindow.Game;

            grid.AddChild(BuildTooltipSection("Default delay (0.6s)", "Save the current document", game, delaySeconds: 0.6f), 0, 0);
            grid.AddChild(BuildTooltipSection("Fast (0.15s)", "Undo the last action", game, delaySeconds: 0.15f), 1, 0);
            grid.AddChild(BuildTooltipSection("Slow (1.5s)", "This tooltip takes its time to appear", game, delaySeconds: 1.5f), 2, 0);
        }

        private static Section BuildTooltipSection(string label, string tooltipText, Game game, float delaySeconds)
        {
            var button = new Button
            {
                Text = label,
                Width = "80%",
                HorizontalAlignment = HorizontalAlignmentType.Center,
                VerticalAlignment = VerticalAlignmentType.Center,
            };

            Tooltip.Attach(button, game, tooltipText, delaySeconds);

            return new Section() { Content = button, Margin = new Thickness(5, 10) };
        }
    }
}
