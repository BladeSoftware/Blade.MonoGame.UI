using Microsoft.Xna.Framework;

namespace Blade.UI.Models
{
    public class DialogButton
    {
        public string Id { get; set; }
        public string Text { get; set; }

        public int? Width { get; set; } = 200;

        // TODO: Allow taking a button Template ?
        public Color? Color { get; set; } = null;
    }
}
