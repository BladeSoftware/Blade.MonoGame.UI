using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using Blade.MG.UI.Components;

namespace Blade.MG.UI.Serialization.Converters
{
    // Transform's Vector3 fields have different real defaults (Scale = Vector3.One, everything
    // else = Vector3.Zero - see Transform.cs). Legacy documents saved before JsonVector3Converter
    // existed wrote every Vector3 out as an empty JSON object ("{}"), regardless of its actual
    // value (Vector3's X/Y/Z are public fields, not properties, so the default reflection-based
    // serializer never saw them). Forcing every such legacy field to Vector3.Zero - as
    // JsonVector3Converter's StartObject fallback does, since it has no way to know which field
    // it's populating - is correct for Translation/Rotation/CenterPoint but wrong for Scale: it
    // silently zeroes out scale, collapsing the control's EffectiveTransform to a degenerate
    // matrix and making it (and its whole subtree) invisible despite otherwise-correct layout.
    //
    // This converter starts from Transform's own parameterless-constructor defaults and only
    // overwrites a field when the JSON actually supplies real (non-legacy) data, leaving legacy
    // empty-object fields untouched so each field keeps its own correct default.
    public class JsonTransformConverter : JsonConverter<Transform>
    {
        public override Transform Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Transform transform = new Transform();

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                reader.Skip();
                return transform;
            }

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
                    case nameof(Transform.CenterPoint):
                        if (TryReadVector3(ref reader, options, out Vector3 centerPoint))
                        {
                            transform.CenterPoint = centerPoint;
                        }
                        break;
                    case nameof(Transform.CenterPointRelative):
                        transform.CenterPointRelative = JsonSerializer.Deserialize<RelativeTo>(ref reader, options);
                        break;
                    case nameof(Transform.Translation):
                        if (TryReadVector3(ref reader, options, out Vector3 translation))
                        {
                            transform.Translation = translation;
                        }
                        break;
                    case nameof(Transform.Rotation):
                        if (TryReadVector3(ref reader, options, out Vector3 rotation))
                        {
                            transform.Rotation = rotation;
                        }
                        break;
                    case nameof(Transform.Scale):
                        if (TryReadVector3(ref reader, options, out Vector3 scale))
                        {
                            transform.Scale = scale;
                        }
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return transform;
        }

        // Returns false (keep the struct's own default) for a legacy empty object ("{}"),
        // since that shape carries no real, recoverable value.
        private static bool TryReadVector3(ref Utf8JsonReader reader, JsonSerializerOptions options, out Vector3 result)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                reader.Skip();
                result = default;
                return false;
            }

            result = JsonSerializer.Deserialize<Vector3>(ref reader, options);
            return true;
        }

        public override void Write(Utf8JsonWriter writer, Transform value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(Transform.CenterPoint));
            JsonSerializer.Serialize(writer, value.CenterPoint, options);

            writer.WriteNumber(nameof(Transform.CenterPointRelative), (int)value.CenterPointRelative);

            writer.WritePropertyName(nameof(Transform.Translation));
            JsonSerializer.Serialize(writer, value.Translation, options);

            writer.WritePropertyName(nameof(Transform.Rotation));
            JsonSerializer.Serialize(writer, value.Rotation, options);

            writer.WritePropertyName(nameof(Transform.Scale));
            JsonSerializer.Serialize(writer, value.Scale, options);

            writer.WriteEndObject();
        }
    }
}
