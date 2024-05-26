using System.Text.Json.Serialization;

namespace Blade.MG.UI.Components
{

    [JsonConverter(typeof(JsonStringEnumConverter<StretchDirection>))]
    public enum StretchDirection
    {
        /// <summary>Scale Both Horizontally and Vertically, Increase in Size or Decrease in Size</summary>
        Both,

        /// <summary>Scale Horizontally, Increase in Size or Decrease in Size</summary>
        Horizontal,

        /// <summary>Scale Vertically, Increase in Size or Decrease in Size</summary>
        Vertical,

        /// <summary>Scale Horizontally, Only allow Increase in Size</summary>
        HorizontalUpOnly,

        /// <summary>Scale Vertically, Only allow Increase in Size</summary>
        VerticalUpOnly,

        /// <summary>Scale Horizontally, Only allow Decrease in Size</summary>
        HorizontalDownOnly,

        /// <summary>Scale Vertically, Only allow Decrease in Size</summary>
        VerticalDownOnly
    }
}
