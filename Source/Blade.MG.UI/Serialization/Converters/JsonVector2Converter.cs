using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Serialization.Converters
{
    /// <summary>MonoGame's Vector2 as a compact "x,y" string - no existing string form exists
    /// for it anywhere in this codebase.</summary>
    public class JsonVector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString() ?? "0,0";
            string[] parts = value.Split(',', StringSplitOptions.TrimEntries);

            float x = parts.Length > 0 ? float.Parse(parts[0], CultureInfo.InvariantCulture) : 0f;
            float y = parts.Length > 1 ? float.Parse(parts[1], CultureInfo.InvariantCulture) : 0f;

            return new Vector2(x, y);
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"{value.X.ToString(CultureInfo.InvariantCulture)},{value.Y.ToString(CultureInfo.InvariantCulture)}");
        }
    }
}
