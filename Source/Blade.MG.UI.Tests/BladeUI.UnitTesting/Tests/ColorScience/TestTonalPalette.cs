using Blade.MG.UI.Theming.ColorScience;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace BladeUI.UnitTesting.Tests.ColorScience
{
    [TestClass]
    public class TestTonalPalette
    {
        [TestMethod]
        public void GetHct_AtMidTone_MatchesSeedHueAndChroma()
        {
            var seed = new Color(0x67, 0x50, 0xA4);
            Hct seedHct = Hct.FromColor(seed);

            TonalPalette palette = TonalPalette.FromColor(seed);

            // Tone 40 is comfortably inside the gamut for this seed's chroma (it's literally
            // where Primary sits in the Light scheme) - safe to check hue/chroma preservation
            // here without hitting the gamut-mapping fallback.
            Hct atMidTone = palette.GetHct(40);

            Assert.AreEqual(seedHct.Hue, atMidTone.Hue, 0.5, "Expected the palette's Hue to match the seed's own Hue.");
            Assert.AreEqual(seedHct.Chroma, atMidTone.Chroma, 0.5, "Expected the palette's Chroma to match the seed's own Chroma at a reachable tone.");
        }

        [TestMethod]
        public void GetTone_AtExtremes_ApproachesBlackAndWhite()
        {
            TonalPalette[] palettes =
            {
                TonalPalette.FromHueAndChroma(0, 40),
                TonalPalette.FromHueAndChroma(120, 60),
                TonalPalette.FromHueAndChroma(280, 20),
            };

            foreach (TonalPalette palette in palettes)
            {
                Color black = palette.GetTone(0);
                Color white = palette.GetTone(100);

                Assert.IsTrue(black.R < 10 && black.G < 10 && black.B < 10, $"Expected Tone 0 to be near-black for hue {palette.Hue}, got {black}.");
                Assert.IsTrue(white.R > 245 && white.G > 245 && white.B > 245, $"Expected Tone 100 to be near-white for hue {palette.Hue}, got {white}.");
            }
        }
    }
}
