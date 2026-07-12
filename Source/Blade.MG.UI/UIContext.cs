using Blade.MG.UI.Renderer;
using Blade.MG.UI.Theming;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;

namespace Blade.MG.UI
{
    public class UIContext
    {
        [JsonIgnore]
        public Game Game { get; set; }
        //public ContentManager Content { get; init; }

        [JsonIgnore]
        public GraphicsDevice GraphicsDevice => Game?.GraphicsDevice;
        //public SpriteBatch SpriteBatch { get; set; }

        [JsonIgnore]
        public Texture2D Pixel { get; set; } // A 1x1 Texture with Color #FFFFFFFF (White)

        [JsonIgnore]
        public UIRenderer Renderer { get; set; }

        [JsonIgnore]
        public UITheme Theme { get; set; }

        [JsonIgnore]
        public GameTime GameTime { get; set; }

        // The true viewport clip chain (window bounds, plus any ScrollPanel/StackPanel/
        // ListView/TreeView/Panel/ExpansionPanel a control is nested inside), maintained
        // separately from the layoutBounds parameter threaded through RenderControl. Ordinary
        // parent-to-child "don't draw outside your own box" narrowing (Control.RenderControl's
        // Rectangle.Intersect(layoutBounds, FinalContentRect) for Content/Children) always
        // collapses layoutBounds down to exactly match a leaf control's own FinalRect, so by
        // the time a Border reaches RenderShadow, layoutBounds can no longer tell "this edge is
        // my own box" apart from "this edge is a real ancestor's boundary". AncestorClipBounds
        // only changes at genuine viewport containers (see their RenderControl overrides), so
        // it stays the correct escape target for a drop shadow: bigger than a free-floating
        // control's own box (so its shadow isn't clipped away), but still exactly the
        // scroll/stack panel's true edge when a control is flush against it.
        [JsonIgnore]
        public Rectangle AncestorClipBounds { get; set; }


        public T LoadContent<T>(string assetName)
        {
            try
            {
                return Game.Content.Load<T>(assetName);
            }
            catch (ContentLoadException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public T LoadLocalizedContent<T>(string assetName)
        {
            try
            {
                return Game.Content.LoadLocalized<T>(assetName);
            }
            catch (ContentLoadException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
