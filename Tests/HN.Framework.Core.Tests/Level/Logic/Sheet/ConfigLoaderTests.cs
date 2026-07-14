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
            global::MemoryPack.MemoryPackFormatterProvider.Register(new TestConfigRowFormatter());
            global::MemoryPack.MemoryPackFormatterProvider.Register(new TestConfigRowArrayFormatter());
        }

        [TearDown]
        public void TearDown()
        {
        }

        /// <summary>
        /// TestConfigRow 的手动 MemoryPack 格式化器。
        /// </summary>
        private sealed class TestConfigRowFormatter : global::MemoryPack.MemoryPackFormatter<TestConfigRow>
        {
            public override void Serialize<TBufferWriter>(ref global::MemoryPack.MemoryPackWriter<TBufferWriter> writer, ref TestConfigRow? value)
            {
                if (value == null) { writer.WriteNullObjectHeader(); return; }
                writer.WriteValue(value.Id);
                writer.WriteValue(value.Name);
            }

            public override void Deserialize(ref global::MemoryPack.MemoryPackReader reader, ref TestConfigRow? value)
            {
                value ??= new TestConfigRow();
                value.Id = reader.ReadValue<int>();
                value.Name = reader.ReadValue<string>();
            }
        }

        /// <summary>
        /// TestConfigRow[] 的手动 MemoryPack 格式化器。
        /// </summary>
        private sealed class TestConfigRowArrayFormatter : global::MemoryPack.MemoryPackFormatter<TestConfigRow[]>
        {
            public override void Serialize<TBufferWriter>(ref global::MemoryPack.MemoryPackWriter<TBufferWriter> writer, ref TestConfigRow[]? value)
            {
                if (value == null) { writer.WriteNullCollectionHeader(); return; }
                writer.WriteCollectionHeader(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    writer.WriteValue(value[i]);
                }
            }

            public override void Deserialize(ref global::MemoryPack.MemoryPackReader reader, ref TestConfigRow[]? value)
            {
                if (!reader.TryReadCollectionHeader(out int length))
                {
                    value = null;
                    return;
                }

                value = new TestConfigRow[length];
                for (int i = 0; i < length; i++)
                {
                    value[i] = reader.ReadValue<TestConfigRow>();
                }
            }
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
