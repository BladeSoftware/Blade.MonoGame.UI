using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI.Components
{
    public struct Thickness
    {
        [JsonIgnore][XmlIgnore] public static Thickness Zero = new Thickness(0);

        public int Left;
        public int Right;
        public int Top;
        public int Bottom;

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

        public static implicit operator Thickness(float value)
        {
            return new Thickness((int)value);
        }

        public static implicit operator Thickness(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new Thickness(0);
            }

            value = value.Trim();

            string[] parts = value.Split(",");

            if (parts == null || parts.Count() == 0)
            {
                return new Thickness(0);
            }

            if (parts.Count() == 1)
            {
                return new Thickness(int.Parse(parts[0]));
            }

            if (parts.Count() == 2)
            {
                return new Thickness(int.Parse(parts[0]), int.Parse(parts[1]));
            }

            if (parts.Count() == 4)
            {
                return new Thickness(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
            }

            throw new FormatException($"Invalid Thickness Format : {value}");
        }

        public override string ToString()
        {
            return $"Left:{Left} Top:{Top} Right:{Right} Bottom:{Bottom}";
        }
    }
}
