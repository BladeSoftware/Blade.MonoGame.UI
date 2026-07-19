using Blade.MG.UI.Theming;
using Blade.MG.UI.Theming.ColorScience;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace BladeUI.UnitTesting.Tests.ColorScience
{
    /// <summary>
    /// Table-driven check that SchemeMapper picks the correct tone per role for Light vs Dark -
    /// cheap to verify without needing real color comparisons, so a swapped light/dark constant
    /// typo is caught immediately rather than only showing up as a subtly-wrong color later.
    /// </summary>
    [TestClass]
    public class TestSchemeMapper
    {
        [TestMethod]
        public void Map_PicksExpectedTonePerRole_ForLightAndDark()
        {
            var seed = new Color(0x67, 0x50, 0xA4);
            CorePalette palette = CorePalette.FromSeed(seed);
            UITheme accents = DefaultThemes.LightTheme();

            UITheme light = SchemeMapper.Map("Test", palette, isDark: false, accents);
            UITheme dark = SchemeMapper.Map("Test", palette, isDark: true, accents);

            AssertMatchesTone(palette.Primary, 40, light.Primary, "light Primary");
            AssertMatchesTone(palette.Primary, 80, dark.Primary, "dark Primary");
            AssertMatchesTone(palette.Primary, 100, light.OnPrimary, "light OnPrimary");
            AssertMatchesTone(palette.Primary, 20, dark.OnPrimary, "dark OnPrimary");
            AssertMatchesTone(palette.Primary, 90, light.PrimaryContainer, "light PrimaryContainer");
            AssertMatchesTone(palette.Primary, 30, dark.PrimaryContainer, "dark PrimaryContainer");
            AssertMatchesTone(palette.Primary, 10, light.OnPrimaryContainer, "light OnPrimaryContainer");
            AssertMatchesTone(palette.Primary, 90, dark.OnPrimaryContainer, "dark OnPrimaryContainer");

            AssertMatchesTone(palette.Neutral, 99, light.Surface, "light Surface");
            AssertMatchesTone(palette.Neutral, 10, dark.Surface, "dark Surface");
            AssertMatchesTone(palette.Neutral, 10, light.OnSurface, "light OnSurface");
            AssertMatchesTone(palette.Neutral, 90, dark.OnSurface, "dark OnSurface");

            AssertMatchesTone(palette.NeutralVariant, 50, light.Outline, "light Outline");
            AssertMatchesTone(palette.NeutralVariant, 60, dark.Outline, "dark Outline");

            Assert.IsFalse(light.IsDark, "Expected the light scheme's IsDark flag to be false.");
            Assert.IsTrue(dark.IsDark, "Expected the dark scheme's IsDark flag to be true.");
        }

        [TestMethod]
        public void Map_PassesNonM3AccentsThroughUnchanged()
        {
            var seed = new Color(0x67, 0x50, 0xA4);
            CorePalette palette = CorePalette.FromSeed(seed);
            UITheme accents = DefaultThemes.CrimsonTheme();

            UITheme mapped = SchemeMapper.Map("Test", palette, isDark: false, accents);

            Assert.AreEqual(accents.Warning, mapped.Warning);
            Assert.AreEqual(accents.OnWarning, mapped.OnWarning);
            Assert.AreEqual(accents.Info, mapped.Info);
            Assert.AreEqual(accents.OnInfo, mapped.OnInfo);
            Assert.AreEqual(accents.Success, mapped.Success);
            Assert.AreEqual(accents.OnSuccess, mapped.OnSuccess);
            Assert.AreEqual(accents.Disabled, mapped.Disabled);
            Assert.AreEqual(accents.OnDisabled, mapped.OnDisabled);
            Assert.AreEqual(accents.Shadow, mapped.Shadow);
        }

        private static void AssertMatchesTone(TonalPalette palette, int tone, Color actual, string label)
        {
            Color expected = palette.GetTone(tone);
            Assert.AreEqual(expected, actual, $"Expected {label} to equal {palette.Hue:F0}/{palette.Chroma:F0} at tone {tone}.");
        }
    }
}
