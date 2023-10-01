using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Theming
{
    public static class DefaultThemes
    {

        /// <summary>
        /// Convert a HEX (#RRGGBB or #RRGGBBAA) Color to a Monogame Color
        /// The leading Hash is optional
        /// </summary>
        /// <param name="hexColor"></param>
        /// <returns></returns>
        public static Color FromHexColor(string hexColor)
        {
            if (string.IsNullOrWhiteSpace(hexColor))
            {
                throw new ArgumentOutOfRangeException(nameof(hexColor));
            }

            byte[] parts;

            if (hexColor.StartsWith("#"))
            {
                if (hexColor.Length == 7 || hexColor.Length == 9)
                {
                    parts = Convert.FromHexString(hexColor.Substring(1));
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(hexColor));
                }
            }
            else
            {
                if (hexColor.Length == 6 || hexColor.Length == 8)
                {
                    parts = Convert.FromHexString(hexColor);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(hexColor));
                }
            }

            if (parts.Length == 3)
            {
                return new Color(parts[0], parts[1], parts[2], (byte)255);
            }
            else
            {
                return new Color(parts[0], parts[1], parts[2], parts[3]);
            }
        }


        // https://m3.material.io/theme-builder#/custom
        public static UITheme LightTheme()
        {
            return new UITheme
            {
                Primary = FromHexColor("#6750A4"),
                OnPrimary = FromHexColor("#FFFFFF"),
                PrimaryContainer = FromHexColor("#EADDFF"),
                OnPrimaryContainer = FromHexColor("#21005D"),

                Secondary = FromHexColor("#625B71"),
                OnSecondary = FromHexColor("#FFFFFF"),
                SecondaryContainer = FromHexColor("#E8DEF8"),
                OnSecondaryContainer = FromHexColor("#1D192B"),

                Tertiary = FromHexColor("#7D5260"),
                OnTertiary = FromHexColor("#FFFFFF"),
                TertiaryContainer = FromHexColor("#FFD8E4"),
                OnTertiaryContainer = FromHexColor("#31111D"),

                Error = FromHexColor("#B3261E"),
                OnError = FromHexColor("#FFFFFF"),
                ErrorContainer = FromHexColor("#F9DEDC"),
                OnErrorContainer = FromHexColor("#410E0B"),

                Background = FromHexColor("#FFFBFE"),
                OnBackground = FromHexColor("#1C1B1F"),

                Surface = FromHexColor("#FFFBFE"),
                OnSurface = FromHexColor("#1C1B1F"),

                SurfaceVariant = FromHexColor("#E7E0EC"),
                OnSurfaceVariant = FromHexColor("#49454F"),

                Outline = FromHexColor("#79747E"),


                Warning = FromHexColor("#ed6c02"),
                OnWarning = FromHexColor("#ffffff"),

                Info = FromHexColor("#0288d1"),
                OnInfo = FromHexColor("#ffffff"),

                Success = FromHexColor("#2e7d32"),
                OnSuccess = FromHexColor("#ffffff"),

            };
        }

        //public static UITheme LightTheme1()
        //{
        //    return new UITheme
        //    {
        //        Primary = FromHexColor("#1976d2"),    // palette.primary.main
        //        Secondary = FromHexColor("#9c27b0"),  // palette.secondary.main
        //        Tertiary = FromHexColor("#9c27b0"),   // palette.secondary.main
        //        Outline = FromHexColor("#9c27b0"),    // palette.secondary.main

        //        Error = FromHexColor("#d32f2f"),      // palette.error.main
        //        Warning = FromHexColor("#ed6c02"),    // palette.warning.main
        //        Info = FromHexColor("#0288d1"),       // palette.info.main
        //        Success = FromHexColor("#2e7d32"),    // palette.success.main

        //        Background = FromHexColor("#ffffff"),
        //        Surface = FromHexColor("#ffffff"),

        //        OnPrimary = FromHexColor("#ffffff"),
        //        OnSecondary = FromHexColor("#000000"),
        //        OnBackground = FromHexColor("#000000"),
        //        OnSurface = FromHexColor("#000000"),

        //        OnError = FromHexColor("#ffffff"),
        //        OnWarning = FromHexColor("#ffffff"),
        //        OnInfo = FromHexColor("#ffffff"),
        //        OnSuccess = FromHexColor("#ffffff"),

        //    };
        //}

        //public static UITheme LightTheme2()
        //{
        //    return new UITheme
        //    {
        //        Primary = FromHexColor("#6200ee"),    // palette.primary.main
        //        Secondary = FromHexColor("#03dac6"),  // palette.secondary.main
        //        Tertiary = FromHexColor("#9c27b0"),   // palette.secondary.main
        //        Outline = FromHexColor("#9c27b0"),    // palette.secondary.main

        //        Error = FromHexColor("#b00020"),      // palette.error.main
        //        Warning = FromHexColor("#ed6c02"),    // palette.warning.main
        //        Info = FromHexColor("#0288d1"),       // palette.info.main
        //        Success = FromHexColor("#2e7d32"),    // palette.success.main

        //        Background = FromHexColor("#ffffff"),
        //        Surface = FromHexColor("#ffffff"),

        //        OnPrimary = FromHexColor("#ffffff"),
        //        OnSecondary = FromHexColor("#000000"),
        //        OnBackground = FromHexColor("#000000"),
        //        OnSurface = FromHexColor("#000000"),

        //        OnError = FromHexColor("#ffffff"),
        //        OnWarning = FromHexColor("#ffffff"),
        //        OnInfo = FromHexColor("#ffffff"),
        //        OnSuccess = FromHexColor("#ffffff"),

        //    };
        //}

        public static UITheme DarkTheme()
        {
            return new UITheme
            {
                Primary = FromHexColor("#D0BCFF"),
                OnPrimary = FromHexColor("#381E72"),
                PrimaryContainer = FromHexColor("#4F378B"),
                OnPrimaryContainer = FromHexColor("#EADDFF"),

                Secondary = FromHexColor("#CCC2DC"),
                OnSecondary = FromHexColor("#332D41"),
                SecondaryContainer = FromHexColor("#4A4458"),
                OnSecondaryContainer = FromHexColor("#E8DEF8"),

                Tertiary = FromHexColor("#EFB8C8"),
                OnTertiary = FromHexColor("#492532"),
                TertiaryContainer = FromHexColor("#633B48"),
                OnTertiaryContainer = FromHexColor("#FFD8E4"),

                Error = FromHexColor("#F2B8B5"),
                OnError = FromHexColor("#601410"),
                ErrorContainer = FromHexColor("#8C1D18"),
                OnErrorContainer = FromHexColor("#F9DEDC"),

                Background = FromHexColor("#1C1B1F"),
                OnBackground = FromHexColor("#E6E1E5"),

                Surface = FromHexColor("#1C1B1F"),
                OnSurface = FromHexColor("#E6E1E5"),

                SurfaceVariant = FromHexColor("#49454F"),
                OnSurfaceVariant = FromHexColor("#CAC4D0"),

                Outline = FromHexColor("#938F99"),


                Warning = FromHexColor("#ed6c02"),
                OnWarning = FromHexColor("#ffffff"),

                Info = FromHexColor("#0288d1"),
                OnInfo = FromHexColor("#ffffff"),

                Success = FromHexColor("#2e7d32"),
                OnSuccess = FromHexColor("#ffffff"),

            };
        }

    }
}
