#nullable enable

using System;
using System.Collections.Generic;
using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Sheet;

namespace HN.Framework.Core.Tests.Level.Logic.Sheet
{
    /// <summary>
    /// <see cref="ISheetManager"/> 接口契约测试。
    /// 使用最小内存实现验证接口行为。
    /// </summary>
    [TestFixture]
    public class ISheetManagerContractTests
    {
        private sealed class SheetManagerStub : ISheetManager
        {
            private readonly Dictionary<string, object> _tables = new Dictionary<string, object>();

            public IConfigTable<TKey, TRow> GetTable<TKey, TRow>(string tableName)
            {
                if (_tables.TryGetValue(tableName, out var table))
                    return (IConfigTable<TKey, TRow>)table;
                throw new KeyNotFoundException($"Table '{tableName}' not found.");
            }

            public void RegisterTable<TKey, TRow>(string tableName, IConfigTable<TKey, TRow> table)
            {
                if (_tables.ContainsKey(tableName))
                    throw new ArgumentException($"Table '{tableName}' is already registered.");
                _tables[tableName] = table;
            }

            public bool HasTable(string tableName) => _tables.ContainsKey(tableName);
        }

        private ISheetManager _manager = null!;
        private IConfigTable<int, TestConfigRow> _table = null!;

        [SetUp]
        public void SetUp()
        {
            _manager = new SheetManagerStub();

            var rows = new List<TestConfigRow>
            {
                new TestConfigRow { Id = 1, Name = "Test" },
            };
            _table = new ConfigTable<int, TestConfigRow>(rows, r => r.Id);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void RegisterAndGetTable_ReturnsSameTable()
        {
            _manager.RegisterTable("TestTable", _table);

            var retrieved = _manager.GetTable<int, TestConfigRow>("TestTable");

            Assert.That(retrieved, Is.SameAs(_table));
        }

        [Test]
        public void GetTable_Unregistered_Throws()
        {
            Assert.Throws<KeyNotFoundException>(() =>
                _manager.GetTable<int, TestConfigRow>("NonExistent"));
        }

        [Test]
        public void RegisterTable_Duplicate_Throws()
        {
            _manager.RegisterTable("TestTable", _table);

            Assert.Throws<ArgumentException>(() =>
                _manager.RegisterTable("TestTable", _table));
        }

        [Test]
        public void HasTable_Registered_True()
        {
            _manager.RegisterTable("TestTable", _table);

            Assert.That(_manager.HasTable("TestTable"), Is.True);
        }

        [Test]
        public void HasTable_Unregistered_False()
        {
            Assert.That(_manager.HasTable("NonExistent"), Is.False);
        }
    }
}
