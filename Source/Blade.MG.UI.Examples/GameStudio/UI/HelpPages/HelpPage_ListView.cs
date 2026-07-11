using Blade.MG.UI;
using Microsoft.Xna.Framework;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Controls.Templates;
using Examples.UI.Components;
using GameStudio;
using System;

namespace Examples.UI.HelpPages
{
    public class HelpPage_ListView : Panel
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
                    Title = "List View",
                    Description = "A scrollable, virtualized list of items. Use Up/Down/Home/End to highlight an item, then Enter to select it."
                });

            var listView = new ListView()
            {
                ItemTemplateType = typeof(ListViewItemTemplate),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                DataContext = Enum.GetNames(typeof(Country)),

                // Arrow keys only move HighlightedItem; Enter/Space (or a mouse click) commits
                // it to SelectedItem - so this page actually exercises Enter-to-select instead
                // of arrow keys already selecting immediately (the default for a standalone
                // ListView).
                CommitSelectionImmediately = false,
            };

            layoutPanel.AddChild(
                new Label()
                {
                    Padding = new Thickness(30, 0, 0, 0),
                    Margin = new Thickness(0, 10, 0, 0),
                    FontSize = 14,
                    TextColor = new Binding<Color>(() => Theme.OnSurfaceVariant),
                    HorizontalAlignment = HorizontalAlignmentType.Left,
                    Text = new Binding<string>(() => $"Selected: {listView.SelectedItem?.ToString() ?? "(none)"}"),
                });

            var border = new Border()
            {
                BorderThickness = new Thickness(1),
                BorderColor = new Binding<Color>(() => Theme.Outline),
                Margin = new Thickness(30, 20, 30, 20),
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Height = 500,
            };

            layoutPanel.AddChild(border);
            layoutPanel.StretchLastChild = true;

            border.Content = listView;
        }

    }
}
