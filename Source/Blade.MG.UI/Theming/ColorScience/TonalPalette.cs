using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Theming.ColorScience
{
    /// <summary>
    /// A fixed Hue+Chroma, walkable across the full Tone range (0-100) - e.g. "the Primary
    /// palette" for a given seed. Every M3 role (Primary/Secondary/Tertiary/Neutral/
    /// NeutralVariant/Error, see <see cref="CorePalette"/>) is one of these; <see cref="SchemeMapper"/>
    /// picks which tone each role resolves to for Light vs Dark.
    /// </summary>
    public class TonalPalette
    {
        public double Hue { get; }
        public double Chroma { get; }

        private TonalPalette(double hue, double chroma)
        {
            Hue = hue;
            Chroma = chroma;
        }

        public static TonalPalette FromHueAndChroma(double hue, double chroma) => new TonalPalette(hue, chroma);

        public static TonalPalette FromColor(Color color)
        {
            Hct hct = Hct.FromColor(color);
            return new TonalPalette(hct.Hue, hct.Chroma);
        }

        public Hct GetHct(int tone) => Hct.From(Hue, Chroma, tone);

        public Color GetTone(int tone) => GetHct(tone).ToColor();
    }
}
