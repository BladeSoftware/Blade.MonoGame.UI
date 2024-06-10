using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI.Components
{
    public struct Thickness
    {
        [JsonIgnore][XmlIgnore] public static Thickness Zero = new Thickness(0);

        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }

        [JsonIgnore][XmlIgnore] public int Horizontal => Left + Right;
        [JsonIgnore][XmlIgnore] public int Vertical => Top + Bottom;

        public Thickness(int uniformLength)
        {
            Left = Right = Top = Bottom = uniformLength;
        }

        public Thickness(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public Thickness(int leftRight, int topBottom)
        {
            Left = leftRight;
            Top = topBottom;
            Right = leftRight;
            Bottom = topBottom;
        }

        public Thickness(string value)
        {
            var t = Thickness.FromString(value);

            this.Left = t.Left;
            this.Top = t.Top;
            this.Right = t.Right;
            this.Bottom = t.Bottom;
        }

        public static Thickness FromString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new Thickness(0);
            }

            var spanValue = value.AsSpan().Trim();

            // set up a span to hold the ranges from the Split() call
            Span<Range> parts = stackalloc Range[5];
            int numParts = 0;

            if (spanValue.Contains(','))
            {
                numParts = spanValue.Split(parts, ',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                numParts = spanValue.Split(parts, ' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            }

            if (numParts == 0) return new Thickness(0);
            if (numParts == 1) return new Thickness(int.Parse(spanValue[parts[0]]));
            if (numParts == 2) return new Thickness(int.Parse(spanValue[parts[0]]), int.Parse(spanValue[parts[1]]));

            if (numParts == 4)
            {
                // Can be either short or long format:
                // 2 4 6 8
                // Left:2 Top:4 Right:6 Bottom:8
                var part0 = spanValue[parts[0]];
                var part1 = spanValue[parts[1]];
                var part2 = spanValue[parts[2]];
                var part3 = spanValue[parts[3]];

                if (part0.Contains(':'))
                {
                    part0 = part0.Slice(part0.IndexOf(':') + 1);
                    part1 = part1.Slice(part1.IndexOf(':') + 1);
                    part2 = part2.Slice(part2.IndexOf(':') + 1);
                    part3 = part3.Slice(part3.IndexOf(':') + 1);
                }

                return new Thickness(int.Parse(part0), int.Parse(part1), int.Parse(part2), int.Parse(part3));
            }

            throw new FormatException($"Invalid Thickness Format : {value}");
        }

        public static implicit operator Thickness(float value)
        {
            return new Thickness((int)value);
        }

        public static implicit operator Thickness(string value)
        {
            return Thickness.FromString(value);
        }

        public override string ToString()
        {
            return $"Left:{Left} Top:{Top} Right:{Right} Bottom:{Bottom}";
        }
    }
}
