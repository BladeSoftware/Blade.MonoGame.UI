using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Animations
{
    /// <summary>
    /// Notify-and-forget property animation: call AnimateTo once with a Binding and a target
    /// value, and the manager takes care of the rest - ticked once per frame from
    /// UIManager.Update, it applies the interpolated value through the Binding's own setter and
    /// removes the request once it reaches its target, with no per-call-site bookkeeping fields
    /// required (contrast with using PropertyAnimation&lt;T&gt; directly, which requires the
    /// caller to hold its own field, track whether the target changed, and read .Value itself
    /// every frame).
    ///
    /// A Binding&lt;T&gt; instance serves as both the write target (via its own settable .Value)
    /// and the dictionary key identifying "this logical animation" - re-calling AnimateTo for a
    /// Binding that's already animating toward the same target is a no-op, so it's safe to call
    /// unconditionally every frame rather than only on a change edge.
    /// </summary>
    public static class PropertyAnimationManager
    {
        private interface IActiveAnimation
        {
            bool IsAnimating { get; }
            void Apply();
        }

        private sealed class ActiveAnimation<T> : IActiveAnimation
        {
            public readonly PropertyAnimation<T> Animation;
            private readonly Binding<T> binding;

            public ActiveAnimation(PropertyAnimation<T> animation, Binding<T> binding)
            {
                Animation = animation;
                this.binding = binding;
            }

            public bool IsAnimating => Animation.IsAnimating;
            public void Apply() => binding.Value = Animation.Value;
        }

        private static readonly Dictionary<object, IActiveAnimation> active = new();

        /// <summary>
        /// Idempotent - safe to call every frame with the same target. No-ops if already
        /// animating toward (or already at rest at) <paramref name="target"/>; otherwise starts
        /// easing from the binding's current live value (its Value if untracked, or the
        /// in-flight animation's own Value if already animating toward something else - so
        /// redirecting mid-flight never jumps).
        /// </summary>
        public static void AnimateTo<T>(Binding<T> binding, T target, TimeSpan duration,
            Func<T, T, float, T> interpolator, Func<float, float> easing = null)
        {
            if (active.TryGetValue(binding, out var existing) && existing is ActiveAnimation<T> typed)
            {
                if (EqualityComparer<T>.Default.Equals(typed.Animation.Target, target))
                {
                    return;
                }

                typed.Animation.AnimateTo(target, duration, easing);
                return;
            }

            if (EqualityComparer<T>.Default.Equals(binding.Value, target))
            {
                return;
            }

            var animation = new PropertyAnimation<T>(interpolator, binding.Value);
            animation.AnimateTo(target, duration, easing);

            active[binding] = new ActiveAnimation<T>(animation, binding);
        }

        // Convenience overloads for the two common cases - mirrors FloatAnimation/ColorAnimation.
        public static void AnimateTo(Binding<float> binding, float target, TimeSpan duration, Func<float, float> easing = null)
            => AnimateTo(binding, target, duration, MathHelper.Lerp, easing);

        public static void AnimateTo(Binding<Color> binding, Color target, TimeSpan duration, Func<float, float> easing = null)
            => AnimateTo(binding, target, duration, Color.Lerp, easing);

        public static void AnimateTo(Binding<Vector2> binding, Vector2 target, TimeSpan duration, Func<float, float> easing = null)
            => AnimateTo(binding, target, duration, Vector2.Lerp, easing);

        /// <summary>
        /// Explicit early-out, e.g. if a control is disposed mid-animation - not required for
        /// correctness (every entry self-removes once its duration elapses) but avoids holding a
        /// reference for the remainder of an animation nobody will read again.
        /// </summary>
        public static void Cancel(object key) => active.Remove(key);

        /// <summary>
        /// Called once per frame (see UIManager.Update) - applies every active animation's
        /// current value through its binding's setter, then prunes ones that finished.
        /// </summary>
        public static void Update()
        {
            if (active.Count == 0)
            {
                return;
            }

            List<object> finished = null;

            foreach (var kvp in active)
            {
                kvp.Value.Apply();

                if (!kvp.Value.IsAnimating)
                {
                    (finished ??= new List<object>()).Add(kvp.Key);
                }
            }

            if (finished != null)
            {
                foreach (var key in finished)
                {
                    active.Remove(key);
                }
            }
        }
    }
}
