using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;
using Microsoft.Xna.Framework;

namespace Examples.UI.HelpPages
{
    /// <summary>
    /// Landing page shown when the showcase first opens: a short intro to the library plus a
    /// card summarizing each nav category, so a first-time visitor knows where to look.
    /// </summary>
    public class HelpPage_Welcome : Panel
    {
        protected override void InitTemplate()
        {
            base.InitTemplate();

            var layoutPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Auto,
            };

            base.AddChild(layoutPanel);

            layoutPanel.AddChild(
                new PageHeader()
                {
                    Padding = new Thickness(30, 0, 0, 0),
                    Title = "Blade.MG.UI",
                    Description = "A retained-mode, theme-aware UI framework for MonoGame."
                });

            var intro = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(30, 10, 30, 20),
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };
            layoutPanel.AddChild(intro);

            foreach (var line in new[]
            {
                "Use the categories on the left to browse every control the library ships with.",
                "Switch themes with the picker in the top-right corner - every page updates live.",
            })
            {
                intro.AddChild(new Label()
                {
                    Text = line,
                    FontSize = 16,
                    TextColor = new Binding<Color>(() => Theme.OnSurfaceVariant),
                    Height = 24,
                    HorizontalAlignment = HorizontalAlignmentType.Left,
                });
            }

            var cardGrid = new Grid()
            {
                Margin = new Thickness(30, 10, 30, 20),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
            };

            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });

            cardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 140) });
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 140) });

            layoutPanel.AddChild(cardGrid);

            var cards = new (string Title, string Body)[]
            {
                ("Get Started", "The live theme swatches and how theming works."),
                ("Layout", "Panel, Grid and Dock Panel for arranging content."),
                ("Input", "Button, form fields and the Combo Box control."),
                ("Display", "Label, List View and Tree View for showing data."),
                ("Navigation", "Tab Panel for grouping content into tabs."),
                ("Data", "Property Editor - a reflection-driven object grid."),
            };

            for (int i = 0; i < cards.Length; i++)
            {
                var (title, body) = cards[i];

                var card = new Border()
                {
                    CornerRadius = new CornerRadius(12),
                    BorderThickness = new Thickness(1),
                    BorderColor = new Binding<Color>(() => Theme.Outline),
                    Background = new Binding<Color>(() => Theme.Surface),
                    Margin = new Thickness(6),
                    Padding = new Thickness(16),
                    Elevation = 1,
                };

                var cardStack = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                    VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
                };

                cardStack.AddChild(new Label()
                {
                    Text = title,
                    FontSize = 18,
                    TextColor = new Binding<Color>(() => Theme.Primary),
                    Height = 26,
                    HorizontalAlignment = HorizontalAlignmentType.Left,
                });

                cardStack.AddChild(new Label()
                {
                    Text = body,
                    FontSize = 14,
                    TextColor = new Binding<Color>(() => Theme.OnSurfaceVariant),
                    Height = 60,
                    HorizontalAlignment = HorizontalAlignmentType.Left,
                });

                card.Content = cardStack;

                cardGrid.AddChild(card, i % 3, i / 3);
            }
        }
    }
}
