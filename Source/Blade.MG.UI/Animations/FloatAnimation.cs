using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Animations
{
    /// <summary>PropertyAnimation&lt;float&gt; pre-wired with MathHelper.Lerp.</summary>
    public class FloatAnimation : PropertyAnimation<float>
    {
        public FloatAnimation(float initialValue) : base(MathHelper.Lerp, initialValue)
        {
        }
    }
}
