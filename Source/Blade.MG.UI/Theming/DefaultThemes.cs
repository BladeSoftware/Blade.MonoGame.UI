using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Theming
{
    public static class DefaultThemes
    {
        // https://m3.material.io/theme-builder#/custom
        public static UITheme LightTheme()
        {
            return new UITheme
            {
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

                SurfaceVariant = ColorHelper.FromHexColor("#E7E0EC"),
                OnSurfaceVariant = ColorHelper.FromHexColor("#49454F"),

                Outline = ColorHelper.FromHexColor("#79747E"),


                Warning = ColorHelper.FromHexColor("#ed6c02"),
                OnWarning = ColorHelper.FromHexColor("#ffffff"),

                Info = ColorHelper.FromHexColor("#0288d1"),
                OnInfo = ColorHelper.FromHexColor("#ffffff"),

                Success = ColorHelper.FromHexColor("#2e7d32"),
                OnSuccess = ColorHelper.FromHexColor("#ffffff"),

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

                SurfaceVariant = ColorHelper.FromHexColor("#49454F"),
                OnSurfaceVariant = ColorHelper.FromHexColor("#CAC4D0"),

                Outline = ColorHelper.FromHexColor("#938F99"),


                Warning = ColorHelper.FromHexColor("#ed6c02"),
                OnWarning = ColorHelper.FromHexColor("#ffffff"),

                Info = ColorHelper.FromHexColor("#0288d1"),
                OnInfo = ColorHelper.FromHexColor("#ffffff"),

                Success = ColorHelper.FromHexColor("#2e7d32"),
                OnSuccess = ColorHelper.FromHexColor("#ffffff"),

            };
        }

    }
}
