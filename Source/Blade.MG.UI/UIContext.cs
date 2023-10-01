using Blade.MG.UI.Renderer;
using Blade.MG.UI.Theming;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Blade.MG.UI
{
    public class UIContext
    {
        public Game Game { get; set; }
        public ContentManager Content { get; init; }
        public GraphicsDevice GraphicsDevice => Game?.GraphicsDevice;
        public SpriteBatch SpriteBatch { get; set; }

        public Texture2D Pixel { get; set; } // A 1x1 Texture with Color #FFFFFFFF (White)

        public FontService FontService { get; set; }

        public UIRenderer Renderer { get; set; }

        public UITheme Theme { get; set; }

        public GameTime GameTime { get; set; }

    }
}
