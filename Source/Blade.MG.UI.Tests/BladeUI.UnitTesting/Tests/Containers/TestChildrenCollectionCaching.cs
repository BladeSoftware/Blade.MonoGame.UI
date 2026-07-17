using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BladeUI.UnitTesting.Tests.Containers
{
    /// <summary>
    /// Container.Children and UIComponent.InternalChildren used to call List&lt;T&gt;.AsReadOnly()
    /// on every single access - allocating a brand-new ReadOnlyCollection&lt;T&gt; wrapper every
    /// time, including from Measure/Arrange/RenderControl (every frame, for every control).
    /// ReadOnlyCollection&lt;T&gt; wraps the underlying list by reference (a live view, not a
    /// snapshot), so the wrapper can safely be allocated once and reused forever - these tests
    /// assert the getter now returns the exact same object reference across repeated calls
    /// (proving the allocation was eliminated) and that it still reflects Add/Remove changes
    /// correctly (proving the "cache once" fix didn't introduce staleness).
    /// </summary>
    [TestClass]
    public class TestChildrenCollectionCaching
    {
        [TestMethod]
        public void Container_Children_ReturnsSameReference_AcrossRepeatedCalls()
        {
            var stack = new StackPanel();

            Assert.AreSame(stack.Children, stack.Children,
                "Expected Container.Children to return the same cached wrapper instance on every call, not allocate a new ReadOnlyCollection<T> each time.");
        }

        [TestMethod]
        public void Container_Children_StillReflectsAddAndRemove_DespiteCaching()
        {
            var stack = new StackPanel();
            var cachedView = stack.Children;

            Assert.AreEqual(0, cachedView.Count);

            var button = new Button();
            stack.AddChild(button);

            Assert.AreEqual(1, cachedView.Count, "Expected the cached Children view to reflect an AddChild that happened after the reference was captured.");
            Assert.AreSame(button, cachedView[0]);

            stack.RemoveChild(button);

            Assert.AreEqual(0, cachedView.Count, "Expected the cached Children view to reflect a RemoveChild that happened after the reference was captured.");
        }
    }
}
