using Blade.MG.UI.Theming;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace BladeUI.UnitTesting.Tests.Core
{
    /// <summary>
    /// Reproduces the bug where Button (and every other control that swaps to
    /// Theme.SecondaryContainer on hover/selection - CheckBox, ListViewItem, MenuItem,
    /// TabHeader, TreeNode) showed no visible hover feedback in the Light and Crimson themes.
    /// Root cause: DefaultThemes.LightTheme/CrimsonTheme picked a Secondary hue essentially
    /// identical to Primary's own hue/tone (Light: #625B71/#E8DEF8 vs Primary's
    /// #6750A4/#EADDFF; Crimson: #77525A/#FFD9E1 vs Primary's #9A2846/#FFD9DF) - both are the
    /// M3 theme-builder's own baseline export, which designs Primary/Secondary to be
    /// distinguished via surrounding typography/iconography in a full app, not via raw color
    /// alone in a two-state hover toggle. Fixed by shifting Secondary/SecondaryContainer/
    /// OnSecondaryContainer in both themes to a teal accent, clearly separated in hue from
    /// each theme's own Primary.
    /// </summary>
    [TestClass]
    public class TestDefaultThemes
    {
        // Empirically, Dark/Ocean/Forest (which never exhibited the bug) sit at an RGB
        // Euclidean distance of ~53-95 between PrimaryContainer and SecondaryContainer, while
        // the two broken themes measured ~2 (Crimson) and ~7 (Light) before the fix - so a
        // threshold well below the "working" themes' floor, but comfortably above the
        // "broken" themes' actual measurements, catches this class of bug without being so
        // strict it demands a specific palette.
        private const double MinimumContainerColorDistance = 30.0;

        private static double ColorDistance(Color a, Color b)
        {
            double dr = a.R - b.R;
            double dg = a.G - b.G;
            double db = a.B - b.B;
            return System.Math.Sqrt(dr * dr + dg * dg + db * db);
        }

        [TestMethod]
        public void EveryDefaultTheme_HasVisuallyDistinctPrimaryAndSecondaryContainers()
        {
            foreach (var theme in DefaultThemes.All)
            {
                // ButtonTemplate's hover state (and CheckBox/ListViewItem/MenuItem/TabHeader/
                // TreeNode's own equivalents) swap BOTH the container fill (PrimaryContainer ->
                // SecondaryContainer) and the content color drawn on top of it
                // (OnPrimaryContainer -> OnSecondaryContainer) at the same time - so hover is
                // still visibly distinguishable if EITHER pair differs enough, even if the
                // other doesn't. HighContrastTheme is exactly this case: every *Container role
                // is deliberately pure black (undistinguishable fills by design), relying
                // entirely on its neon On*Container colors (yellow vs cyan) to carry the
                // hover/selection cue instead.
                double fillDistance = ColorDistance(theme.PrimaryContainer, theme.SecondaryContainer);
                double contentDistance = ColorDistance(theme.OnPrimaryContainer, theme.OnSecondaryContainer);

                Assert.IsTrue(fillDistance >= MinimumContainerColorDistance || contentDistance >= MinimumContainerColorDistance,
                    $"Theme '{theme.Name}': neither the container fill (PrimaryContainer {theme.PrimaryContainer} vs SecondaryContainer {theme.SecondaryContainer}, distance {fillDistance:F1}) nor its content color (OnPrimaryContainer {theme.OnPrimaryContainer} vs OnSecondaryContainer {theme.OnSecondaryContainer}, distance {contentDistance:F1}) differ enough for controls that swap between them (e.g. Button's hover state) to show a visible change, matching the reported 'hover isn't visible' bug.");
            }
        }
    }
}
