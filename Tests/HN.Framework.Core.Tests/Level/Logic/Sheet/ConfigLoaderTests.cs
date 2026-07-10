#nullable enable

using System;
using NUnit.Framework;
using HN.Framework.Core.Level.Logic.Sheet;
using HN.Framework.Core.Driver.Common.Serialization;

namespace HN.Framework.Core.Tests.Level.Logic.Sheet
{
    /// <summary>
    /// <see cref="ConfigLoader"/> 的单元测试。
    /// </summary>
    [TestFixture]
    public class ConfigLoaderTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void LoadFromBytes_Roundtrip()
        {
            var original = new TestConfigRow { Id = 5, Name = "Roundtrip" };

            var data = MemoryPackSerializer.Serialize(original);
            var result = ConfigLoader.LoadFromBytes<TestConfigRow>(data);

            Assert.That(result.Id, Is.EqualTo(original.Id));
            Assert.That(result.Name, Is.EqualTo(original.Name));
        }

        [Test]
        public void LoadTable_BuildsConfigTable()
        {
            var rows = new[]
            {
                new TestConfigRow { Id = 1, Name = "A" },
                new TestConfigRow { Id = 2, Name = "B" },
            };

            var data = MemoryPackSerializer.Serialize(rows);
            var table = ConfigLoader.LoadTable<int, TestConfigRow>(data, r => r.Id);

            Assert.That(table.Count, Is.EqualTo(2));
            Assert.That(table.Get(1).Name, Is.EqualTo("A"));
            Assert.That(table.Get(2).Name, Is.EqualTo("B"));
        }

        [Test]
        public void LoadFromBytes_NullData_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ConfigLoader.LoadFromBytes<TestConfigRow>(null!));
        }

        [Test]
        public void LoadFromBytes_EmptyData_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                ConfigLoader.LoadFromBytes<TestConfigRow>(Array.Empty<byte>()));
        }

        [Test]
        public void LoadTable_NullData_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ConfigLoader.LoadTable<int, TestConfigRow>(null!, r => r.Id));
        }

        [Test]
        public void LoadTable_NullKeySelector_Throws()
        {
            var data = MemoryPackSerializer.Serialize(Array.Empty<TestConfigRow>());

            Assert.Throws<ArgumentNullException>(() =>
                ConfigLoader.LoadTable<int, TestConfigRow>(data, null!));
        }
    }
}
