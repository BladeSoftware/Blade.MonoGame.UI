using Microsoft.Xna.Framework;
using System;

namespace Blade.MG.UI.Theming.ColorScience
{
    /// <summary>
    /// Derives the 5 M3 "role" tonal palettes (plus a fixed Error palette) from a single seed
    /// color, via the exact hue/chroma rules Google's own Material Theme Builder-style tools use
    /// ("non-content" mode - the standard default, not the alternate "content" mode tuned for
    /// deriving a theme from an image/icon rather than a plain seed color).
    /// </summary>
    public class CorePalette
    {
        public TonalPalette Primary { get; }
        public TonalPalette Secondary { get; }
        public TonalPalette Tertiary { get; }
        public TonalPalette Neutral { get; }
        public TonalPalette NeutralVariant { get; }
        public TonalPalette Error { get; }

        private CorePalette(TonalPalette primary, TonalPalette secondary, TonalPalette tertiary, TonalPalette neutral, TonalPalette neutralVariant, TonalPalette error)
        {
            Primary = primary;
            Secondary = secondary;
            Tertiary = tertiary;
            Neutral = neutral;
            NeutralVariant = neutralVariant;
            Error = error;
        }

        public static CorePalette FromSeed(Color seed)
        {
            Hct seedHct = Hct.FromColor(seed);
            double hue = seedHct.Hue;
            double chroma = seedHct.Chroma;

            // A near-gray seed still yields a moderately colorful Primary (chroma floor of 48) -
            // counterintuitive on first read, but this is exactly what the spec calls for: a
            // low-chroma seed shouldn't produce a fully desaturated (gray) Primary role.
            var primary = TonalPalette.FromHueAndChroma(hue, Math.Max(48, chroma));
            var secondary = TonalPalette.FromHueAndChroma(hue, 16);
            var tertiary = TonalPalette.FromHueAndChroma(SanitizeDegrees(hue + 60), 24);
            var neutral = TonalPalette.FromHueAndChroma(hue, 4);
            var neutralVariant = TonalPalette.FromHueAndChroma(hue, 8);

            // Error ignores the seed entirely - always the same hue/chroma regardless of theme.
            var error = TonalPalette.FromHueAndChroma(25, 84);

            return new CorePalette(primary, secondary, tertiary, neutral, neutralVariant, error);
        }

        private static double SanitizeDegrees(double degrees)
        {
            degrees %= 360.0;
            if (degrees < 0)
            {
                degrees += 360.0;
            }

            return degrees;
        }
    }
}
