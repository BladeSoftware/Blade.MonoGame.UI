#nullable enable

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
    }
}
