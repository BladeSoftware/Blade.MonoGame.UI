using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Microsoft.Xna.Framework;

namespace Examples.UI.Components
{
    /// <summary>
    /// Standard title block used at the top of every help page: a bold page title and an
    /// optional one-line description underneath, both colored from the active theme so they
    /// stay legible across Light/Dark/High-Contrast.
    /// </summary>
    public class PageHeader : Control
    {
        public string Title { get; set; }

        public string Description { get; set; }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Top;

            var stack = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Top,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };

            stack.AddChild(new Label()
            {
                Text = Title,
                FontSize = 30,
                TextColor = new Binding<Color>(() => Theme.OnBackground),
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,
                Margin = new Thickness(0, 0, 0, string.IsNullOrEmpty(Description) ? 0 : 4),
            });

            if (!string.IsNullOrEmpty(Description))
            {
                stack.AddChild(new Label()
                {
                    Text = Description,
                    FontSize = 16,
                    TextColor = new Binding<Color>(() => Theme.OnSurfaceVariant),
                    HorizontalAlignment = HorizontalAlignmentType.Left,
                    VerticalAlignment = VerticalAlignmentType.Top,
                });
            }

            Content = stack;
        }

    }
}
