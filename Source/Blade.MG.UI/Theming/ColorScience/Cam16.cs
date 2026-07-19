using System;

namespace Blade.MG.UI.Theming.ColorScience
{
    /// <summary>
    /// The fixed CAM16 viewing conditions HCT is always computed under - ported verbatim from
    /// Google's `material-color-utilities` `ViewingConditions.make()` (default whitepoint D65,
    /// backgroundLstar 50, surround 2.0, illuminant not discounted). Every downstream HCT
    /// color in this library uses <see cref="Default"/> exclusively - there is no reason for a
    /// UI theme to vary viewing conditions per color, so the general `Make` overload exists only
    /// to document/preserve the derivation, not because callers should use it directly.
    /// </summary>
    internal readonly struct ViewingConditions
    {
        public double N { get; }
        public double Aw { get; }
        public double Nbb { get; }
        public double Ncb { get; }
        public double C { get; }
        public double Nc { get; }
        public double[] RgbD { get; }
        public double Fl { get; }
        public double FlRoot { get; }
        public double Z { get; }

        private ViewingConditions(double n, double aw, double nbb, double ncb, double c, double nc, double[] rgbD, double fl, double flRoot, double z)
        {
            N = n;
            Aw = aw;
            Nbb = nbb;
            Ncb = ncb;
            C = c;
            Nc = nc;
            RgbD = rgbD;
            Fl = fl;
            FlRoot = flRoot;
            Z = z;
        }

        public static readonly ViewingConditions Default = Make();

        public static ViewingConditions Make(
            (double X, double Y, double Z)? whitePoint = null,
            double? adaptingLuminance = null,
            double backgroundLstar = 50.0,
            double surround = 2.0,
            bool discountingIlluminant = false)
        {
            var xyz = whitePoint ?? ColorSpaceConversions.WhitePointD65;
            double la = adaptingLuminance ?? (200.0 / Math.PI) * ColorSpaceConversions.YFromLstar(50.0) / 100.0;

            double rW = xyz.X * 0.401288 + xyz.Y * 0.650173 + xyz.Z * -0.051461;
            double gW = xyz.X * -0.250268 + xyz.Y * 1.204414 + xyz.Z * 0.045854;
            double bW = xyz.X * -0.002079 + xyz.Y * 0.048952 + xyz.Z * 0.953127;

            double f = 0.8 + surround / 10.0;
            double c = f >= 0.9
                ? Lerp(0.59, 0.69, (f - 0.9) * 10.0)
                : Lerp(0.525, 0.59, (f - 0.8) * 10.0);

            double d = discountingIlluminant
                ? 1.0
                : f * (1.0 - (1.0 / 3.6) * Math.Exp((-la - 42.0) / 92.0));
            d = Math.Clamp(d, 0.0, 1.0);

            double nc = f;
            double[] rgbD =
            {
                d * (100.0 / rW) + 1.0 - d,
                d * (100.0 / gW) + 1.0 - d,
                d * (100.0 / bW) + 1.0 - d,
            };

            double k = 1.0 / (5.0 * la + 1.0);
            double k4 = k * k * k * k;
            double k4F = 1.0 - k4;
            double fl = k4 * la + 0.1 * k4F * k4F * Math.Cbrt(5.0 * la);

            double n = ColorSpaceConversions.YFromLstar(backgroundLstar) / xyz.Y;
            double z = 1.48 + Math.Sqrt(n);
            double nbb = 0.725 / Math.Pow(n, 0.2);
            double ncb = nbb;

            double[] rgbAFactors =
            {
                Math.Pow(fl * rgbD[0] * rW / 100.0, 0.42),
                Math.Pow(fl * rgbD[1] * gW / 100.0, 0.42),
                Math.Pow(fl * rgbD[2] * bW / 100.0, 0.42),
            };
            double[] rgbA =
            {
                400.0 * rgbAFactors[0] / (rgbAFactors[0] + 27.13),
                400.0 * rgbAFactors[1] / (rgbAFactors[1] + 27.13),
                400.0 * rgbAFactors[2] / (rgbAFactors[2] + 27.13),
            };

            double aw = (2.0 * rgbA[0] + rgbA[1] + 0.05 * rgbA[2]) * nbb;

            return new ViewingConditions(n, aw, nbb, ncb, c, nc, rgbD, fl, Math.Pow(fl, 0.25), z);
        }

        private static double Lerp(double a, double b, double amount) => a + (b - a) * amount;
    }

