using FontStashSharp;
using System.Text.Json.Serialization;

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
        private static Dictionary<string, FontSystem> Fonts = new Dictionary<string, FontSystem>(StringComparer.InvariantCultureIgnoreCase);

        // Tracks which font data has already been added to each named FontSystem, so that
        // registering the same font twice (e.g. every UIWindow registering "Default" on
        // Initialize) is a no-op instead of adding duplicate font faces to the atlas.
        [JsonIgnore]
        private static Dictionary<string, List<byte[]>> RegisteredFontData = new Dictionary<string, List<byte[]>>(StringComparer.InvariantCultureIgnoreCase);

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
        /// Registering identical font data under the same name more than once (e.g. from
        /// multiple UIWindows) is safe and will not add duplicate font faces.
        /// </summary>
        /// <param name="fontName"></param>
        /// <param name="fontFile"></param>
        public static void RegisterFont(string fontName, byte[] fontFile, bool makeDefaultFont = false)
        {
            if (!Fonts.TryGetValue(fontName, out FontSystem fontSystem))
            {
                fontSystem = new FontSystem();
                Fonts.Add(fontName, fontSystem);
                RegisteredFontData.Add(fontName, new List<byte[]>());
            }

            List<byte[]> registered = RegisteredFontData[fontName];
            bool alreadyRegistered = registered.Any(existing => existing.AsSpan().SequenceEqual(fontFile));

            if (!alreadyRegistered)
            {
                fontSystem.AddFont(fontFile);
                registered.Add(fontFile);
            }

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

        public static SpriteFontBase GetDefaultFont(float? size = null)
        {
            return GetFont(DefaultFontName, size ?? DefaultFontSize);
        }

    }
}
