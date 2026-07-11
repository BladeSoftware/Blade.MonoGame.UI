namespace Blade.MG.UI.Animations
{
    /// <summary>
    /// Named easing curves for use with PropertyAnimation&lt;T&gt;.AnimateTo - each maps a linear
    /// 0..1 progress fraction to an eased 0..1 fraction. Fields (not methods), so they can be
    /// passed and defaulted directly, e.g. `easing ?? Easing.Linear`.
    /// </summary>
    public static class Easing
    {
        public static readonly Func<float, float> Linear = t => t;

        public static readonly Func<float, float> EaseInQuad = t => t * t;
        public static readonly Func<float, float> EaseOutQuad = t => 1f - (1f - t) * (1f - t);
        public static readonly Func<float, float> EaseInOutQuad =
            t => t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f;

        public static readonly Func<float, float> EaseOutCubic = t => 1f - MathF.Pow(1f - t, 3f);
        public static readonly Func<float, float> EaseInOutCubic =
            t => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f;
    }
}
