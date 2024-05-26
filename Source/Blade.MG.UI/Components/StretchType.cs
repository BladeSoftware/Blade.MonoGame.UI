using System.Text.Json.Serialization;

namespace Blade.MG.UI.Components
{
    [JsonConverter(typeof(JsonStringEnumConverter<StretchType>))]
    public enum StretchType
    {
        None,

        Fill,
        Fill_GrowOnly,
        Fill_ShrinkOnly,

        Uniform,
        Uniform_GrowOnly,
        Uniform_ShrinkOnly,

        UniformToFill,
        UniformToFill_GrowOnly,
        UniformToFill_ShrinkOnly,

        Tile,
        TileHorizontal,
        TileVertical,
    }

}
