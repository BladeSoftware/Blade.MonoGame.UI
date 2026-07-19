using Blade.MG.UI.Theming.ColorScience;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace BladeUI.UnitTesting.Tests.ColorScience
{
    [TestClass]
    public class TestHct
    {
        private static void AssertColorsClose(Color expected, Color actual, int maxDelta, string message)
        {
            int dr = System.Math.Abs(expected.R - actual.R);
            int dg = System.Math.Abs(expected.G - actual.G);
            int db = System.Math.Abs(expected.B - actual.B);

            Assert.IsTrue(dr <= maxDelta && dg <= maxDelta && db <= maxDelta,
                $"{message} Expected {expected}, got {actual} (deltas R={dr} G={dg} B={db}, max allowed {maxDelta}).");
        }

        [TestMethod]
        public void FromColor_ToColor_RoundTrips_ForASpreadOfKnownColors()
        {
            Color[] samples =
            {
                Color.Red, Color.Lime, Color.Blue,
                Color.White, Color.Black, new Color(128, 128, 128),
                new Color(0x67, 0x50, 0xA4), // the M3 baseline seed
                new Color(0x9A, 0x28, 0x46), // Crimson theme's Primary
            };

            foreach (Color sample in samples)
            {
                Hct hct = Hct.FromColor(sample);
                Color roundTripped = hct.ToColor();
                AssertColorsClose(sample, roundTripped, 2, $"Round-tripping {sample} through Hct");
            }
        }

        [TestMethod]
        public void FromColor_ExtractsExpectedToneForKnownGrays()
        {
            Assert.AreEqual(100.0, Hct.FromColor(Color.White).Tone, 0.5, "Expected white to have Tone ~100.");
            Assert.AreEqual(0.0, Hct.FromColor(Color.Black).Tone, 0.5, "Expected black to have Tone ~0.");

            double midGrayTone = Hct.FromColor(new Color(119, 119, 119)).Tone;
            Assert.IsTrue(midGrayTone > 40 && midGrayTone < 60, $"Expected #777777 to have a mid-range Tone, got {midGrayTone:F1}.");
        }

        [TestMethod]
        public void WithTone_PreservesHueAndChroma()
        {
            Hct original = Hct.From(hue: 250, chroma: 40, tone: 50);
            Hct retoned = original.WithTone(70);

            Assert.AreEqual(original.Hue, retoned.Hue, 0.5, "Expected Hue to stay the same after WithTone.");
            Assert.AreEqual(original.Chroma, retoned.Chroma, 0.5, "Expected Chroma to stay the same after WithTone.");
            Assert.AreEqual(70.0, retoned.Tone, 0.5, "Expected Tone to actually change.");
        }

        [TestMethod]
        public void From_ClampsUnreachableChroma_WithoutThrowing()
        {
            // Far beyond anything representable in sRGB at any hue/tone - the gamut-mapping
            // fallback (binary search on chroma, see HctSolver) must still return a valid color
            // instead of throwing or returning an obviously-wrong (e.g. pure black) result.
            Hct extreme = Hct.From(hue: 300, chroma: 1000, tone: 50);
            Color color = extreme.ToColor();

            Assert.IsTrue(color.R != 0 || color.G != 0 || color.B != 0, "Expected a real, non-black clamped color for an unreachable chroma request.");
        }
    }
}
