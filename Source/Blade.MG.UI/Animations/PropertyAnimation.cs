namespace Blade.MG.UI.Animations
{
    /// <summary>
    /// Eases an arbitrary value of type <typeparamref name="T"/> from its current value to a
    /// new target over a duration, queried on demand via wall-clock time (no GameTime tick
    /// required) - matching the convention already used for time-based per-control behavior
    /// throughout Blade.MG.UI (e.g. TextBoxTemplate's cursor-blink timer), so it can be driven
    /// from anywhere that already reads DateTime.Now per frame, such as a control's own
    /// RenderControl.
    ///
    /// Generic with no constraint on <typeparamref name="T"/>: interpolation is supplied as a
    /// delegate rather than requiring <typeparamref name="T"/> to implement some lerp interface,
    /// so it works with types that can't be retrofitted with one (MonoGame's own Color, Vector2,
    /// etc). See FloatAnimation/ColorAnimation for ready-made common cases; add more of these by
    /// subclassing with a one-line constructor, e.g. Vector2Animation via Vector2.Lerp. Rectangle
    /// has no built-in Lerp and would need a small custom interpolator (lerping X/Y/Width/Height
    /// independently) if ever needed.
    ///
    /// For the common "just animate a Binding-backed property and forget about it" case, see
    /// PropertyAnimationManager instead of using this class directly.
    /// </summary>
    public class PropertyAnimation<T>
    {
        private readonly Func<T, T, float, T> interpolator;
        private T from;
        private T to;
        private DateTime startTime;
        private TimeSpan duration;
        private Func<float, float> easing;

        public PropertyAnimation(Func<T, T, float, T> interpolator, T initialValue)
        {
            this.interpolator = interpolator;
            SnapTo(initialValue);
        }

        /// <summary>Elapsed fraction of the current animation, clamped to [0, 1]. 1 when at rest.</summary>
        public float Progress =>
            duration <= TimeSpan.Zero
                ? 1f
                : Math.Clamp((float)((DateTime.Now - startTime).TotalMilliseconds / duration.TotalMilliseconds), 0f, 1f);

        public bool IsAnimating => Progress < 1f;

        /// <summary>The current interpolated value. Returns the target directly once at rest
        /// (skipping the interpolator entirely) so there's no float round-trip drift.</summary>
        public T Value => IsAnimating ? interpolator(from, to, easing(Progress)) : to;

        /// <summary>The value this animation is currently easing toward (or already at rest at).</summary>
        public T Target => to;

        /// <summary>Jumps straight to <paramref name="value"/> with no animation - e.g. for
        /// establishing the correct initial state on first render, with no unwanted animate-in.</summary>
        public void SnapTo(T value)
        {
            from = value;
            to = value;
            startTime = DateTime.Now;
            duration = TimeSpan.Zero;
            easing = Easing.Linear;
        }

        /// <summary>Starts easing from the current (possibly still mid-flight) value to
        /// <paramref name="target"/> over <paramref name="duration"/>. Re-triggering while
        /// already animating never jumps, since the new "from" is a snapshot of Value, not
        /// whatever "to" was previously targeting.</summary>
        public void AnimateTo(T target, TimeSpan duration, Func<float, float> easing = null)
        {
            from = Value;
            to = target;
            startTime = DateTime.Now;
            this.duration = duration;
            this.easing = easing ?? Easing.Linear;
        }
    }
}
