using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

namespace Blade.MG.UI.Components
{
    public static class FloatHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNaN(float value) => float.IsNaN(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNaN(Length length) => float.IsNaN(length.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ValueOrZero(float value) => float.IsNaN(value) ? 0f : value;

    }

    public enum LengthUnit
    {
        Pixels,   // Value specified in Pixels

        Percent,  // Value specified as % of parent layout area

        //Auto,     // Size of Child elements

        VWidth,
        VHeight,
        VMin,
        VMax
    }

    public record struct Length
    {
        public float Value;
        public LengthUnit Unit;

        public Length()
        {
            Value = float.NaN;
            Unit = LengthUnit.Pixels;
        }

        public Length(float value, LengthUnit unit = LengthUnit.Pixels)
        {
            Value = value;
            Unit = unit;
        }

        public float ToPixelsWidth(UIComponent control, Rectangle parent)
        {
            float width = ToPixels(parent.Width);

            // For Absolute Lengths, return the values
            if (Unit == LengthUnit.Pixels)
            {
                return width;
            }

            // For Relative Lengths, take into account our margins
            width -= control.Margin.Value.Left + control.Margin.Value.Right;

            return width;
        }

        public float ToPixelsHeight(UIComponent control, Rectangle parent)
        {
            float height = ToPixels(parent.Height);

            // For Absolute Lengths, return the values
            if (Unit == LengthUnit.Pixels)
            {
                return height;
            }

            // For Relative Lengths, take into account our margins
            height -= control.Margin.Value.Top + control.Margin.Value.Bottom;

            return height;
        }

        public float ToPixels(float parentLength = 0f)
        {
            if (float.IsNaN(Value)) return float.NaN;

            if (Unit == LengthUnit.Pixels) return Value;

            if (Unit == LengthUnit.Percent) return Value / 100f * parentLength;
            if (Unit == LengthUnit.Percent) return Value / 100f * parentLength;

            //if (Unit == LengthUnit.VWidth) return (Value / 100f) * UIManager.SafeLayoutRect.Width;
            //if (Unit == LengthUnit.VHeight) return (Value / 100f) * UIManager.SafeLayoutRect.Height;
            //if (Unit == LengthUnit.VMin) return (Value / 100f) * Math.Min(UIManager.SafeLayoutRect.Width, UIManager.SafeLayoutRect.Height);
            //if (Unit == LengthUnit.VMax) return (Value / 100f) * Math.Max(UIManager.SafeLayoutRect.Width, UIManager.SafeLayoutRect.Height);

            return Value;
        }

        public static implicit operator Length(float value)
        {
            return new Length(value, LengthUnit.Pixels);
        }

        public static implicit operator Length(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new Length(float.NaN, LengthUnit.Pixels);
            }

            value = value.Trim();

            if (value.EndsWith("px", StringComparison.InvariantCultureIgnoreCase))
            {
                value = value.Substring(0, value.Length - 2);
                if (!float.TryParse(value, out float f))
                {
                    f = 0f;
                }

                return new Length(f, LengthUnit.Pixels);
            }

            if (value.EndsWith("%", StringComparison.InvariantCultureIgnoreCase))
            {
                value = value.Substring(0, value.Length - 1);
                if (!float.TryParse(value, out float f))
                {
                    f = 0f;
                }

                return new Length(f, LengthUnit.Percent);
            }

            // Default to Pixels
            if (float.TryParse(value, out float f2))
            {
                return new Length(f2, LengthUnit.Pixels);
            }


            throw new FormatException($"Invalid Length Format : {value}");
        }

        //public static explicit operator T(Binding<T> value)
        //{
        //    return value.Value;
        //}

        public override string ToString()
        {
            return $"{Value} {Enum.GetName(Unit)}";
        }
    }
}
