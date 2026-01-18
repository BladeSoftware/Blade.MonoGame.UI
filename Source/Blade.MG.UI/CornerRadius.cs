using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI.Components
{
    /// <summary>
    /// Represents the radii of a rectangle's corners.
    /// </summary>
    public struct CornerRadius
    {
        [JsonIgnore][XmlIgnore] public static CornerRadius Zero = new(0f);

        public float TopLeft { get; set; }
        public float TopRight { get; set; }
        public float BottomLeft { get; set; }
        public float BottomRight { get; set; }

        [JsonIgnore][XmlIgnore] public bool HasRadius => TopLeft > 0 || TopRight > 0 || BottomLeft > 0 || BottomRight > 0;
        [JsonIgnore][XmlIgnore] public bool IsUniform => TopLeft == TopRight && TopRight == BottomRight && BottomRight == BottomLeft;
        [JsonIgnore][XmlIgnore] public float MaxRadius => Math.Max(Math.Max(TopLeft, TopRight), Math.Max(BottomLeft, BottomRight));

        public CornerRadius(float uniformRadius)
        {
            TopLeft = TopRight = BottomLeft = BottomRight = uniformRadius;
        }

        public CornerRadius(float topLeft, float topRight, float bottomRight, float bottomLeft)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
        }

        public CornerRadius(string value)
        {
            var cr = FromString(value);
            TopLeft = cr.TopLeft;
            TopRight = cr.TopRight;
            BottomRight = cr.BottomRight;
            BottomLeft = cr.BottomLeft;
        }

        public static CornerRadius FromString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new CornerRadius(0f);

            var spanValue = value.AsSpan().Trim();
            Span<Range> parts = stackalloc Range[5];
            int numParts;

            if (spanValue.Contains(','))
                numParts = spanValue.Split(parts, ',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            else
                numParts = spanValue.Split(parts, ' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            return numParts switch
            {
                0 => new CornerRadius(0f),
                1 => new CornerRadius(float.Parse(spanValue[parts[0]])),
                4 => new CornerRadius(
                    float.Parse(spanValue[parts[0]]),
                    float.Parse(spanValue[parts[1]]),
                    float.Parse(spanValue[parts[2]]),
                    float.Parse(spanValue[parts[3]])),
                _ => throw new FormatException($"Invalid CornerRadius Format: {value}")
            };
        }

        public static implicit operator CornerRadius(float value) => new(value);
        public static implicit operator CornerRadius(string value) => FromString(value);

        public override readonly string ToString() =>
            $"TopLeft:{TopLeft} TopRight:{TopRight} BottomRight:{BottomRight} BottomLeft:{BottomLeft}";
    }
}