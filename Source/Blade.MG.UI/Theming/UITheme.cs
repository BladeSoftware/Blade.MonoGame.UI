using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Theming
{
    /// <summary>
    /// A complete color palette for the UI. Controls should always source color from
    /// Theme.XXX rather than hard-coding a Color, so that swapping the active theme (see
    /// UIManager.SetTheme) restyles the whole app live. See DefaultThemes for the built-in
    /// Light/Dark/HighContrast palettes, and use them as a template for custom themes.
    /// </summary>
    public class UITheme
    {
        /// <summary>Display name shown in theme-picker UI, e.g. "Light", "Dark".</summary>
        public string Name;

        /// <summary>
        /// True for themes with a dark Surface/Background, so controls that need to pick
        /// between two variants of an asset/effect (e.g. shadow opacity) can branch on it
        /// instead of comparing colors.
        /// </summary>
        public bool IsDark;

        public Color Primary;
        public Color OnPrimary;
        public Color PrimaryContainer;
        public Color OnPrimaryContainer;

        public Color Secondary;
        public Color OnSecondary;
        public Color SecondaryContainer;
        public Color OnSecondaryContainer;

        public Color Tertiary;
        public Color OnTertiary;
        public Color TertiaryContainer;
        public Color OnTertiaryContainer;

        public Color Error;
        public Color OnError;
        public Color ErrorContainer;
        public Color OnErrorContainer;

        public Color Outline;

        public Color Warning;
        public Color OnWarning;

        public Color Info;
        public Color OnInfo;

        public Color Success;
        public Color OnSuccess;

        public Color Background;
        public Color OnBackground;

        public Color Surface;
        public Color OnSurface;

        public Color SurfaceVariant;
        public Color OnSurfaceVariant;

        /// <summary>Color for disabled controls' content (text, icons).</summary>
        public Color Disabled;
        /// <summary>Background for disabled controls.</summary>
        public Color OnDisabled;

        /// <summary>Base color for drop shadows (e.g. Border's Elevation); apply alpha at the draw site.</summary>
        public Color Shadow;

        public override string ToString() => Name ?? base.ToString();
    }
}
