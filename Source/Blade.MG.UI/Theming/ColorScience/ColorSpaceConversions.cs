using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Theming.ColorScience
{
    /// <summary>
    /// sRGB &lt;-&gt; linear RGB &lt;-&gt; XYZ (D65 whitepoint) &lt;-&gt; CIE L* conversions - the
    /// foundation every other file in this folder builds on (<see cref="Cam16"/>,
    /// <see cref="HctSolver"/>). Every constant/formula here is ported verbatim from Google's
    /// own open-source `material-color-utilities` (MIT licensed, color_utils.ts) rather than
    /// re-derived, so results match the reference Material 3 tools exactly - these are the same
    /// matrices/formulas used to produce <see cref="DefaultThemes.LightTheme"/>/
    /// <see cref="DefaultThemes.DarkTheme"/>'s own hand-picked values in the first place (both
    /// are Google's own M3 baseline demo theme, seed #6750A4), which is what
    /// TestThemeGeneratorBaselineRoundTrip verifies against.
    /// </summary>
    internal static class ColorSpaceConversions
    {
        // sRGB (D65) linear RGB -> XYZ and its inverse. Rows/values exact per Google's source.
        private static readonly double[,] SrgbToXyz =
        {
            { 0.41233895, 0.35762064, 0.18051042 },
            { 0.2126,     0.7152,     0.0722     },
            { 0.01932141, 0.11916382, 0.95034478 },
        };

        private static readonly double[,] XyzToSrgb =
        {
            {  3.2413774792388685, -1.5376652402851851, -0.49885366846268053 },
            { -0.9691452513005321,  1.8758853451067872,  0.04156585616912061 },
            {  0.05562093689691305, -0.20395524564742123, 1.0571799111220335 },
        };

        // Relative luminance (Y) contribution of each linear RGB channel - also doubles as the
        // middle row of SrgbToXyz above (X/Y/Z happen to share Y's row for the sRGB primaries).
        public static readonly double[] YFromLinrgb = { 0.2126, 0.7152, 0.0722 };

        public static readonly (double X, double Y, double Z) WhitePointD65 = (95.047, 100.0, 108.883);

        /// <summary>sRGB byte (0-255) -> linear RGB component (0-100 scale, matching XYZ's own scale).</summary>
        public static double Linearized(double srgbComponent0To255)
        {
            double normalized = srgbComponent0To255 / 255.0;
            return normalized <= 0.040449936
                ? normalized / 12.92 * 100.0
                : System.Math.Pow((normalized + 0.055) / 1.055, 2.4) * 100.0;
        }

        /// <summary>Linear RGB component (0-100 scale) -> delinearized sRGB byte, clamped [0,255].</summary>
        public static int Delinearized(double linearComponent0To100)
        {
            double normalized = linearComponent0To100 / 100.0;
            double delinearized = normalized <= 0.0031308
                ? normalized * 12.92
                : 1.055 * System.Math.Pow(normalized, 1.0 / 2.4) - 0.055;

            return System.Math.Clamp((int)System.Math.Round(delinearized * 255.0), 0, 255);
        }

        /// <summary>Linear RGB (each 0-100) -> XYZ (each roughly 0-100, D65-scaled).</summary>
        public static (double X, double Y, double Z) LinearRgbToXyz(double r, double g, double b)
        {
            double x = SrgbToXyz[0, 0] * r + SrgbToXyz[0, 1] * g + SrgbToXyz[0, 2] * b;
            double y = SrgbToXyz[1, 0] * r + SrgbToXyz[1, 1] * g + SrgbToXyz[1, 2] * b;
            double z = SrgbToXyz[2, 0] * r + SrgbToXyz[2, 1] * g + SrgbToXyz[2, 2] * b;
            return (x, y, z);
        }

        /// <summary>XYZ -> linear RGB (each 0-100), the inverse of <see cref="LinearRgbToXyz"/>.</summary>
        public static (double R, double G, double B) XyzToLinearRgb(double x, double y, double z)
        {
            double r = XyzToSrgb[0, 0] * x + XyzToSrgb[0, 1] * y + XyzToSrgb[0, 2] * z;
            double g = XyzToSrgb[1, 0] * x + XyzToSrgb[1, 1] * y + XyzToSrgb[1, 2] * z;
            double b = XyzToSrgb[2, 0] * x + XyzToSrgb[2, 1] * y + XyzToSrgb[2, 2] * z;
            return (r, g, b);
        }

        /// <summary>Delinearizes a linear RGB triple (each 0-100) straight to an opaque Color.</summary>
        public static Color ColorFromLinearRgb(double r, double g, double b)
        {
            return new Color(Delinearized(r), Delinearized(g), Delinearized(b), 255);
        }

        public static Color ColorFromXyz(double x, double y, double z)
        {
            var (r, g, b) = XyzToLinearRgb(x, y, z);
            return ColorFromLinearRgb(r, g, b);
        }

        public static (double X, double Y, double Z) XyzFromColor(Color color)
        {
            double r = Linearized(color.R);
            double g = Linearized(color.G);
            double b = Linearized(color.B);
            return LinearRgbToXyz(r, g, b);
        }

        // CIE L* <-> relative luminance Y (0-100 scale), via the standard Lab "f"/"f-inverse"
        // piecewise functions - epsilon/kappa are the exact CIE-published constants (216/24389,
        // 24389/27), not approximations.
        private const double Epsilon = 216.0 / 24389.0;
        private const double Kappa = 24389.0 / 27.0;

        private static double LabF(double t)
        {
            return t > Epsilon ? System.Math.Cbrt(t) : (Kappa * t + 16.0) / 116.0;
        }

        private static double LabInvF(double ft)
        {
            double ft3 = ft * ft * ft;
            return ft3 > Epsilon ? ft3 : (116.0 * ft - 16.0) / Kappa;
        }

        /// <summary>Relative luminance Y (0-100) -> CIE L* / HCT "Tone" (0-100).</summary>
        public static double LstarFromY(double y)
        {
            return LabF(y / 100.0) * 116.0 - 16.0;
        }

        /// <summary>CIE L* / HCT "Tone" (0-100) -> relative luminance Y (0-100).</summary>
        public static double YFromLstar(double lstar)
        {
            return 100.0 * LabInvF((lstar + 16.0) / 116.0);
        }
    }
}
