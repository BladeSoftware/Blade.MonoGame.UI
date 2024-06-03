using Microsoft.Xna.Framework;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    [JsonConverter(typeof(JsonStringEnumConverter<LengthUnit>))]
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

    [JsonConverter(typeof(JsonLengthConverter))]
    public record struct Length
    {
        public float Value { get; set; }
        public LengthUnit Unit { get; set; }

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

        public Length(string value)
        {
            Length l = Length.FromString(value);

            Value = l.Value;
            Unit = l.Unit;
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

        public static Length FromString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new Length(float.NaN, LengthUnit.Pixels);
            }

            var spanValue = value.AsSpan().Trim();

            var unitIndex = spanValue.IndexOfAnyExcept("0123456789.".AsSpan());

            var valueSpan = spanValue.Slice(0, unitIndex).Trim();
            var unitSpan = spanValue.Slice(unitIndex).Trim();

            LengthUnit lengthUnit = unitSpan switch
            {
                var p when p.Equals("px", StringComparison.OrdinalIgnoreCase) => LengthUnit.Pixels,
                var p when p.Equals("%", StringComparison.OrdinalIgnoreCase) => LengthUnit.Percent,
                var p when p.Equals("pixels", StringComparison.OrdinalIgnoreCase) => LengthUnit.Pixels,
                var p when p.Equals("percent", StringComparison.OrdinalIgnoreCase) => LengthUnit.Percent,
                var p when p.Equals("vmin", StringComparison.OrdinalIgnoreCase) => LengthUnit.VMin,
                var p when p.Equals("vmax", StringComparison.OrdinalIgnoreCase) => LengthUnit.VMax,
                var p when p.Equals("vw", StringComparison.OrdinalIgnoreCase) => LengthUnit.VWidth,
                var p when p.Equals("vwidth", StringComparison.OrdinalIgnoreCase) => LengthUnit.VWidth,
                var p when p.Equals("vh", StringComparison.OrdinalIgnoreCase) => LengthUnit.VHeight,
                var p when p.Equals("vheight", StringComparison.OrdinalIgnoreCase) => LengthUnit.VHeight,

                _ => throw new FormatException($"Invalid Length Format : {value}")
            };

            if (!float.TryParse(valueSpan, out float f))
            {
                f = 0f;
            }

            return new Length(f, lengthUnit);
        }

        public static implicit operator Length(float value)
        {
            return new Length(value, LengthUnit.Pixels);
        }

        public static implicit operator Length(string value)
        {
            return Length.FromString(value);

            //if (string.IsNullOrWhiteSpace(value))
            //{
            //    return new Length(float.NaN, LengthUnit.Pixels);
            //}

            //value = value.Trim();

            //if (value.EndsWith("px", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    value = value.Substring(0, value.Length - 2);
            //    if (!float.TryParse(value, out float f))
            //    {
            //        f = 0f;
            //    }

            //    return new Length(f, LengthUnit.Pixels);
            //}

            //if (value.EndsWith("%", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    value = value.Substring(0, value.Length - 1);
            //    if (!float.TryParse(value, out float f))
            //    {
            //        f = 0f;
            //    }

            //    return new Length(f, LengthUnit.Percent);
            //}

            //// Default to Pixels
            //if (float.TryParse(value, out float f2))
            //{
            //    return new Length(f2, LengthUnit.Pixels);
            //}


            //throw new FormatException($"Invalid Length Format : {value}");
        }

        //public static explicit operator T(Binding<T> value)
        //{
        //    return value.Value;
        //}

        public override string ToString()
        {
            string unitCode = Unit switch
            {
                LengthUnit.Pixels => "px",
                LengthUnit.Percent => "%",
                LengthUnit.VMin => "vmin",
                LengthUnit.VMax => "vmax",
                LengthUnit.VWidth => "vw",
                LengthUnit.VHeight => "vh",

                _ => Enum.GetName(Unit),
            };


            return $"{Value} {Enum.GetName(Unit)}";
        }
    }

    internal class JsonLengthConverter : JsonConverter<Length>
    {
        public override Length Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Length value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"{value.Value} {value.Unit}");
        }
    }


}
