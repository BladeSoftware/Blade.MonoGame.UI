using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;
using GameStudio;
using Microsoft.Xna.Framework;
using System;

namespace Examples.UI.HelpPages
{
    public class HelpPage_ComboBox : Panel
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
                    Title = "Combo Box",
                    Description = "A dropdown select, with optional inline text-filtering."
                });

            var grid = new Grid()
            {
                Margin = new Thickness(30, 10, 30, 20),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 180) });

            layoutPanel.AddChild(grid);

            var countries = Enum.GetNames(typeof(Country));

            grid.AddChild(
                BuildVariant(
                    "Standard",
                    "Closed selection - pick from the list, no typing.",
                    countries,
                    isEditable: false,
                    strictMode: true),
                0, 0);

            grid.AddChild(
                BuildVariant(
                    "Filterable",
                    "Type to filter the list; any value can be entered.",
                    countries,
                    isEditable: true,
                    strictMode: false),
                1, 0);

            grid.AddChild(
                BuildVariant(
                    "Strict + Editable",
                    "Type to filter, but the value must match a list item.",
                    countries,
                    isEditable: true,
                    strictMode: true),
                2, 0);
        }

        private Section BuildVariant(string title, string description, string[] items, bool isEditable, bool strictMode)
        {
            var stack = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(16),
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };

            stack.AddChild(new Label()
            {
                Text = title,
                FontSize = 16,
                TextColor = new Binding<Color>(() => Theme.OnSurface),
                Height = 24,
                HorizontalAlignment = HorizontalAlignmentType.Left,
            });

            stack.AddChild(new Label()
            {
                Text = description,
                FontSize = 13,
                TextColor = new Binding<Color>(() => Theme.OnSurfaceVariant),
                Height = 40,
                Margin = new Thickness(0, 0, 0, 10),
                HorizontalAlignment = HorizontalAlignmentType.Left,
            });

            stack.AddChild(new ComboBox()
            {
                ItemsSource = items,
                IsEditable = isEditable,
                StrictMode = strictMode,
                SelectedItem = items[0],
                Height = 40,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
            });

            return new Section() { Content = stack };
        }
    }
}
