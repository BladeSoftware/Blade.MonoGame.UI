using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Theming;
using Examples.UI.Components;
using Microsoft.Xna.Framework;
using System;

namespace Examples.UI.HelpPages
{
    /// <summary>
    /// Live swatch grid of every color in the active <see cref="UITheme"/>, paired swatch +
    /// on-color the same way the theme intends them to be used together. Each swatch reads
    /// straight from the live theme (via computed Binding lambdas), so switching themes with
    /// the AppBar picker restyles this page immediately - a working demo of the theming system
    /// itself, not just a static reference.
    /// </summary>
    public class HelpPage_Theme : Panel
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
                    Title = "Theming",
                    Description = "Every color in the active theme, paired the way it's meant to be used. Switch themes above to see it update live."
                });

            var swatches = new (string Label, Func<UITheme, Color> Background, Func<UITheme, Color> Foreground)[]
            {
                ("Primary",            t => t.Primary,            t => t.OnPrimary),
                ("Primary Container",  t => t.PrimaryContainer,   t => t.OnPrimaryContainer),
                ("Secondary",          t => t.Secondary,          t => t.OnSecondary),
                ("Secondary Container",t => t.SecondaryContainer, t => t.OnSecondaryContainer),
                ("Tertiary",           t => t.Tertiary,           t => t.OnTertiary),
                ("Tertiary Container", t => t.TertiaryContainer,  t => t.OnTertiaryContainer),
                ("Error",              t => t.Error,              t => t.OnError),
                ("Error Container",    t => t.ErrorContainer,     t => t.OnErrorContainer),
                ("Background",         t => t.Background,         t => t.OnBackground),
                ("Surface",            t => t.Surface,            t => t.OnSurface),
                ("Surface Variant",    t => t.SurfaceVariant,     t => t.OnSurfaceVariant),
                ("Warning",            t => t.Warning,            t => t.OnWarning),
                ("Info",               t => t.Info,               t => t.OnInfo),
                ("Success",            t => t.Success,            t => t.OnSuccess),
                ("Disabled",           t => t.Disabled,           t => t.OnDisabled),
                ("Outline",            t => t.Surface,            t => t.OnSurface),
            };

            const int columns = 4;
            int rows = (swatches.Length + columns - 1) / columns;

            var swatchGrid = new Grid()
            {
                Margin = new Thickness(30, 10, 30, 20),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
            };

            for (int c = 0; c < columns; c++)
            {
                swatchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });
            }

            for (int r = 0; r < rows; r++)
            {
                swatchGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 100) });
            }

            layoutPanel.AddChild(swatchGrid);

            for (int i = 0; i < swatches.Length; i++)
            {
                var (label, background, foreground) = swatches[i];
                bool isOutline = label == "Outline";

                var card = new Border()
                {
                    CornerRadius = new CornerRadius(10),
                    BorderThickness = new Thickness(isOutline ? 3 : 1),
                    BorderColor = new Binding<Color>(() => Theme.Outline),
                    Background = new Binding<Color>(() => background(Theme)),
                    Margin = new Thickness(6),
                    HorizontalAlignment = HorizontalAlignmentType.Stretch,
                    VerticalAlignment = VerticalAlignmentType.Stretch,
                };

                card.Content = new Label()
                {
                    Text = label,
                    FontSize = 14,
                    TextColor = new Binding<Color>(() => foreground(Theme)),
                    HorizontalAlignment = HorizontalAlignmentType.Stretch,
                    VerticalAlignment = VerticalAlignmentType.Stretch,
                    HorizontalTextAlignment = HorizontalAlignmentType.Center,
                    VerticalTextAlignment = VerticalAlignmentType.Center,
                };

                swatchGrid.AddChild(card, i % columns, i / columns);
            }
        }
    }
}
