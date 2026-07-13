using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Serialization.Converters
{
    // Vector3's X/Y/Z are public fields (not properties), so without this converter
    // System.Text.Json's default reflection-based serialization - which only considers public
    // properties - writes every Vector3 out as an empty JSON object ("{}"), regardless of its
    // actual value. Deserializing that empty object back then constructs a brand new
    // default(Vector3) (0,0,0), silently discarding whatever value was there before - most
    // notably Transform.Scale, whose real default is Vector3.One (1,1,1): after a JSON
    // round-trip it silently becomes Vector3.Zero, collapsing EffectiveTransform to an all-zero
    // matrix and making the control (and its whole subtree) invisible despite otherwise-correct
    // layout/FinalRect. Mirrors JsonVector2Converter's "x,y" string format, extended to "x,y,z".
    public class JsonVector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Legacy documents (saved before this converter existed) may still have Vector3
            // properties written as a JSON object by the default reflection-based serializer
            // (typically "{}", since X/Y/Z are fields, not properties). Tolerate that shape here
            // rather than throwing, falling back to 0 for any component that isn't present.
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                float ox = 0f, oy = 0f, oz = 0f;

                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        continue;
                    }

                    string propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName)
                    {
                        case "X":
                            ox = reader.GetSingle();
                            break;
                        case "Y":
                            oy = reader.GetSingle();
                            break;
                        case "Z":
                            oz = reader.GetSingle();
                            break;
                    }
                }

                return new Vector3(ox, oy, oz);
            }

            string value = reader.GetString() ?? "0,0,0";
            string[] parts = value.Split(',', StringSplitOptions.TrimEntries);

            float x = parts.Length > 0 ? float.Parse(parts[0], CultureInfo.InvariantCulture) : 0f;
            float y = parts.Length > 1 ? float.Parse(parts[1], CultureInfo.InvariantCulture) : 0f;
            float z = parts.Length > 2 ? float.Parse(parts[2], CultureInfo.InvariantCulture) : 0f;

            return new Vector3(x, y, z);
        }

        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"{value.X.ToString(CultureInfo.InvariantCulture)},{value.Y.ToString(CultureInfo.InvariantCulture)},{value.Z.ToString(CultureInfo.InvariantCulture)}");
        }
    }
}
