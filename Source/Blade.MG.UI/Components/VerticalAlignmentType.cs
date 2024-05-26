using System.Text.Json.Serialization;

namespace Blade.MG.UI.Components
{
    [JsonConverter(typeof(JsonStringEnumConverter<VerticalAlignmentType>))]
    public enum VerticalAlignmentType
    {
        Top,
        Center,
        Bottom,
        Stretch,
        Absolute
    }
}
