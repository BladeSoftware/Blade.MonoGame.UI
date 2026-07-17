#nullable enable

using System;
using Blade.MG.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BladeUI.UnitTesting.Tests.Core
{
    [TestClass]
    public class TestBinding
    {
        [TestMethod]
        public void ImplicitCast_FromValue_CreatesWorkingBinding()
        {
            Binding<int> binding = 5;

            Assert.AreEqual(5, binding.Value);
            Assert.IsTrue(binding.IsImplicitCast);
        }

        [TestMethod]
        public void GetterSetterConstructor_ReadsAndWritesBackingVariable()
        {
            int backing = 10;
            var binding = new Binding<int>(() => backing, value => backing = value);

            Assert.AreEqual(10, binding.Value);

            binding.Value = 42;

            Assert.AreEqual(42, backing);
            Assert.AreEqual(42, binding.Value);
        }

        [TestMethod]
        public void SetField_OnNullField_CreatesUsableBinding()
        {
            // Mirrors UIComponent.SetField<T>(ref Binding<T> field, T value) when field starts out null.
            Binding<int>? field = null;

            if (field == null)
            {
                field = new Binding<int>();
            }

            field.Value = 99;

            Assert.AreEqual(99, field.Value);
        }

        [TestMethod]
        public void FromString_ValidInput_UpdatesValue()
        {
            Binding<int> binding = 0;

            binding.FromString("30");

            Assert.AreEqual(30, binding.Value);
        }

        [TestMethod]
        public void FromString_InvalidInput_LeavesValueUnchangedAndDoesNotThrow()
        {
            Binding<int> binding = 80;

            binding.FromString("not a number");

            Assert.AreEqual(80, binding.Value);
        }

        [TestMethod]
        public void ToString_ReturnsUnderlyingValueAsString()
        {
            Binding<int> binding = 80;

            Assert.AreEqual("80", binding.ToString());
        }

        [TestMethod]
        public void Equals_ComparesUnderlyingValue()
        {
            Binding<int> a = 7;
            Binding<int> b = 7;

            Assert.IsTrue(a.Equals(7));
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        // Binding<T>'s plain-value constructor (used by every implicit T->Binding<T> cast, e.g.
        // `border.Background = Color.Red;`) used to unconditionally allocate a Getter/Setter
        // closure pair over `this` even though that mode never uses them - _backingVar is read/
        // written directly instead now. Confirms the allocation is actually gone, not just that
        // behavior is unchanged (the other tests in this file already cover that).
        [TestMethod]
        public void PlainValueConstruction_DoesNotAllocateGetterSetterClosures()
        {
            // Warm up the JIT/GC before measuring so first-call overhead doesn't skew the count.
            for (int i = 0; i < 1000; i++)
            {
                _ = new Binding<int>(i);
            }

            const int count = 20000;
            var bindings = new Binding<int>[count];

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < count; i++)
            {
                bindings[i] = new Binding<int>(i);
            }
            long after = GC.GetAllocatedBytesForCurrentThread();

            long bytesPerInstance = (after - before) / count;

            // A Binding<int> object itself (header + _getter/_setter/_backingVar/Changed/
            // IsImplicitCast fields) is on the order of 40-56 bytes on a 64-bit runtime. Before
            // this fix, construction additionally allocated 2 delegate objects (Getter/Setter),
            // each its own heap allocation - comfortably pushing the total past 100 bytes/
            // instance. 80 sits well below that combined size while leaving headroom for
            // runtime/architecture measurement differences.
            Assert.IsTrue(bytesPerInstance < 80,
                $"Expected plain-value Binding<int> construction to allocate only the Binding object itself (no Getter/Setter closures), but measured {bytesPerInstance} bytes/instance.");

            GC.KeepAlive(bindings);
        }

        [TestMethod]
        public void ConverterBinding_ValueConstructor_ConvertsOnGetAndSet()
        {
            var binding = new Binding<string, int, IntToStringBindingConverter>(5);

            Assert.AreEqual("5", binding.Value);

            binding.Value = "42";

            Assert.AreEqual("42", binding.Value);
        }

        [TestMethod]
        public void ConverterBinding_GetterSetterConstructor_RoundTripsThroughConverterAndBackingVariable()
        {
            int backing = 5;
            var binding = new Binding<string, int, IntToStringBindingConverter>(() => backing, v => backing = v);

            Assert.AreEqual("5", binding.Value);

            binding.Value = "42";

            Assert.AreEqual(42, backing);
            Assert.AreEqual("42", binding.Value);
        }

        [TestMethod]
        public void ConverterBinding_FromString_WritesConvertedValueToBackingVariable()
        {
            int backing = 0;
            var binding = new Binding<string, int, IntToStringBindingConverter>(() => backing, v => backing = v);

            binding.FromString("77");

            Assert.AreEqual(77, backing);
        }

        [TestMethod]
        public void ConverterBinding_ToString_ReturnsConvertedValue()
        {
            var binding = new Binding<string, int, IntToStringBindingConverter>(() => 123, null);

            Assert.AreEqual("123", binding.ToString());
        }

        [TestMethod]
        public void ConverterBinding_ReadOnlyRelay_FromString_IsNoOp()
        {
            int backing = 9;
            var binding = new Binding<string, int, IntToStringBindingConverter>(() => backing, null);

            binding.FromString("123");

            Assert.AreEqual(9, backing);
        }

        [TestMethod]
        public void PlainBinding_ReadOnlyRelay_FromString_IsNoOp()
        {
            int backing = 9;
            var binding = new Binding<int>(() => backing, null);

            binding.FromString("123");

            Assert.AreEqual(9, backing);
        }
    }
}
