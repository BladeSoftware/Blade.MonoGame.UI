using System.Text.Json.Serialization;

namespace Blade.MG.UI.Components
{
    [JsonConverter(typeof(JsonStringEnumConverter<Visibility>))]
    public enum Visibility
    {
        Visible,
        Collapsed,
        Hidden
    }
}
