using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Blade.MG.UI.Theming
{
    public static class DefaultThemes
    {
        // https://m3.material.io/theme-builder#/custom
        public static UITheme LightTheme()
        {
            return new UITheme
            {
                Name = "Light",
                IsDark = false,

                Primary = ColorHelper.FromHexColor("#6750A4"),
                OnPrimary = ColorHelper.FromHexColor("#FFFFFF"),
                PrimaryContainer = ColorHelper.FromHexColor("#EADDFF"),
                OnPrimaryContainer = ColorHelper.FromHexColor("#21005D"),

                Secondary = ColorHelper.FromHexColor("#625B71"),
                OnSecondary = ColorHelper.FromHexColor("#FFFFFF"),
                SecondaryContainer = ColorHelper.FromHexColor("#E8DEF8"),
                OnSecondaryContainer = ColorHelper.FromHexColor("#1D192B"),

                Tertiary = ColorHelper.FromHexColor("#7D5260"),
                OnTertiary = ColorHelper.FromHexColor("#FFFFFF"),
                TertiaryContainer = ColorHelper.FromHexColor("#FFD8E4"),
                OnTertiaryContainer = ColorHelper.FromHexColor("#31111D"),

                Error = ColorHelper.FromHexColor("#B3261E"),
                OnError = ColorHelper.FromHexColor("#FFFFFF"),
                ErrorContainer = ColorHelper.FromHexColor("#F9DEDC"),
                OnErrorContainer = ColorHelper.FromHexColor("#410E0B"),

                Background = ColorHelper.FromHexColor("#FFFBFE"),
                OnBackground = ColorHelper.FromHexColor("#1C1B1F"),

                Surface = ColorHelper.FromHexColor("#FFFBFE"),
                OnSurface = ColorHelper.FromHexColor("#1C1B1F"),

                SurfaceContainer = ColorHelper.FromHexColor("#F3EDF7"),

                SurfaceVariant = ColorHelper.FromHexColor("#E7E0EC"),
                OnSurfaceVariant = ColorHelper.FromHexColor("#49454F"),

                Outline = ColorHelper.FromHexColor("#79747E"),

                Warning = ColorHelper.FromHexColor("#ed6c02"),
                OnWarning = ColorHelper.FromHexColor("#ffffff"),

                Info = ColorHelper.FromHexColor("#0288d1"),
                OnInfo = ColorHelper.FromHexColor("#ffffff"),

                Success = ColorHelper.FromHexColor("#2e7d32"),
                OnSuccess = ColorHelper.FromHexColor("#ffffff"),

                Disabled = ColorHelper.FromHexColor("#9E9E9E"),
                OnDisabled = ColorHelper.FromHexColor("#ECECEC"),

                Shadow = ColorHelper.FromHexColor("#000000"),
            };
        }

        public static UITheme DarkTheme()
        {
            return new UITheme
            {
                Name = "Dark",
                IsDark = true,

                Primary = ColorHelper.FromHexColor("#D0BCFF"),
                OnPrimary = ColorHelper.FromHexColor("#381E72"),
                PrimaryContainer = ColorHelper.FromHexColor("#4F378B"),
                OnPrimaryContainer = ColorHelper.FromHexColor("#EADDFF"),

                Secondary = ColorHelper.FromHexColor("#CCC2DC"),
                OnSecondary = ColorHelper.FromHexColor("#332D41"),
                SecondaryContainer = ColorHelper.FromHexColor("#4A4458"),
                OnSecondaryContainer = ColorHelper.FromHexColor("#E8DEF8"),

                Tertiary = ColorHelper.FromHexColor("#EFB8C8"),
                OnTertiary = ColorHelper.FromHexColor("#492532"),
                TertiaryContainer = ColorHelper.FromHexColor("#633B48"),
                OnTertiaryContainer = ColorHelper.FromHexColor("#FFD8E4"),

                Error = ColorHelper.FromHexColor("#F2B8B5"),
                OnError = ColorHelper.FromHexColor("#601410"),
                ErrorContainer = ColorHelper.FromHexColor("#8C1D18"),
                OnErrorContainer = ColorHelper.FromHexColor("#F9DEDC"),

                Background = ColorHelper.FromHexColor("#1C1B1F"),
                OnBackground = ColorHelper.FromHexColor("#E6E1E5"),

                Surface = ColorHelper.FromHexColor("#1C1B1F"),
                OnSurface = ColorHelper.FromHexColor("#E6E1E5"),

                SurfaceContainer = ColorHelper.FromHexColor("#211F26"),

                SurfaceVariant = ColorHelper.FromHexColor("#49454F"),
                OnSurfaceVariant = ColorHelper.FromHexColor("#CAC4D0"),

                Outline = ColorHelper.FromHexColor("#938F99"),

                Warning = ColorHelper.FromHexColor("#ed6c02"),
                OnWarning = ColorHelper.FromHexColor("#ffffff"),

                Info = ColorHelper.FromHexColor("#0288d1"),
                OnInfo = ColorHelper.FromHexColor("#ffffff"),

                Success = ColorHelper.FromHexColor("#2e7d32"),
                OnSuccess = ColorHelper.FromHexColor("#ffffff"),

                Disabled = ColorHelper.FromHexColor("#6C6C6C"),
                OnDisabled = ColorHelper.FromHexColor("#2E2E2E"),

                Shadow = ColorHelper.FromHexColor("#000000"),
            };
        }

        /// <summary>
        /// A high-contrast theme (pure black/white/yellow) for accessibility - also a good
        /// example to copy when building a custom theme, since every UITheme field is set
        /// explicitly here rather than relying on defaults.
        /// </summary>
        public static UITheme HighContrastTheme()
        {
            return new UITheme
            {
                Name = "High Contrast",
                IsDark = true,

                Primary = ColorHelper.FromHexColor("#FFFF00"),
                OnPrimary = ColorHelper.FromHexColor("#000000"),
                PrimaryContainer = ColorHelper.FromHexColor("#000000"),
                OnPrimaryContainer = ColorHelper.FromHexColor("#FFFF00"),

                Secondary = ColorHelper.FromHexColor("#00FFFF"),
                OnSecondary = ColorHelper.FromHexColor("#000000"),
                SecondaryContainer = ColorHelper.FromHexColor("#000000"),
                OnSecondaryContainer = ColorHelper.FromHexColor("#00FFFF"),

                Tertiary = ColorHelper.FromHexColor("#FF00FF"),
                OnTertiary = ColorHelper.FromHexColor("#000000"),
                TertiaryContainer = ColorHelper.FromHexColor("#000000"),
                OnTertiaryContainer = ColorHelper.FromHexColor("#FF00FF"),

                Error = ColorHelper.FromHexColor("#FF0000"),
                OnError = ColorHelper.FromHexColor("#000000"),
                ErrorContainer = ColorHelper.FromHexColor("#000000"),
                OnErrorContainer = ColorHelper.FromHexColor("#FF0000"),

                Background = ColorHelper.FromHexColor("#000000"),
                OnBackground = ColorHelper.FromHexColor("#FFFFFF"),

                Surface = ColorHelper.FromHexColor("#000000"),
                OnSurface = ColorHelper.FromHexColor("#FFFFFF"),

                SurfaceContainer = ColorHelper.FromHexColor("#0D0D0D"),

                SurfaceVariant = ColorHelper.FromHexColor("#1A1A1A"),
                OnSurfaceVariant = ColorHelper.FromHexColor("#FFFFFF"),

                Outline = ColorHelper.FromHexColor("#FFFFFF"),

                Warning = ColorHelper.FromHexColor("#FFA500"),
                OnWarning = ColorHelper.FromHexColor("#000000"),

                Info = ColorHelper.FromHexColor("#00FFFF"),
                OnInfo = ColorHelper.FromHexColor("#000000"),

                Success = ColorHelper.FromHexColor("#00FF00"),
                OnSuccess = ColorHelper.FromHexColor("#000000"),

                Disabled = ColorHelper.FromHexColor("#808080"),
                OnDisabled = ColorHelper.FromHexColor("#000000"),

                Shadow = ColorHelper.FromHexColor("#000000"),
            };
        }

        /// <summary>
        /// Teal/blue accent skin on light neutrals - an alternative to Light/Dark for
        /// developers or end users who just want a different accent color rather than a
        /// different brightness.
        /// </summary>
        public static UITheme OceanTheme()
        {
            return new UITheme
            {
                Name = "Ocean",
                IsDark = false,

                Primary = ColorHelper.FromHexColor("#006A6A"),
                OnPrimary = ColorHelper.FromHexColor("#FFFFFF"),
                PrimaryContainer = ColorHelper.FromHexColor("#6FF7F6"),
                OnPrimaryContainer = ColorHelper.FromHexColor("#002020"),

                Secondary = ColorHelper.FromHexColor("#4A6363"),
                OnSecondary = ColorHelper.FromHexColor("#FFFFFF"),
                SecondaryContainer = ColorHelper.FromHexColor("#CCE8E7"),
                OnSecondaryContainer = ColorHelper.FromHexColor("#051F1F"),

                Tertiary = ColorHelper.FromHexColor("#4B607C"),
                OnTertiary = ColorHelper.FromHexColor("#FFFFFF"),
                TertiaryContainer = ColorHelper.FromHexColor("#D3E4FF"),
                OnTertiaryContainer = ColorHelper.FromHexColor("#041C35"),

                Error = ColorHelper.FromHexColor("#BA1A1A"),
                OnError = ColorHelper.FromHexColor("#FFFFFF"),
                ErrorContainer = ColorHelper.FromHexColor("#FFDAD6"),
                OnErrorContainer = ColorHelper.FromHexColor("#410002"),

                Background = ColorHelper.FromHexColor("#F4FEFD"),
                OnBackground = ColorHelper.FromHexColor("#161D1C"),

                Surface = ColorHelper.FromHexColor("#F4FEFD"),
                OnSurface = ColorHelper.FromHexColor("#161D1C"),

                SurfaceContainer = ColorHelper.FromHexColor("#E8F3F2"),

                SurfaceVariant = ColorHelper.FromHexColor("#DAE5E4"),
                OnSurfaceVariant = ColorHelper.FromHexColor("#3F4948"),

                Outline = ColorHelper.FromHexColor("#6F7979"),

                Warning = ColorHelper.FromHexColor("#ed6c02"),
                OnWarning = ColorHelper.FromHexColor("#ffffff"),

                Info = ColorHelper.FromHexColor("#0288d1"),
                OnInfo = ColorHelper.FromHexColor("#ffffff"),

                Success = ColorHelper.FromHexColor("#2e7d32"),
                OnSuccess = ColorHelper.FromHexColor("#ffffff"),

                Disabled = ColorHelper.FromHexColor("#9E9E9E"),
                OnDisabled = ColorHelper.FromHexColor("#ECECEC"),

                Shadow = ColorHelper.FromHexColor("#000000"),
            };
        }

        /// <summary>
        /// Green accent skin on light neutrals.
        /// </summary>
        public static UITheme ForestTheme()
        {
            return new UITheme
            {
                Name = "Forest",
                IsDark = false,

                Primary = ColorHelper.FromHexColor("#386A20"),
                OnPrimary = ColorHelper.FromHexColor("#FFFFFF"),
                PrimaryContainer = ColorHelper.FromHexColor("#B7F397"),
                OnPrimaryContainer = ColorHelper.FromHexColor("#062100"),

                Secondary = ColorHelper.FromHexColor("#55624C"),
                OnSecondary = ColorHelper.FromHexColor("#FFFFFF"),
                SecondaryContainer = ColorHelper.FromHexColor("#D9E7CB"),
                OnSecondaryContainer = ColorHelper.FromHexColor("#131F0D"),

                Tertiary = ColorHelper.FromHexColor("#386667"),
                OnTertiary = ColorHelper.FromHexColor("#FFFFFF"),
                TertiaryContainer = ColorHelper.FromHexColor("#BBECEC"),
                OnTertiaryContainer = ColorHelper.FromHexColor("#002020"),

                Error = ColorHelper.FromHexColor("#BA1A1A"),
                OnError = ColorHelper.FromHexColor("#FFFFFF"),
                ErrorContainer = ColorHelper.FromHexColor("#FFDAD6"),
                OnErrorContainer = ColorHelper.FromHexColor("#410002"),

                Background = ColorHelper.FromHexColor("#FBFDF7"),
                OnBackground = ColorHelper.FromHexColor("#1A1C18"),

                Surface = ColorHelper.FromHexColor("#FBFDF7"),
                OnSurface = ColorHelper.FromHexColor("#1A1C18"),

                SurfaceContainer = ColorHelper.FromHexColor("#EFF3E7"),

                SurfaceVariant = ColorHelper.FromHexColor("#DFE4D7"),
                OnSurfaceVariant = ColorHelper.FromHexColor("#43483E"),

                Outline = ColorHelper.FromHexColor("#73796D"),

                Warning = ColorHelper.FromHexColor("#ed6c02"),
                OnWarning = ColorHelper.FromHexColor("#ffffff"),

                Info = ColorHelper.FromHexColor("#0288d1"),
                OnInfo = ColorHelper.FromHexColor("#ffffff"),

                Success = ColorHelper.FromHexColor("#2e7d32"),
                OnSuccess = ColorHelper.FromHexColor("#ffffff"),

                Disabled = ColorHelper.FromHexColor("#9E9E9E"),
                OnDisabled = ColorHelper.FromHexColor("#ECECEC"),

                Shadow = ColorHelper.FromHexColor("#000000"),
            };
        }

        /// <summary>
        /// Red/rose accent skin on light neutrals.
        /// </summary>
        public static UITheme CrimsonTheme()
        {
            return new UITheme
            {
                Name = "Crimson",
                IsDark = false,

                Primary = ColorHelper.FromHexColor("#9A2846"),
                OnPrimary = ColorHelper.FromHexColor("#FFFFFF"),
                PrimaryContainer = ColorHelper.FromHexColor("#FFD9DF"),
                OnPrimaryContainer = ColorHelper.FromHexColor("#3E001C"),

                Secondary = ColorHelper.FromHexColor("#77525A"),
                OnSecondary = ColorHelper.FromHexColor("#FFFFFF"),
                SecondaryContainer = ColorHelper.FromHexColor("#FFD9E1"),
                OnSecondaryContainer = ColorHelper.FromHexColor("#2C151B"),

                Tertiary = ColorHelper.FromHexColor("#7C5635"),
                OnTertiary = ColorHelper.FromHexColor("#FFFFFF"),
                TertiaryContainer = ColorHelper.FromHexColor("#FFDCBE"),
                OnTertiaryContainer = ColorHelper.FromHexColor("#2C1600"),

                Error = ColorHelper.FromHexColor("#BA1A1A"),
                OnError = ColorHelper.FromHexColor("#FFFFFF"),
                ErrorContainer = ColorHelper.FromHexColor("#FFDAD6"),
                OnErrorContainer = ColorHelper.FromHexColor("#410002"),

                Background = ColorHelper.FromHexColor("#FFFBFF"),
                OnBackground = ColorHelper.FromHexColor("#201A1B"),

                Surface = ColorHelper.FromHexColor("#FFFBFF"),
                OnSurface = ColorHelper.FromHexColor("#201A1B"),

                SurfaceContainer = ColorHelper.FromHexColor("#FBEAEE"),

                SurfaceVariant = ColorHelper.FromHexColor("#F2DDE1"),
                OnSurfaceVariant = ColorHelper.FromHexColor("#514347"),

                Outline = ColorHelper.FromHexColor("#837377"),

                Warning = ColorHelper.FromHexColor("#ed6c02"),
                OnWarning = ColorHelper.FromHexColor("#ffffff"),

                Info = ColorHelper.FromHexColor("#0288d1"),
                OnInfo = ColorHelper.FromHexColor("#ffffff"),

                Success = ColorHelper.FromHexColor("#2e7d32"),
                OnSuccess = ColorHelper.FromHexColor("#ffffff"),

                Disabled = ColorHelper.FromHexColor("#9E9E9E"),
                OnDisabled = ColorHelper.FromHexColor("#ECECEC"),

                Shadow = ColorHelper.FromHexColor("#000000"),
            };
        }

        /// <summary>
        /// All built-in themes, in the order they should appear in a theme picker.
        /// </summary>
        public static IReadOnlyList<UITheme> All => new[]
        {
            LightTheme(),
            DarkTheme(),
            OceanTheme(),
            ForestTheme(),
            CrimsonTheme(),
            HighContrastTheme(),
        };

    }
}
