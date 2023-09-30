using BladeUI.UnitTesting.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// [assembly: Parallelize(Scope = ExecutionScope.ClassLevel)]

namespace BladeUI.UnitTesting
{

    [TestClass]
    internal class TestAssemblyInitialize
    {

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            // Create Resources to be shared by all unit tests in this class

        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            FakeGame.Instance?.Dispose();
        }

    }
}
