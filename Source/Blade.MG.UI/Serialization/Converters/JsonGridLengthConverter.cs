using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blade.MG.UI.Components;

namespace Blade.MG.UI.Serialization.Converters
{
    /// <summary>GridLength has no existing string form (unlike Thickness/CornerRadius/Length) -
    /// this adds the familiar WPF-style grammar: "Auto", "*", "2*", or a plain pixel number
    /// ("120").</summary>
    public class JsonGridLengthConverter : JsonConverter<GridLength>
    {
        public override GridLength Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString()?.Trim();

            if (string.IsNullOrEmpty(value) || value.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            {
                return new GridLength(GridUnitType.Auto);
            }

            if (value.EndsWith('*'))
            {
                string starAmount = value[..^1];
                float star = string.IsNullOrEmpty(starAmount) ? 1f : float.Parse(starAmount, CultureInfo.InvariantCulture);
                return new GridLength(GridUnitType.Star, star);
            }

            return new GridLength(GridUnitType.Pixel, float.Parse(value, CultureInfo.InvariantCulture));
        }

        public override void Write(Utf8JsonWriter writer, GridLength value, JsonSerializerOptions options)
        {
            string text = value.GridUnitType switch
            {
                GridUnitType.Auto => "Auto",
                GridUnitType.Star => $"{value.Value.ToString(CultureInfo.InvariantCulture)}*",
                _ => value.Value.ToString(CultureInfo.InvariantCulture),
            };

            writer.WriteStringValue(text);
        }
    }
}