    /// <summary>
    /// CAM16 color appearance model - the Hue/Chroma half of HCT (Tone is CIE L*, computed
    /// separately, see <see cref="ColorSpaceConversions.LstarFromY"/>). <see cref="FromXyz"/> is
    /// ported verbatim from `Cam16.fromXyzInViewingConditions` in Google's
    /// `material-color-utilities` - every constant/operation matches, including the CAM16-UCS
    /// terms (Jstar/Astar/Bstar) even though this library only currently consumes Hue/Chroma/J,
    /// so a future need for perceptually-uniform color distance (CAM16-UCS deltaE) doesn't
    /// require touching this method again.
    /// </summary>
    internal readonly struct Cam16
    {
        public double Hue { get; }
        public double Chroma { get; }
        public double J { get; }
        public double Q { get; }
        public double M { get; }
        public double S { get; }
        public double Jstar { get; }
        public double Astar { get; }
        public double Bstar { get; }

        private Cam16(double hue, double chroma, double j, double q, double m, double s, double jstar, double astar, double bstar)
        {
            Hue = hue;
            Chroma = chroma;
            J = j;
            Q = q;
            M = m;
            S = s;
            Jstar = jstar;
            Astar = astar;
            Bstar = bstar;
        }

        public static Cam16 FromXyz(double x, double y, double z, ViewingConditions vc)
        {
            double rC = 0.401288 * x + 0.650173 * y - 0.051461 * z;
            double gC = -0.250268 * x + 1.204414 * y + 0.045854 * z;
            double bC = -0.002079 * x + 0.048952 * y + 0.953127 * z;

            double rD = vc.RgbD[0] * rC;
            double gD = vc.RgbD[1] * gC;
            double bD = vc.RgbD[2] * bC;

            double rAF = Math.Pow(vc.Fl * Math.Abs(rD) / 100.0, 0.42);
            double gAF = Math.Pow(vc.Fl * Math.Abs(gD) / 100.0, 0.42);
            double bAF = Math.Pow(vc.Fl * Math.Abs(bD) / 100.0, 0.42);
            double rA = Math.Sign(rD) * 400.0 * rAF / (rAF + 27.13);
            double gA = Math.Sign(gD) * 400.0 * gAF / (gAF + 27.13);
            double bA = Math.Sign(bD) * 400.0 * bAF / (bAF + 27.13);

            double a = (11.0 * rA + -12.0 * gA + bA) / 11.0;
            double b = (rA + gA - 2.0 * bA) / 9.0;

            double u = (20.0 * rA + 20.0 * gA + 21.0 * bA) / 20.0;
            double p2 = (40.0 * rA + 20.0 * gA + bA) / 20.0;

            double atan2 = Math.Atan2(b, a);
            double atanDegrees = atan2 * 180.0 / Math.PI;
            double hue = atanDegrees < 0 ? atanDegrees + 360.0
                : atanDegrees >= 360 ? atanDegrees - 360
                : atanDegrees;
            double hueRadians = hue * Math.PI / 180.0;

            double ac = p2 * vc.Nbb;

            double j = 100.0 * Math.Pow(ac / vc.Aw, vc.C * vc.Z);
            double q = 4.0 / vc.C * Math.Sqrt(j / 100.0) * (vc.Aw + 4.0) * vc.FlRoot;

            double huePrime = hue < 20.14 ? hue + 360 : hue;
            double eHue = 1.0 / 4.0 * (Math.Cos(huePrime * Math.PI / 180.0 + 2.0) + 3.8);
            double p1 = 50000.0 / 13.0 * eHue * vc.Nc * vc.Ncb;
            double t = p1 * Math.Sqrt(a * a + b * b) / (u + 0.305);
            double alpha = Math.Pow(t, 0.9) * Math.Pow(1.64 - Math.Pow(0.29, vc.N), 0.73);

            double cChroma = alpha * Math.Sqrt(j / 100.0);
            double m = cChroma * vc.FlRoot;
            double s = 50.0 * Math.Sqrt(alpha * vc.C / (vc.Aw + 4.0));

            double jstar = (1.0 + 100.0 * 0.007) * j / (1.0 + 0.007 * j);
            double mstar = Math.Log(1.0 + 0.0228 * m) / 0.0228;
            double astar = mstar * Math.Cos(hueRadians);
            double bstar = mstar * Math.Sin(hueRadians);

            return new Cam16(hue, cChroma, j, q, m, s, jstar, astar, bstar);
        }

        public static Cam16 FromXyz(double x, double y, double z) => FromXyz(x, y, z, ViewingConditions.Default);
    }
}
