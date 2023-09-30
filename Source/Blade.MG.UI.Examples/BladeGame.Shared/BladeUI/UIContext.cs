using System;
using System.Collections.Generic;
using System.Text;
using BladeGame.BladeUI.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BladeGame.BladeUI
{
    public class UIContext
    {
        public Game Game { get; set; }
        public ContentManager Content { get; set; }
        public SpriteBatch SpriteBatch { get; set; }

        public Texture2D Pixel { get; set; } // A 1x1 Texture with Color #FFFFFFFF (White)
        public SpriteFont DefaultFont { get; set; }

        public UIRenderer Renderer { get; set; }

    }
}
