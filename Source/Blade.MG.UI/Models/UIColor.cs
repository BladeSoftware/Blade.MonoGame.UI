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
            if (string.IsNullOrWhiteSpace(color))
            {
                // No color specified
                this.color = Color.Transparent;
            }
            else if (color.StartsWith("#"))
            {
                // Hex Color (Web Format)
                this.color = ColorHelper.FromHexColor(color);
            }
            else if (color.StartsWith("{"))
            {
                // {R:0 G:128 B:0 A:255}
                color = color[1..^1];
                string[] parts = color.Split(' ');

                int r = 255;
                int g = 255;
                int b = 255;
                int a = 255;

                if (parts.Length >= 1) r = int.Parse(parts[0][2..]);
                if (parts.Length >= 2) g = int.Parse(parts[1][2..]);
                if (parts.Length >= 3) b = int.Parse(parts[2][2..]);
                if (parts.Length >= 4) a = int.Parse(parts[3][2..]);

                this.color = new Color(r, g, b, a);

            }
            else
            {
                // Color Key - Lookup color from Resource Dictionary

            }
        }

        public Color ToColor()
        {
            return this.color;
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
