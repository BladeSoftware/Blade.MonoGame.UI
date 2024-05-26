using System.Text.Json.Serialization;

namespace Blade.MG.UI.Components
{
    [JsonConverter(typeof(JsonStringEnumConverter<HorizontalAlignmentType>))]
    public enum HorizontalAlignmentType
    {
        Left,
        Center,
        Right,
        Stretch,
        Absolute
    }
}
