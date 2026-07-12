using System.Text.Json;
using System.Text.Json.Serialization;
using Blade.MG.UI.Components;

namespace Blade.MG.UI.Serialization.Converters
{
    /// <summary>Thickness as its existing compact string form ("10,20,10,20" etc.),
    /// reusing Thickness.FromString/ToString rather than its default {Left,Top,Right,Bottom}
    /// object shape.</summary>
    public class JsonThicknessConverter : JsonConverter<Thickness>
    {
        public override Thickness Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Thickness.FromString(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Thickness value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"{value.Left},{value.Top},{value.Right},{value.Bottom}");
        }
    }
}
