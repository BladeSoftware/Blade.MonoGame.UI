using System;
using Blade.MG.UI;
using Blade.MG.UI.Animations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace BladeUI.UnitTesting.Tests.Core
{
    /// <summary>
    /// PropertyAnimationManager used to key its `active` dictionary on the Binding&lt;T&gt;
    /// instance itself, relying on Dictionary's default comparer - but Binding&lt;T&gt; overrides
    /// Equals/GetHashCode to compare/hash by its wrapped VALUE (see Binding.cs), which is exactly
    /// what ActiveAnimation&lt;T&gt;.Apply() mutates every tick. Once an entry's key had changed
    /// value since insertion, Update()'s `active.Remove(key)` recomputed a different hash than the
    /// one used at insertion, missed the bucket the entry actually lived in, and silently failed
    /// to remove it - meaning literally every animation that ever finished leaked one permanent
    /// entry, walked by Update()'s per-frame foreach forever after. This is the root cause behind
    /// a real reported bug: repeatedly hovering back and forth over a row of buttons (each hover
    /// triggering an animated color transition) caused framerate to degrade catastrophically and
    /// monotonically the more times it was repeated (80fps -> 60fps -> 10fps -> ~1fps).
    /// </summary>
    [TestClass]
    public class TestPropertyAnimationManagerLeak
    {
        [TestMethod]
        public void CompletedAnimation_IsActuallyRemoved_NotLeakedForever()
        {
            var binding = new Binding<Color>(Color.White);

            int countBefore = PropertyAnimationManager.ActiveCount;

            // duration = TimeSpan.Zero makes the animation immediately "at rest" (IsAnimating
            // false) - Update()'s very first tick both applies the target value (mutating the
            // binding, and therefore its hash) and prunes it in the same call, deterministically
            // reproducing the insert-hash-vs-remove-hash mismatch without needing to wait out a
            // real transition duration.
            PropertyAnimationManager.AnimateTo(binding, Color.Red, TimeSpan.Zero, Color.Lerp);
            PropertyAnimationManager.Update();

            Assert.AreEqual(Color.Red, binding.Value, "Test setup error - expected the animation to have already applied its target value.");

            // Compare against the before-count (not an absolute 0) since `active` is a
            // process-wide static dictionary that other tests/controls may also be using.
            Assert.AreEqual(countBefore, PropertyAnimationManager.ActiveCount,
                "Expected the finished animation's entry to be actually removed, not permanently stuck in the dictionary.");
        }

        [TestMethod]
        public void RepeatedAnimationCycles_OnTheSameBinding_DoNotAccumulateEntries()
        {
            var binding = new Binding<Color>(Color.White);
            int countBefore = PropertyAnimationManager.ActiveCount;

            // Mirrors repeatedly hovering on/off a button - each cycle starts and immediately
            // completes an animation to a different target (matching hover-in vs. hover-out
            // colors), the exact pattern that used to mint one permanently-leaked entry per cycle.
            for (int i = 0; i < 10; i++)
            {
                Color target = (i % 2 == 0) ? Color.Red : Color.White;
                PropertyAnimationManager.AnimateTo(binding, target, TimeSpan.Zero, Color.Lerp);
                PropertyAnimationManager.Update();
            }

            Assert.AreEqual(countBefore, PropertyAnimationManager.ActiveCount,
                "Expected repeated complete animation cycles on the same binding to leave no residual entries.");
        }
    }
}
