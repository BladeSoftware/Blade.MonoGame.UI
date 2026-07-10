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
                    Description = "A scrollable, virtualized list of items."
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

            var listView = new ListView()
            {
                ItemTemplateType = typeof(ListViewItemTemplate),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                DataContext = Enum.GetNames(typeof(Country)),
            };

            border.Content = listView;
        }

    }
}
