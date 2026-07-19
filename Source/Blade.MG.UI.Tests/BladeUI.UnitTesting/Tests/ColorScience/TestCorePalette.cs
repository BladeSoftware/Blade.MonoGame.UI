using Blade.MG.UI.Theming.ColorScience;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace BladeUI.UnitTesting.Tests.ColorScience
{
    [TestClass]
    public class TestCorePalette
    {
        [TestMethod]
        public void FromSeed_DerivesFixedChromaAndHueOffsetsPerRole()
        {
            var seed = new Color(0x9A, 0x28, 0x46); // Crimson theme's own Primary - a saturated seed
            Hct seedHct = Hct.FromColor(seed);

            CorePalette palette = CorePalette.FromSeed(seed);

            Assert.AreEqual(seedHct.Hue, palette.Primary.Hue, 0.5, "Primary hue should match the seed.");
            Assert.AreEqual(System.Math.Max(48, seedHct.Chroma), palette.Primary.Chroma, 0.5, "Primary chroma should be max(48, seedChroma).");

            Assert.AreEqual(seedHct.Hue, palette.Secondary.Hue, 0.5, "Secondary hue should match the seed.");
            Assert.AreEqual(16.0, palette.Secondary.Chroma, 0.5, "Secondary chroma should be fixed at 16.");

            double expectedTertiaryHue = (seedHct.Hue + 60) % 360;
            Assert.AreEqual(expectedTertiaryHue, palette.Tertiary.Hue, 0.5, "Tertiary hue should be seedHue + 60.");
            Assert.AreEqual(24.0, palette.Tertiary.Chroma, 0.5, "Tertiary chroma should be fixed at 24.");

            Assert.AreEqual(seedHct.Hue, palette.Neutral.Hue, 0.5, "Neutral hue should match the seed.");
            Assert.AreEqual(4.0, palette.Neutral.Chroma, 0.5, "Neutral chroma should be fixed at 4.");

            Assert.AreEqual(seedHct.Hue, palette.NeutralVariant.Hue, 0.5, "NeutralVariant hue should match the seed.");
            Assert.AreEqual(8.0, palette.NeutralVariant.Chroma, 0.5, "NeutralVariant chroma should be fixed at 8.");

            Assert.AreEqual(25.0, palette.Error.Hue, 0.5, "Error hue is fixed regardless of seed.");
            Assert.AreEqual(84.0, palette.Error.Chroma, 0.5, "Error chroma is fixed regardless of seed.");
        }

        [TestMethod]
        public void FromSeed_WithLowChromaSeed_ClampsPrimaryChromaToFloor()
        {
            // A near-gray seed - Chroma well under 48.
            var nearGraySeed = new Color(120, 122, 118);
            Hct seedHct = Hct.FromColor(nearGraySeed);
            Assert.IsTrue(seedHct.Chroma < 48, $"Test setup assumption failed - expected a low-chroma seed, got Chroma={seedHct.Chroma:F1}.");

            CorePalette palette = CorePalette.FromSeed(nearGraySeed);

            Assert.AreEqual(48.0, palette.Primary.Chroma, 0.5, "Expected Primary's chroma floor (48) to apply even for a near-gray seed.");
        }

        [TestMethod]
        public void FromSeed_TertiaryHueWrapsAround360()
        {
            // A seed hue near the top of the range so seedHue + 60 must wrap past 360.
            var highHueSeed = Hct.From(hue: 340, chroma: 40, tone: 40).ToColor();
            CorePalette palette = CorePalette.FromSeed(highHueSeed);

            Assert.IsTrue(palette.Tertiary.Hue is >= 0 and < 360, $"Expected Tertiary hue to be normalized into [0,360), got {palette.Tertiary.Hue:F1}.");
        }
    }
}
