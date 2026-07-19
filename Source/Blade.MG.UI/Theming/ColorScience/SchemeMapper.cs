namespace Blade.MG.UI.Theming.ColorScience
{
    /// <summary>
    /// Maps a <see cref="CorePalette"/> + Light/Dark to a complete <see cref="UITheme"/>, using
    /// the fixed tone-per-role table (tone 0-100 = CIE L*/HCT Tone) Material 3 defines.
    /// Cross-checked (and, for OnPrimaryContainer/OnSecondaryContainer/OnTertiaryContainer/
    /// OnErrorContainer and Background/Surface, corrected) against `DefaultThemes.cs`'s own
    /// LightTheme/DarkTheme - both are Google's own M3 baseline demo theme (seed #6750A4), so
    /// computing `Hct.FromColor(existingColor).Tone` for every role there is a more reliable
    /// ground truth than a hand-transcribed copy of the published spec table, and is exactly
    /// what `TestThemeGeneratorBaselineRoundTrip` verifies this table against. Warning/Info/
    /// Success/Disabled/Shadow aren't standard M3 roles - every existing hand-authored theme in
    /// `DefaultThemes.cs` reuses the same fixed accents regardless of seed/brightness (aside from
    /// Disabled/OnDisabled, which genuinely differs between Light and Dark) - so those come from
    /// <paramref name="accentSource"/> rather than being derived here.
    /// </summary>
    internal static class SchemeMapper
    {
        public static UITheme Map(string name, CorePalette palette, bool isDark, UITheme accentSource)
        {
            TonalPalette p = palette.Primary;
            TonalPalette s = palette.Secondary;
            TonalPalette t = palette.Tertiary;
            TonalPalette e = palette.Error;
            TonalPalette n = palette.Neutral;
            TonalPalette nv = palette.NeutralVariant;

            return new UITheme
            {
                Name = name,
                IsDark = isDark,

                Primary = p.GetTone(isDark ? 80 : 40),
                OnPrimary = p.GetTone(isDark ? 20 : 100),
                PrimaryContainer = p.GetTone(isDark ? 30 : 90),
                OnPrimaryContainer = p.GetTone(isDark ? 90 : 10),

                Secondary = s.GetTone(isDark ? 80 : 40),
                OnSecondary = s.GetTone(isDark ? 20 : 100),
                SecondaryContainer = s.GetTone(isDark ? 30 : 90),
                OnSecondaryContainer = s.GetTone(isDark ? 90 : 10),

                Tertiary = t.GetTone(isDark ? 80 : 40),
                OnTertiary = t.GetTone(isDark ? 20 : 100),
                TertiaryContainer = t.GetTone(isDark ? 30 : 90),
                OnTertiaryContainer = t.GetTone(isDark ? 90 : 10),

                Error = e.GetTone(isDark ? 80 : 40),
                OnError = e.GetTone(isDark ? 20 : 100),
                ErrorContainer = e.GetTone(isDark ? 30 : 90),
                OnErrorContainer = e.GetTone(isDark ? 90 : 10),

                Background = n.GetTone(isDark ? 10 : 99),
                OnBackground = n.GetTone(isDark ? 90 : 10),

                Surface = n.GetTone(isDark ? 10 : 99),
                OnSurface = n.GetTone(isDark ? 90 : 10),

                // Not a fixed-list "core" M3 role in the verified table, but a well-established
                // later addition (the surfaceContainer tier) - tone 12/94 is the middle of the
                // 5-step surfaceContainer[Lowest|Low||High|Highest] ramp, on the Neutral palette
                // like Surface/Background.
                SurfaceContainer = n.GetTone(isDark ? 12 : 94),

                SurfaceVariant = nv.GetTone(isDark ? 30 : 90),
                OnSurfaceVariant = nv.GetTone(isDark ? 80 : 30),

                Outline = nv.GetTone(isDark ? 60 : 50),

                // Non-M3 roles - passed through from accentSource, not derived from the seed.
                Warning = accentSource.Warning,
                OnWarning = accentSource.OnWarning,
                Info = accentSource.Info,
                OnInfo = accentSource.OnInfo,
                Success = accentSource.Success,
                OnSuccess = accentSource.OnSuccess,
                Disabled = accentSource.Disabled,
                OnDisabled = accentSource.OnDisabled,
                Shadow = accentSource.Shadow,
            };
        }
    }
}
