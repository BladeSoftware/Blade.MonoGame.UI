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
    /// Material 3 "dynamic color": pick a single seed color, toggle Light/Dark, and preview a
    /// full generated <see cref="UITheme"/> - the same one-seed-to-whole-scheme workflow as
    /// Google's own Material Theme Builder (see <see cref="ThemeGenerator"/>). The swatch grid
    /// below mirrors <see cref="HelpPage_Theme"/>'s, but reads from this page's own locally-held
    /// <see cref="generatedTheme"/> field rather than the live active theme - this previews
    /// before committing, it doesn't restyle the app until "Apply" is pressed.
    /// </summary>
    public class HelpPage_DynamicColor : Panel
    {
        private Color seedColor = ColorHelper.FromHexColor("#6750A4");
        private bool isDark;
        private UITheme generatedTheme;

        private Border seedSwatch;

        protected override void InitTemplate()
        {
            base.InitTemplate();

            Regenerate();

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
                    Title = "Dynamic Color",
                    Description = "Generate a full Material 3 color scheme from a single seed color, in either Light or Dark - the same seed produces both. There's no color-picker dialog in this library yet, so enter a hex value directly."
                });

            var controlsRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(30, 10, 30, 10),
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Center,

                // StackPanel extends ScrollPanel, whose HorizontalScrollBarVisible/
                // VerticalScrollBarVisible both default to Auto - a bad default for a plain
                // horizontal toolbar row that never needs to scroll in either direction. Left at
                // the default, this caused the exact feedback loop reported: if the row's
                // children didn't quite fit its own auto-sized height, a vertical scrollbar
                // appeared, narrowing the available width enough to trigger a horizontal
                // scrollbar too, which then squeezed the height further - each frame's Arrange
                // re-evaluating from a different starting size than the last, with nothing to
                // converge on a fixed point.
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };
            layoutPanel.AddChild(controlsRow);

            seedSwatch = new Border
            {
                Width = 40,
                Height = 40,
                CornerRadius = new CornerRadius(6),
                BorderThickness = new Thickness(1),
                BorderColor = new Binding<Color>(() => Theme.Outline),
                Background = seedColor,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignmentType.Center,
            };
            controlsRow.AddChild(seedSwatch);

            var seedTextBox = new TextBox
            {
                Text = ColorHelper.ToHexColor(seedColor),
                Width = 140,
                Margin = new Thickness(0, 0, 20, 0),
                VerticalAlignment = VerticalAlignmentType.Center,
            };
            seedTextBox.OnFocusChanged = (sender, uiEvent) =>
            {
                if (uiEvent.Focused)
                {
                    return;
                }

                try
                {
                    seedColor = ColorHelper.FromString(seedTextBox.Text.Value);
                    Regenerate();
                }
                catch (Exception ex) when (ex is FormatException or IndexOutOfRangeException)
                {
                    // Invalid hex - snap back to whatever the current seed already is, matching
                    // PropertyEditor's own Color editor behavior for the same failure case.
                    seedTextBox.Text.Value = ColorHelper.ToHexColor(seedColor);
                }
            };
            controlsRow.AddChild(seedTextBox);

            var darkModeCheckBox = new CheckBox
            {
                Text = "Dark Mode",
                IsChecked = isDark,
                VerticalAlignment = VerticalAlignmentType.Center,
                Margin = new Thickness(0, 0, 20, 0),
            };
            darkModeCheckBox.OnValueChanged = isChecked =>
            {
                isDark = isChecked ?? false;
                Regenerate();
            };
            controlsRow.AddChild(darkModeCheckBox);

            // On its own row rather than appended to controlsRow - the swatch/TextBox/CheckBox
            // combination alone is already a comfortable width for the content pane, but adding
            // the button on the same row pushed the total past the available width (with no
            // scrollbar to reveal the overflow now that controlsRow's are correctly hidden - see
            // the comment above), clipping the button's right edge. No explicit Width here either
            // (unlike the row above, which pins the swatch/TextBox to a fixed size on purpose) -
            // let it auto-size to whatever "Apply as Active Theme" actually needs, rather than
            // risking a second, narrower hardcoded guess.
            var applyRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(30, 0, 30, 10),
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Center,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };
            layoutPanel.AddChild(applyRow);

            var applyButton = new Button
            {
                Text = "Apply as Active Theme",
                VerticalAlignment = VerticalAlignmentType.Center,
                Padding = new Thickness(20, 10, 20, 10),
            };
            applyButton.OnActivate = (sender, uiEvent) => UIManager.SetTheme(generatedTheme);
            applyRow.AddChild(applyButton);

            layoutPanel.AddChild(BuildSwatchGrid());
        }

        private void Regenerate()
        {
            generatedTheme = ThemeGenerator.GenerateFromSeed("Preview", seedColor, isDark);

            if (seedSwatch != null)
            {
                seedSwatch.Background = seedColor;
            }
        }

        // Same swatch table/layout as HelpPage_Theme.cs, but each swatch reads generatedTheme
        // (this page's own local field, updated by Regenerate) instead of the live active Theme
        // - a preview, not a restyle, until "Apply as Active Theme" is pressed.
        private Grid BuildSwatchGrid()
        {
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
                ("Surface Container",  t => t.SurfaceContainer,   t => t.OnSurface),
                ("Surface Variant",    t => t.SurfaceVariant,     t => t.OnSurfaceVariant),
                ("Outline",            t => t.Outline,            t => t.OnSurface),
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

            for (int i = 0; i < swatches.Length; i++)
            {
                var (label, background, foreground) = swatches[i];

                var card = new Border()
                {
                    CornerRadius = new CornerRadius(10),
                    BorderThickness = new Thickness(1),
                    BorderColor = new Binding<Color>(() => generatedTheme.Outline),
                    Background = new Binding<Color>(() => background(generatedTheme)),
                    Margin = new Thickness(6),
                    HorizontalAlignment = HorizontalAlignmentType.Stretch,
                    VerticalAlignment = VerticalAlignmentType.Stretch,
                };

                card.Content = new Label()
                {
                    Text = label,
                    FontSize = 14,
                    TextColor = new Binding<Color>(() => foreground(generatedTheme)),
                    HorizontalAlignment = HorizontalAlignmentType.Stretch,
                    VerticalAlignment = VerticalAlignmentType.Stretch,
                    HorizontalTextAlignment = HorizontalAlignmentType.Center,
                    VerticalTextAlignment = VerticalAlignmentType.Center,
                };

                swatchGrid.AddChild(card, i % columns, i / columns);
            }

            return swatchGrid;
        }
    }
}
