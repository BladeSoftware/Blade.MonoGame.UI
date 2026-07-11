using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Animations
{
    /// <summary>PropertyAnimation&lt;Color&gt; pre-wired with Color.Lerp.</summary>
    public class ColorAnimation : PropertyAnimation<Color>
    {
        public ColorAnimation(Color initialValue) : base(Color.Lerp, initialValue)
        {
        }
    }
}
