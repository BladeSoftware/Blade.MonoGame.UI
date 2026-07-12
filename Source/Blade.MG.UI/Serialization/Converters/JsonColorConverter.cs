using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Serialization.Converters
{
    /// <summary>MonoGame's Color as a compact hex string ("#RRGGBBAA"), reusing
    /// Blade.MG.Core's existing ColorHelper.FromString/ToHexColor.</summary>
    public class JsonColorConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ColorHelper.FromString(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(ColorHelper.ToHexColor(value));
        }
    }
}
