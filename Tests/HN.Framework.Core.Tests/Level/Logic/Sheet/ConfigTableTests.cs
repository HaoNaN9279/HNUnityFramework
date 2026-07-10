#nullable enable

using System;
using System.Collections.Generic;
using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Sheet;

namespace HN.Framework.Core.Tests.Level.Logic.Sheet
{
    /// <summary>
    /// <see cref="ConfigTable{TKey, TRow}"/> 的单元测试。
    /// 直接通过构造函数创建表实例，不依赖序列化。
    /// </summary>
    [TestFixture]
    public class ConfigTableTests
    {
        private List<TestConfigRow> _rows = null!;

        [SetUp]
        public void SetUp()
        {
            _rows = new List<TestConfigRow>
            {
                new TestConfigRow { Id = 1, Name = "Test1" },
                new TestConfigRow { Id = 2, Name = "Test2" },
                new TestConfigRow { Id = 3, Name = "Test3" },
            };
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void Create_WithValidData_ReturnsRows()
        {
            var table = new ConfigTable<int, TestConfigRow>(_rows, r => r.Id);

            Assert.That(table.Count, Is.EqualTo(3));
            Assert.That(table.GetAll().Length, Is.EqualTo(3));
        }

        [Test]
        public void Get_ByKey_ReturnsCorrectRow()
        {
            var table = new ConfigTable<int, TestConfigRow>(_rows, r => r.Id);

            var row = table.Get(1);

            Assert.That(row.Name, Is.EqualTo("Test1"));
        }

        [Test]
        public void TryGet_ExistingKey_ReturnsTrue()
        {
            var table = new ConfigTable<int, TestConfigRow>(_rows, r => r.Id);

            var found = table.TryGet(2, out var row);

            Assert.That(found, Is.True);
            Assert.That(row, Is.Not.Null);
            Assert.That(row.Name, Is.EqualTo("Test2"));
        }

        [Test]
        public void TryGet_MissingKey_ReturnsFalse()
        {
            var table = new ConfigTable<int, TestConfigRow>(_rows, r => r.Id);

            var found = table.TryGet(99, out var row);

            Assert.That(found, Is.False);
            Assert.That(row, Is.Null);
        }

        [Test]
        public void ContainsKey_Existing_True()
        {
            var table = new ConfigTable<int, TestConfigRow>(_rows, r => r.Id);

            Assert.That(table.ContainsKey(1), Is.True);
            Assert.That(table.ContainsKey(3), Is.True);
        }

        [Test]
        public void ContainsKey_Missing_False()
        {
            var table = new ConfigTable<int, TestConfigRow>(_rows, r => r.Id);

            Assert.That(table.ContainsKey(99), Is.False);
        }

        [Test]
        public void Get_MissingKey_Throws()
        {
            var table = new ConfigTable<int, TestConfigRow>(_rows, r => r.Id);

            Assert.Throws<KeyNotFoundException>(() => table.Get(99));
        }

        [Test]
        public void Count_MatchesInput()
        {
            var table = new ConfigTable<int, TestConfigRow>(_rows, r => r.Id);

            Assert.That(table.Count, Is.EqualTo(3));
        }

        [Test]
        public void Create_WithDuplicateKeys_Throws()
        {
            var rows = new List<TestConfigRow>
            {
                new TestConfigRow { Id = 1, Name = "A" },
                new TestConfigRow { Id = 1, Name = "B" },
            };

            Assert.Throws<ArgumentException>(() =>
                new ConfigTable<int, TestConfigRow>(rows, r => r.Id));
        }

        [Test]
        public void Create_NullRows_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ConfigTable<int, TestConfigRow>(null!, r => r.Id));
        }

        [Test]
        public void Create_NullKeySelector_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ConfigTable<int, TestConfigRow>(_rows, null!));
        }
    }
}
