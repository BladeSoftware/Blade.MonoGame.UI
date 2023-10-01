using FontStashSharp;

namespace Blade.MG.UI
{
    /// <summary>
    /// Service to manage multiple registered fonts
    /// </summary>
    public class FontService
    {
        public static string DefaultFontName { get; set; } = "Default";
        public static float DefaultFontSize { get; set; } = 18;

        protected Dictionary<string, FontSystem> Fonts = new Dictionary<string, FontSystem>(StringComparer.InvariantCultureIgnoreCase);

        public void RegisterFont(string fontName, byte[] fontFile)
        {
            if (!Fonts.TryGetValue(fontName, out FontSystem fontSystem))
            {
                fontSystem = new FontSystem();
                Fonts.Add(fontName, fontSystem);
            }

            fontSystem.AddFont(fontFile);
        }

        public SpriteFontBase GetFont(string fontName, float size)
        {
            if (!Fonts.TryGetValue(fontName, out FontSystem fontSystem))
            {
                return null;
            }

            return fontSystem.GetFont(size);
        }

        public SpriteFontBase GetFontOrDefault(string fontName, float? size)
        {
            fontName = string.IsNullOrWhiteSpace(fontName) ? DefaultFontName : fontName;
            size = size == null || size <= 0 ? DefaultFontSize : size;

            return GetFont(fontName, size.Value);
        }

    }
}
