using Blade.MG.UI.Components;
using Blade.MG.UI.Models;
using Blade.MG.UI.Theming;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI
{
    public abstract class UIComponentDrawable : UIComponentEvents
    {
        [JsonIgnore]
        [XmlIgnore]
        public string ResourceKey { get => resourceKey ?? this.GetType().Name; set => resourceKey = value; }
        private string resourceKey;

        [JsonIgnore]
        [XmlIgnore]
        public ResourceDict ResourceDict { get; set; } = new ResourceDict(); // Component Resource Dictionary

        [JsonIgnore]
        [XmlIgnore]
        public UITheme Theme => ParentWindow?.Context?.Theme ?? (this as UIWindow)?.Context?.Theme ?? UIManager.DefaultTheme;

        public Binding<Color> Background { get; set; } = Color.Transparent;

        [JsonIgnore]
        [XmlIgnore]
        public Texture2D BackgroundTexture { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public TextureLayout BackgroundLayout { get; set; }


        public T GetResourceValue<T>(string property)
        {
            string value = GetResourceValue(property);

            Type typeT = typeof(T);

            if (typeT == typeof(Color))
            {
                var color = ((UIColor)Activator.CreateInstance(typeof(UIColor), value)).ToColor();
                return (T)Activator.CreateInstance(typeof(Color), color.R, color.G, color.B, color.A);
            }

            if (typeT == typeof(UIColor) || typeT == typeof(Length) || typeT == typeof(Thickness))
            {
                return (T)Activator.CreateInstance(typeT, value);
            }

            try
            {
                return (T)Convert.ChangeType(value, typeT);
            }
            catch (Exception ex)
            {
                return default(T);
            }

            //if (typeT.IsEquivalentTo(typeof(string)))
            //{
            //    return (T)Convert.ChangeType("ABC", typeof(string));
            //}

            //if (typeT.IsEquivalentTo(typeof(float)))
            //{
            //    return (T)Convert.ChangeType("123.45", typeof(float));
            //}

            //return default(T);
        }

        public string GetResourceValue(string property)
        {
            // First try the local resource dictionary
            if (ResourceDict != null)
            {
                if (ResourceDict.TryGetValue(property, out var value))
                {
                    return value;
                }
            }

            // Then the global resource dictionary
            if (UIManager.ResourceDicts != null)
            {
                // First look for a resource dictionary specific to the Type (e.g. CheckBox)
                UIManager.ResourceDicts.TryGetValue(ResourceKey, out var resourceDict);
                if (resourceDict != null)
                {
                    if (resourceDict.TryGetValue(property, out var value))
                    {
                        return value;
                    }
                }

                // Then look for the default resource dictionary (type = blank)
                UIManager.ResourceDicts.TryGetValue("", out resourceDict);
                if (resourceDict != null)
                {
                    if (resourceDict.TryGetValue(property, out var value))
                    {
                        return value;
                    }
                }
            }

            return "";
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            base.RenderControl(context, layoutBounds, parentTransform);

            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            if (Background.Value != Color.Transparent)
            {
                try
                {
                    using var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
                    //context.Renderer.FillRect(spriteBatch, layoutBounds, Background.Value, layoutBounds);
                    context.Renderer.FillRect(spriteBatch, FinalRect, Background.Value, layoutBounds);
                }
                finally
                {
                    context.Renderer.EndBatch();
                }
            }

            if (BackgroundTexture != null)
            {
                //var layoutParams = BackgroundLayout.GetLayoutRect(BackgroundTexture, FinalContentRect);
                var layoutParams = BackgroundLayout.GetLayoutRect(BackgroundTexture, FinalRect);
                var scale = new Vector2(layoutParams.Scale.X / BackgroundLayout.TextureScale.X, layoutParams.Scale.Y / BackgroundLayout.TextureScale.Y);
                context.Renderer.DrawTexture(BackgroundTexture, layoutParams.LayoutRect, BackgroundLayout, scale, layoutBounds);
                //context.Renderer.DrawTexture(BackgroundTexture, layoutParams.LayoutRect, BackgroundLayout, scale, FinalContentRect);
            }
        }

        //// TODO: Move this to a helper class
        //public void GetImageStrech(Rectangle layoutBounds, TextureLayout textureLayout)
        //{
        //    Rectangle srcImageRect = textureLayout.Texture.Bounds;

        //    //float aspect = srcImageRect.Width / (float)srcImageRect.Height;
        //    float scaleX = srcImageRect.Width / (float)layoutBounds.Width; //(float)finalContentRect.Width;
        //    float scaleY = srcImageRect.Height / (float)layoutBounds.Height; //(float)finalContentRect.Height;

        //    Rectangle dstImageRect = textureLayout.Texture.Bounds;

        //    switch (textureLayout.StretchType)
        //    {
        //        case StretchType.None:
        //            dstImageRect = new Rectangle(layoutBounds.Left, layoutBounds.Top, BackgroundTexture.Texture.Width, BackgroundTexture.Texture.Height);
        //            break;

        //        case StretchType.Fill:
        //            dstImageRect = FinalRect; //finalContentRect;
        //            break;

        //        case StretchType.Uniform:
        //            float maxFactor = scaleX > scaleY ? scaleX : scaleY;

        //            if (maxFactor < 1f)
        //            {
        //                maxFactor = 1f / maxFactor;
        //            }

        //            dstImageRect = layoutBounds with { Width = (int)(BackgroundTexture.Texture.Width * maxFactor), Height = (int)(BackgroundTexture.Texture.Height * maxFactor) };


        //            //imageRect = new Rectangle(layoutBounds.Left, layoutBounds.Top, BackgroundTexture.Width, BackgroundTexture.Height); 
        //            break;

        //        case StretchType.UniformToFill:
        //            float minFactor = scaleX < scaleY ? scaleX : scaleY;

        //            if (minFactor < 1f)
        //            {
        //                minFactor = 1f / minFactor;
        //            }

        //            dstImageRect = layoutBounds with { Width = (int)(BackgroundTexture.Texture.Width * minFactor), Height = (int)(BackgroundTexture.Texture.Height * minFactor) };
        //            break;

        //    }
        //}


    }
}


