using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;
using Microsoft.Xna.Framework;

namespace Examples.UI.HelpPages
{
    public class HelpPage_Panel : Panel
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
                    Title = "Panels",
                    Description = "Child widths can be a fixed pixel size or a percentage of the parent."
                });

            var demoSurface = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Background = new Binding<Color>(() => Theme.SurfaceVariant),
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Auto,
                Height = "350px",
                Width = "800px",
            };

            var border = new Border()
            {
                CornerRadius = new CornerRadius(10),
                BorderThickness = new Thickness(1),
                BorderColor = new Binding<Color>(() => Theme.Outline),
                HorizontalAlignment = HorizontalAlignmentType.Center,
                VerticalAlignment = VerticalAlignmentType.Center,
            };

            border.Content = demoSurface;

            layoutPanel.AddChild(border);

            AddRow(demoSurface, "Full width (100%)", ("100%", HorizontalAlignmentType.Left));
            AddRow(demoSurface, "Split 50 / 50", ("50%", HorizontalAlignmentType.Left), ("50%", HorizontalAlignmentType.Left));
            AddRow(demoSurface, "Split 33 / 33 / 34", ("33%", HorizontalAlignmentType.Left), ("33%", HorizontalAlignmentType.Left), ("34%", HorizontalAlignmentType.Left));
            AddRow(demoSurface, "Split 25 / 25 / 25 / 25", ("25%", HorizontalAlignmentType.Left), ("25%", HorizontalAlignmentType.Left), ("25%", HorizontalAlignmentType.Left), ("25%", HorizontalAlignmentType.Left));
        }

        private void AddRow(StackPanel host, string rowLabel, params (string Width, HorizontalAlignmentType Align)[] columns)
        {
            var row = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
                Height = "80px",
                Width = "100%",
            };

            host.AddChild(row);

            for (int i = 0; i < columns.Length; i++)
            {
                row.AddChild(
                    new Section()
                    {
                        HorizontalAlignment = columns[i].Align,
                        VerticalAlignment = VerticalAlignmentType.Stretch,
                        Width = columns[i].Width,
                        Height = "100%",

                        Content = new Button()
                        {
                            Text = i == 0 ? rowLabel : columns[i].Width,
                            HorizontalAlignment = HorizontalAlignmentType.Stretch,
                            VerticalAlignment = VerticalAlignmentType.Stretch,
                        }
                    });
            }
        }
    }
}
