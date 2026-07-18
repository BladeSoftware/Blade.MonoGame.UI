using Blade.MG.UI;
using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace BladeUI.UnitTesting.Tests.Core
{
    /// <summary>
    /// Reproduces a reported "Collection was modified after the enumerator was instantiated"
    /// crash from showing a Popup. uiWindows (UIManager.cs) is a SortedList&lt;string, UIWindow&gt;
    /// - UIManager.Update/Draw iterate it in several places (the eventLockedWindow check, the
    /// PerformLayout pass, PreRenderLayout, and RenderLayout). Add(UIWindow, ...) used to be the
    /// only mutation method (unlike Remove/RemoveAll/RemoveOthers/Clear, which just enqueue) that
    /// also called HandleTaskQueue() immediately, mutating uiWindows synchronously - which meant
    /// showing a window (Popup.ShowAt -> UIManager.Add) as a side effect of code that was itself
    /// in the middle of iterating uiWindows (e.g. a window's own PerformLayout, or - the
    /// real-world trigger - a button's OnActivate handler running via RunInputHandler's "let it
    /// continue in the background" path when the surrounding dispatch chain doesn't complete
    /// perfectly synchronously) could invalidate that enumerator mid-iteration.
    ///
    /// Fixed at the root: Add() now only enqueues too, matching every other mutation method -
    /// uiWindows is only ever actually mutated inside HandleTaskQueue()'s own dequeue loop, which
    /// runs exactly once per frame at the top of Update() and doesn't itself foreach over
    /// uiWindows. This structurally removes the failure mode (nothing can mutate uiWindows while
    /// any of the loops above are active) instead of defensively snapshotting every read site via
    /// ToArray(), which would have reintroduced real per-frame GC pressure.
    ///
    /// This test reproduces the mechanism deterministically (a window that adds another window
    /// from inside its own PerformLayout) rather than relying on real mouse/async timing, and
    /// also documents the resulting one-frame-later semantics: the reentrantly-added window isn't
    /// actually present until the frame *after* the one that called Add().
    /// </summary>
    [TestClass]
    public class TestUIManagerReentrantWindowAdd
    {
        private class ReentrantAddWindow : UIWindow
        {
            private readonly UIManager targetManager;
            private readonly UIWindow windowToAdd;
            private bool alreadyAdded;

            public ReentrantAddWindow(UIManager targetManager, UIWindow windowToAdd)
            {
                this.targetManager = targetManager;
                this.windowToAdd = windowToAdd;
            }

            public override void PerformLayout(GameTime gameTime)
            {
                if (!alreadyAdded)
                {
                    alreadyAdded = true;
                    targetManager.Add(windowToAdd);
                }

                base.PerformLayout(gameTime);
            }
        }

        [TestMethod]
        public void AddingAWindow_FromInsideAnotherWindowsPerformLayout_DoesNotThrow()
        {
            var uiManager = new FakeUIManager();
            var newWindow = new EmptyUI();
            var reentrantWindow = new ReentrantAddWindow(uiManager, newWindow);

            uiManager.AddUI(reentrantWindow);

            // Before the fix, UIManager.Update's `foreach (var ui in uiWindows) { ... ui.Value.PerformLayout(gameTime); }`
            // loop would throw InvalidOperationException here, since reentrantWindow's own
            // PerformLayout override (invoked mid-loop) mutated uiWindows via Add immediately.
            uiManager.PerformLayout();

            // Add() now only enqueues (like every other mutation method) - the reentrantly-added
            // window isn't actually inserted into uiWindows until the NEXT frame's
            // HandleTaskQueue() call, not this same one.
            Assert.IsNull(uiManager.Find<EmptyUI>(),
                "Expected the reentrantly-added window to not be present yet - Add() only enqueues, it shouldn't take effect until the next frame.");

            uiManager.PerformLayout();

            Assert.IsNotNull(uiManager.Find<EmptyUI>(), "Expected the reentrantly-added window to have been added by the following frame.");
        }
    }
}
