using Blade.MG.UI.Renderer;
using Blade.MG.UI.Theming;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI
{
    public class UIContext
    {
        [JsonIgnore]
        [XmlIgnore]
        public Game Game { get; set; }
        //public ContentManager Content { get; init; }

        [JsonIgnore]
        [XmlIgnore]
        public GraphicsDevice GraphicsDevice => Game?.GraphicsDevice;
        //public SpriteBatch SpriteBatch { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public Texture2D Pixel { get; set; } // A 1x1 Texture with Color #FFFFFFFF (White)

        [JsonIgnore]
        [XmlIgnore]
        public UIRenderer Renderer { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public UITheme Theme { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public GameTime GameTime { get; set; }

    }
}
