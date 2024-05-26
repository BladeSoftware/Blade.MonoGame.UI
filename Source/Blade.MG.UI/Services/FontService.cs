using FontStashSharp;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI.Services
{
    /// <summary>
    /// Service to manage multiple registered fonts
    /// </summary>
    public static class FontService
    {
        public static string DefaultFontName { get; set; } = "Default";
        public static float DefaultFontSize { get; set; } = 18;

        [JsonIgnore]
        [XmlIgnore]
        private static Dictionary<string, FontSystem> Fonts = new Dictionary<string, FontSystem>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Load a font from a file name
        /// e.g. RegisterFont("GameFont-Bold", @"Content/Fonts/GameFont-Bold.ttf");
        /// </summary>
        /// <param name="fontName"></param>
        /// <param name="fontFile"></param>
        public static void RegisterFont(string fontName, string fontFile, bool makeDefaultFont = false)
        {
            RegisterFont(fontName, File.ReadAllBytes(fontFile), makeDefaultFont);
        }

        /// <summary>
        /// Load a font from a Byte[]
        /// e.g. RegisterFont("GameFont-Bold", File.ReadAllBytes(@"Content/Fonts/GameFont-Bold.ttf"));
        /// </summary>
        /// <param name="fontName"></param>
        /// <param name="fontFile"></param>
        public static void RegisterFont(string fontName, byte[] fontFile, bool makeDefaultFont = false)
        {
            if (!Fonts.TryGetValue(fontName, out FontSystem fontSystem))
            {
                fontSystem = new FontSystem();
                Fonts.Add(fontName, fontSystem);
            }

            fontSystem.AddFont(fontFile);

            if (makeDefaultFont)
            {
                DefaultFontName = fontName;
            }
        }

        public static SpriteFontBase GetFont(string fontName, float size)
        {
            if (!Fonts.TryGetValue(fontName, out FontSystem fontSystem))
            {
                return null;
            }

            return fontSystem.GetFont(size);
        }

        public static SpriteFontBase GetFontOrDefault(string fontName, float? size)
        {
            fontName = string.IsNullOrWhiteSpace(fontName) ? DefaultFontName : fontName;
            size = size == null || size <= 0 ? DefaultFontSize : size;

            return GetFont(fontName, size.Value) ?? GetFont(DefaultFontName, size.Value);
        }

    }
}
