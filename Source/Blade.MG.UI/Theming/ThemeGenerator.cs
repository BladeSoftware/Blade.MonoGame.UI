using Blade.MG.UI.Theming.ColorScience;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Theming
{
    /// <summary>
    /// Generates a complete, perceptually-balanced <see cref="UITheme"/> from a single seed
    /// color - Material 3's "dynamic color" behavior, matching Google's own Material Theme
    /// Builder and similar tools. Not intended for per-frame use - each call runs a real HCT
    /// solve (<see cref="ColorScience.HctSolver"/>) per role, which is one-shot-generation cost,
    /// not hot-path cost.
    /// </summary>
    public static class ThemeGenerator
    {
        /// <summary>
        /// Builds a theme from a seed color and brightness. <paramref name="accentSource"/>
        /// supplies the roles Material 3 doesn't define (Warning/Info/Success/Shadow, which
        /// every built-in theme reuses unchanged regardless of seed/brightness, and Disabled/
        /// OnDisabled, which - unlike those - genuinely differs between Light and Dark) - when
        /// omitted, defaults to this library's own <see cref="DefaultThemes.LightTheme"/>/
        /// <see cref="DefaultThemes.DarkTheme"/> picked by <paramref name="isDark"/>.
        /// </summary>
        public static UITheme GenerateFromSeed(string name, Color seed, bool isDark, UITheme accentSource = null)
        {
            accentSource ??= isDark ? DefaultThemes.DarkTheme() : DefaultThemes.LightTheme();
            CorePalette corePalette = CorePalette.FromSeed(seed);
            return SchemeMapper.Map(name, corePalette, isDark, accentSource);
        }

        /// <summary>Both brightness variants of the same seed in one call.</summary>
        public static (UITheme Light, UITheme Dark) GenerateBoth(string name, Color seed)
        {
            return (GenerateFromSeed(name, seed, false), GenerateFromSeed(name, seed, true));
        }

        /// <summary>
        /// Derives the opposite-brightness counterpart of an existing theme (built-in or
        /// custom) - e.g. a Dark Crimson from Light Crimson - using that theme's own Primary
        /// color as the seed. Preserves <paramref name="source"/>'s own Warning/Info/Success/
        /// Shadow accents, but Disabled/OnDisabled come from the matching-brightness built-in
        /// default instead of a straight copy - copying them directly would pair (e.g.) Light's
        /// Disabled colors with a newly-generated Dark theme, since that pair genuinely differs
        /// by brightness even though it isn't seed-derived.
        /// </summary>
        public static UITheme GenerateCounterpart(UITheme source)
        {
            bool isDark = !source.IsDark;
            UITheme generated = GenerateFromSeed(source.Name, source.Primary, isDark, source);

            UITheme brightnessDefault = isDark ? DefaultThemes.DarkTheme() : DefaultThemes.LightTheme();
            generated.Disabled = brightnessDefault.Disabled;
            generated.OnDisabled = brightnessDefault.OnDisabled;

            return generated;
        }
    }
}
