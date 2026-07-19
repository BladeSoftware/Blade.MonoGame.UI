using Microsoft.Xna.Framework;
using System;

namespace Blade.MG.UI.Theming.ColorScience
{
    /// <summary>
    /// Reverse HCT solve: given a target Hue/Chroma/Tone, find the closest representable sRGB
    /// color. There's no closed form for this direction (CAM16's hue/chroma don't invert
    /// cleanly, and not every Hue/Chroma/Tone combination is even representable in sRGB), so
    /// this needs an iterative solve.
    ///
    /// <see cref="TryFindResultByJ"/> is a verbatim port of Google's `material-color-utilities`
    /// `HctSolver.findResultByJ` (Newton's method on CAM16 lightness J, 5 iterations) - this
    /// handles the common case where the target is representable in-gamut, which covers every
    /// role/tone this library's <see cref="ThemeGenerator"/> actually asks for (Primary/
    /// Secondary/Tertiary/Neutral/NeutralVariant/Error's fixed chroma values, at the fixed M3
    /// tone-per-role table) - confirmed by <c>TestThemeGeneratorBaselineRoundTrip</c>
    /// reproducing <see cref="DefaultThemes.LightTheme"/>/<see cref="DefaultThemes.DarkTheme"/>'s
    /// existing hand-picked values from their shared seed.
    ///
    /// Google's own solver falls back to a geometric RGB-cube-boundary search (bisecting across
    /// 255 precomputed "critical plane" values) when the exact Newton solve fails - i.e. when the
    /// requested chroma exceeds what's achievable at that hue/tone. This port takes a simpler,
    /// from-first-principles equivalent instead: binary-search the CHROMA itself downward from
    /// the target using <see cref="TryFindResultByJ"/> as the in-gamut oracle, converging on the
    /// maximum achievable chroma at that hue/tone. Same end result (the closest representable
    /// color, hue/tone preserved, chroma reduced only as far as gamut requires), built entirely
    /// from the already-verified interior solve above rather than a second, separate algorithm
    /// needing its own 255-entry lookup table.
    /// </summary>
    internal static class HctSolver
    {
        // Folds CAT16 adaptation + ViewingConditions.Default's own rgbD scale factors into one
        // fixed matrix, since findResultByJ only ever runs under the Default viewing conditions.
        private static readonly double[,] LinrgbFromScaledDiscount =
        {
            { 1373.2198709594231,  -1100.4251190754821,   -7.278681089101213  },
            { -271.815969077903,    559.6580465940733,   -32.46047482791194  },
            {    1.9622899599665666, -57.173814538844006, 308.7233197812385  },
        };

        public static Color SolveToColor(double hueDegrees, double chroma, double tone)
        {
            if (chroma < 0.0001 || tone < 0.0001 || tone > 99.9999)
            {
                return GrayFromTone(tone);
            }

            double hueRadians = SanitizeDegrees(hueDegrees) / 180.0 * Math.PI;
            double y = ColorSpaceConversions.YFromLstar(tone);

            if (TryFindResultByJ(hueRadians, chroma, y, out Color exact))
            {
                return exact;
            }

            // Requested chroma isn't reachable in-gamut at this hue/tone - binary-search down
            // from the target to find the maximum chroma that is.
            double lo = 0.0;
            double hi = chroma;
            Color best = GrayFromTone(tone);
            for (int i = 0; i < 40; i++)
            {
                double mid = (lo + hi) / 2.0;
                if (TryFindResultByJ(hueRadians, mid, y, out Color candidate))
                {
                    best = candidate;
                    lo = mid;
                }
                else
                {
                    hi = mid;
                }
            }

            return best;
        }

        private static bool TryFindResultByJ(double hueRadians, double chroma, double y, out Color result)
        {
            result = default;

            double j = Math.Sqrt(y) * 11.0;

            ViewingConditions vc = ViewingConditions.Default;
            double tInnerCoeff = 1.0 / Math.Pow(1.64 - Math.Pow(0.29, vc.N), 0.73);
            double eHue = 0.25 * (Math.Cos(hueRadians + 2.0) + 3.8);
            double p1 = eHue * (50000.0 / 13.0) * vc.Nc * vc.Ncb;
            double hSin = Math.Sin(hueRadians);
            double hCos = Math.Cos(hueRadians);

            for (int iterationRound = 0; iterationRound < 5; iterationRound++)
            {
                double jNormalized = j / 100.0;
                double alpha = chroma == 0.0 || j == 0.0 ? 0.0 : chroma / Math.Sqrt(jNormalized);
                double t = Math.Pow(alpha * tInnerCoeff, 1.0 / 0.9);
                double ac = vc.Aw * Math.Pow(jNormalized, 1.0 / vc.C / vc.Z);
                double p2 = ac / vc.Nbb;
                double gamma = 23.0 * (p2 + 0.305) * t / (23.0 * p1 + 11.0 * t * hCos + 108.0 * t * hSin);
                double a = gamma * hCos;
                double b = gamma * hSin;
                double rA = (460.0 * p2 + 451.0 * a + 288.0 * b) / 1403.0;
                double gA = (460.0 * p2 - 891.0 * a - 261.0 * b) / 1403.0;
                double bA = (460.0 * p2 - 220.0 * a - 6300.0 * b) / 1403.0;
                double rCScaled = InverseChromaticAdaptation(rA);
                double gCScaled = InverseChromaticAdaptation(gA);
                double bCScaled = InverseChromaticAdaptation(bA);

                double linR = rCScaled * LinrgbFromScaledDiscount[0, 0] + gCScaled * LinrgbFromScaledDiscount[0, 1] + bCScaled * LinrgbFromScaledDiscount[0, 2];
                double linG = rCScaled * LinrgbFromScaledDiscount[1, 0] + gCScaled * LinrgbFromScaledDiscount[1, 1] + bCScaled * LinrgbFromScaledDiscount[1, 2];
                double linB = rCScaled * LinrgbFromScaledDiscount[2, 0] + gCScaled * LinrgbFromScaledDiscount[2, 1] + bCScaled * LinrgbFromScaledDiscount[2, 2];

                if (linR < 0 || linG < 0 || linB < 0)
                {
                    return false;
                }

                double fnj = ColorSpaceConversions.YFromLinrgb[0] * linR + ColorSpaceConversions.YFromLinrgb[1] * linG + ColorSpaceConversions.YFromLinrgb[2] * linB;
                if (fnj <= 0)
                {
                    return false;
                }

                if (iterationRound == 4 || Math.Abs(fnj - y) < 0.002)
                {
                    if (linR > 100.01 || linG > 100.01 || linB > 100.01)
                    {
                        return false;
                    }

                    result = ColorSpaceConversions.ColorFromLinearRgb(linR, linG, linB);
                    return true;
                }

                // Newton's method, using 2*fn(j)/j as the approximation of fn'(j).
                j -= (fnj - y) * j / (2 * fnj);
            }

            return false;
        }

        private static double InverseChromaticAdaptation(double adapted)
        {
            double adaptedAbs = Math.Abs(adapted);
            double baseValue = Math.Max(0, 27.13 * adaptedAbs / (400.0 - adaptedAbs));
            return Math.Sign(adapted) * Math.Pow(baseValue, 1.0 / 0.42);
        }

        private static Color GrayFromTone(double tone)
        {
            double y = ColorSpaceConversions.YFromLstar(tone);
            int component = ColorSpaceConversions.Delinearized(y);
            return new Color(component, component, component, 255);
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
