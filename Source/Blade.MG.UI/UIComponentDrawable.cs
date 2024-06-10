using Blade.MG.UI.Components;
using Blade.MG.UI.Models;
using Blade.MG.UI.Theming;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
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
        public UITheme Theme => ParentWindow?.Context?.Theme ?? (this as UIWindow)?.Context?.Theme ?? UIManager.DefaultTheme;

        public Binding<Color> Background { get; set; } = Color.Transparent;

        [JsonIgnore]
        [XmlIgnore]
        public Texture2D BackgroundTexture { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public TextureLayout BackgroundLayout { get; set; }

        //public T GetResourceValue<T>(string property)
        //{
        //    return GetResourceValue<T>(ResourceKey, property);
        //}

        //public T GetResourceValue<T>(string resource, string property)
        //{
        //    string value = GetResourceValue(resource, property);

        //    Type typeT = typeof(T);

        //    if (typeT == typeof(Color))
        //    {
        //        var color = ((UIColor)Activator.CreateInstance(typeof(UIColor), value)).ToColor();
        //        return (T)Activator.CreateInstance(typeof(Color), color.R, color.G, color.B, color.A);
        //    }

        //    if (typeT == typeof(UIColor) || typeT == typeof(Length) || typeT == typeof(Thickness))
        //    {
        //        return (T)Activator.CreateInstance(typeT, value);
        //    }

        //    try
        //    {
        //        return (T)Convert.ChangeType(value, typeT);
        //    }
        //    catch (Exception ex)
        //    {
        //        return default(T);
        //    }

        //    //if (typeT.IsEquivalentTo(typeof(string)))
        //    //{
        //    //    return (T)Convert.ChangeType("ABC", typeof(string));
        //    //}

        //    //if (typeT.IsEquivalentTo(typeof(float)))
        //    //{
        //    //    return (T)Convert.ChangeType("123.45", typeof(float));
        //    //}

        //    //return default(T);
        //}


        //public string GetResourceValue(string property)
        //{
        //    return GetResourceValue(ResourceKey, property);
        //}

        //public string GetResourceValue(string resource, string property)
        //{
        //    //Debug.WriteLine($"GetResourceValue : {resource}, {property}");

        //    // TODO: Decide on the order of property inheritance
        //    string value;

        //    // First try the local window resource dictionary
        //    var windowResourceDict = ParentWindow?.ResourceDict;
        //    if (windowResourceDict != null)
        //    {
        //        if (windowResourceDict.TryGetValue(resource, property, out value))
        //        {
        //            return value;
        //        }

        //        if (windowResourceDict.TryGetValue(property, out value))
        //        {
        //            return value;
        //        }
        //    }

        //    // Then the global resource dictionary
        //    if (UIManager.ResourceDict != null)
        //    {
        //        if (UIManager.ResourceDict.TryGetValue(resource, property, out value))
        //        {
        //            return value;
        //        }

        //        if (UIManager.ResourceDict.TryGetValue(property, out value))
        //        {
        //            return value;
        //        }
        //    }

        //    return "";
        //}

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


