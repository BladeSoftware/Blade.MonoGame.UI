using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Models
{
    public class UIColor
    {
        private Color color;

        public UIColor()
        {

        }

        public UIColor(Color color)
        {
            this.color = color;
        }

        public UIColor(string color)
        {
            UIColor t = UIColor.FromString(color);
            this.color = t.color;
        }

        public Color ToColor()
        {
            return this.color;
        }

        public static UIColor FromString(string value)
        {
            UIColor uiColor = new UIColor();

            if (string.IsNullOrWhiteSpace(value))
            {
                // No color specified
                uiColor.color = Color.Transparent;
            }
            else if (value.StartsWith('#'))
            {
                // Hex Color (Web Format)
                uiColor.color = ColorHelper.FromHexColor(value);
            }
            else if (value.Contains('{'))
            {
                // {"R":0, "G":128, "B":0, "A":255} or {R:0 G:128 B:0 A:255}
                uiColor.color = ColorHelper.FromJsonColor(value);

            }
            else
            {
                // Color Key - Lookup color from Resource Dictionary
                // TODO:
            }

            return uiColor;
        }


        // Convert from a Hex Color to a UIColor
        public static implicit operator UIColor(string value)
        {
            return new UIColor(value);
        }

        // Convert from a Color to a UIColor
        public static implicit operator UIColor(Color color)
        {
            return new UIColor(color);
        }

        // Convert from Binding<Color> to a UIColor
        public static implicit operator UIColor(Binding<Color> color)
        {
            return new UIColor(color.Value);
        }

        // Convert from a UIColor to a Color
        public static implicit operator Color(UIColor color)
        {
            return color.ToColor();
        }

        public static implicit operator UIColor(Binding<UIColor> color)
        {
            return color.Value;
        }

        ///// <summary>
        ///// Convert from UIColor to Color
        ///// </summary>
        ///// <param name="value"></param>
        //public static explicit operator Color(UIColor value)
        //{
        //    return value.ToColor();
        //}

        ///// <summary>
        ///// Cast from Binding<UIColor> to Color
        ///// </summary>
        ///// <param name="value"></param>
        //public static explicit operator Color(UIColor value)
        //{
        //    return value.Value.ToColor();
        //}

    }
}
