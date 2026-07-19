using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Theming.ColorScience
{
    /// <summary>
    /// A color expressed in Hue/Chroma/Tone - Material 3's perceptually-uniform alternative to
    /// RGB/HSB. Hue (0-360) and Chroma (0+, practically ~0-120) come from the CAM16 color
    /// appearance model (<see cref="Cam16"/>); Tone (0-100) is CIE L*, not CAM16's own J
    /// lightness - deliberately, since L* has an exact closed-form relationship to relative
    /// luminance and is what the M3 tone-per-role table (see `SchemeMapper`) is defined in terms
    /// of. "Tone 90" looks equally light regardless of Hue/Chroma, which is the whole point:
    /// classic HSB's Brightness has no such guarantee across different hues.
    /// </summary>
    public readonly struct Hct
    {
        public double Hue { get; }
        public double Chroma { get; }
        public double Tone { get; }

        private Hct(double hue, double chroma, double tone)
        {
            Hue = hue;
            Chroma = chroma;
            Tone = tone;
        }

        /// <summary>
        /// Solves for the closest representable sRGB color at this Hue/Chroma/Tone (see
        /// <see cref="HctSolver"/>), then re-derives Hue/Chroma/Tone from the ACTUAL resulting
        /// color rather than storing the raw request - so an unreachable chroma (e.g. asking for
        /// more saturation than sRGB can represent at that hue/tone) is reflected honestly:
        /// this instance's own <see cref="Chroma"/> is whatever was actually achieved, not the
        /// impossible value that was asked for.
        /// </summary>
        public static Hct From(double hue, double chroma, double tone)
        {
            Color color = HctSolver.SolveToColor(hue, chroma, tone);
            return FromColor(color);
        }

        public static Hct FromColor(Color color)
        {
            var (x, y, z) = ColorSpaceConversions.XyzFromColor(color);
            Cam16 cam = Cam16.FromXyz(x, y, z);
            double tone = ColorSpaceConversions.LstarFromY(y);
            return new Hct(cam.Hue, cam.Chroma, tone);
        }

        public Color ToColor() => HctSolver.SolveToColor(Hue, Chroma, Tone);

        /// <summary>Same Hue/Chroma, a different Tone - e.g. walking a <see cref="TonalPalette"/>.</summary>
        public Hct WithTone(double newTone) => From(Hue, Chroma, newTone);

        public override string ToString() => $"H{Hue:F1} C{Chroma:F1} T{Tone:F1}";
    }
}
