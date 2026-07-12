using System.Text.Json;
using System.Text.Json.Serialization;
using Blade.MG.UI.Components;

namespace Blade.MG.UI.Serialization.Converters
{
    /// <summary>CornerRadius as its existing compact string form ("8,8,0,0" etc.), reusing
    /// CornerRadius.FromString/ToString.</summary>
    public class JsonCornerRadiusConverter : JsonConverter<CornerRadius>
    {
        public override CornerRadius Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return CornerRadius.FromString(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, CornerRadius value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"{value.TopLeft},{value.TopRight},{value.BottomRight},{value.BottomLeft}");
        }
    }
}
