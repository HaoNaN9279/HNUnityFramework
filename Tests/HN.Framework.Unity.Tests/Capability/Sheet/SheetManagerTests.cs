#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Sheet;
using HN.Framework.Unity.Capability.Sheet;

namespace HN.Framework.Unity.Tests.Capability.Sheet
{
    /// <summary>
    /// <see cref="SheetManager"/> 的单元测试。
    /// </summary>
    [TestFixture]
    public class SheetManagerTests
    {
        private SheetManager _manager = null!;

        [SetUp]
        public void SetUp()
        {
            _manager = new SheetManager();
        }

        [Test]
        public void RegisterAndGetTable_ReturnsSameTable()
        {
            var rows = new[] { "a", "bc" };
            var table = new ConfigTable<int, string>(rows, x => x.Length);
            _manager.RegisterTable("test", table);

            var result = _manager.GetTable<int, string>("test");

            Assert.That(result, Is.SameAs(table));
        }

        [Test]
        public void GetTable_Unregistered_ThrowsKeyNotFound()
        {
            Assert.That(
                () => _manager.GetTable<int, string>("missing"),
                Throws.InstanceOf<System.Collections.Generic.KeyNotFoundException>());
        }

        [Test]
        public void HasTable_Registered_ReturnsTrue()
        {
            var table = new ConfigTable<int, string>(new[] { "a" }, x => x.Length);
            _manager.RegisterTable("test", table);

            Assert.That(_manager.HasTable("test"), Is.True);
        }

        [Test]
        public void HasTable_Unregistered_ReturnsFalse()
        {
            Assert.That(_manager.HasTable("missing"), Is.False);
        }

        [Test]
        public void RegisterTable_NullOrEmptyName_Throws()
        {
            var table = new ConfigTable<int, string>(new[] { "a" }, x => x.Length);

            Assert.That(() => _manager.RegisterTable(null!, table), Throws.ArgumentNullException);
            Assert.That(() => _manager.RegisterTable(string.Empty, table), Throws.ArgumentNullException);
        }

        [Test]
        public void RegisterTable_DuplicateName_Throws()
        {
            var table1 = new ConfigTable<int, string>(new[] { "a" }, x => x.Length);
            var table2 = new ConfigTable<int, string>(new[] { "b" }, x => x.Length);
            _manager.RegisterTable("dup", table1);

            Assert.That(() => _manager.RegisterTable("dup", table2), Throws.ArgumentException);
        }
    }
}
